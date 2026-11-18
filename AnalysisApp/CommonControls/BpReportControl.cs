using AnalysisApp.Commons;
using AnalysisSystem.Base;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.DAL;
using Zoncare.Utilities;

namespace AnalysisApp.CommonControls
{
    public partial class BpReportControl : BaseUserControl
    {
        private bool _initializing = true;

        /// <summary>当前语言下 systemType == "bp" 的报告模板列表</summary>
        private List<reportTemplate> ReportTemplateList;
        //函数
        Dictionary<string, TemplateFunctionInfo> Functions = new Dictionary<string, TemplateFunctionInfo>();

        private int TotalPages = 0;
        private int CurrentPageIndex = 0;

        private Dictionary<string, int> PageIdxs = new Dictionary<string, int>();
        private string PrintPreviewType = "All";
        private string ReportAdvice = "";
        private static readonly string[] EnableAASI8337 = { "iE6T", "iE6Pro", "iE7T", "iE7Pro", "VIS9", "iE75" };

        private List<List<JObject>> SummaryInfo = new List<List<JObject>>();
        private List<List<JObject>> DataListInfo = new List<List<JObject>>();
        private List<List<JObject>> HourInfo = new List<List<JObject>>();
        private List<List<JObject>> TrendWaveInfo = new List<List<JObject>>();
        private List<List<JObject>> HistogramInfo = new List<List<JObject>>();
        private List<List<JObject>> PieInfo = new List<List<JObject>>();
        private List<List<JObject>> FitInfo = new List<List<JObject>>();
        private List<List<JObject>> ArteriosclerosisInfo = new List<List<JObject>>();

        private BpPrintReportPage printReportPage = new BpPrintReportPage();
        private BpReportPrintManager ReportPrintManager = null;

        private bpDiagnoseSettingValues SettingValues;
        private BPSummaryData BpSummaryData;

        private List<int> SbpMaxMin = new List<int>(3);
        private List<Double> SbpMaxMinPercent = new List<Double>(3);

        private List<int> DbpMaxMin = new List<int>(3);
        private List<Double> DbpMaxMinPercent = new List<Double>(3);

        private List<int> PulseMaxMin = new List<int>(3);
        private List<Double> PulseMaxMinPercent = new List<Double>(3);

        public BpReportControl()
        {
            InitializeComponent();

            // 界面只有一个模板单选按钮且设计时已选中，重复点击不会再触发 CheckedChanged，
            // 因此用 Click 保证每次点击都能回填模板内容（事件只绑定一次）
            this.radioButton1.Click += templateRadio_Click;
            // 模板编辑窗体保存后重新加载模板列表（设计器中该按钮未绑定事件，这里统一绑定一次）
            this.btnModifyTemplate.Click += btnModifyTemplate_Click;
        }

        #region 打印勾选状态持久化

        /// <summary>
        /// 勾选状态记录文件，以程序运行目录为基准；
        /// 本机调试时即 E:\project\EcgBpAnalysis\EcgAnalyseStart\bin\Debug\config\bpPrintCheck.json。
        /// </summary>
        private static readonly string PrintCheckFilePath = Path.Combine(Application.StartupPath, "config", "bpPrintCheck.json");

        /// <summary>程序化恢复勾选状态时抑制写文件，避免把设计器默认值回写</summary>
        private bool _restoringPrintCheck = false;

        /// <summary>
        /// 界面上参与打印内容选择的复选框（initPrintData 用它们决定打印哪些页）。
        /// 结论选项（cbxDownPercent、cbxMedicine 等 9 个）按病例保存在 bpReportConclusion，
        /// 不纳入全局记录，避免打开不同病例时被上次偏好覆盖。
        /// </summary>
        private List<CheckBox> GetPrintCheckBoxes()
        {
            return new List<CheckBox>
            {
                cbxSummarize, cbxDataList, cbxHourDataList, cbxTrendWave,
                cbxHistogram, cbxPie, cbxFitPlot, cbxAtherosclerosis
            };
        }

        /// <summary>
        /// 界面加载时读取 config\bpPrintCheck.json，按控件名恢复勾选状态；
        /// 同时绑定 CheckedChanged，用户每次勾选/取消勾选都立即写回文件。
        /// 文件不存在、为空或格式损坏时保持设计器默认状态，并补写一份默认记录。
        /// </summary>
        private void InitPrintCheckState()
        {
            List<CheckBox> checkBoxes = GetPrintCheckBoxes();
            bool restored = false;

            _restoringPrintCheck = true;
            try
            {
                if (File.Exists(PrintCheckFilePath))
                {
                    string json = File.ReadAllText(PrintCheckFilePath, Encoding.UTF8);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        JObject saved = JObject.Parse(json);
                        foreach (CheckBox cbx in checkBoxes)
                        {
                            JToken token = saved[cbx.Name];
                            if (token != null)
                            {
                                cbx.Checked = token.Value<bool>();
                            }
                        }
                        restored = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "血压打印勾选状态读取");
            }
            finally
            {
                _restoringPrintCheck = false;
            }

            if (!restored)
            {
                SavePrintCheckState();
            }

            // 状态变化时写文件；先解绑再绑定，避免重复订阅
            foreach (CheckBox cbx in checkBoxes)
            {
                cbx.CheckedChanged -= printCheckBox_CheckedChanged;
                cbx.CheckedChanged += printCheckBox_CheckedChanged;
            }
        }

        /// <summary>把当前所有打印复选框状态按控件名写入 JSON 文件</summary>
        private void SavePrintCheckState()
        {
            try
            {
                JObject state = new JObject();
                foreach (CheckBox cbx in GetPrintCheckBoxes())
                {
                    state[cbx.Name] = cbx.Checked;
                }

                string dir = Path.GetDirectoryName(PrintCheckFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(PrintCheckFilePath, state.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "血压打印勾选状态保存");
            }
        }

        private void printCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_restoringPrintCheck || DesignMode)
            {
                return;
            }
            SavePrintCheckState();
        }

        #endregion

        public void RefreshUI()
        {
            InitPrintCheckState();

            SettingValues = CommonValues.SystemSettings.bpSets.bpDiagnoseSets;
            BpSummaryData = CalcBPInfo.GetBPSummaryData();
            Functions = GetTemplateFunctions();

            InitCheckBoxStatus();
            InitAdviceInfo();
            InitReportTemplateRadio();

            this.cbxAtherosclerosis.Visible = false;
            this.picAtherosclerosis.Visible = false;
            if (AnaValues.CurrentCase.deviceType == "4" || EnableAASI8337.Contains(AnaValues.CurrentCase.deviceModel))
            {
                this.cbxAtherosclerosis.Visible = true;
                this.picAtherosclerosis.Visible = true;
            }

            _initializing = false;
        }

        private void InitCheckBoxStatus()
        {
            string conclusionStr = AnaValues.CurrentCase.bpReportConclusion;
            if (conclusionStr != null)
            {
                JObject conclusionObj = JObject.Parse(conclusionStr);
                int percent = conclusionObj["percent"]?.Value<int>() ?? 0;
                if (percent == 1)
                {
                    this.cbxDownPercent.Checked = true;
                }
                else
                {
                    this.cbxDownPercent.Checked = false;
                }
                int type = conclusionObj["type"]?.Value<int>() ?? 0;
                if (type == 1)
                {
                    this.cbxFluctuationType.Checked = true;
                }
                else
                {
                    this.cbxFluctuationType.Checked = false;
                }
                int wave = conclusionObj["wave"]?.Value<int>() ?? 0;
                string record1 = conclusionObj["record1"]?.Value<string>() ?? "";
                string record2 = conclusionObj["record2"]?.Value<string>() ?? "";
                if (wave == 1)
                {
                    this.cbxHightBp.Checked = true;
                    this.txtHightBpRecord.Text = record1;
                    this.txtLowBpRecord.Text = record2;
                }
                else
                {
                    this.cbxHightBp.Checked = false;
                }
                int option1 = conclusionObj["option1"]?.Value<int>() ?? 0;
                if (option1 == 1)
                {
                    this.cbxMedicine.Checked = true;
                }
                else if (option1 == 2)
                {
                    this.cbxDiscomfort.Checked = true;
                }
                else
                {
                    this.cbxMedicineDiscomfort.Checked = true;
                }
                int abmp = conclusionObj["abmp"]?.Value<int>() ?? 0;
                if (abmp == 1)
                {
                    this.cbxStandard.Checked = true;
                }
                else
                {
                    this.cbxStandard.Checked = false;
                }

                int wch = conclusionObj["wch"]?.Value<int>() ?? 0;
                if (wch == 1)
                {
                    this.chxWch.Checked = true;
                }
                else
                {
                    this.chxWch.Checked = false;
                }
                int maskedH = conclusionObj["maskedH"]?.Value<int>() ?? 0;
                if (maskedH == 1)
                {
                    this.chxMaskedH.Checked = true;
                }
                else
                {
                    this.chxMaskedH.Checked = false;
                }
                string result = conclusionObj["result"]?.Value<string>() ?? "";
                // 已保存结论中可能残留模板函数占位符（旧数据或直接入库的模板原文），显示前统一替换
                this.advice.Text = ApplyTemplateFunctions(result);
            }
            else
            {
                examCase caseInfo = ZoncareEntities.Instance.ExamCases.Where(o => o.id == AnaValues.CurrentCase.id).FirstOrDefault();
                if (caseInfo.status < 3)
                {
                    List<BPDataInfo> bpDataList = CalcBPInfo.GetBPDataList(2);
                    if (CalcBPInfo.isWCHOrMasked(bpDataList, 0))
                    {
                        this.chxWch.Checked = true;
                    }
                    else
                    {
                        this.chxWch.Checked = false;
                    }
                    if (CalcBPInfo.isWCHOrMasked(bpDataList, 1))
                    {
                        this.chxMaskedH.Checked = true;
                    }
                    else
                    {
                        this.chxMaskedH.Checked = false;
                    }
                }
            }
        }

