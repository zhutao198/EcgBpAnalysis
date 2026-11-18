using AnalysisApp.CommonControls;
using AnalysisApp.Commons;
using AnalysisSystem.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.Component.Controls;

namespace AnalysisApp
{
    public partial class PrintSettingForm : BaseDialogForm
    {
        public bool bSelectSingleHeartbeat = true;
        public string printLen = "";
        public int printEndSample = 0;
        //打印起始秒
        public double printStartSeconds = 0;
        //每行绘制的秒数
        public int rowSeconds = 0;
        //每行数据点的个数
        public int rowPoints = 0;
        //每页每导联绘制的行数
        public int currentRows = 1;
        //绘制的导联数
        public int currentLeadCount = 12;
        //打印的页数
        public int totalPages = 1;
        //打印纸速
        private readonly double speed = 25;
        //打印增益
        private double gain = 10;
        //打印总采样点数
        private int totalPrintPoints = 0;
        //导联索引
        private readonly List<int> leadIdxs = new List<int>();
        public string segmentFilePath = "";                 // 片段图文件路径
        public List<WaveInfo> ecgData = new List<WaveInfo>();
        // 存储所有心搏信息的列表
        private List<BeatInfo> heartbeatInfos = new List<BeatInfo>();
        private EcgPrintManager printManager = null;
        private readonly List<int> StartSamples = new List<int>();
        private int endSample = 0;
        private int isPortrait = 1;
        private int type = 0;   //0-打印属性设置  1-片段图保存 2-片段图属性设置
        private List<JObject> SegmentInfoJsons = new List<JObject>();
        private List<JObject> SegmentPageInfos = new List<JObject>();
        private List<List<JObject>> SegmentPageDatas = new List<List<JObject>>();
        private List<List<JObject>> SegmentAllPageInfo = new List<List<JObject>>();

        public SegmentWaveViewControl segmentWaveView = null;

        private SegmentPrintManager segmentPrintManager = null;

        public PrintSettingForm(int type, List<int> startSamples, string title) : base("", title)
        {
            InitializeComponent();
            this.Text = title;

            segmentFilePath = Path.Combine(AnaValues.RootPath, AnaValues.SegmentFileName);
            this.type = type;
            StartSamples = startSamples;

            if (type == 0 || type == 3)
            {
                btnPrint.Hide();
            }
            else
            {
                btnPrint.Show();
            }
            getAvgHR(StartSamples[0], 7, 1);
            string dateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat] + " HH:mm:ss";

            DateTime startTime = AnaCode.getDataTimeFromSample(StartSamples[0]);
            string startTimeStr = startTime.ToString(dateFormat);
            tbxStartTime.Text = startTimeStr;
        }

        private void getAvgHR(int startSample, int rowSeconds, int rows)
        {
            int pageMaxSeconds = rowSeconds * rows;

            int endSample = startSample + (pageMaxSeconds * AnaValues.holterConfig.SampleRate);
            int avgHR = AnaCode.CalcAvgHR(startSample, endSample);
            tbxAvgHR.Text = $"{avgHR}";
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (type == 0)
            {
                if(bSelectSingleHeartbeat == true)
                {
                    CalcSegmentPageInfo(Convert.ToInt32(cbxDuration.SelectedValue));
                    printSegmentWaves();
                }
                else
                {
                    rowSeconds = Convert.ToInt32(cbxDirection.SelectedValue) == 1 ? 7 : 10;
                    int maxRowsPerPage = currentLeadCount < 6 ? 10 : currentLeadCount;
                    int maxPartPerPage = maxRowsPerPage / currentLeadCount;

                    int pageMaxSeconds = maxPartPerPage * rowSeconds;

                    int endSample = printEndSample;
                    int totalPoints = printEndSample - StartSamples[0];
                    int pageMaxPoints = pageMaxSeconds * AnaValues.holterConfig.SampleRate;
                    totalPages = (totalPoints + pageMaxPoints - 1) / pageMaxPoints;
                    totalPoints = totalPages * pageMaxSeconds * AnaValues.holterConfig.SampleRate;

                    endSample = StartSamples[0] + totalPoints;

                    CalcSegmentPageInfo(totalPages * pageMaxSeconds);
                    printSegmentWaves();

                }
            }
            else if (type == 1)
            {
                saveSegmentAttr();
            }
            else
            {
                modifySegmentAttr();
            }
            if (segmentWaveView != null)
            {
                segmentWaveView.UpdateDataForceWaveformRedraw();
            }
            else
            {
                AnaValues.DataSourceState = 1;
            }
            Close();
        }

