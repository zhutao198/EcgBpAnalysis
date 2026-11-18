using AnalysisApp.Commons;
using AnalysisSystem.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.Component.Widgets;
using Zoncare.DAL;
using Zoncare.Utilities;

namespace AnalysisApp
{
    public partial class ReanalysisPage : BaseUserControl
    {
        private HolterConfig _originalConfig;
        private HolterConfig _newConfig;
        private BeatInfo _selectedBeat; //一个心拍的起止位置及QRS波峰位置信息(不一定是真实心拍)
        private int _filterClickCount = 0;
        private int[] _suggestLeads;
        private int _mainLead, _auxiliaryLead1, _auxiliaryLead2;
        private Stopwatch _filterStopwatch = new Stopwatch();

        public ReanalysisPage()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            //this.SuspendLayout();
        }

        public override void OnParentFormShown(EventArgs e)
        {
            if (DesignMode) return;

            //this.BeginInvoke(new MethodInvoker(() =>
            //{
            showCombobox();

            //this.ResumeLayout(false);
            //this.PerformLayout();

            base.OnParentFormShown(e);
            //}));
        }

        public override async Task RefreshData(bool resetUI = false)
        {
            while (!IsShown) await Task.Delay(1);

            _originalConfig = AnaValues.holterConfig;
            _newConfig = PubFuns.DeepClone(_originalConfig);

            if (resetUI)
            {
                _selectedBeat = BeatInfo.Empty;
                loadConfig();
            }
            loadData(resetUI);
            //this.Invalidate(true);
        }


        #region 事件
        private void ReanalysisPage_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;