        private void InitAdviceInfo()
        {
            this.cbxHightBp.Text = $"最高血压:{BpSummaryData.bpAllData.SbpMax}/{BpSummaryData.bpAllData.SbpMaxDbp} {SettingValues.unit},发生于:{BpSummaryData.bpAllData.SbpMaxTime}";
            this.lblLowBp.Text = $"最低血压:{BpSummaryData.bpAllData.SbpMin}/{BpSummaryData.bpAllData.SbpMinDbp} {SettingValues.unit},发生于:{BpSummaryData.bpAllData.SbpMinTime}";
            this.cbxDownPercent.Text = $"夜间下降百分率: {BpSummaryData.bpAllData.SbpPercentage.ToString("F2")}%";
            this.cbxFluctuationType.Text = $"昼夜节律类型: {BpSummaryData.bpAllData.ZyjlType}";

        }

        #region 报告模板（单个 RadioButton）

        /// <summary>
        /// 加载 languageId == CommonValues.LanguageID 且 systemType == "bp" 的报告模板，
        /// 把模板名（template_name）显示在 radioButton1 上，并把模板记录存入 Tag 供点击时取用。
        /// 与 EcgReportControl.initReportTemplateRadios 的区别：血压界面只有一个模板按钮，
        /// 因此优先取默认模板（is_default == 1），没有默认模板时取第一条。
        /// 注意：此处不修改 advice.Text，避免覆盖 InitCheckBoxStatus 恢复的病例已保存结论。
        /// </summary>
        private void InitReportTemplateRadio()
        {
            try
            {
                ReportTemplateList = ZoncareEntities.Instance.ReportTemplates
                    .Where(o => o.languageId == CommonValues.LanguageID && o.systemType == "bp")
                    .ToList();

                reportTemplate item = ReportTemplateList.FirstOrDefault(o => o.isDefault == 1)
                                      ?? ReportTemplateList.FirstOrDefault();

                this.radioButton1.Tag = item;
                this.radioButton1.Text = item == null ? "" : item.templateName;
                this.radioButton1.Visible = item != null;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "血压报告模板初始化");
            }
        }

        /// <summary>
        /// 点击模板单选按钮：把该模板的 content 填入结论框 advice，并同步写回病例结论。
        /// 模仿 EcgReportControl.templateRadio_CheckedChanged：先把选中模板置为默认，
        /// 再回填文本并调用 SaveAdvice 持久化，避免下次刷新时结论被还原。
        /// </summary>
        private void templateRadio_Click(object sender, EventArgs e)
        {
            if (_initializing) return;

            reportTemplate selected = this.radioButton1.Tag as reportTemplate;
            if (selected == null)
            {
                return;
            }

            try
            {
                if (ReportTemplateList != null && ReportTemplateList.Count > 0)
                {
                    bool changed = false;
                    foreach (reportTemplate item in ReportTemplateList)
                    {
                        int newDefault = ReferenceEquals(item, selected) ? 1 : 0;
                        if (item.isDefault != newDefault)
                        {
                            item.isDefault = newDefault;
                            changed = true;
                        }
                    }
                    if (changed)
                    {
                        ZoncareEntities.Instance.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "血压报告模板默认设置");
            }

            // 先把模板中的函数占位符渲染成实际数值，再规范化换行赋给 RichTextBox
            this.advice.Text = NormalizeLineBreaks(ApplyTemplateFunctions(selected.content));

            // 同步写入 bpReportConclusion.result，与 advice_KeyUp 的保存行为保持一致
            if (!string.IsNullOrEmpty(selected.content))
            {
                try
                {
                    SaveAdvice(null);
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex, "血压报告模板结论保存");
                }
            }
        }

        /// <summary>编辑模板：打开 bp 模板编辑窗体，保存后重新加载模板列表并刷新按钮文字</summary>
        private void btnModifyTemplate_Click(object sender, EventArgs e)
        {
            // 传入当前病例的 BP 模板函数，编辑窗体据此生成预览与函数列表
            using (ReportTemplateForm reportTemplate = new ReportTemplateForm(Functions, "bp"))
            {
                reportTemplate.ShowDialogEx(this.FindForm(), showLayer: true);
                if (reportTemplate.TemplateSaved)
                {
                    InitReportTemplateRadio();
                }
            }
        }

        /// <summary>
        /// 规范化模板内容后再赋给 RichTextBox，保证换行正确且没有多余空行。
        /// 说明：RichTextBox 的换行符是 LF(\n)，若传入 CRLF(\r\n)，
        /// RichEdit 会把 CR 当作段落分隔、LF 再当作一次换行，导致每个换行中间多一个空行；
        /// 另外数据库中的 content 也可能存的是转义文本（反斜杠 + n 两个字符）。
        /// </summary>
        private static string NormalizeLineBreaks(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text ?? "";
            }

            // 没有真实换行符时，才按转义文本处理，避免破坏正文里本就存在的反斜杠
            if (text.IndexOf('\n') < 0 && text.IndexOf('\r') < 0)
            {
                text = text.Replace("\\r\\n", "\n").Replace("\\n", "\n");
            }

            // 统一为 LF（RichTextBox 原生换行符），去掉 CR 以免产生多余空行
            text = text.Replace("\r\n", "\n").Replace("\r", "\n");

            // 合并多余空行：连续多个换行只保留一个
            while (text.Contains("\n\n"))
            {
                text = text.Replace("\n\n", "\n");
            }

            return text;
        }

        /// <summary>
        /// 把文本中的模板函数占位符（Functions 的 Key，如 &lt;$MaxSBPAll$&gt;）
        /// 替换为「Value + Unit」（如 "130" + "mmHg" = "130mmHg"）。
        /// 模板编辑窗体保存的是带占位符的 content，回填结论前必须渲染成实际数值。
        /// 模仿 EcgReportControl.ApplyTemplateFunctions。
        /// </summary>
        private string ApplyTemplateFunctions(string text)
        {
            if (string.IsNullOrEmpty(text) || Functions == null || Functions.Count == 0)
            {
                return text;
            }

            string result = text;
            foreach (KeyValuePair<string, TemplateFunctionInfo> kv in Functions)
            {
                if (string.IsNullOrEmpty(kv.Key))
                {
                    continue;
                }
                // 替换为「值 + 单位」：值或单位为空时按空串处理
                string replaceValue = (kv.Value.Value ?? "") + (kv.Value.Unit ?? "");
                result = result.Replace(kv.Key, replaceValue);
            }
            return result;
        }

        #endregion

        private Dictionary<string, TemplateFunctionInfo> GetTemplateFunctions()
        {
            Dictionary<string, TemplateFunctionInfo> functions = new Dictionary<string, TemplateFunctionInfo>();
            BPDetailData allData = BpSummaryData.bpAllData;
            BPDetailData dayData = BpSummaryData.bpDayData;
            BPDetailData nightData = BpSummaryData.bpNightData;

            functions["<$AllTimes$>"] = new TemplateFunctionInfo(CommonValues.Langs["全程检测血压次数"], allData.Count.ToString(), CommonValues.Langs["次"]);
            functions["<$EffectiveTims$>"] = new TemplateFunctionInfo(CommonValues.Langs["全程检测血压有效次数"], allData.EffectiveCount.ToString(), CommonValues.Langs["次"]);
            functions["<$EffectivePercentage$>"] = new TemplateFunctionInfo(CommonValues.Langs["全程检测血压有效率"], allData.EffectiveCountRate.ToString(), "%");
            functions["<$MaxSBPAll$>"] = new TemplateFunctionInfo(CommonValues.Langs["最大收缩压-全天"], allData.SbpMax.ToString(), SettingValues.unit);
            functions["<$MaxSBPAllTime$>"] = new TemplateFunctionInfo(CommonValues.Langs["最大收缩压-全天(发生时间)"], allData.SbpMaxTime.ToString(), "");
            functions["<$MaxDBPAll$>"] = new TemplateFunctionInfo(CommonValues.Langs["最大舒张压-全天"], allData.DbpMax.ToString(), SettingValues.unit);
            functions["<$MaxDBPAllTime$>"] = new TemplateFunctionInfo(CommonValues.Langs["最大舒张压-全天(发生时间)"], allData.DbpMaxTime.ToString(), "");
            functions["<$AvgSBPAll$>"] = new TemplateFunctionInfo(CommonValues.Langs["平均收缩压-全天"], allData.SbpAvg.ToString(), SettingValues.unit);
            functions["<$AvgDBPAll$>"] = new TemplateFunctionInfo(CommonValues.Langs["平均舒张压-全天"], allData.DbpAvg.ToString(), SettingValues.unit);
            functions["<$AvgMapAll$>"] = new TemplateFunctionInfo(CommonValues.Langs["平均动脉压平均值-全天"], allData.MapAvg.ToString(), SettingValues.unit);
            functions["<$AvgSBPAllNormal$>"] = new TemplateFunctionInfo(CommonValues.Langs["平均收缩压-全天(正常值)"], SettingValues.avgSys.ToString(), SettingValues.unit);
            functions["<$AvgDBPAllNormal$>"] = new TemplateFunctionInfo(CommonValues.Langs["平均舒张压-全天(正常值)"], SettingValues.avgDia.ToString(), SettingValues.unit);
            functions["<$AvgSBPDay$>"] = new TemplateFunctionInfo(CommonValues.Langs["平均收缩压-白天"], dayData.SbpAvg.ToString(), SettingValues.unit);
            functions["<$AvgDBPDay$>"] = new TemplateFunctionInfo(CommonValues.Langs["平均舒张压-白天"], dayData.DbpAvg.ToString(), SettingValues.unit);
            functions["<$AvgMapDay$>"] = new TemplateFunctionInfo(CommonValues.Langs["平均动脉压平均值-白天"], dayData.MapAvg.ToString(), SettingValues.unit);
            functions["<$AvgSBPDayNormal$>"] = new TemplateFunctionInfo(CommonValues.Langs["平均收缩压-白天(正常值)"], SettingValues.avgSysOfDay.ToString(), SettingValues.unit);
            functions["<$AvgDBPDayNormal$>"] = new TemplateFunctionInfo(CommonValues.Langs["平均舒张压-白天(正常值)"], SettingValues.avgDiaOfDay.ToString(), SettingValues.unit);
            functions["<$AvgSBPNight$>"] = new TemplateFunctionInfo(CommonValues.Langs["平均收缩压-晚间"], nightData.SbpAvg.ToString(), SettingValues.unit);
            functions["<$AvgDBPNight$>"] = new TemplateFunctionInfo(CommonValues.Langs["平均舒张压-晚间"], nightData.DbpAvg.ToString(), SettingValues.unit);
            functions["<$AvgMapNight$>"] = new TemplateFunctionInfo(CommonValues.Langs["平均动脉压平均值-晚间"], nightData.MapAvg.ToString(), SettingValues.unit);
            functions["<$AvgSBPNightNormal$>"] = new TemplateFunctionInfo(CommonValues.Langs["平均收缩压-晚间(正常值)"], SettingValues.avgSysOfNight.ToString(), SettingValues.unit);
            functions["<$AvgDBPNightNormal$>"] = new TemplateFunctionInfo(CommonValues.Langs["平均舒张压-晚间(正常值)"], SettingValues.avgDiaOfNight.ToString(), SettingValues.unit);
            functions["<$SBPLoadDay$>"] = new TemplateFunctionInfo(CommonValues.Langs["收缩压血压负荷-白天"], dayData.SbpLoadNum.ToString(), "");
            functions["<$SBPLoadNight$>"] = new TemplateFunctionInfo(CommonValues.Langs["收缩压血压负荷-晚间"], nightData.SbpLoadNum.ToString(), "");
            functions["<$DBPLoadDay$>"] = new TemplateFunctionInfo(CommonValues.Langs["舒张压血压负荷-白天"], dayData.DbpLoadNum.ToString(), "");
            functions["<$DBPLoadNight$>"] = new TemplateFunctionInfo(CommonValues.Langs["舒张压血压负荷-晚间"], nightData.DbpLoadNum.ToString(), "");
            functions["<$MorningBloodPressure$>"] = new TemplateFunctionInfo(CommonValues.Langs["早间血压"], $"{allData.MorningSbpAvg}/{allData.MorningDbpAvg}", SettingValues.unit);
            functions["<$AvgMorningSBPNormal$>"] = new TemplateFunctionInfo(CommonValues.Langs["早间收缩压参考值"], SettingValues.avgSysOfMorning.ToString(), SettingValues.unit);
            functions["<$AvgMorningDBPNormal$>"] = new TemplateFunctionInfo(CommonValues.Langs["早间舒张压参考值"], SettingValues.avgDiaOfMorning.ToString(), SettingValues.unit);
            functions["<$AvgMorningBP$>"] = new TemplateFunctionInfo(CommonValues.Langs["血压晨峰指数"], allData.MorningPre.ToString(), SettingValues.unit);
            functions["<$SBPcircadian$>"] = new TemplateFunctionInfo(CommonValues.Langs["收缩压昼夜节律"], CalcBPInfo.GetDAN(allData.SbpPercentage).ToString(), "");
            functions["<$DBPcircadian$>"] = new TemplateFunctionInfo(CommonValues.Langs["舒张压昼夜节律"], CalcBPInfo.GetDAN(allData.DbpPercentage).ToString(), "");
            functions["<$AASI$>"] = new TemplateFunctionInfo(CommonValues.Langs["AASI"], allData.AASI.ToString(), SettingValues.unit);

            return functions;
        }

        private void cbxHightBp_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;

            if (this.cbxHightBp.Checked)
            {
                this.txtHightBpRecord.Enabled = true;
                this.txtLowBpRecord.Enabled = true;
            }
            else
            {
                this.txtHightBpRecord.Enabled = false;
                this.txtLowBpRecord.Enabled = false;
            }

            SaveAdvice(null);
        }

        private void cbxMedicine_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;

            if (this.cbxMedicine.Checked)
            {
                this.cbxDiscomfort.Checked = false;
                this.cbxMedicineDiscomfort.Checked = false;
            }
            SaveAdvice(null);
        }

        private void cbxDiscomfort_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;

            if (this.cbxDiscomfort.Checked)
            {
                this.cbxMedicine.Checked = false;
                this.cbxMedicineDiscomfort.Checked = false;
            }
            SaveAdvice(null);
        }

        private void cbxMedicineDiscomfort_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;

            if (this.cbxMedicineDiscomfort.Checked)
            {
                this.cbxMedicine.Checked = false;
                this.cbxDiscomfort.Checked = false;
            }
            SaveAdvice(null);
        }

        private void cbxDownPercent_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;

            SaveAdvice(null);
        }

        private void cbxFluctuationType_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;

            SaveAdvice(null);
        }

        private void txtHightBpRecord_KeyUp(object sender, KeyEventArgs e)
        {
            SaveAdvice(null);
        }

        private void txtLowBpRecord_KeyUp(object sender, KeyEventArgs e)
        {
            SaveAdvice(null);
        }

        private void cbxStandard_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;

            SaveAdvice(null);
        }

        private void chxWch_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;

            SaveAdvice(null);
        }

        private void chxMaskedH_CheckedChanged(object sender, EventArgs e)
        {
            if (_initializing) return;

            SaveAdvice(null);
        }

        private void advice_KeyUp(object sender, KeyEventArgs e)
        {
            SaveAdvice(null);
        }

        private bool SaveAdvice(int? status)
        {
            examCase caseInfo = ZoncareEntities.Instance.ExamCases.Where(o => o.id == AnaValues.CurrentCase.id).FirstOrDefault();
            if (caseInfo != null)
            {
                string wholeResult = "";
                JObject adviceJson = new JObject();
                if (this.cbxDownPercent.Checked)
                {
                    adviceJson["percent"] = 1;
                    adviceJson["percentText"] = this.cbxDownPercent.Text;
                    wholeResult += this.cbxDownPercent.Text;
                    wholeResult += "\n";
                }
                else
                {
                    adviceJson["percent"] = 0;
                }

                if (this.cbxFluctuationType.Checked)
                {
                    adviceJson["type"] = 1;
                    adviceJson["typeText"] = this.cbxFluctuationType.Text;
                    wholeResult += this.cbxFluctuationType.Text;
                    wholeResult += "\n";
                }
                else
                {
                    adviceJson["type"] = 0;
                }

                if (this.cbxHightBp.Checked)
                {
                    adviceJson["wave"] = 1;
                    adviceJson["waveText"] = this.cbxHightBp.Text;
                    adviceJson["maxSbpText"] = this.cbxHightBp.Text;
                    wholeResult += this.cbxHightBp.Text;
                    if (this.txtHightBpRecord.Text != "")
                    {
                        adviceJson["maxSbpText"] += this.lblRecord1Title.Text + this.txtHightBpRecord.Text;
                        adviceJson["record1"] = this.txtHightBpRecord.Text;

                        wholeResult += this.lblRecord1Title.Text + this.txtHightBpRecord.Text;
                    }
                    wholeResult += "\n";

                    adviceJson["minSbpText"] = this.lblLowBp.Text;
                    wholeResult += this.lblLowBp.Text;
                    if (this.txtLowBpRecord.Text != "")
                    {
                        adviceJson["minSbpText"] += this.lblRecord2Title.Text + this.txtLowBpRecord.Text;
                        adviceJson["record2"] = this.txtLowBpRecord.Text;

                        wholeResult += this.lblRecord2Title.Text + this.txtLowBpRecord.Text;
                    }
                    wholeResult += "\n";

                }
                else
                {
                    adviceJson["wave"] = 0;
                }

                if (this.cbxMedicine.Checked)
                {
                    adviceJson["option1"] = 1;
                    adviceJson["option1Text"] = this.cbxMedicine.Text;

                    wholeResult += this.cbxMedicine.Text;
                    wholeResult += "\n";

                }
                if (this.cbxDiscomfort.Checked)
                {
                    adviceJson["option1"] = 2;
                    adviceJson["option1Text"] = this.cbxDiscomfort.Text;

                    wholeResult += this.cbxDiscomfort.Text;
                    wholeResult += "\n";
                }
                if (this.cbxMedicineDiscomfort.Checked)
                {
                    adviceJson["option1"] = 3;
                    adviceJson["option1Text"] = this.cbxMedicineDiscomfort.Text;

                    wholeResult += this.cbxMedicineDiscomfort.Text;
                    wholeResult += "\n";
                }

                if (this.cbxStandard.Checked)
                {
                    adviceJson["abmp"] = 1;
                    adviceJson["abmpText"] = this.cbxStandard.Text;

                    wholeResult += this.cbxStandard.Text;
                    wholeResult += "\n";
                }
                else
                {
                    adviceJson["abmp"] = 0;
                }

                if (this.chxWch.Checked)
                {
                    adviceJson["wch"] = 1;
                    adviceJson["wchText"] = this.chxWch.Text;

                    wholeResult += this.chxWch.Text;
                    wholeResult += "\n";
                }
                else
                {
                    adviceJson["wch"] = 0;
                }

                if (this.chxMaskedH.Checked)
                {
                    adviceJson["maskedH"] = 1;
                    adviceJson["maskedHText"] = this.chxMaskedH.Text;

                    wholeResult += this.chxMaskedH.Text;
                    wholeResult += "\n";
                }
                else
                {
                    adviceJson["maskedH"] = 0;
                }

                adviceJson["result"] = this.advice.Text;

                if (this.advice.Text != "")
                {
                    wholeResult += this.advice.Text;
                }

                adviceJson["wholeResult"] = wholeResult;
                adviceJson["id"] = caseInfo.id;

                caseInfo.bpReportConclusion = adviceJson.ToString();
                caseInfo.updateTime = DateTime.Now;
                caseInfo.updateBy = AnaValues.CurrentUser.userName;
                caseInfo.diagnoseTime = DateTime.Now;
                if (status != null)
                {
                    caseInfo.status = status.Value;
                }
            }
            bool result = AppCode.SaveCaseInfo(null, caseInfo, false, DateTime.Now, null, false);
            if (result)
            {
                AnaValues.CurrentCase = PubFuns.CloneObject(caseInfo);
                AnaValues.DataSourceState = 1;
            }
            return result;
        }

        private void btnDia_Click(object sender, EventArgs e)
        {
            bool result = SaveAdvice(3);
            if (result)
            {
                ScreenCenterTip.Show("诊断成功");
            }
            else
            {
                CommonFuns.PromptError("操作失败");
            }
        }

        private void picSummarize_Click(object sender, EventArgs e)
        {
            TotalPages = 0;
            PageIdxs.Clear();
            initSummaryPrintData();

            PrintPreviewType = "Summarize";
            PrintPreview();
        }

        private void picDataList_Click(object sender, EventArgs e)
        {
            TotalPages = 0;
            PageIdxs.Clear();
            initDataListPrintData();
            if (DataListInfo.Count == 0)
            {
                ScreenCenterTip.Show(CommonValues.Langs["数据列表为空"]);
                return;
            }
            PrintPreviewType = "DataList";
            PrintPreview();
        }

        private void picHourDataList_Click(object sender, EventArgs e)
        {
            TotalPages = 0;
            PageIdxs.Clear();
            initHourListPrintData();
            if (HourInfo.Count == 0)
            {
                ScreenCenterTip.Show(CommonValues.Langs["小时数据表为空"]);
                return;
            }
            PrintPreviewType = "HourList";
            PrintPreview();
        }
        private void picTrendWave_Click(object sender, EventArgs e)
        {
            TotalPages = 0;
            PageIdxs.Clear();
            initTrendWavePrintData();

            PrintPreviewType = "TrendWave";
            PrintPreview();
        }
        private void picHistogram_Click(object sender, EventArgs e)
        {
            TotalPages = 0;
            PageIdxs.Clear();
            initHistogramPrintData();

            PrintPreviewType = "Histogram";
            PrintPreview();
        }

        private void picPie_Click(object sender, EventArgs e)
        {
            TotalPages = 0;
            PageIdxs.Clear();
            initPiePrintData();

            PrintPreviewType = "Pie";
            PrintPreview();
        }

        private void picFitPlot_Click(object sender, EventArgs e)
        {
            TotalPages = 0;
            PageIdxs.Clear();
            initFitPrintData();

            PrintPreviewType = "Fit";
            PrintPreview();
        }

        private void picAtherosclerosis_Click(object sender, EventArgs e)
        {
            TotalPages = 0;
            PageIdxs.Clear();
            initArteriosclerosisPrintData();

            PrintPreviewType = "Arteriosclerosis";
            PrintPreview();
        }
        private void initSummaryPrintData()
        {
            SummaryInfo.Clear();
            PageIdxs["summary"] = TotalPages;
            ++TotalPages;
            List<JObject> infoList = new List<JObject>();
            JObject allInfo = new JObject();
            initBpSummaryData(0, BpSummaryData.bpAllData, allInfo);
            infoList.Add(allInfo);
            JObject dayInfo = new JObject();
            initBpSummaryData(1, BpSummaryData.bpDayData, dayInfo);
            infoList.Add(dayInfo);
            JObject nightInfo = new JObject();
            initBpSummaryData(2, BpSummaryData.bpNightData, nightInfo);
            infoList.Add(nightInfo);
            SummaryInfo.Add(infoList);

            examCase caseInfo = ZoncareEntities.Instance.ExamCases.Where(o => o.id == AnaValues.CurrentCase.id).FirstOrDefault();
            string conclusionStr = caseInfo.bpReportConclusion;
            if (conclusionStr != null)
            {
                JObject conclusionObj = JObject.Parse(conclusionStr);
                // wholeResult 由 advice.Text 等拼接入库，旧数据里可能带未渲染的模板函数占位符
                ReportAdvice = ApplyTemplateFunctions(conclusionObj["wholeResult"]?.Value<string>() ?? "");
            }

        }

        private void initBpSummaryData(int type, BPDetailData bpDetailData, JObject info)
        {
            if (type == 0)
            {
                info["count"] = BpSummaryData.bpAllData.Count;
                //info["effectiveCount"] = BpSummaryData.bpAllData.EffectiveCount;
                //info["effectiveCountRate"] = BpSummaryData.bpAllData.EffectiveCountRate;
                info["errorNum"] = BpSummaryData.bpAllData.ErrorNum;
                info["errorNumPercent"] = BpSummaryData.bpAllData.ErrorNumPercent;
                info["dayCount"] = BpSummaryData.bpDayData.Count;
                info["nightCount"] = BpSummaryData.bpNightData.Count;
                info["AASI"] = BpSummaryData.bpAllData.AASI;
                info["morningSbpAvg"] = BpSummaryData.bpDayData.MorningSbpAvg;
                info["morningDbpAvg"] = BpSummaryData.bpDayData.MorningDbpAvg;
                info["MorningPre"] = BpSummaryData.bpAllData.MorningPre;
                info["TotalTime"] = BpSummaryData.bpAllData.TotalTime;
            }

            if (type == 0)
            {
                info["duration"] = bpDetailData.StartTime + "--" + bpDetailData.EndTime;
            }
            else if (type == 1)
            {
                info["duration"] = SettingValues.daytimeStart + "--" + SettingValues.daytimeEnd;

            }
            else
            {
                info["duration"] = SettingValues.daytimeEnd + "--" + SettingValues.daytimeStart;
            }


            info["SbpMin"] = bpDetailData.SbpMin;
            info["SbpMinTime"] = bpDetailData.SbpMinTime;
            info["SbpAvg"] = bpDetailData.SbpAvg;
            info["SbpMax"] = bpDetailData.SbpMax;
            info["SbpMaxTime"] = bpDetailData.SbpMaxTime;
            info["SbpBzc"] = bpDetailData.SbpBzc;
            info["SbpByxs"] = bpDetailData.SbpByxs;

            info["DbpMin"] = bpDetailData.DbpMin;
            info["DbpMinTime"] = bpDetailData.DbpMinTime;
            info["DbpAvg"] = bpDetailData.DbpAvg;
            info["DbpMax"] = bpDetailData.DbpMax;
            info["DbpMaxTime"] = bpDetailData.DbpMaxTime;
            info["DbpBzc"] = bpDetailData.DbpBzc;
            info["DbpByxs"] = bpDetailData.DbpByxs;


            info["MapMin"] = bpDetailData.MapMin;
            info["MapAvg"] = bpDetailData.MapAvg;
            info["MapMax"] = bpDetailData.MapMax;
            info["MapBzc"] = bpDetailData.MapBzc;
            info["MapByxs"] = bpDetailData.MapByxs;

            info["PulseMin"] = bpDetailData.PulseMin;
            info["PulseMinTime"] = bpDetailData.PulseMinTime;
            info["PulseAvg"] = bpDetailData.PulseAvg;
            info["PulseMax"] = bpDetailData.PulseMax;
            info["PulseMaxTime"] = bpDetailData.PulseMaxTime;
            info["PulseBzc"] = bpDetailData.PulseBzc;
            info["PulseByxs"] = bpDetailData.PulseByxs;

            info["SbpGtLimit"] = bpDetailData.SbpGtLimit;
            info["SbpLoadNum"] = bpDetailData.SbpLoadNum;
            info["DbpGtLimit"] = bpDetailData.DbpGtLimit;
            info["DbpLoadNum"] = bpDetailData.DbpLoadNum;

            info["effectiveCount"] = bpDetailData.EffectiveCount;
            info["effectiveCountRate"] = bpDetailData.EffectiveCountRate;
            info["RelationShip"] = bpDetailData.RelationShip;
            info["Correl"] = bpDetailData.Correl;
        }

        private void initDataListPrintData()
        {
            PageIdxs["DataList"] = TotalPages;

            GetDataListInfo();

            TotalPages += DataListInfo.Count;
        }

        private void GetDataListInfo()
        {
            DataListInfo.Clear();
            List<BPDataInfo> dataInfoList = CalcBPInfo.GetBPDataList(3);
            if (dataInfoList != null)
            {
                int prePageRow = 40;
                int pageCount = (dataInfoList.Count + prePageRow - 1) / prePageRow;
                for (int i = 0; i < pageCount; i++)
                {
                    List<JObject> infoList = new List<JObject>();
                    for (int j = i * prePageRow; j < (i + 1) * prePageRow; j++)
                    {
                        if (j == dataInfoList.Count) break;
                        JObject dataInfo = new JObject();
                        dataInfo["id"] = j + 1;
                        dataInfo["bDay"] = dataInfoList[j].bDay;
                        dataInfo["date"] = dataInfoList[j].Date;
                        dataInfo["time"] = dataInfoList[j].Time;
                        if (SettingValues.unit == "mmHg")
                        {
                            dataInfo["sbp"] = dataInfoList[j].SBP;
                            dataInfo["dbp"] = dataInfoList[j].DBP;
                            dataInfo["map"] = dataInfoList[j].MAP;
                        }
                        else
                        {
                            dataInfo["sbp"] = CommonFuns.mmHg2KPa(Convert.ToInt32(dataInfoList[j].SBP), 2).ToString("F2");
                            dataInfo["dbp"] = CommonFuns.mmHg2KPa(Convert.ToInt32(dataInfoList[j].DBP), 2).ToString("F2");
                            dataInfo["map"] = CommonFuns.mmHg2KPa(Convert.ToInt32(dataInfoList[j].MAP), 2).ToString("F2");
                        }
                        dataInfo["pulse"] = dataInfoList[j].Pulse;
                        infoList.Add(dataInfo);
                    }
                    DataListInfo.Add(infoList);
                }
            }
        }
        private void initHourListPrintData()
        {
            PageIdxs["HourList"] = TotalPages;

            GetHourInfo();

            TotalPages += HourInfo.Count;
        }
        private void initTrendWavePrintData()
        {
            PageIdxs["TrendWave"] = TotalPages;
            ++TotalPages;
            GetTrendWaveInfo();
        }
        private void initHistogramPrintData()
        {
            PageIdxs["Histogram"] = TotalPages;
            ++TotalPages;
            GetHistogramInfo();
        }
        private void initPiePrintData()
        {
            PageIdxs["Pie"] = TotalPages;
            ++TotalPages;
            GetPieInfo();
        }
        private void initFitPrintData()
        {
            PageIdxs["Fit"] = TotalPages;
            ++TotalPages;
            GetFitInfo();
        }
        private void initArteriosclerosisPrintData()
        {
            PageIdxs["Arteriosclerosis"] = TotalPages;
            ++TotalPages;
            GetArteriosclerosisInfo();
        }
        private void GetHourInfo()
        {
            HourInfo.Clear();
            List<BPHourListInfo> hourList = CalcBPInfo.GetHourList();
            if (hourList != null)
            {
                int prePageRow = 40;
                int pageCount = (hourList.Count + prePageRow - 1) / prePageRow;
                for (int i = 0; i < pageCount; i++)
                {
                    List<JObject> infoList = new List<JObject>();
                    for (int j = i * prePageRow; j < (i + 1) * prePageRow; j++)
                    {
                        if (j == hourList.Count) break;
                        JObject dataInfo = new JObject();
                        dataInfo["id"] = j + 1;
                        dataInfo["date"] = hourList[j].Date;
                        dataInfo["time"] = hourList[j].Time;
                        if (SettingValues.unit == "mmHg")
                        {
                            dataInfo["sbp"] = hourList[j].SBP;
                            dataInfo["dbp"] = hourList[j].DBP;
                            dataInfo["map"] = hourList[j].MAP;
                        }
                        else
                        {
                            dataInfo["sbp"] = CommonFuns.mmHg2KPa(Convert.ToInt32(hourList[j].SBP), 2).ToString("F2");
                            dataInfo["dbp"] = CommonFuns.mmHg2KPa(Convert.ToInt32(hourList[j].DBP), 2).ToString("F2");
                            dataInfo["map"] = CommonFuns.mmHg2KPa(Convert.ToInt32(hourList[j].MAP), 2).ToString("F2");
                        }
                        dataInfo["pulse"] = hourList[j].Pulse;
                        dataInfo["count"] = hourList[j].Count;
                        infoList.Add(dataInfo);
                    }
                    HourInfo.Add(infoList);
                }
            }
        }
        private void GetTrendWaveInfo()
        {
            TrendWaveInfo.Clear();
            List<JObject> infoList = new List<JObject>();
            List<BPDataInfo> bpDataInfoList = CalcBPInfo.GetBPDataList(2);
            JObject dataInfo = new JObject();
            dataInfo["bpDataInfoList"] = JArray.FromObject(bpDataInfoList);
            infoList.Add(dataInfo);
            TrendWaveInfo.Add(infoList);
        }
        private void GetHistogramInfo()
        {
            HistogramInfo.Clear();
            List<JObject> infoList = new List<JObject>();
            List<BPDataInfo> bpDataInfoList = CalcBPInfo.GetBPDataList(2);
            JObject dataInfo = new JObject();
            dataInfo["HistogramInfo"] = JArray.FromObject(bpDataInfoList);
            infoList.Add(dataInfo);
            HistogramInfo.Add(infoList);
        }
        private void GetPieInfo()
        {
            PieInfo.Clear();
            List<JObject> infoList = new List<JObject>();
            List<BPDataInfo> bpDataInfoList = CalcBPInfo.GetBPDataList(2);

            DateTime DayTimeStart = DateTime.Parse(SettingValues.daytimeStart);
            DateTime DayTimeEnd = DateTime.Parse(SettingValues.daytimeEnd);

            List<BPDataInfo> dayDataInfoList = bpDataInfoList.FindAll(b => (CalcBPInfo.IsInTimeRange(b.bpDateTime, TimeSpan.FromHours(DayTimeStart.Hour), TimeSpan.FromHours(DayTimeEnd.Hour))));
            List<BPDataInfo> nightDataInfoList = bpDataInfoList.FindAll(b => (!CalcBPInfo.IsInTimeRange(b.bpDateTime, TimeSpan.FromHours(DayTimeStart.Hour), TimeSpan.FromHours(DayTimeEnd.Hour))));

            List<int> sbpMaxMin = new List<int>(3);
            List<Double> sbpMaxMinPercent = new List<Double>(3);

            List<int> dbpMaxMin = new List<int>(3);
            List<Double> dbpMaxMinPercent = new List<Double>(3);

            List<int> pulseMaxMin = new List<int>(3);
            List<Double> pulseMaxMinPercent = new List<Double>(3);

            double minValue = 0;
            double maxValue = 0;
            if (SettingValues.unit == "mmHg")
            {
                minValue = SettingValues.minWakeSys;
                maxValue = SettingValues.maxWakeSys;
            }
            else
            {
                minValue = CommonFuns.mmHg2KPa(SettingValues.minWakeSys, 2);
                maxValue = CommonFuns.mmHg2KPa(SettingValues.maxWakeSys, 2);
            }
            GetMinMaxCount(dayDataInfoList, minValue, maxValue, 0);
            if (SettingValues.unit == "mmHg")
            {
                minValue = SettingValues.minWakeDia;
                maxValue = SettingValues.maxWakeDia;
            }
            else
            {
                minValue = CommonFuns.mmHg2KPa(SettingValues.minWakeDia, 2);
                maxValue = CommonFuns.mmHg2KPa(SettingValues.maxWakeDia, 2);
            }
            GetMinMaxCount(dayDataInfoList, minValue, maxValue, 1);

            GetMinMaxCount(bpDataInfoList, 60, 100, 2);

            sbpMaxMin.AddRange(SbpMaxMin);
            dbpMaxMin.AddRange(DbpMaxMin);
            pulseMaxMin.AddRange(PulseMaxMin);
            pulseMaxMinPercent.AddRange(PulseMaxMinPercent);

            if (SettingValues.unit == "mmHg")
            {
                minValue = SettingValues.minSleepSys;
                maxValue = SettingValues.maxSleepSys;
            }
            else
            {
                minValue = CommonFuns.mmHg2KPa(SettingValues.minSleepSys, 2);
                maxValue = CommonFuns.mmHg2KPa(SettingValues.maxSleepSys, 2);
            }
            GetMinMaxCount(nightDataInfoList, minValue, maxValue, 0);
            if (SettingValues.unit == "mmHg")
            {
                minValue = SettingValues.minSleepDia;
                maxValue = SettingValues.maxSleepDia;
            }
            else
            {
                minValue = CommonFuns.mmHg2KPa(SettingValues.minSleepDia, 2);
                maxValue = CommonFuns.mmHg2KPa(SettingValues.maxSleepDia, 2);
            }
            GetMinMaxCount(nightDataInfoList, minValue, maxValue, 1);

            for (int i = 0; i < 3; i++)
            {
                sbpMaxMin[i] += SbpMaxMin[i];
                dbpMaxMin[i] += DbpMaxMin[i];

                sbpMaxMinPercent.Add((double)sbpMaxMin[i] / bpDataInfoList.Count);
                dbpMaxMinPercent.Add((double)dbpMaxMin[i] / bpDataInfoList.Count);
            }
            JObject dataInfo = new JObject();
            dataInfo["bpDataInfoList"] = JArray.FromObject(bpDataInfoList);
            dataInfo["sbpMaxMin"] = JArray.FromObject(sbpMaxMin);
            dataInfo["sbpMaxMinPercent"] = JArray.FromObject(sbpMaxMinPercent);
            dataInfo["dbpMaxMin"] = JArray.FromObject(dbpMaxMin);
            dataInfo["dbpMaxMinPercent"] = JArray.FromObject(dbpMaxMinPercent);
            dataInfo["pulseMaxMin"] = JArray.FromObject(pulseMaxMin);
            dataInfo["pulseMaxMinPercent"] = JArray.FromObject(pulseMaxMinPercent);

            infoList.Add(dataInfo);
            PieInfo.Add(infoList);
        }

        private void GetMinMaxCount(List<BPDataInfo> dataInfoList, double minValue, double maxValue, int flag)
        {
            if (flag == 0)
            {
                SbpMaxMin.Clear();
                SbpMaxMinPercent.Clear();

                if (SettingValues.unit == "kPa")
                {
                    minValue = CommonFuns.kPa2mmHg(minValue);
                    maxValue = CommonFuns.kPa2mmHg(maxValue);
                }
                int minCount = dataInfoList.Count(x => x.SBP < minValue);
                int middleCount = dataInfoList.Count(x => x.SBP >= minValue && x.SBP < maxValue);
                int maxCount = dataInfoList.Count(x => x.SBP >= maxValue);
                SbpMaxMin.Add(minCount);
                SbpMaxMin.Add(middleCount);
                SbpMaxMin.Add(maxCount);
                SbpMaxMinPercent.Add((double)minCount / dataInfoList.Count);
                SbpMaxMinPercent.Add((double)middleCount / dataInfoList.Count);
                SbpMaxMinPercent.Add((double)maxCount / dataInfoList.Count);
            }
            else if (flag == 1)
            {
                DbpMaxMin.Clear();
                DbpMaxMinPercent.Clear();

                if (SettingValues.unit == "kPa")
                {
                    minValue = CommonFuns.kPa2mmHg(minValue);
                    maxValue = CommonFuns.kPa2mmHg(maxValue);
                }

                int minCount = dataInfoList.Count(x => x.DBP < minValue);
                int middleCount = dataInfoList.Count(x => x.DBP >= minValue && x.DBP < maxValue);
                int maxCount = dataInfoList.Count(x => x.DBP >= maxValue);

                DbpMaxMin.Add(minCount);
                DbpMaxMin.Add(middleCount);
                DbpMaxMin.Add(maxCount);
                DbpMaxMinPercent.Add((double)minCount / dataInfoList.Count);
                DbpMaxMinPercent.Add((double)middleCount / dataInfoList.Count);
                DbpMaxMinPercent.Add((double)maxCount / dataInfoList.Count);
            }
            else
            {
                PulseMaxMin.Clear();
                PulseMaxMinPercent.Clear();

                int minCount = dataInfoList.Count(x => x.Pulse < minValue);
                int middleCount = dataInfoList.Count(x => x.Pulse >= minValue && x.Pulse < maxValue);
                int maxCount = dataInfoList.Count(x => x.Pulse >= maxValue);

                PulseMaxMin.Add(minCount);
                PulseMaxMin.Add(middleCount);
                PulseMaxMin.Add(maxCount);
                PulseMaxMinPercent.Add((double)minCount / dataInfoList.Count);
                PulseMaxMinPercent.Add((double)middleCount / dataInfoList.Count);
                PulseMaxMinPercent.Add((double)maxCount / dataInfoList.Count);
            }
        }
        private void GetFitInfo()
        {
            FitInfo.Clear();
            List<JObject> infoList = new List<JObject>();
            infoList.Add(CalcFitInfo());
            FitInfo.Add(infoList);
        }
        private JObject CalcFitInfo()
        {
            List<BPDataInfo> bpDataInfoList = CalcBPInfo.GetBPDataList(2);
            DateTime DayTimeStart = DateTime.Parse(SettingValues.daytimeStart);
            DateTime DayTimeEnd = DateTime.Parse(SettingValues.daytimeEnd);
            List<BPDataInfo> dayBPDataInfoList = bpDataInfoList.FindAll(b => (CalcBPInfo.IsInTimeRange(b.bpDateTime, TimeSpan.FromHours(DayTimeStart.Hour), TimeSpan.FromHours(DayTimeEnd.Hour))));
            List<BPDataInfo> nightBPDataInfoList = bpDataInfoList.FindAll(b => (!CalcBPInfo.IsInTimeRange(b.bpDateTime, TimeSpan.FromHours(DayTimeStart.Hour), TimeSpan.FromHours(DayTimeEnd.Hour))));

            int dayDataCount = dayBPDataInfoList.Count;
            List<double> daySbpList = new List<double>(dayDataCount);
            List<double> dayDbpList = new List<double>(dayDataCount);
            List<double> dayPulseList = new List<double>(dayDataCount);
            for (int i = 0; i < dayDataCount; i++)
            {
                daySbpList.Add(dayBPDataInfoList[i].SBP);
                dayDbpList.Add(dayBPDataInfoList[i].DBP);
                dayPulseList.Add(dayBPDataInfoList[i].Pulse);
            }

            int nightDataCount = nightBPDataInfoList.Count;
            List<double> nightSbpList = new List<double>(nightDataCount);
            List<double> nightDbpList = new List<double>(nightDataCount);
            List<double> nightPulseList = new List<double>(nightDataCount);
            for (int i = 0; i < nightDataCount; i++)
            {
                nightSbpList.Add(nightBPDataInfoList[i].SBP);
                nightDbpList.Add(nightBPDataInfoList[i].DBP);
                nightPulseList.Add(nightBPDataInfoList[i].Pulse);
            }

            int dataCount = bpDataInfoList.Count;
            List<double> sbpList = new List<double>(dataCount);
            List<double> dbpList = new List<double>(dataCount);
            List<double> pulseList = new List<double>(dataCount);
            for (int i = 0; i < dataCount; i++)
            {
                sbpList.Add(bpDataInfoList[i].SBP);
                dbpList.Add(bpDataInfoList[i].DBP);
                pulseList.Add(bpDataInfoList[i].Pulse);
            }
            List<SKPoint> sbpSlopePoints = new List<SKPoint>(2);
            double y0 = CalcBPInfo.GetY0(sbpList, dbpList, 0);
            double y1 = CalcBPInfo.GetY1(sbpList, dbpList, 0, y0);
            sbpSlopePoints.Add(CalcBPInfo.GetSlopePoint1(y0, y1));
            sbpSlopePoints.Add(CalcBPInfo.GetSlopePoint2(y0, y1));

            List<SKPoint> pulseSlopePoints = new List<SKPoint>(2);
            double pulseY0 = CalcBPInfo.GetY0(sbpList, pulseList, 0);
            double pulseY1 = CalcBPInfo.GetY1(sbpList, pulseList, 0, pulseY0);
            pulseSlopePoints.Add(CalcBPInfo.GetSlopePoint1(pulseY0, pulseY1));
            pulseSlopePoints.Add(CalcBPInfo.GetSlopePoint2(pulseY0, pulseY1));

            JObject dataInfo = new JObject();
            dataInfo["bpDataInfoList"] = JArray.FromObject(bpDataInfoList);
            dataInfo["DaySbpList"] = JArray.FromObject(daySbpList);
            dataInfo["DayDbpList"] = JArray.FromObject(dayDbpList);
            dataInfo["DayPulseList"] = JArray.FromObject(dayPulseList);
            dataInfo["NightSbpList"] = JArray.FromObject(nightSbpList);
            dataInfo["NightDbpList"] = JArray.FromObject(nightDbpList);
            dataInfo["NightPulseList"] = JArray.FromObject(nightPulseList);
            dataInfo["SbpSlopePoints"] = JArray.FromObject(sbpSlopePoints);
            dataInfo["PulseSlopePoints"] = JArray.FromObject(pulseSlopePoints);
            return dataInfo;
        }
        private void GetArteriosclerosisInfo()
        {
            ArteriosclerosisInfo.Clear();
            List<JObject> infoList = new List<JObject>();
            JObject allInfo = new JObject();
            initBpSummaryData(0, BpSummaryData.bpAllData, allInfo);
            infoList.Add(allInfo);
            JObject dayInfo = new JObject();
            initBpSummaryData(1, BpSummaryData.bpDayData, dayInfo);
            infoList.Add(dayInfo);
            JObject nightInfo = new JObject();
            initBpSummaryData(2, BpSummaryData.bpNightData, nightInfo);
            infoList.Add(nightInfo);
            infoList.Add(CalcFitInfo());
            ArteriosclerosisInfo.Add(infoList);
        }
        public void initPrintData()
        {
            TotalPages = 0;
            PageIdxs.Clear();
            if (cbxSummarize.Checked)
            {
                initSummaryPrintData();
            }
            if (cbxDataList.Checked)
            {
                initDataListPrintData();
            }
            if (cbxHourDataList.Checked)
            {
                initHourListPrintData();
            }
            if (cbxTrendWave.Checked)
            {
                initTrendWavePrintData();
            }
            if (cbxHistogram.Checked)
            {
                initHistogramPrintData();
            }
            if (cbxPie.Checked)
            {
                initPiePrintData();
            }
            if (cbxFitPlot.Checked)
            {
                initFitPrintData();
            }
            if (cbxAtherosclerosis.Checked)
            {
                initArteriosclerosisPrintData();
            }
        }

        public void printReport()
        {

            bool bHorizontalPrint = false;

            ReportPrintManager = new BpReportPrintManager(bHorizontalPrint, TotalPages);
            ReportPrintManager.PageIdxs = PageIdxs;
            ReportPrintManager.AllPageInfo["summary"] = SummaryInfo;
            ReportPrintManager.AllPageInfo["DataList"] = DataListInfo;
            ReportPrintManager.AllPageInfo["HourList"] = HourInfo;
            ReportPrintManager.AllPageInfo["TrendWave"] = TrendWaveInfo;
            ReportPrintManager.AllPageInfo["Histogram"] = HistogramInfo;
            ReportPrintManager.AllPageInfo["Pie"] = PieInfo;
            ReportPrintManager.AllPageInfo["Fit"] = FitInfo;
            ReportPrintManager.AllPageInfo["Arteriosclerosis"] = ArteriosclerosisInfo;

            ReportPrintManager.advice = ReportAdvice;
            ReportPrintManager.SetOnDemandRendering();
            ReportPrintManager.ShowPrintDialog();
        }

        private void PrintPreview()
        {
            try
            {
                // 创建自定义打印预览窗体
                using (var previewForm = new BaseForm())
                {
                    previewForm.Text = CommonValues.Langs["打印预览"];
                    previewForm.Size = new Size(1000, 700);
                    previewForm.ShowInTaskbar = false;
                    previewForm.StartPosition = FormStartPosition.CenterScreen;
                    previewForm.WindowState = FormWindowState.Maximized;

                    // 创建PrintDocument
                    var printDocument = new PrintDocument();
                    printDocument.PrintPage += (s, pe) =>
                    {
                        PrintDocument_PrintPageForPreview(s, pe);
                    };

                    // 创建PrintPreviewControl
                    var previewControl = new PrintPreviewControl()
                    {
                        Dock = DockStyle.Fill,
                        Document = printDocument,
                        Zoom = 0.85,
                        Rows = 1,
                        Columns = 1,
                        StartPage = 0
                    };

                    // 创建工具栏
                    var toolPanel = new Panel()
                    {
                        Dock = DockStyle.Top,
                        Height = 50,
                        BackColor = SystemColors.Control
                    };

                    // 创建导航控件
                    var lblPageInfo = new Label()
                    {
                        Text = CommonValues.Langs["第"] + "1" + CommonValues.Langs["页"] + " / " + CommonValues.Langs["共"] + TotalPages + CommonValues.Langs["页"],
                        Location = new Point(10, 15),
                        Size = new Size(150, 20),
                        Font = new Font("Microsoft YaHei", 9)
                    };

                    var btnPrevPage = new Button()
                    {
                        Text = CommonValues.Langs["上一页"],
                        Location = new Point(170, 10),
                        Size = new Size(60, 30),
                        Font = new Font("Microsoft YaHei", 9)
                    };

                    var btnNextPage = new Button()
                    {
                        Text = CommonValues.Langs["下一页"],
                        Location = new Point(240, 10),
                        Size = new Size(60, 30),
                        Font = new Font("Microsoft YaHei", 9)
                    };

                    var btnZoomIn = new Button()
                    {
                        Text = CommonValues.Langs["放大"],
                        Location = new Point(320, 10),
                        Size = new Size(60, 30),
                        Font = new Font("Microsoft YaHei", 9)
                    };

                    var btnZoomOut = new Button()
                    {
                        Text = CommonValues.Langs["缩小"],
                        Location = new Point(390, 10),
                        Size = new Size(60, 30),
                        Font = new Font("Microsoft YaHei", 9)
                    };

                    var btnPrint = new Button()
                    {
                        Text = CommonValues.Langs["打印"],
                        Location = new Point(460, 10),
                        Size = new Size(80, 30),
                        Font = new Font("Microsoft YaHei", 9)
                    };

                    var btnClose = new Button()
                    {
                        Text = CommonValues.Langs["关闭"],
                        Location = new Point(550, 10),
                        Size = new Size(80, 30),
                        Font = new Font("Microsoft YaHei", 9)
                    };

                    // 添加控件到工具栏
                    toolPanel.Controls.AddRange(new Control[] {
                        lblPageInfo, btnPrevPage, btnNextPage,
                        btnZoomIn, btnZoomOut, btnPrint, btnClose
                    });

                    // 添加控件到窗体
                    previewForm.Controls.AddRange(new Control[] {
                        previewControl, toolPanel
                    });

                    // 绑定事件
                    btnPrevPage.Click += (s, ev) =>
                    {
                        if (previewControl.StartPage > 0)
                        {
                            previewControl.StartPage--;
                            int pageIdx = previewControl.StartPage + 1;
                            string text = CommonValues.Langs["第"] + pageIdx + CommonValues.Langs["页"] + " / " + CommonValues.Langs["共"] + TotalPages + CommonValues.Langs["页"];
                            lblPageInfo.Text = text;
                            btnPrevPage.Enabled = previewControl.StartPage > 0;
                            btnNextPage.Enabled = previewControl.StartPage < TotalPages - 1;
                        }
                    };

                    btnNextPage.Click += (s, ev) =>
                    {
                        if (previewControl.StartPage < TotalPages - 1)
                        {
                            previewControl.StartPage++;
                            int pageIdx = previewControl.StartPage + 1;
                            string text = CommonValues.Langs["第"] + pageIdx + CommonValues.Langs["页"] + " / " + CommonValues.Langs["共"] + TotalPages + CommonValues.Langs["页"];
                            lblPageInfo.Text = text;
                            btnPrevPage.Enabled = previewControl.StartPage > 0;
                            btnNextPage.Enabled = previewControl.StartPage < TotalPages - 1;
                        }
                    };

                    btnZoomIn.Click += (s, ev) =>
                    {
                        previewControl.Zoom = Math.Min(previewControl.Zoom + 0.1, 3.0);
                    };

                    btnZoomOut.Click += (s, ev) =>
                    {
                        previewControl.Zoom = Math.Max(previewControl.Zoom - 0.1, 0.1);
                    };

                    btnPrint.Click += (s, ev) =>
                    {
                        try
                        {
                            if (PrintPreviewType == "All")
                            {
                                initPrintData();
                                printReport();
                            }
                            else
                            {
                                printReport();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"打印失败: {ex.Message}", "打印错误",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    };

                    btnClose.Click += (s, ev) =>
                    {
                        CurrentPageIndex = 0;
                        previewForm.Close();
                    };

                    previewForm.FormClosed += (s, ev) =>
                    {
                        CurrentPageIndex = 0;
                        previewForm.Close();
                    };

                    previewForm.MouseWheel += (s, ev) =>
                    {
                        // 根据滚轮方向翻页
                        if (ev.Delta > 0)
                        {
                            if (previewControl.StartPage > 0)
                            {
                                previewControl.StartPage--;
                                int pageIdx = previewControl.StartPage + 1;
                                string text = CommonValues.Langs["第"] + pageIdx + CommonValues.Langs["页"] + " / " + CommonValues.Langs["共"] + TotalPages + CommonValues.Langs["页"];
                                lblPageInfo.Text = text;
                                btnPrevPage.Enabled = previewControl.StartPage > 0;
                                btnNextPage.Enabled = previewControl.StartPage < TotalPages - 1;
                            }
                        }
                        else if (ev.Delta < 0)
                        {
                            if (previewControl.StartPage < TotalPages - 1)
                            {
                                previewControl.StartPage++;
                                int pageIdx = previewControl.StartPage + 1;
                                string text = CommonValues.Langs["第"] + pageIdx + CommonValues.Langs["页"] + " / " + CommonValues.Langs["共"] + TotalPages + CommonValues.Langs["页"];
                                lblPageInfo.Text = text; btnPrevPage.Enabled = previewControl.StartPage > 0;
                                btnNextPage.Enabled = previewControl.StartPage < TotalPages - 1;
                            }
                        }
                    };

                    // 显示预览窗体
                    previewForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"打印预览初始化失败: {ex.Message}", "错误",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintDocument_PrintPageForPreview(object sender, PrintPageEventArgs e)
        {
            try
            {

                // 渲染当前页
                Bitmap page = printReport(CurrentPageIndex);

                if (page != null)
                {
                    // 计算打印区域，确保图像适应页面
                    Rectangle pageBounds = e.PageBounds;
                    Rectangle imageBounds = new Rectangle(0, 0, page.Width, page.Height);

                    // 保持宽高比进行缩放
                    float scale = Math.Min(
                        pageBounds.Width / (float)imageBounds.Width,
                        pageBounds.Height / (float)imageBounds.Height
                    );

                    int scaledWidth = (int)(imageBounds.Width * scale);
                    int scaledHeight = (int)(imageBounds.Height * scale);

                    // 居中显示
                    int x = (pageBounds.Width - scaledWidth) / 2;
                    int y = (pageBounds.Height - scaledHeight) / 2;

                    Rectangle destRect = new Rectangle(x, y, scaledWidth, scaledHeight);

                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    e.Graphics.DrawImage(page, destRect, imageBounds, GraphicsUnit.Pixel);

                    // 释放资源
                    page.Dispose();

                    // 更新页码并判断是否有更多页面
                    CurrentPageIndex++;
                    e.HasMorePages = CurrentPageIndex < TotalPages;
                }
                else
                {
                    e.HasMorePages = false;
                }
            }
            catch (Exception ex)
            {
                e.HasMorePages = false;
                Console.WriteLine($"渲染第{CurrentPageIndex + 1}页时发生错误: {ex.Message}",
                              "渲染错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Bitmap printReport(int pageIndex)
        {
            Bitmap page = null;
            if (pageIndex < TotalPages)
            {
                printReportPage.TotalPages = TotalPages;
                if ((PageIdxs.ContainsKey("summary")) && (pageIndex >= PageIdxs["summary"] && pageIndex < PageIdxs["summary"] + SummaryInfo.Count))
                {
                    printReportPage.SummaryInfo = SummaryInfo;

                    printReportPage.Advice = ReportAdvice;
                    page = printReportPage.RenderReportPage(pageIndex, "summary");
                }
                if ((PageIdxs.ContainsKey("DataList")) && (pageIndex >= PageIdxs["DataList"] && pageIndex <= PageIdxs["DataList"] + DataListInfo.Count))
                {
                    printReportPage.DataListInfo = DataListInfo;
                    printReportPage.DataListIdx = PageIdxs["DataList"];
                    page = printReportPage.RenderReportPage(pageIndex, "DataList");
                }
                if ((PageIdxs.ContainsKey("HourList")) && (pageIndex >= PageIdxs["HourList"] && pageIndex <= PageIdxs["HourList"] + HourInfo.Count))
                {
                    printReportPage.HourInfo = HourInfo;
                    printReportPage.DataListIdx = PageIdxs["HourList"];
                    page = printReportPage.RenderReportPage(pageIndex, "HourList");
                }
                if ((PageIdxs.ContainsKey("TrendWave")) && (pageIndex >= PageIdxs["TrendWave"] && pageIndex <= PageIdxs["TrendWave"] + TrendWaveInfo.Count))
                {
                    printReportPage.TrendWaveInfo = TrendWaveInfo;
                    page = printReportPage.RenderReportPage(pageIndex, "TrendWave");
                }
                if ((PageIdxs.ContainsKey("Histogram")) && (pageIndex >= PageIdxs["Histogram"] && pageIndex <= PageIdxs["Histogram"] + HistogramInfo.Count))
                {
                    printReportPage.HistogramInfo = HistogramInfo;
                    page = printReportPage.RenderReportPage(pageIndex, "Histogram");
                }
                if ((PageIdxs.ContainsKey("Pie")) && (pageIndex >= PageIdxs["Pie"] && pageIndex <= PageIdxs["Pie"] + PieInfo.Count))
                {
                    printReportPage.PieInfo = PieInfo;
                    page = printReportPage.RenderReportPage(pageIndex, "Pie");
                }
                if ((PageIdxs.ContainsKey("Fit")) && (pageIndex >= PageIdxs["Fit"] && pageIndex <= PageIdxs["Fit"] + FitInfo.Count))
                {
                    printReportPage.FitInfo = FitInfo;
                    page = printReportPage.RenderReportPage(pageIndex, "Fit");
                }
                if ((PageIdxs.ContainsKey("Arteriosclerosis")) && (pageIndex >= PageIdxs["Arteriosclerosis"] && pageIndex <= PageIdxs["Arteriosclerosis"] + ArteriosclerosisInfo.Count))
                {
                    printReportPage.ArteriosclerosisInfo = ArteriosclerosisInfo;
                    page = printReportPage.RenderReportPage(pageIndex, "Arteriosclerosis");
                }
            }

            return page;
        }

        private void btnPrintPreview_Click(object sender, EventArgs e)
        {
            TotalPages = 0;
            initPrintData();

            PrintPreviewType = "All";
            PrintPreview();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            Stopwatch sw = Stopwatch.StartNew();
            TotalPages = 0;
            initPrintData();
            printReport();

            object para = new { AnaValues.CurrentCase.caseNo };
            OperateLog.logParam("血压报告打印", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
        }
    }
}