        private JObject getSegmentAttr()
        {
            SegmentInfo segmentInfo = AnaValues.segmentInfo;
            JObject segmentInfoJson = JObject.Parse(JsonConvert.SerializeObject(segmentInfo, SysConstants.JsonSettings));
            List<TextBoxEx> textBoxList = CommonFuns.RetriveSubControls<TextBoxEx>(tableLayoutPanel1);
            foreach (TextBoxEx textBox in textBoxList)
            {
                segmentInfoJson[textBox.Name] = textBox.Text;
            }

            List<ComboBox> comboboxList = CommonFuns.RetriveSubControls<ComboBox>(tableLayoutPanel1);
            foreach (ComboBox comboBox in comboboxList)
            {
                segmentInfoJson[comboBox.Name] = comboBox.Name == "cbxLabel"
                    ? (JToken)Convert.ToString(comboBox.Text)
                    : comboBox.Name == "cbxGain" ? (JToken)Convert.ToDouble(comboBox.SelectedValue) : (JToken)Convert.ToInt32(comboBox.SelectedValue);
            }

            // 将leadIdxs赋值给segmentInfoJson的leadIdxs属性
            segmentInfoJson["leadIdxs"] = JArray.FromObject(leadIdxs);

            segmentInfoJson["startSample"] = StartSamples[0];

            segmentInfoJson["eventName"] = segmentInfoJson["cbxLabel"];

            return segmentInfoJson;
        }

        private void saveSegmentAttr()
        {
            JObject segmentInfoJson = getSegmentAttr();
            // 处理holter.pic文件：判断文件是否存在，更新或新增JSON对象
            OperateSegment.ProcessHolterPicFile(segmentInfoJson);
        }

        private void modifySegmentAttr()
        {
            if (StartSamples.Count > 1)
            {
                foreach (JObject segmentInfoJson in SegmentInfoJsons)
                {
                    List<ComboBox> comboboxList = CommonFuns.RetriveSubControls<ComboBox>(tableLayoutPanel1);
                    foreach (ComboBox comboBox in comboboxList)
                    {
                        if (comboBox.Name != "cbxLabel")
                        {
                            segmentInfoJson[comboBox.Name] = Convert.ToInt32(comboBox.SelectedValue);
                        }
                    }
                    segmentInfoJson["leadIdxs"] = JArray.FromObject(leadIdxs);
                    OperateSegment.ProcessHolterPicFile(segmentInfoJson);
                }
            }
            else
            {
                foreach (JObject segmentInfoJson in SegmentInfoJsons)
                {
                    List<TextBoxEx> textBoxList = CommonFuns.RetriveSubControls<TextBoxEx>(tableLayoutPanel1);
                    foreach (TextBoxEx textBox in textBoxList)
                    {
                        segmentInfoJson[textBox.Name] = textBox.Text;
                    }

                    List<ComboBox> comboboxList = CommonFuns.RetriveSubControls<ComboBox>(tableLayoutPanel1);
                    foreach (ComboBox comboBox in comboboxList)
                    {
                        segmentInfoJson[comboBox.Name] = comboBox.Name == "cbxLabel"
                            ? (JToken)Convert.ToString(comboBox.Text)
                            : comboBox.Name == "cbxGain" ? (JToken)Convert.ToDouble(comboBox.SelectedValue) : (JToken)Convert.ToInt32(comboBox.SelectedValue);
                    }
                    segmentInfoJson["leadIdxs"] = JArray.FromObject(leadIdxs);
                    OperateSegment.ProcessHolterPicFile(segmentInfoJson);
                }
            }
        }