            try
            {
                if (!IsUiInitialized) InitUI();

                _originalConfig = AnaValues.holterConfig;
                _newConfig = PubFuns.DeepClone(_originalConfig);
                _selectedBeat = BeatInfo.Empty;
                loadConfig();
                loadData(true);

                Console.WriteLine("重分析界面已加载");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "重分析");
            }
        }

        private void ReanalysisPage_Resize(object sender, EventArgs e)
        {
            //if (!IsShown)
            //    SuspendLayout();
            //else
            //    ResumeLayout(true);
        }

        private void ReanalysisPage_SizeChanged(object sender, EventArgs e)
        {
            //this.scatterPlot1.Width = this.scatterPlot1.Height + 34;
            //this.overlayPlot01.Width = this.overlayPlot02.Width = this.overlayPlot03.Width = (int)(this.overlayPlot01.Height * 1.8);

            //this.scatterPlot2.Width = this.scatterPlot2.Height + 34;
            //this.overlayPlot11.Width = this.overlayPlot12.Width = this.overlayPlot13.Width = (int)(this.overlayPlot11.Height * 1.8);
        }

        //心电参数设置
        private void btnEcgSets_Click(object sender, EventArgs e)
        {

        }

        //开始分析
        private void btnAnalyse_Click(object sender, EventArgs e)
        {
            //保存参数配置到holter.info
            bool multiChannel = this.chkMultiChanel.Checked;
            int mainChannel = Convert.ToInt32(this.cbxChanel0.SelectedValue);
            _newConfig.AnalysisSetting.MainChannel = mainChannel;
            _newConfig.AnalysisSetting.SecondChannel1 = Convert.ToInt32(this.cbxChanel1.SelectedValue);
            _newConfig.AnalysisSetting.SecondChannel2 = Convert.ToInt32(this.cbxChanel2.SelectedValue);

            AnalysisSetting anaSettings = _newConfig.AnalysisSetting;
            anaSettings.Frequency = Convert.ToInt32(this.cbxFrequency.SelectedValue);
            anaSettings.Baseline = Convert.ToInt32(this.cbxBaseline.SelectedValue);
            anaSettings.Myoelectric = Convert.ToInt32(this.cbxMyoelectric.SelectedValue);

            anaSettings.PaceAnalysisEnable = this.chkPace.Checked;
            anaSettings.PaceAnalysisType = this.cbxPacemakerType.SelectedValue.ToString();
            anaSettings.PaceMethod = Convert.ToInt32(this.cbxPaceMode.SelectedValue);

            anaSettings.EbAnalysisEnable = this.chkEscape.Checked;
            anaSettings.DisturbDelEnable = this.chkDisturbDel.Checked;
            anaSettings.MultichannelEnable = multiChannel;
            anaSettings.QrsRefractoryPeriod = Convert.ToInt32(this.tbxRefractoryPeriod.Text);
            anaSettings.Sensitivity = Convert.ToInt32(this.cbxRSensitivity.SelectedValue);

            anaSettings.InvalidInterval.Clear();
            if (ecgCommonWave.CutRanges != null && ecgCommonWave.CutRanges.Count > 0)
            {
                foreach (var range in ecgCommonWave.CutRanges)
                {
                    anaSettings.InvalidInterval.Add(new int[] { range.StartIndex, range.EndIndex });
                }
            }
            //_newConfig.LeadConfig.LfLatest = ecgCommonWave.Limbleads;

            _newConfig.ParamsSetting.Eb.Spercent = Convert.ToInt32(this.tbxSpercent.Text);
            _newConfig.ParamsSetting.Eb.Vpercent = Convert.ToInt32(this.tbxVpercent.Text);

            _newConfig.SaveToFile(Path.Combine(AnaValues.RootPath, "holter.info"));

            AnaCode.IsAnalysisST = true;
            //调用算法 aecgReanalyze
            int result = -1, resultST = -1;
            DateTime dtStart = DateTime.Now;
            Stopwatch sw = Stopwatch.StartNew();
            Form parentForm = this.FindForm();
            AppCode.ShowProgress(_langs["分析中..."], this.ParentForm, async (args) =>
            {
                dynamic paraInfo = args as ExpandoObject;
                ProgressForm frm = paraInfo.Form as ProgressForm;

                bool isProcessing = true;
                Task.Run(() =>
                {
                    result = AEcgHandler.Reanalyze();
                    resultST = AEcgHandler.AnalyzeST();
                    isProcessing = false;
                });

                int progressValue = 0;
                while (isProcessing)
                {
                    await Task.Delay(50);
                    AEcgHandler.FunctionPercents.TryGetValue(AEcgHandler.AEcgFuncType.AecgReanalyze, out int processingPercent);
                    progressValue = processingPercent;
                    //progressValue = Math.Min(progressValue + 1, processingPercent);
                    progressValue = Math.Min(progressValue, 99);
                    frm.InvokeIfRequired(() =>
                    {
                        frm.Progress = progressValue;
                        frm.Refresh();
                    });
                }
            }, null, new Rectangle(parentForm.PointToScreen(Point.Empty), parentForm.ClientSize), true);

            string duration = CommonFuns.GetTimeDurationText(AnaValues.CurrentCase.dataDuration);
            Logger.LogRuntimeInfo($"心电数据重分析(病历号:{AnaValues.CurrentCase.caseNo}, 采样数据时长:{duration})", sw.ElapsedMilliseconds);

            if (result == 0)
            {
                ResetAdvice();
                _newConfig.StartReanalysisTime = dtStart.ToString(SysConstants.DATETIME_FORMAT);
                _newConfig.StartReanalysisFinishTime = DateTime.Now.ToString(SysConstants.DATETIME_FORMAT);
                _newConfig.StartReanalysis = true;
                _newConfig.SaveToFile(Path.Combine(AnaValues.RootPath, "holter.info"));
                AnaValues.holterConfig = _newConfig;

                this.ecgCommonWave.RefreshData(true);
                AnalysisWindow.Instance.SwitchTabPage("btnInfo");

                if (resultST != 0) ScreenCenterTip.Show("ST分析异常");
            }
            else
            {
                CommonFuns.PromptError("操作失败");
            }
            object para = new { CaseNo = AnaValues.CurrentCase.caseNo, AnalysisSetting = _newConfig.AnalysisSetting };
            OperateLog.logParam("病例心电数据重分析", para, sw.ElapsedMilliseconds, AnaValues.CurrentUser.userName);
        }

        private void cbxChanel_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cbx = sender as Zoncare.Component.Controls.ZoncareComboBox;

            this.BeginInvoke(new Action(() =>
            {
                if (cbx != null)
                {
                    int leadIndex = Convert.ToInt32(cbx.SelectedValue);
                    _suggestLeads = retrieveSuggestLeads(new int[] { _mainLead, _auxiliaryLead1, _auxiliaryLead2 });

                    if (cbx == this.cbxChanel0)
                    {
                        if (_selectedBeat != BeatInfo.Empty) this.stSettingPlot1.SetBeatSource(_selectedBeat, leadIndex);
                        _mainLead = leadIndex;
                    }
                    else if (cbx == this.cbxChanel1)
                    {
                        if (_selectedBeat != BeatInfo.Empty) this.stSettingPlot2.SetBeatSource(_selectedBeat, leadIndex);
                        _auxiliaryLead1 = leadIndex;
                    }
                    else if (cbx == this.cbxChanel2)
                    {
                        if (_selectedBeat != BeatInfo.Empty) this.stSettingPlot3.SetBeatSource(_selectedBeat, leadIndex);
                        _auxiliaryLead2 = leadIndex;
                    }

                    int[] channels = retrieveChannels();
                    _mainLead = channels[0];
                    _auxiliaryLead1 = channels[1];
                    _auxiliaryLead2 = channels[2];

                    //更新UI
                    this.cbxChanel0.SelectedValue = _mainLead;
                    this.cbxChanel1.SelectedValue = _auxiliaryLead1;
                    this.cbxChanel2.SelectedValue = _auxiliaryLead2;

                    this.ecgCommonWave.leadIdxs = channels.ToList();
                    this.ecgCommonWave.forceWaveformRedraw();
                }
            }));
        }

        private void chkPace_CheckedChanged(object sender, EventArgs e)
        {
            this.cbxPacemakerType.Enabled = this.cbxPaceMode.Enabled = this.chkPace.Checked;
        }

        private void chkEscape_CheckedChanged(object sender, EventArgs e)
        {
            this.tbxSpercent.Enabled = this.tbxVpercent.Enabled = this.chkEscape.Checked;
            if (this.chkEscape.Checked)
            {
                this.tbxSpercent.MinValue = this.tbxVpercent.MinValue = 10;
                this.tbxSpercent.MaxValue = this.tbxVpercent.MaxValue = 110;
                this.tbxSpercent.Text = _originalConfig.ParamsSetting.Eb.Spercent.ToString();
                this.tbxVpercent.Text = _originalConfig.ParamsSetting.Eb.Vpercent.ToString();
            }
            else
            {
                this.tbxSpercent.MinValue = this.tbxVpercent.MinValue = 0;
                this.tbxSpercent.Text = this.tbxVpercent.Text = "0";
            }
        }
        #endregion


        public override void InitUI()
        {
            base.InitUI();

            try
            {
                init();
                fillLeadComboBox(this.cbxChanel0, _originalConfig.Lead);
                fillLeadComboBox(this.cbxChanel1, _originalConfig.Lead);
                fillLeadComboBox(this.cbxChanel2, _originalConfig.Lead);
                this.cbxChanel0.Visible = this.cbxChanel1.Visible = this.cbxChanel2.Visible = true;

                CommonFuns.InitCommboBoxItems(this.cbxFrequency, SysConstants.ECG_FREQUENCY);
                CommonFuns.InitCommboBoxItems(this.cbxMyoelectric, SysConstants.ECG_EMG);
                CommonFuns.InitCommboBoxItems(this.cbxBaseline, SysConstants.ECG_BASELINE);

                CommonFuns.InitCommboBoxItems(this.cbxPacemakerType, AnaValues.PacemakerType);
                CommonFuns.InitCommboBoxItems(this.cbxPaceMode, AnaValues.PaceMode);

                PersistDictionary<int> dictSensitivityLevel = new PersistDictionary<int> { { 0, "高" }, { 1, "中" }, { 2, "低" } };
                CommonFuns.InitCommboBoxItems(this.cbxRSensitivity, dictSensitivityLevel);

                registerEvents();
                Console.WriteLine("重分析界面已初始化完成");
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "重分析");
            }
        }

        private void init()
        {
            //隐藏滤波设置
            this.groupBox4.Visible = false;
            this.tableLayoutPanel1.ColumnStyles[1].Width = 0;

            _originalConfig = AnaValues.holterConfig;
            _newConfig = PubFuns.DeepClone(_originalConfig);
        }

        private void loadConfig()
        {
            _suggestLeads = retrieveSuggestLeads(new int[] { _originalConfig.MainChannel, _originalConfig.SecondChannel1, _originalConfig.SecondChannel2 });
            _mainLead = _suggestLeads[0];
            _auxiliaryLead1 = _suggestLeads[1];
            _auxiliaryLead2 = _suggestLeads[2];

            this.cbxChanel0.SelectedValue = _mainLead;
            this.cbxChanel1.SelectedValue = _auxiliaryLead1;
            this.cbxChanel2.SelectedValue = _auxiliaryLead2;

            HolterConfig cfg = AnaValues.holterConfig;
            AnalysisSetting anaSettings = cfg.AnalysisSetting;
            this.lblBaseOffset.Text = _langs["基线点偏移:"] + $" {anaSettings.BaseOffset}ms";
            this.lblJOffset.Text = _langs["J点偏移:"] + $" {anaSettings.JOffset}ms";
            this.lblSTOffset.Text = _langs["ST点偏移:"] + $" {anaSettings.StOffset}ms";

            this.cbxFrequency.SelectedValue = anaSettings.Frequency;
            this.cbxMyoelectric.SelectedValue = anaSettings.Myoelectric;
            this.cbxBaseline.SelectedValue = anaSettings.Baseline;

            if (_originalConfig.StartReanalysis == false)
                this.chkPace.Checked = AnaValues.CurrentCase.hasPacemaker == 1;
            else
                this.chkPace.Checked = anaSettings.PaceAnalysisEnable;

            this.cbxPacemakerType.SelectedValue = anaSettings.PaceAnalysisType;
            this.cbxPaceMode.SelectedValue = anaSettings.PaceMethod;
            chkPace_CheckedChanged(null, null);

            //this.chkEscape.Checked = cfg.ParamsSetting.Eb.Spercent > 0 && cfg.ParamsSetting.Eb.Vpercent > 0;
            this.chkEscape.Checked = anaSettings.EbAnalysisEnable;
            chkEscape_CheckedChanged(null, null);

            this.chkDisturbDel.Checked = anaSettings.DisturbDelEnable;
            this.chkMultiChanel.Checked = anaSettings.MultichannelEnable;
            this.cbxChanel1.IsUnresponsive = this.cbxChanel2.IsUnresponsive = !this.chkMultiChanel.Checked;

            this.tbxRefractoryPeriod.Text = anaSettings.QrsRefractoryPeriod.ToString();
            this.cbxRSensitivity.SelectedValue = anaSettings.Sensitivity;
        }

        private void loadData(bool resetUI = false)
        {
            if (this.ecgCommonWave.IsLoaded)
            {
                int[] leads = new int[] {
                    Convert.ToInt32(this.cbxChanel0.SelectedValue),
                    Convert.ToInt32(this.cbxChanel1.SelectedValue),
                    Convert.ToInt32(this.cbxChanel2.SelectedValue)
                };
                this.ecgCommonWave.leadIdxs = leads.ToList();
                this.ecgCommonWave.RefreshData(resetUI);

                //将通用波形视图定位到当前页的中间位置
                float pageTimeSpan = this.ecgCommonWave.Width / SysConstants.PIX_SCALE / 25F;
                int middleSample = (int)(pageTimeSpan / 2 * _originalConfig.SampleRate);
                //this.ecgCommonWave.BeginInvoke(new Action(() => this.ecgCommonWave.OnWaveformStartTimeChanged(middleSample)));
                this.ecgCommonWave.OnWaveformStartTimeChanged(middleSample);

                this.stSettingPlot1.SetBeatSource(BeatInfo.Empty, leads[0]);
                this.stSettingPlot2.SetBeatSource(BeatInfo.Empty, leads[1]);
                this.stSettingPlot3.SetBeatSource(BeatInfo.Empty, leads[2]);

                //ST分析设置波形显示
                refreshWave(middleSample);
            }
        }

        //订阅事件
        private void registerEvents()
        {
            this.cbxChanel0.SelectedIndexChanged += cbxChanel_SelectedIndexChanged;
            this.cbxChanel1.SelectedIndexChanged += cbxChanel_SelectedIndexChanged;
            this.cbxChanel2.SelectedIndexChanged += cbxChanel_SelectedIndexChanged;

            this.cbxChanel0.SelectionChangeCommitted += (o, e) =>
            {
                if (this.chkMultiChanel.Checked)
                {
                    int leadIndex = Convert.ToInt32(cbxChanel0.SelectedValue);
                    if (leadIndex == _auxiliaryLead1 || leadIndex == _auxiliaryLead2)
                    {
                        ScreenCenterTip.Show("分析通道不能相同，请重新选择");
                        cbxChanel0.SelectedValue = _mainLead;
                        return;
                    }
                }
            };
            this.cbxChanel1.SelectionChangeCommitted += (o, e) =>
            {
                int leadIndex = Convert.ToInt32(cbxChanel1.SelectedValue);
                if (leadIndex == _mainLead || leadIndex == _auxiliaryLead2)
                {
                    ScreenCenterTip.Show("分析通道不能相同，请重新选择");
                    cbxChanel1.SelectedValue = _auxiliaryLead1;
                    return;
                }
            };
            this.cbxChanel2.SelectionChangeCommitted += (o, e) =>
            {
                int leadIndex = Convert.ToInt32(cbxChanel2.SelectedValue);
                if (leadIndex == _mainLead || leadIndex == _auxiliaryLead1)
                {
                    ScreenCenterTip.Show("分析通道不能相同，请重新选择");
                    cbxChanel2.SelectedValue = _auxiliaryLead2;
                    return;
                }
            };

            this.stSettingPlot1.OnDeviationAdjusted += setDeviation;
            this.stSettingPlot2.OnDeviationAdjusted += setDeviation;
            this.stSettingPlot3.OnDeviationAdjusted += setDeviation;

            this.chkPace.CheckedChanged += chkPace_CheckedChanged;
            this.chkEscape.CheckedChanged += chkEscape_CheckedChanged;
            this.chkMultiChanel.CheckedChanged += (o, e) => { this.cbxChanel1.IsUnresponsive = this.cbxChanel2.IsUnresponsive = !this.chkMultiChanel.Checked; };

            this.ecgCommonWave.OnEcgCommonWaveClickChanged += (waveformTimeOffsetSeconds, start, midSample, end) =>
            {
                refreshWave(midSample);
            };
            this.ecgCommonWave.OnEcgCommonRowSecondsChanged += this.smallWaveView.OnWaveformSpeedChanged;
            this.ecgCommonWave.OnEcgCommonWaveStartTimeChanged += this.smallWaveView.OnWaveformStartTimeChanged;
            this.smallWaveView.OnSmallWaveSelectStartTimeChanged += this.ecgCommonWave.OnSmallWaveStartTimeChanged;

            tableLayoutPanel1.MouseClick += (o, e) =>
            {
                if (!_filterStopwatch.IsRunning)
                {
                    _filterStopwatch.Start();
                    _filterClickCount = 1;
                }
                else
                {
                    if (_filterStopwatch.ElapsedMilliseconds < 2000)
                    {
                        _filterClickCount++;
                        if (_filterClickCount >= 5)
                        {
                            this.groupBox4.Visible = true;
                            this.tableLayoutPanel1.ColumnStyles[1].Width = 50;
                            _filterStopwatch.Reset();
                            _filterClickCount = 0;
                        }
                    }
                    else
                    {
                        // 超过间隔，重置计数器
                        _filterStopwatch.Restart();
                        _filterClickCount = 1;
                    }
                }
            };
            this.cbxChanel1.MouseClick += (o, e) => { if (cbxChanel1.IsUnresponsive) ScreenCenterTip.Show("请勾选多通道分析选项"); };
            this.cbxChanel2.MouseClick += (o, e) => { if (cbxChanel2.IsUnresponsive) ScreenCenterTip.Show("请勾选多通道分析选项"); };
        }

        private async void refreshWave(int middleSample)
        {
            QrsInfo qrs = await Task.Run(() =>
            {
                return AEcgHandler.GetFollowingQrsInfo(middleSample);
            });

            //_selectedBeat = AnaValues.cachedAllBeats.Find(o => o.Sample == qrs.Peak);
            int duration = Convert.ToInt32((double)(qrs.Peak - qrs.Start) / _originalConfig.SampleRate * 1000);
            int start = (int)(qrs.Peak - (qrs.Peak - qrs.Start) * 0.4);
            int end = (int)(qrs.Peak + (qrs.End - qrs.Peak) * 0.6);
            _selectedBeat = new BeatInfo(qrs.Peak, duration, 'N', -1) { StartIndex = start, EndIndex = end };

            this.stSettingPlot1.SetBeatSource(_selectedBeat, Convert.ToInt32(this.cbxChanel0.SelectedValue));
            this.stSettingPlot2.SetBeatSource(_selectedBeat, Convert.ToInt32(this.cbxChanel1.SelectedValue));
            this.stSettingPlot3.SetBeatSource(_selectedBeat, Convert.ToInt32(this.cbxChanel2.SelectedValue));
        }

        private void setDeviation(int baseOffset, int jOffset, int stOffset)
        {
            this.lblBaseOffset.Text = _langs["基线点偏移:"] + $" {baseOffset}ms";
            this.lblJOffset.Text = _langs["J点偏移:"] + $" {jOffset}ms";
            this.lblSTOffset.Text = _langs["ST点偏移:"] + $" {stOffset}ms";
            _newConfig.AnalysisSetting.BaseOffset = baseOffset;
            _newConfig.AnalysisSetting.JOffset = jOffset;
            _newConfig.AnalysisSetting.StOffset = stOffset;
        }

        private void showCombobox()
        {
            this.cbxChanel0.Visible = this.cbxChanel1.Visible = this.cbxChanel2.Visible = true;

            this.cbxFrequency.Visible = true;
            this.cbxMyoelectric.Visible = true;
            this.cbxBaseline.Visible = true;

            this.cbxPacemakerType.Visible = true;
            this.cbxPaceMode.Visible = true;
            this.cbxRSensitivity.Visible = true;
        }

        private int[] retrieveSuggestLeads(int[] originalLeads)
        {
            int[] leads = new int[3];
            if (originalLeads != null && originalLeads.Length >= 3)
            {
                leads[0] = originalLeads[0];
                leads[1] = originalLeads[1];
                leads[2] = originalLeads[2];
            }
            if (_originalConfig.StartReanalysis == false
                && (AnaValues.LeadQualities != null && AnaValues.LeadQualities.Count == 3))
            {
                leads[0] = AnaValues.LeadQualities[0];
                leads[1] = AnaValues.LeadQualities[1];
                leads[2] = AnaValues.LeadQualities[2];
            }
            return leads;
        }

        private int[] retrieveChannels()
        {
            int[] channels = new int[3];
            int mainChannel = Convert.ToInt32(this.cbxChanel0.SelectedValue);
            channels[0] = mainChannel;

            bool multiChannel = this.chkMultiChanel.Checked;
            channels[1] = Convert.ToInt32(this.cbxChanel1.SelectedValue);
            channels[2] = Convert.ToInt32(this.cbxChanel2.SelectedValue);
            if (!multiChannel) //单通道
            {
                if (_suggestLeads.Length > 0 && (_suggestLeads[1] == mainChannel || _suggestLeads[2] == mainChannel))
                {
                    IEnumerable<int> reservedChannels = _suggestLeads.Where(o => o != mainChannel);
                    channels[1] = reservedChannels.ElementAt(0);
                    channels[2] = reservedChannels.ElementAt(1);
                }
            }
            return channels;
        }

        public void ResetAdvice()
        {
            examCase caseInfo = ZoncareEntities.Instance.ExamCases.Where(o => o.id == AnaValues.CurrentCase.id).FirstOrDefault();
            if (caseInfo != null)
            {
                caseInfo.holterReportConclusion = "";
                caseInfo.holterAutoReportConclusion = "";
                caseInfo.updateTime = DateTime.Now;
                caseInfo.updateBy = AnaValues.CurrentUser.userName;
                caseInfo.diagnoseTime = DateTime.Now;
            }
            bool result = AppCode.SaveCaseInfo(null, caseInfo, false, DateTime.Now, null, false);
            if (result)
            {
                AnaValues.CurrentCase = PubFuns.CloneObject(caseInfo);
            }
        }

    }
}
