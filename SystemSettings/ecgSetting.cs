using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.Component.Controls;
using Zoncare.DAL;

namespace SystemSettings
{
    public partial class ecgSetting : UserControl
    {
        public ecgSetting()
        {
            InitializeComponent();
        }

        #region 事件
        private void ecgSetting_Load(object sender, EventArgs e)
        {
            initUI();

            if (!DesignMode)
            {
                loadSettings();
            }
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            string errMsg = "";

            ecgSettingValues settingValues = CommonValues.SystemSettings.ecgSets;
            var settingValuesJson = JObject.Parse(JsonConvert.SerializeObject(settingValues, SysConstants.JsonSettings));
            List<ComboBox> comboboxList = CommonFuns.RetriveSubControls<ComboBox>(this.groupBox1);
            foreach (ComboBox comboBox in comboboxList)
            {
                settingValuesJson["filterSets"][comboBox.Name] = Convert.ToInt32(comboBox.SelectedValue);
            }
            List<TextBoxEx> textBoxList2 = CommonFuns.RetriveSubControls<TextBoxEx>(this.groupBox2);
            foreach (TextBoxEx textBox in textBoxList2)
            {
                settingValuesJson["customSets"][textBox.Name] = Convert.ToInt32(textBox.Text);
            }
            List<ComboBox> comboboxList2 = CommonFuns.RetriveSubControls<ComboBox>(this.groupBox2);
            foreach (ComboBox comboBox in comboboxList2)
            {
                settingValuesJson["customSets"][comboBox.Name] = Convert.ToInt32(comboBox.SelectedValue);
            }
            List<CheckBox> listCheckBox = CommonFuns.RetriveSubControls<CheckBox>(this.groupBox2);
            foreach (CheckBox checkBox in listCheckBox)
            {
                settingValuesJson["customSets"][checkBox.Name] = Convert.ToInt32(checkBox.Checked);
            }
            List<TextBoxEx> textBoxList3 = CommonFuns.RetriveSubControls<TextBoxEx>(this.groupBox6);
            foreach (TextBoxEx textBox in textBoxList3)
            {
                settingValuesJson["advanceSets"][textBox.Name] = textBox.Text;
            }
            List<ComboBox> comboboxList3 = CommonFuns.RetriveSubControls<ComboBox>(this.groupBox6);
            foreach (ComboBox comboBox in comboboxList3)
            {
                settingValuesJson["advanceSets"][comboBox.Name] = Convert.ToInt32(comboBox.SelectedValue);
            }

            string settingValuesStr = JsonConvert.SerializeObject(settingValuesJson, SysConstants.JsonSettings);

            ZoncareEntities dbEntities = ZoncareEntities.Instance;
            try
            {
                holterSetting holterSettingInfo = dbEntities.HolterSettings.Where(o => o.userId == user.Loginer.id).FirstOrDefault();
                if (holterSettingInfo != null)
                {
                    holterSettingInfo.ecgSetting = settingValuesStr;
                    holterSettingInfo.updateTime = DateTime.Now;
                }
            }
            catch (DataException ex)
            {
                errMsg = ex.Message;
            }
            dbEntities.SaveChanges();
            CommonValues.SystemSettings.ecgSets = JsonConvert.DeserializeObject<ecgSettingValues>(settingValuesStr);

            Form parentForm = this.FindForm();
            if (ParentForm != null)
            {
                ParentForm.DialogResult = DialogResult.OK;
                ParentForm.Close();
            }
        }

        private void resetBtn_Click(object sender, EventArgs e)
        {
            string errMsg = "";

            ecgSettingValues settingValues = new ecgSettingValues();

            string settingValuesStr = JsonConvert.SerializeObject(settingValues, SysConstants.JsonSettings);

            ZoncareEntities dbEntities = ZoncareEntities.Instance;
            try
            {
                holterSetting holterSettingInfo = dbEntities.HolterSettings.Where(o => o.userId == user.Loginer.id).FirstOrDefault();
                if (holterSettingInfo != null)
                {
                    holterSettingInfo.ecgSetting = settingValuesStr;
                }
            }
            catch (DataException ex)
            {
                errMsg = ex.Message;
            }
            dbEntities.SaveChanges();

            CommonValues.SystemSettings.ecgSets = JsonConvert.DeserializeObject<ecgSettingValues>(settingValuesStr);

            this.FindForm().Close();
        }