        private void printWave()
        {
            InitData();
            Console.WriteLine($"leadIdxs: {leadIdxs}");
            bool bHorizontalPrint = Convert.ToInt32(cbxDirection.SelectedValue) != 1;

            printManager = new EcgPrintManager(bHorizontalPrint)
            {
                EcgData = ecgData,
                heartbeatInfos = heartbeatInfos,
                totalPages = totalPages,
                RowPoints = rowPoints,
                currentRows = currentRows,
                leadIdxs = leadIdxs,
                speed = speed,
                gain = gain,
                startSample = StartSamples[0],
                totalPrintPoints = totalPrintPoints,
                label = cbxLabel.Text
            };
            printManager.SetOnDemandRendering();
            _ = printManager.ShowPrintDialog();

            //drawTest();
        }

        private void drawTest()
        {
            PrintEcgWaveRenderer waveRenderer = new PrintEcgWaveRenderer();
            waveRenderer.ecgData = ecgData;
            waveRenderer.heartbeatInfos = heartbeatInfos;
            waveRenderer.totalPages = totalPages;
            waveRenderer.RowPoints = rowPoints;
            waveRenderer.leadIdxs = leadIdxs;
            waveRenderer.speed = speed;
            waveRenderer.gain = gain;
            waveRenderer.label = this.cbxLabel.Text;
            waveRenderer.bHorizontalPrint = Convert.ToInt32(this.cbxDirection.SelectedValue) == 1 ? false : true;


            //纵向
            waveRenderer.PaperWidthMm = 210.0;
            waveRenderer.PaperHeightMm = 297.0;
            waveRenderer.MarginLeftMm = 10.0;       // 左边距
            waveRenderer.MarginRightMm = 15.0;      // 右边距
            waveRenderer.MarginTopMm = 20.0;          //上边距
            waveRenderer.MarginBottonMm = 17.0;       //下边距


            waveRenderer.waveTopMargin = (waveRenderer.MarginTopMm + 5) * 3.7795f;
            waveRenderer.waveLeftMargin = (waveRenderer.MarginLeftMm + 10) * 3.7795f;

            double pageWidthPx = 210 * 3.777;
            double pageHeightPx = 297 * 3.777;

            for (int i = 0; i < totalPages; i++)
            {
                // 创建SKSurface用于绘制
                using (var surface = SKSurface.Create(new SKImageInfo((int)pageWidthPx, (int)pageHeightPx)))
                {
                    var canvas = surface.Canvas;

                    // 清除画布为白色背景
                    canvas.Clear(SKColors.White);

                    waveRenderer.DrawGrid(canvas, pageWidthPx, pageHeightPx);
                    waveRenderer.DrawEcgWaveforms(canvas, i, StartSamples[0],
                                   pageWidthPx, pageHeightPx);
                    waveRenderer.DrawBeatHeartInfo(canvas, i, StartSamples[0],
                                   pageWidthPx, pageHeightPx);
                    waveRenderer.DrawPageHeader(canvas, i, StartSamples[0]);
                    // 保存为PNG文件
                    string outputPath = $"{i}_black_curve.png";
                    using (var image = surface.Snapshot())
                    using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                    using (MemoryStream stream = new MemoryStream(data.ToArray()))
                    using (var streamdata = File.OpenWrite(outputPath))
                    {
                        data.SaveTo(streamdata);
                        new Bitmap(stream);
                    }
                }
            }
        }
        private void printSegmentWaves()
        {
            bool bHorizontalPrint = Convert.ToInt32(cbxDirection.SelectedValue) != 1;
            int totalPages = SegmentAllPageInfo.Count;
            segmentPrintManager = new SegmentPrintManager(bHorizontalPrint, totalPages)
            {
                SegmentAllPageInfo = SegmentAllPageInfo,
                
            };
            segmentPrintManager.SetOnDemandRendering();
            segmentPrintManager.ShowPrintDialog();
        }

