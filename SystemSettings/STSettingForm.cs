using AnalysisSystem.Base;
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
    public partial class STSettingForm : BaseDialogForm
    {
        public STSettingForm() : base("", "12导ST异常标准设置")
        {
            InitializeComponent();
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void STSettingForm_Load(object sender, EventArgs e)
        {
            this.loadSettings();
        }

        public void loadSettings()
        {
            //CommonValues.SystemSettings = ZqQuery.loadSettings(user.Loginer.id);
            STUpFallValues settingValues = CommonValues.SystemSettings.ecgSets.stUpFallSets;
            var settingValuesJson = JObject.Parse(JsonConvert.SerializeObject(settingValues, SysConstants.JsonSettings));
            List<TextBoxEx> textBoxList = CommonFuns.RetriveSubControls<TextBoxEx>(this.tableLayoutPanel1);
            foreach (TextBoxEx textBox in textBoxList)
            {
                textBox.Text = settingValuesJson[textBox.Name]?.ToString();
            }
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            string errMsg = "";

            ecgSettingValues settingValues = CommonValues.SystemSettings.ecgSets;
            var settingValuesJson = JObject.Parse(JsonConvert.SerializeObject(settingValues, SysConstants.JsonSettings));
            List<TextBoxEx> textBoxList = CommonFuns.RetriveSubControls<TextBoxEx>(this.tableLayoutPanel1);
            foreach (TextBoxEx textBox in textBoxList)
            {
                settingValuesJson["stUpFallSets"][textBox.Name] = textBox.Text;
            }

            string settingValuesStr = JsonConvert.SerializeObject(settingValuesJson, SysConstants.JsonSettings);

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

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
