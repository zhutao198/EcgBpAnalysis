using AnalysisApp.Commons;
using AnalysisSystem.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;
using static AnalysisApp.Commons.AEcgHandler;

namespace AnalysisApp
{
    public partial class AddSTEventForm : BaseDialogForm
    {
        // 定义事件，用于通知ST列表刷新
        public event Action OnRefreshStTable;

        public AddSTEventForm() : base("", "新增")
        {
            InitializeComponent();
        }

        private void AddSTEventForm_Load(object sender, EventArgs e)
        {
            initControl();
        }

        private void initControl()
        {
            int dateFormat = CommonValues.SystemSettings.baseSets.dateFormat;
            if (dateFormat == 0)
            {
                this.pickerStartTime.CustomFormat = "MM-dd HH:mm:ss";
            }
            else
            {
                this.pickerStartTime.CustomFormat = "dd-MM HH:mm:ss";
            }

            if (!DateTime.TryParse(AnaValues.holterConfig.StartTime, out DateTime startTime))
            {
                startTime = DateTime.Today;
            }
            this.pickerStartTime.Value = startTime;
            fillLeadComboBox(this.cmbLead);
        }

        private void fillLeadComboBox(ComboBox cbx)
        {
            cbx.ValueMember = "Value";
            cbx.DisplayMember = "Text";

            List<ListItem> items = new List<ListItem>();
            string leads = SysConstants.ECG_LT_STANDARD[1];
            if (AnaValues.holterConfig.Lead == 3) leads = SysConstants.ECG_LT_STANDARD[5];
            string[] arrLead = leads.Split(',');
            for (int i = 0; i < arrLead.Length; i++)
            {
                items.Add(new ListItem(i, arrLead[i]));
            };
            cbx.DataSource = items;
            cbx.SelectedValue = AnaValues.holterConfig.MainChannel;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.tbxDuration.Text))
            {
                ScreenCenterTip.Show("请输入");
                this.tbxDuration.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(this.tbxSlope.Text))
            {
                ScreenCenterTip.Show("请输入");
                this.tbxSlope.Focus();
                return;
            }

            int StartSample = AnaCode.getSampleFromDateTime(this.pickerStartTime.Value);
            int EndSample = StartSample + Convert.ToInt32(Convert.ToDouble(this.tbxDuration.Text) * 60 * AnaValues.holterConfig.SampleRate);
            AEcgStEventInfo stEvent = new AEcgStEventInfo();
            stEvent.statusFlag = 0;
            stEvent.lead = Convert.ToInt32(this.cmbLead.SelectedValue);
            stEvent.startPosition = StartSample;
            stEvent.durationTime = Convert.ToInt32(Convert.ToDouble(this.tbxDuration.Text) * 60 * AnaValues.holterConfig.SampleRate);
            stEvent.stPointVoltage = (float)(Convert.ToDouble(this.tbxSlope.Text) * 1000);
            stEvent.stSlope = stEvent.stPointVoltage == 0 ? 0 : (stEvent.stPointVoltage > 0 ? 1 : 2);

            List<BeatInfo> cachedAllBeats = AnaValues.CachedAllBeats;
            List<BeatInfo> relevantHeartbeats = cachedAllBeats
                .Where(h => (h.EndIndex >= StartSample && h.StartIndex < EndSample) && !AnaCode.IsInvalidBeat(h))
                .OrderBy(h => h.StartIndex)
                .ToList();

            stEvent.hrAvg = AnaEvent.CalcAvgHR(relevantHeartbeats, false);

            List<AnaEvent.EventInfo> fastestHr = AnaEvent.CalcFastestSlowestHrEvent(relevantHeartbeats, AnaEvent.EventType.fastestHr);
            if (fastestHr.Count == 0)
            {
                stEvent.hrMax = 0;
            }
            else
            {
                stEvent.hrMax = fastestHr[0].AvgHr;
            }

            List<AnaEvent.EventInfo> slowestHr = AnaEvent.CalcFastestSlowestHrEvent(relevantHeartbeats, AnaEvent.EventType.slowestHr);
            if (slowestHr.Count == 0)
            {
                stEvent.hrMin = 0;
            }
            else
            {
                stEvent.hrMin = slowestHr[0].AvgHr;
            }

            int ret = AddSTEvent(stEvent);
            if (ret == 0)
            {
                OnRefreshStTable?.Invoke();
                this.Close();
            }
            else
            {
                ScreenCenterTip.Show("输入参数错误");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