        private void printSegmentTest()
         {
            PrintSegmentWave waveRenderer = new PrintSegmentWave();
            waveRenderer.SegmentAllPageInfo = SegmentAllPageInfo;
            waveRenderer.TotalPages = SegmentAllPageInfo.Count;

            float pixMM = 3.7795f;

            //纵向
            float PaperWidthMm = 210.0f;
            float PaperHeightMm = 297.0f;
            float MarginLeftMm = 10.0f;       // 左边距
            float MarginRightMm = 15.0f;      // 右边距
            float MarginTopMm = 20.0f;          //上边距
            float MarginBottonMm = 17.0f;       //下边距

            double pageWidthPx = PaperWidthMm * pixMM;
            double pageHeightPx = PaperHeightMm * pixMM;

            float PageTopMargin = MarginTopMm * pixMM;
            float PageBottomMargin = MarginBottonMm * pixMM;

            float segmentInfoHeight = 10 * pixMM;

            List<JObject> SegmentPageInfo = SegmentAllPageInfo[0];

            float leadRowHeight = 0;
            var temp = JToken.FromObject(SegmentPageInfo[0]);
            List<int> firstLeadIdxs = temp["leadIdx"].ToObject<List<int>>() ?? new List<int>();
            int firstPartLeadCount = firstLeadIdxs.Count;
            if (SegmentPageInfo.Count == 1 && firstPartLeadCount > 5)
            {
                leadRowHeight = (float)(pageHeightPx - PageTopMargin - PageBottomMargin - segmentInfoHeight) / firstPartLeadCount;
            }
            else
            {
                leadRowHeight = (float)(pageHeightPx - PageTopMargin - PageBottomMargin - segmentInfoHeight * SegmentPageInfo.Count) / 10;
            }
            // 创建SKSurface用于绘制
            using (var surface = SKSurface.Create(new SKImageInfo((int)pageWidthPx, (int)pageHeightPx)))
            {
                var canvas = surface.Canvas;

                // 清除画布为白色背景
                canvas.Clear(SKColors.White);
                waveRenderer.DrawPageHeader(canvas, 0);
                int leadSum = 0;
                for (int i = 0; i < SegmentPageInfo.Count; i++)
                {
                    var partInfo = JToken.FromObject(SegmentPageInfo[i]);
                    List<int> leadIdxs = partInfo["leadIdx"].ToObject<List<int>>() ?? new List<int>();
                    int leadCount = leadIdxs.Count;
                    int startSample = partInfo["startSample"]?.Value<int>() ?? 0;
                    int rowSeconds = partInfo["rowSeconds"]?.Value<int>() ?? 0;
                    List<WaveInfo> ecgData = AnaCode.ReadAllWaveData(leadIdxs, startSample, startSample + rowSeconds * AnaValues.holterConfig.SampleRate - 1);

                    if (ecgData == null || ecgData.Count == 0)
                    {
                        continue;
                    }

                    float left = MarginLeftMm * pixMM;
                    float top = MarginTopMm * pixMM + i * segmentInfoHeight + leadSum*leadRowHeight;
                    float right = (float)(pageWidthPx - MarginRightMm * pixMM);
                    float bottom = top + leadCount * leadRowHeight;
                    SKRect WaveRect = new SKRect(left, top, right, bottom);
                    waveRenderer.WaveRect = WaveRect;
                    waveRenderer.DrawGrid(canvas);

                    waveRenderer.Gain = partInfo["gain"]?.Value<double>() ?? 0.0;
                    waveRenderer.EcgData = ecgData;
                    waveRenderer.LeadIdxs = leadIdxs;
                    waveRenderer.StartSample = startSample;
                    waveRenderer.SegmentLabel = partInfo["segmentLabel"]?.Value<string>() ?? "";
                    waveRenderer.DrawEcgWave(canvas);

                    leadSum += leadCount;
                }
                // 保存为PNG文件
                string outputPath = $"0_black_curve.png";
                using (var image = surface.Snapshot())
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (MemoryStream stream = new MemoryStream(data.ToArray()))
                using (var streamdata = File.OpenWrite(outputPath))
                {
                    data.SaveTo(streamdata);
                    new Bitmap(stream);
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void PrintSettingForm_Load(object sender, EventArgs e)
        {
            initUI();
            initControl();
            AttachCheckboxEvents();

        }

        private void initUI()
        {
            fillLengthCombobox();
            fillGainCombobox();
            fillDirectionCombobox();
            fillLabelCombobox();
        }

        private void fillLengthCombobox()
        {
            cbxDuration.ValueMember = "ItemValue";
            cbxDuration.DisplayMember = "ItemText";

            // 初始化时根据当前方向设置选项
            UpdateLengthComboBoxOptions();
        }

        private void fillGainCombobox()
        {
            cbxGain.ValueMember = "ItemValue";
            cbxGain.DisplayMember = "ItemText";

            List<ListBoxItem> items = new List<ListBoxItem>
            {
                new ListBoxItem(2.5, "2.5mm/mV"),
                new ListBoxItem(5.0, "5mm/mV"),
                new ListBoxItem(10.0, "10mm/mV"),
                new ListBoxItem(20.0, "20mm/mV")
            };
            cbxGain.DataSource = items;
            cbxGain.SelectedValue = 10.0;
        }

        private void fillDirectionCombobox()
        {
            cbxDirection.ValueMember = "ItemValue";
            cbxDirection.DisplayMember = "ItemText";

            List<ListBoxItem> items = new List<ListBoxItem>
            {
                new ListBoxItem(1, "纵向"),
                new ListBoxItem(0, "横向")
            };
            cbxDirection.DataSource = items;
            cbxDirection.SelectedValue = 1;

            // 添加方向选择改变事件
            cbxDirection.SelectedIndexChanged += cbxDirection_SelectedIndexChanged;
        }

        private void fillLabelCombobox()
        {
            cbxLabel.ValueMember = "ItemValue";
            cbxLabel.DisplayMember = "ItemText";

            List<ListBoxItem> items = new List<ListBoxItem>
            {
                new ListBoxItem("ECG", "ECG"),
                new ListBoxItem("单发室早", "单发室早"),
                new ListBoxItem("成对室早", "成对室早"),
                new ListBoxItem("室速", "室速"),
                new ListBoxItem("室早二联律", "室早二联律"),
                new ListBoxItem("室早三联律", "室早三联律"),
                new ListBoxItem("单发房早", "单发房早"),
                new ListBoxItem("房速", "房速"),
                new ListBoxItem("房早二联律", "房早二联律"),
                new ListBoxItem("房早三联律", "房早三联律"),
                new ListBoxItem("RonT", "RonT"),
                new ListBoxItem("ST抬高", "ST抬高"),
                new ListBoxItem("ST压低", "ST压低"),
                new ListBoxItem("最快心率", "最快心率"),
                new ListBoxItem("最慢心率", "最慢心率"),
                new ListBoxItem("室性逸搏", "室性逸搏"),
                new ListBoxItem("房性逸搏", "房性逸搏"),
                new ListBoxItem("交界性逸搏", "交界性逸搏"),
                new ListBoxItem("心动过速", "心动过速"),
                new ListBoxItem("心动过缓", "心动过缓"),
                new ListBoxItem("房早未下传", "房早未下传")
            };
            cbxLabel.DataSource = items;
            cbxLabel.SelectedValue = "ECG";
        }

        /// <summary>
        /// 方向选择改变事件处理
        /// </summary>
        private void cbxDirection_SelectedIndexChanged(object sender, EventArgs e)
        {
            isPortrait = Convert.ToInt32(cbxDirection.SelectedValue);
            // 根据选择的方向更新打印长度下拉框选项
            UpdateLengthComboBoxOptions();
        }

        /// <summary>
        /// 根据方向更新打印长度下拉框选项
        /// </summary>
        private void UpdateLengthComboBoxOptions()
        {
            List<ListBoxItem> items = new List<ListBoxItem>();

            if (isPortrait == 1)
            {
                // 纵向：选项为7的倍数，最大到5倍（7, 14, 21, 28, 35）
                for (int i = 1; i <= 10; i++)
                {
                    int value = i * 7;
                    string displayText = $"{value}";
                    items.Add(new ListBoxItem(value, displayText));
                }
            }
            else
            {
                // 横向：选项为10的倍数，最大到5倍（10, 20, 30, 40, 50）
                for (int i = 1; i <= 10; i++)
                {
                    int value = i * 10;
                    string displayText = $"{value}";
                    items.Add(new ListBoxItem(value, displayText));
                }
            }

            // 更新下拉框数据源
            cbxDuration.DataSource = items;
            cbxDuration.SelectedIndex = 0; // 默认选择第一个选项
        }

        private void initControl()
        {
            if (bSelectSingleHeartbeat)
            {
                cbxDuration.Visible = true;
                lblLength.Visible = false;
            }
            else
            {
                cbxDuration.Visible = false;
                lblLength.Visible = true;
                lblLength.Text = printLen;
            }

            // 初始化时更新label1文字
            UpdateLabel1Text();

            // 初始化时确保至少有一个导联被选中
            EnsureAtLeastOneLeadSelected();
            if (type == 2 || type == 3)
            {
                SegmentInfoJsons = OperateSegment.getSegmentObj(segmentFilePath, StartSamples);

                if(SegmentInfoJsons.Count > 0)
                {
                    SegmentPageInfos = OperateSegment.getSegmentPageInfo(SegmentInfoJsons);

                    SegmentAllPageInfo = OperateSegment.CalcSegmentPrintPageData(SegmentPageInfos, SegmentInfoJsons);
                    initSegmentInfo();
                    if (StartSamples.Count > 1)
                    {
                        cbxLabel.Enabled = false;
                        tbxAvgHR.Enabled = false;
                    }
                }
            }
        }

        private void CalcSegmentPageInfo(int printSeconds)
        {
            SegmentAllPageInfo.Clear();
            JObject segmentInfoJson = getSegmentAttr();
            int segmentStartSample = segmentInfoJson["startSample"]?.Value<int>() ?? 0;
            string segmentLabel = segmentInfoJson["cbxLabel"]?.Value<string>() ?? "";
            int duration = printSeconds;
            int direction = segmentInfoJson["cbxDirection"]?.Value<int>() ?? 0;
            double gain = segmentInfoJson["cbxGain"]?.Value<double>() ?? 0.0;
            List<int> leadIdxs = segmentInfoJson["leadIdxs"].ToObject<List<int>>() ?? new List<int>();
            int leadCount = leadIdxs.Count;
            int rowSeconds = direction == 0 ? 10 : 7;

            // 确定每页最大行数和每导联最大行数
            int maxRowsPerPage = leadCount < 6 ? 10 : leadCount;
            int maxPartPerPage = maxRowsPerPage / leadCount;
            int segmentTotalPart = duration / rowSeconds;
            int pageCount = (segmentTotalPart + maxPartPerPage -1) / maxPartPerPage;
            int partIdx = 0;
            for(int i=0; i<pageCount; i++)
            {
                List<JObject> segmentOnePageInfo = new List<JObject>();
                for(int j=0; j<maxPartPerPage; j++)
                {
                    if (partIdx == segmentTotalPart) break;

                    var pagePartInfo = new JObject();
                    pagePartInfo["startSample"] = segmentStartSample + partIdx * rowSeconds * AnaValues.holterConfig.SampleRate;
                    pagePartInfo["segmentLabel"] = segmentLabel;
                    pagePartInfo["gain"] = gain;
                    pagePartInfo["leadIdx"] = JToken.FromObject(leadIdxs);
                    pagePartInfo["direction"] = direction;
                    pagePartInfo["rowSeconds"] = rowSeconds;
                    segmentOnePageInfo.Add(pagePartInfo);
                    partIdx++;
                }

                SegmentAllPageInfo.Add(segmentOnePageInfo);
            }
        }

        private void initSegmentInfo()
        {
            JObject segmentInfoJson = SegmentInfoJsons[0];

            List<TextBoxEx> textBoxList = CommonFuns.RetriveSubControls<TextBoxEx>(tableLayoutPanel1);
            foreach (TextBoxEx textBox in textBoxList)
            {
                textBox.Text = segmentInfoJson[textBox.Name]?.ToString();
            }

            List<ComboBox> comboboxList = CommonFuns.RetriveSubControls<ComboBox>(tableLayoutPanel1);
            foreach (ComboBox comboBox in comboboxList)
            {
                if (comboBox.Name == "cbxLabel")
                {
                    comboBox.Text = segmentInfoJson[comboBox.Name]?.ToString();
                }
                else
                {
                    comboBox.SelectedValue = comboBox.Name == "cbxGain"
                        ? Convert.ToDouble(segmentInfoJson[comboBox.Name])
                        : (object)Convert.ToInt32(segmentInfoJson[comboBox.Name]);
                }
            }
            foreach (Control subControl in tableLayoutPanel2.Controls)
            {

                if (subControl is CheckBox checkBox)
                {
                    checkBox.Checked = false;
                }
            }
            leadIdxs.Clear();
            List<int> leadIdxList = segmentInfoJson["leadIdxs"].ToObject<List<int>>() ?? new List<int>();
            foreach (int idx in leadIdxList)
            {
                leadIdxs.Add(idx);
                foreach (Control subControl in tableLayoutPanel2.Controls)
                {
                    if (subControl is CheckBox checkBox)
                    {
                        if (AnaValues.LeadNameToIdxMap[checkBox.Text] == idx)
                        {
                            checkBox.Checked = true;
                            break;
                        }
                    }
                }

            }

        }

        /// <summary>
        /// 更新label1文字，根据选中的checkbox数量显示对应的导联数
        /// </summary>
        private void UpdateLabel1Text()
        {
            label1.Text = $"{currentLeadCount}导联连续片段图";
        }

        /// <summary>
        /// 获取groupBox1中选中的checkbox数量
        /// </summary>
        /// <returns>选中的checkbox数量</returns>
        private int GetSelectedCheckboxCount()
        {
            int count = 0;
            leadIdxs.Clear();
            // 在TableLayoutPanel中查找checkbox
            foreach (Control subControl in tableLayoutPanel2.Controls)
            {
                if (subControl is CheckBox checkBox && checkBox.Checked)
                {
                    count++;
                    leadIdxs.Add(AnaValues.LeadNameToIdxMap[checkBox.Text]);
                }
            }

            return count;
        }

        /// <summary>
        /// 为所有checkbox添加CheckedChanged事件处理
        /// </summary>
        private void AttachCheckboxEvents()
        {
            // 遍历groupBox1中的所有checkbox控件
            foreach (Control control in groupBox1.Controls)
            {
                if (control is TableLayoutPanel tablePanel)
                {
                    // 在TableLayoutPanel中查找checkbox
                    foreach (Control subControl in tablePanel.Controls)
                    {
                        if (subControl is CheckBox checkBox)
                        {
                            // 移除可能已存在的事件处理程序，然后添加新的
                            checkBox.CheckedChanged -= Checkbox_CheckedChanged;
                            checkBox.CheckedChanged += Checkbox_CheckedChanged;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// checkbox选中状态改变事件处理
        /// </summary>
        private void Checkbox_CheckedChanged(object sender, EventArgs e)
        {
            currentLeadCount = GetSelectedCheckboxCount();

            // 当任何checkbox的选中状态改变时，更新label1文字
            UpdateLabel1Text();

            // 检查是否至少有一个导联被选中
            EnsureAtLeastOneLeadSelected();
        }

        /// <summary>
        /// 确保至少有一个导联被选中
        /// </summary>
        private void EnsureAtLeastOneLeadSelected()
        {
            int selectedCount = GetSelectedCheckboxCount();

            // 如果没有选中任何导联，自动选中第一个可用的导联
            if (selectedCount == 0)
            {
                SelectFirstAvailableLead();
            }
        }

        /// <summary>
        /// 选中第一个可用的导联
        /// </summary>
        private void SelectFirstAvailableLead()
        {
            // 遍历groupBox1中的所有checkbox控件，找到第一个可用的并选中
            foreach (Control control in groupBox1.Controls)
            {
                if (control is TableLayoutPanel tablePanel)
                {
                    // 在TableLayoutPanel中查找checkbox
                    foreach (Control subControl in tablePanel.Controls)
                    {
                        if (subControl is CheckBox checkBox && checkBox.Enabled)
                        {
                            if (checkBox.TabIndex == AnaValues.holterConfig.MainChannel)
                            {
                                // 选中第一个可用的checkbox
                                checkBox.Checked = true;
                                return; // 选中一个后就返回
                            }

                        }
                    }
                }
            }
        }

        private void InitData()
        {
            gain = Convert.ToDouble(cbxGain.SelectedValue);
            ecgData.Clear();
            heartbeatInfos.Clear();

            int leadMaxRows = 1;
            if (currentLeadCount < 5)
            {
                switch (currentLeadCount)
                {
                    case 1:
                        leadMaxRows = 9;
                        break;
                    case 2:
                        leadMaxRows = 4;
                        break;
                    case 3:
                        leadMaxRows = 3;
                        break;
                    case 4:
                        leadMaxRows = 2;
                        break;
                    default:
                        break;
                }
            }

            rowSeconds = Convert.ToInt32(cbxDirection.SelectedValue) == 1 ? 7 : 10;

            int pageMaxSeconds = leadMaxRows * rowSeconds;

            rowPoints = rowSeconds * AnaValues.holterConfig.SampleRate;

            if (bSelectSingleHeartbeat)
            {
                int selectSeconds = Convert.ToInt32(cbxDuration.SelectedValue);
                currentRows = selectSeconds / rowSeconds > leadMaxRows ? leadMaxRows : selectSeconds / rowSeconds;
                totalPages = selectSeconds < pageMaxSeconds
                    ? 1
                    : (selectSeconds % pageMaxSeconds == 0) ? selectSeconds / pageMaxSeconds : (selectSeconds / pageMaxSeconds) + 1;
                endSample = StartSamples[0] + (totalPages * rowPoints * currentRows);
                totalPrintPoints = selectSeconds * AnaValues.holterConfig.SampleRate;
            }
            else
            {
                endSample = printEndSample;
                int totalPoints = printEndSample - StartSamples[0];
                int pageMaxPoints = pageMaxSeconds * AnaValues.holterConfig.SampleRate;
                totalPages = totalPoints < pageMaxPoints
                    ? 1
                    : totalPoints % pageMaxPoints != 0 ? (totalPoints / pageMaxPoints) + 1 : totalPoints / pageMaxPoints;
                totalPoints = totalPages * pageMaxSeconds * AnaValues.holterConfig.SampleRate;
                currentRows = totalPoints / (rowPoints * totalPages);

                endSample = StartSamples[0] + totalPoints;

                totalPrintPoints = totalPoints;
            }

            ecgData = AnaCode.ReadAllWaveData(leadIdxs, StartSamples[0], StartSamples[0] + totalPages * rowPoints * currentRows);

            heartbeatInfos = AnaValues.CachedAllBeats
            .Where(h => h.EndIndex > StartSamples[0] && h.StartIndex < endSample)
            .OrderBy(h => h.StartIndex)
            .ToList();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if(type == 1)
            {
                CalcSegmentPageInfo(Convert.ToInt32(cbxDuration.SelectedValue));
            }
            if (type == 2)
            {
                modifySegmentAttr();
                CalcSegmentPageInfo(Convert.ToInt32(cbxDuration.SelectedValue));
                if (segmentWaveView != null)
                {
                    segmentWaveView.UpdateDataForceWaveformRedraw();
                }
            }
            printSegmentWaves();
        }
    }
}
