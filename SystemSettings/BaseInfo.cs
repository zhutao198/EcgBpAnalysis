using AnalysisSystem.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.Component.Controls;
using Zoncare.DAL;

namespace SystemSettings
{
    public partial class BaseInfo : UserControl
    {
        private string hospitalLogoBase64Image;
        private baseSettingValues originalSettings;

        public event Action OnBaseInfoSaved;

        public BaseInfo()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
        }

        private void BaseInfo_Load(object sender, EventArgs e)
        {
            try
            {
                initUI();

                if (!DesignMode)
                {
                    loadSettings();
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "系统设置");
            }
        }

        private void languageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void dataFormatComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void dateFormat_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void uploadBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                hospitalLogo.Image = Image.FromFile(dlg.FileName);
                // 转换为Base64
                hospitalLogoBase64Image = CommonFuns.FileToBase64(dlg.FileName);

            }
        }

        private void clearBtn_Click(object sender, EventArgs e)
        {
            hospitalLogo.Image = null;
            hospitalLogoBase64Image = "";
        }


        private void saveBtn_Click(object sender, EventArgs e)
        {
            bool languageChanged = false;
            try
            {
                JObject settingValuesJson = JObject.Parse(JsonConvert.SerializeObject(originalSettings, SysConstants.JsonSettings));
                List<TextBoxEx> textBoxList = CommonFuns.RetriveSubControls<TextBoxEx>(this.baseInfoPanel);
                foreach (TextBoxEx textBox in textBoxList)
                {
                    string tbxText = textBox.Text.Trim();
                    if (textBox == this.holterDataPath) tbxText = tbxText.Replace("/", "\\");

                    settingValuesJson[textBox.Name] = tbxText;
                }

                List<ComboBox> comboboxList = CommonFuns.RetriveSubControls<ComboBox>(this.baseInfoPanel);
                foreach (ComboBox comboBox in comboboxList)
                {
                    settingValuesJson[comboBox.Name] = Convert.ToInt32(comboBox.SelectedValue);
                }
                settingValuesJson["hospitalLogo"] = hospitalLogoBase64Image;

                string settingValuesStr = JsonConvert.SerializeObject(settingValuesJson, SysConstants.JsonSettings);
                sysSetting sysSettingInfo = ZoncareEntities.Instance.SysSettings.Where(o => o.id == 1).FirstOrDefault();
                if (sysSettingInfo != null)
                {
                    sysSettingInfo.baseInfo = settingValuesStr;
                }
                ZoncareEntities.Instance.SaveChanges();
                CommonValues.SystemSettings.baseSets = JsonConvert.DeserializeObject<baseSettingValues>(settingValuesStr);

                //更新配置文件中区域语言代码
                int lang = CommonValues.SystemSettings.baseSets.language;
                if (lang != CommonValues.LanguageID)
                {
                    AppCode.UpdateSystemSetting("CultureInfo", lang.ToString());
                    CommonValues.LanguageID = lang;
                    languageChanged = true;
                }

                OnBaseInfoSaved?.Invoke();

                Form parentForm = this.FindForm();
                if (ParentForm != null)
                {
                    ParentForm.DialogResult = DialogResult.OK;
                    ParentForm.Close();
                }
                if (languageChanged) { ScreenCenterTip.Show("保存成功，请重启软件以使设置生效"); }
            }
            catch (Exception ex)
            {
                CommonFuns.PromptError("操作失败");
                Logger.CreateLog(ex, "系统设置");
            }
        }

        private void resetBtn_Click(object sender, EventArgs e)
        {
            try
            {
                baseSettingValues settingValues = new baseSettingValues();
                string settingValuesStr = JsonConvert.SerializeObject(settingValues, SysConstants.JsonSettings);

                sysSetting sysSettingInfo = ZoncareEntities.Instance.SysSettings.Where(o => o.id == 1).FirstOrDefault();
                if (sysSettingInfo != null)
                {
                    sysSettingInfo.baseInfo = settingValuesStr;
                }

                ZoncareEntities.Instance.SaveChanges();

                CommonValues.SystemSettings.baseSets = JsonConvert.DeserializeObject<baseSettingValues>(settingValuesStr);
                this.FindForm().Close();

                if (settingValues.language != originalSettings.language)
                {
                    //更新配置文件中区域语言代码
                    AppCode.UpdateSystemSetting("CultureInfo", settingValues.language.ToString());
                    CommonValues.LanguageID = settingValues.language;
                    ScreenCenterTip.Show("保存成功，请重启软件以使设置生效");
                }
            }
            catch (DataException ex)
            {
                CommonFuns.PromptError("操作失败");
                Logger.CreateLog(ex, "系统设置");
            }
        }


        private void initUI()
        {
            //初始化下拉框选项
            fillLanguageCombobox();
            fillDataFormatComboBox();
        }

        private void loadSettings()
        {
            //CommonValues.SystemSettings = ZqQuery.loadSettings(user.Loginer.id);
            originalSettings = CommonValues.SystemSettings.baseSets;

            var settingValuesJson = JObject.Parse(JsonConvert.SerializeObject(originalSettings, SysConstants.JsonSettings));
            List<TextBoxEx> textBoxList = CommonFuns.RetriveSubControls<TextBoxEx>(this.baseInfoPanel);
            foreach (TextBoxEx textBox in textBoxList)
            {
                textBox.Text = settingValuesJson[textBox.Name]?.ToString();
            }
            List<ComboBox> comboboxList = CommonFuns.RetriveSubControls<ComboBox>(this.baseInfoPanel);
            foreach (ComboBox comboBox in comboboxList)
            {
                comboBox.SelectedValue = int.Parse(settingValuesJson[comboBox.Name]?.ToString());
            }
            List<PictureBox> pictureBoxList = CommonFuns.RetriveSubControls<PictureBox>(this.baseInfoPanel);
            foreach (PictureBox pictureBox in pictureBoxList)
            {
                if (settingValuesJson[pictureBox.Name]?.ToString() != null && settingValuesJson[pictureBox.Name]?.ToString() != "")
                {
                    pictureBox.Image = CommonFuns.Base64ToImage(settingValuesJson[pictureBox.Name]?.ToString());
                }
            }
            hospitalLogoBase64Image = originalSettings.hospitalLogo;
        }

        private void fillLanguageCombobox()
        {
            this.language.ValueMember = "ItemValue";
            this.language.DisplayMember = "ItemText";

            List<ListBoxItem> languageItems = new List<ListBoxItem>
            {
                new ListBoxItem(2052, "中文简体"),
                new ListBoxItem(1033, "English"),
                //new ListBoxItem(3082, "Español"),
                //new ListBoxItem(1049, "Русский"),
                //new ListBoxItem(1036, "Français"),
                //new ListBoxItem(2070, "Português"),
                //new ListBoxItem(1054, "ไทย"),
                //new ListBoxItem(1038, "Magyar")
            };
            this.language.DataSource = languageItems;
            this.language.SelectedIndexChanged += languageComboBox_SelectedIndexChanged;
            this.language.SelectedValue = CommonValues.LanguageID;
        }

        private void fillDataFormatComboBox()
        {
            this.dateFormat.ValueMember = "ItemValue";
            this.dateFormat.DisplayMember = "ItemText";

            List<ListBoxItem> dataFormatItems = new List<ListBoxItem>
            {
                new ListBoxItem(0, "Y-M-D"),
                new ListBoxItem(1, "D-M-Y")
            };
            this.dateFormat.DataSource = dataFormatItems;
            this.dateFormat.SelectedIndexChanged += dataFormatComboBox_SelectedIndexChanged;
            this.dateFormat.SelectedValue = 0;
        }

       
    }
}