        private void STSetBtn_Click(object sender, EventArgs e)
        {
            STSettingForm stSettingForm = new STSettingForm() { ShowInTaskbar = false };
            stSettingForm.ShowDialog();
        }

        #endregion


        private void initUI()
        {
            #region 初始化下拉框
            initFrequencyCombox();
            initMyoelectricCombox();
            initBaselineCombox();
            initTachycardiaCalcCombox();
            initBradycardiaCalcCombox();
            initStModelCombox();
            initAnalysisStepCombox();
            #endregion

            this.longRrPanel.Layout += (o, e) => { this.longRR.Width = this.longRrPanel.Width - this.longRrPanel.CornerRadius * 2 - this.label11.Width; };
            this.asystoleTimePanel1.Layout += (o, e) => { this.asystoleTime.Width = this.asystoleTimePanel1.Width - this.asystoleTimePanel1.CornerRadius * 2 - this.label10.Width; };
            this.senseFailedPanel.Layout += (o, e) => { this.senseFailed.Width = this.senseFailedPanel.Width - this.senseFailedPanel.CornerRadius * 2 - this.label29.Width; };
            this.outputFailedPanel.Layout += (o, e) => { this.outputFailed.Width = this.outputFailedPanel.Width - this.outputFailedPanel.CornerRadius * 2 - this.label30.Width; };
            this.twaMinAvgPanel.Layout += (o, e) => { this.twaMinAvg.Width = this.twaMinAvgPanel.Width - this.twaMinAvgPanel.CornerRadius * 2 - this.label41.Width; };
            this.twaEventDurationPanel.Layout += (o, e) => { this.twaEventDuration.Width = this.twaEventDurationPanel.Width - this.twaEventDurationPanel.CornerRadius * 2 - this.label40.Width; };
        }

        private void initFrequencyCombox()
        {
            this.frequency.ValueMember = "ItemValue";
            this.frequency.DisplayMember = "ItemText";

            List<ListBoxItem> dataFormatItems = new List<ListBoxItem>
            {
                new ListBoxItem(0, "OFF"),
                new ListBoxItem(50, "50Hz"),
                new ListBoxItem(60, "60Hz")
            };
            this.frequency.DataSource = dataFormatItems;
            this.frequency.SelectedValue = 0;
        }

        private void initMyoelectricCombox()
        {
            this.myoelectric.ValueMember = "ItemValue";
            this.myoelectric.DisplayMember = "ItemText";

            List<ListBoxItem> dataFormatItems = new List<ListBoxItem>
            {
                new ListBoxItem(0, "OFF"),
                new ListBoxItem(45, "ON")
            };
            this.myoelectric.DataSource = dataFormatItems;
            this.myoelectric.SelectedValue = 0;
        }

        private void initBaselineCombox()
        {
            this.baseline.ValueMember = "ItemValue";
            this.baseline.DisplayMember = "ItemText";

            List<ListBoxItem> dataFormatItems = new List<ListBoxItem>
            {
                new ListBoxItem(0, "OFF"),
                new ListBoxItem(60, "ON")
            };
            this.baseline.DataSource = dataFormatItems;
            this.baseline.SelectedValue = 0;
        }

        private void initTachycardiaCalcCombox()
        {
            this.tachycardiaCalc.ValueMember = "ItemValue";
            this.tachycardiaCalc.DisplayMember = "ItemText";

            List<ListBoxItem> dataFormatItems = new List<ListBoxItem>
            {
                new ListBoxItem(4, "4"),
                new ListBoxItem(5, "5"),
                new ListBoxItem(6, "6"),
                new ListBoxItem(7, "7")
            };
            this.tachycardiaCalc.DataSource = dataFormatItems;
            this.tachycardiaCalc.SelectedValue = 4;
        }

