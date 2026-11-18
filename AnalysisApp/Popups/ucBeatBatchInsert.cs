using AnalysisApp.Commons;
using System;
using System.Collections.Generic;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.Component.Widgets;

namespace AnalysisApp.Popups
{
    public partial class ucBeatBatchInsert : InputPanel
    {
        private List<BeatInfo> _sourceBeats;
        private char _beatType;
        private int _direction;

        public ucBeatBatchInsert(List<BeatInfo> beats, char beatType, string title, int direction) : base("", title)
        {
            InitializeComponent();
            _sourceBeats = beats;
            _beatType = beatType;
            _direction = direction;
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
                    for (int i = 0; i < _sourceBeats.Count; i++)
                    {
                        BeatInfo beat = _sourceBeats[i];
                        if (_direction == -1)
                            samples[i] = Convert.ToInt32(beat.Sample - distance / 1000d * sampleRate);
                        else
                            samples[i] = Convert.ToInt32(beat.Sample + distance / 1000d * sampleRate);
                    }

                    //插入心搏
                    int res = AEcgHandler.AddBeats(samples, _beatType);
                    if (res != 0)
                    {
                        CommonFuns.PromptError("操作失败");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex, "插入心搏");
                    CommonFuns.PromptError("操作失败");
                    return;
                }
            }

            //this.DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

        }
    }
}
