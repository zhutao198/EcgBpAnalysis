using AnalysisApp.Commons;
using AnalysisSystem.Base;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;

namespace AnalysisApp
{
    public partial class BeatBatchInsertPopup : BaseDialogForm
    {
        private List<BeatInfo> _sourceBeats;
        private char _beatType;
        private int _direction;

        /// <summary>
        /// 用于在当前心搏R波位置之前或之后按指定的距离(ms)插入心搏
        /// </summary>
        /// <param name="beats">当前心搏</param>
        /// <param name="beatType">插入的心搏类型</param>
        /// <param name="title">当前窗口标题</param>
        /// <param name="direction">向前还是向后插入心搏(-1:向前;1:向后)</param>
        public BeatBatchInsertPopup(List<BeatInfo> beats, char beatType, string title, int direction) : base("", title)
        {
            InitializeComponent();

            ShowInTaskbar = false;
            _sourceBeats = beats;
            _beatType = beatType;
            _direction = direction;
        }

        private void BeatBatchAddPopup_Load(object sender, EventArgs e)
        {

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