        private void initBradycardiaCalcCombox()
        {
            this.bradycardiaCalc.ValueMember = "ItemValue";
            this.bradycardiaCalc.DisplayMember = "ItemText";

            List<ListBoxItem> dataFormatItems = new List<ListBoxItem>
            {
                new ListBoxItem(4, "4"),
                new ListBoxItem(5, "5"),
                new ListBoxItem(6, "6"),
                new ListBoxItem(7, "7")
            };
            this.bradycardiaCalc.DataSource = dataFormatItems;
            this.bradycardiaCalc.SelectedValue = 4;
        }

        private void initStModelCombox()
        {
            this.STMode.ValueMember = "ItemValue";
            this.STMode.DisplayMember = "ItemText";

            List<ListBoxItem> dataFormatItems = new List<ListBoxItem>
            {
                new ListBoxItem(0, "绝对ST"),
                new ListBoxItem(1, "相对ST")
            };
            this.STMode.DataSource = dataFormatItems;
            this.STMode.SelectedValue = 0;
        }

        private void initAnalysisStepCombox()
        {
            this.analysisStep.ValueMember = "ItemValue";
            this.analysisStep.DisplayMember = "ItemText";

            List<ListBoxItem> dataFormatItems = new List<ListBoxItem>
            {
                new ListBoxItem(1, "1"),
                new ListBoxItem(2, "2"),
                new ListBoxItem(3, "3"),
                new ListBoxItem(4, "4"),
                new ListBoxItem(5, "5"),
                new ListBoxItem(10, "10"),
                new ListBoxItem(20, "20"),
                new ListBoxItem(30, "30"),
                new ListBoxItem(60, "60"),
                new ListBoxItem(120, "120"),
                new ListBoxItem(180, "180")
            };
            this.analysisStep.DataSource = dataFormatItems;
            this.analysisStep.SelectedValue = 4;
        }

        private void loadSettings()
        {
            ecgSettingValues settingValues = CommonValues.SystemSettings.ecgSets;
            var settingValuesJson = JObject.Parse(JsonConvert.SerializeObject(settingValues, SysConstants.JsonSettings));
            List<ComboBox> comboboxList = CommonFuns.RetriveSubControls<ComboBox>(this.groupBox1);
            foreach (ComboBox comboBox in comboboxList)
            {
                comboBox.SelectedValue = int.Parse(settingValuesJson["filterSets"][comboBox.Name]?.ToString());
            }

            List<TextBoxEx> textBoxList2 = CommonFuns.RetriveSubControls<TextBoxEx>(this.groupBox2);
            foreach (TextBoxEx textBox in textBoxList2)
            {
                textBox.Text = settingValuesJson["customSets"][textBox.Name]?.ToString();
            }
            List<ComboBox> comboboxList2 = CommonFuns.RetriveSubControls<ComboBox>(this.groupBox2);
            foreach (ComboBox comboBox in comboboxList2)
            {
                comboBox.SelectedValue = int.Parse(settingValuesJson["customSets"][comboBox.Name]?.ToString());
            }
            List<CheckBox> listCheckBox = CommonFuns.RetriveSubControls<CheckBox>(this.groupBox2);
            foreach (CheckBox checkBox in listCheckBox)
            {
                checkBox.Checked = Convert.ToBoolean(int.Parse(settingValuesJson["customSets"][checkBox.Name]?.ToString()));
            }
            List<TextBoxEx> textBoxList3 = CommonFuns.RetriveSubControls<TextBoxEx>(this.groupBox6);
            foreach (TextBoxEx textBox in textBoxList3)
            {
                textBox.Text = settingValuesJson["advanceSets"][textBox.Name]?.ToString();
            }
            List<ComboBox> comboboxList3 = CommonFuns.RetriveSubControls<ComboBox>(this.groupBox6);
            foreach (ComboBox comboBox in comboboxList3)
            {
                comboBox.SelectedValue = int.Parse(settingValuesJson["advanceSets"][comboBox.Name]?.ToString());
            }
        }
    }
}
