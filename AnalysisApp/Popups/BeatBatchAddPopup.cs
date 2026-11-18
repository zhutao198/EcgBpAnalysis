using AnalysisApp.Commons;
using AnalysisSystem.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;

namespace AnalysisApp
{
    public partial class BeatBatchAddPopup : BaseDialogForm
    {
        private List<BeatInfo> _sourceBeats;
        private readonly Dictionary<char, string> BeatTypes = new Dictionary<char, string>
        {
            { 'N', "正常" },
            { 'V', "室早模板" },
            { 'J', "交界早模板" },
            { 'S', "房早模板" },
            { 'E', "室性逸搏" },
            { 'G', "交界性逸搏" },
            { 'W', "房性逸搏" },
            { 'X', "伪差" },
            { 'A', "房早未下传" }
        };

        //用于在前一个心搏之后按指定的距离(ms)批量插入心搏
        public BeatBatchAddPopup(List<BeatInfo> beats) : base("", "批量插入心搏")
        {
            InitializeComponent();

            ShowInTaskbar = false;
            _sourceBeats = beats;
        }

        private void BeatBatchAddPopup_Load(object sender, EventArgs e)
        {
            //心搏类型下拉框
            this.cbxBeatType.ValueMember = "Value";
            this.cbxBeatType.DisplayMember = "Text";
            List<ListItem> lstItem = new List<ListItem>();
            foreach (char key in BeatTypes.Keys)
            {
                ListItem item = new ListItem(key.ToString(), _langs[BeatTypes[key]] + $"[{key}]");
                lstItem.Add(item);
            }
            this.cbxBeatType.DataSource = lstItem;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.tbxInterval.Text))
            {
                ScreenCenterTip.Show("请输入心搏距离");
                this.tbxInterval.Focus();
                return;
            }

            if (_sourceBeats != null && _sourceBeats.Count > 0)
            {
                int distance = Convert.ToInt32(this.tbxInterval.Text);
                int sampleRate = AnaValues.holterConfig.SampleRate;
                int[] samples = new int[_sourceBeats.Count];

                try
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    char uiBeatType = Convert.ToChar(this.cbxBeatType.SelectedValue);
                    if (uiBeatType != 'A')
                    {
                        for (int i = 0; i < _sourceBeats.Count; i++)
                        {
                            BeatInfo beat = _sourceBeats[i];
                            samples[i] = Convert.ToInt32(beat.Sample - (beat.RRInterval - distance) / 1000d * sampleRate);
                        }
                        //插入心搏
                        int res = AEcgHandler.AddBeats(samples, uiBeatType);
                        if (res != 0)
                        {
                            CommonFuns.PromptError("操作失败");
                            return;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < _sourceBeats.Count; i++)
                        {
                            samples[i] = _sourceBeats[i].Sample;
                        }
                        AnaEvent.AddNcaOrFBeat(samples, distance, "房早未下传");
                    }
                    Logger.LogRuntimeInfo($"批量插入心搏(心搏个数:{_sourceBeats.Count})", sw.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex, "插入心搏");
                    CommonFuns.PromptError("操作失败");
                    return;
                }
            }

            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tbxInterval_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                btnOK_Click(sender, e);
            }
        }

    }
}
