using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component.Controls;
using Zoncare.DAL;

namespace SystemSettings
{
    public partial class bpSetting : UserControl
    {
        public bpSetting()
        {
            InitializeComponent();
        }

        private void bpSetting_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                registerEvent();
                loadSettings();
            }
        }

        private void registerEvent()
        {
            List<TextBoxEx> textBoxList = CommonFuns.RetriveSubControls<TextBoxEx>(this.tableLayoutPanel2);
            foreach (TextBoxEx textBox in textBoxList)
            {
                textBox.TextChanged += textBox_TextChanged;
            }
        }

        private void loadSettings()
        {
            bpDiagnoseSettingValues settingValues = CommonValues.SystemSettings.bpSets.bpDiagnoseSets;
            var settingValuesJson = JObject.Parse(JsonConvert.SerializeObject(settingValues, SysConstants.JsonSettings));

            List<TextBoxEx> textBoxList = CommonFuns.RetriveSubControls<TextBoxEx>(this.tableLayoutPanel2);
            foreach (TextBoxEx textBox in textBoxList)
            {
                textBox.Text = settingValuesJson[textBox.Name]?.ToString();
            }

            // 读取DateTimePicker控件数据
            List<DateTimePicker> dataTimeList = CommonFuns.RetriveSubControls<DateTimePicker>(this.tableLayoutPanel2);
            foreach (DateTimePicker dataTime in dataTimeList)
            {
                if (settingValuesJson[dataTime.Name] != null)
                {
                    DateTime dateValue = new DateTime();
                    if (DateTime.TryParse(settingValuesJson[dataTime.Name]?.ToString(), out dateValue))
                    {
                        dataTime.Value = dateValue;
                    }
                }
            }

            if (settingValuesJson["unit"]?.ToString() == "mmHg")
            {
                this.radioMmHg.Checked = true;
                this.radioKPa.Checked = false;
            }
            else
            {
                this.radioMmHg.Checked = false;
                this.radioKPa.Checked = true;
            }
        }

        private void resetBtn_Click(object sender, EventArgs e)
        {
            string errMsg = "";

            bpDiagnoseSettingValues settingValues = new bpDiagnoseSettingValues();

            string settingValuesStr = JsonConvert.SerializeObject(settingValues, SysConstants.JsonSettings);

            ZoncareEntities dbEntities = ZoncareEntities.Instance;
            try
            {
                holterSetting holterSettingInfo = dbEntities.HolterSettings.Where(o => o.userId == user.Loginer.id).FirstOrDefault();
                if (holterSettingInfo != null)
                {
                    holterSettingInfo.bpDiagnoseSetting = settingValuesStr;
                }
            }
            catch (DataException ex)
            {
                errMsg = ex.Message;
            }
            dbEntities.SaveChanges();

            CommonValues.SystemSettings.bpSets.bpDiagnoseSets = JsonConvert.DeserializeObject<bpDiagnoseSettingValues>(settingValuesStr);

            this.FindForm().Close();
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            string errMsg = "";

            bpDiagnoseSettingValues settingValues = CommonValues.SystemSettings.bpSets.bpDiagnoseSets;
            var settingValuesJson = JObject.Parse(JsonConvert.SerializeObject(settingValues, SysConstants.JsonSettings));

            List<TextBoxEx> textBoxList = CommonFuns.RetriveSubControls<TextBoxEx>(this.groupBox2);
            foreach (TextBoxEx textBox in textBoxList)
            {
                settingValuesJson[textBox.Name] = Convert.ToInt32(textBox.Text);
            }

            // 读取DateTimePicker控件数据
            List<DateTimePicker> dataTimeList = CommonFuns.RetriveSubControls<DateTimePicker>(this.tableLayoutPanel2);
            foreach (DateTimePicker dataTime in dataTimeList)
            {
                settingValuesJson[dataTime.Name] = dataTime.Value.ToString("HH:mm");
            }

            if (this.radioMmHg.Checked)
            {
                settingValuesJson["unit"] = "mmHg";
            }
            else
            {
                settingValuesJson["unit"] = "kPa";
            }

            string settingValuesStr = JsonConvert.SerializeObject(settingValuesJson, SysConstants.JsonSettings);

            ZoncareEntities dbEntities = ZoncareEntities.Instance;
            try
            {
                holterSetting holterSettingInfo = dbEntities.HolterSettings.Where(o => o.userId == user.Loginer.id).FirstOrDefault();
                if (holterSettingInfo != null)
                {
                    holterSettingInfo.bpDiagnoseSetting = settingValuesStr;
                    holterSettingInfo.updateTime = DateTime.Now;
                }
            }
            catch (DataException ex)
            {
                errMsg = ex.Message;
            }
            dbEntities.SaveChanges();
            CommonValues.SystemSettings.bpSets.bpDiagnoseSets = JsonConvert.DeserializeObject<bpDiagnoseSettingValues>(settingValuesStr);

            Form parentForm = this.FindForm();
            if (ParentForm != null)
            {
                ParentForm.DialogResult = DialogResult.OK;
                ParentForm.Close();
            }
        }

        #region 控件事件处理
        private void daytimeStart_ValueChanged(object sender, EventArgs e)
        {
            this.nighttimeEnd.Text = this.daytimeStart.Value.ToString("HH:mm");
        }

        private void daytimeEnd_ValueChanged(object sender, EventArgs e)
        {
            this.nighttimeStart.Text = this.daytimeEnd.Value.ToString("HH:mm");
        }


        private void textBox_TextChanged(object sender, EventArgs e)
        {
            TextBoxEx textBox = sender as TextBoxEx;
            if (textBox != null)
            {
                // 处理文本变化逻辑
                string textLabelName = textBox.Name + "Unit";
                List<Label> labelList = CommonFuns.RetriveSubControls<Label>(this.tableLayoutPanel2);
                foreach (Label label in labelList)
                {
                    if(label.Name == textLabelName)
                    {
                        string kPaValue = Math.Round(Convert.ToDouble(textBox.Text) * 0.133322, 1).ToString("F1");
                        label.Text = $"mmHg({kPaValue} kPa)";
                    }
                }
            }
        }
        #endregion
    }
}
