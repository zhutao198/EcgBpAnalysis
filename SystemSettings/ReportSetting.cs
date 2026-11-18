using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.Component.Controls;
using Zoncare.DAL;

namespace SystemSettings
{
    public partial class ReportSetting : UserControl
    {
        private int SignWidth = 150;
        private int SignHeight = 80;
        private string diagnoseSignBase64Image;
        private string auditSignBase64Image;
        public ReportSetting()
        {
            InitializeComponent();
        }

        private void initUI()
        {
            #region 初始化下拉框
            this.displayPageNum.ValueMember = "ItemValue";
            this.displayPageNum.DisplayMember = "ItemText";
            List<ListBoxItem> displayPageNumItems = new List<ListBoxItem>
            {
                new ListBoxItem(1, "显示"),
                new ListBoxItem(0, "隐藏")
            };
            this.displayPageNum.DataSource = displayPageNumItems;
            this.displayPageNum.SelectedValue = 0;

            this.signMode.ValueMember = "ItemValue";
            this.signMode.DisplayMember = "ItemText";
            List<ListBoxItem> signModeItems = new List<ListBoxItem>
            {
                new ListBoxItem(0, "报告医生&审核医生"),
                new ListBoxItem(1, "报告医生")
            };
            this.signMode.DataSource = signModeItems;
            this.signMode.SelectedValue = 0;

            //this.displayConclusion.ValueMember = "ItemValue";
            //this.displayConclusion.DisplayMember = "ItemText";
            //List<ListBoxItem> displayConclusionItems = new List<ListBoxItem>
            //{
            //    new ListBoxItem(1, "显示"),
            //    new ListBoxItem(0, "隐藏")
            //};
            //this.displayConclusion.DataSource = displayConclusionItems;
            //this.displayConclusion.SelectedValue = 0;

            this.displayConclusionType.ValueMember = "ItemValue";
            this.displayConclusionType.DisplayMember = "ItemText";
            List<ListBoxItem> displayConclusionTypeItems = new List<ListBoxItem>
            {
                new ListBoxItem(0, "综述"),
                new ListBoxItem(1, "结论"),
                new ListBoxItem(2, "综述&结论")
            };
            this.displayConclusionType.DataSource = displayConclusionTypeItems;
            this.displayConclusionType.SelectedValue = 0;
            #endregion
        }

        public void loadSettings()
        {
            reportSettingValues settingValues = CommonValues.SystemSettings.reportSets;
            var settingValuesJson = JObject.Parse(JsonConvert.SerializeObject(settingValues, SysConstants.JsonSettings));
            List<TextBoxEx> textBoxList = CommonFuns.RetriveSubControls<TextBoxEx>(this.tableLayoutPanel1);
            foreach (TextBoxEx textBox in textBoxList)
            {
                textBox.Text = settingValuesJson[textBox.Name]?.ToString();
            }
            List<ComboBox> comboboxList = CommonFuns.RetriveSubControls<ComboBox>(this.tableLayoutPanel1);
            foreach (ComboBox comboBox in comboboxList)
            {
                comboBox.SelectedValue = Convert.ToInt32(settingValuesJson[comboBox.Name]?.ToString());
            }
            //List<PictureBox> pictureBoxList = CommonFuns.RetriveSubControls<PictureBox>(this.tableLayoutPanel1);
            //foreach (PictureBox pictureBox in pictureBoxList)
            //{
            //    if (settingValuesJson[pictureBox.Name]?.ToString() != null && settingValuesJson[pictureBox.Name]?.ToString() != "")
            //    {
            //        pictureBox.Image = CommonFuns.Base64ToImage(settingValuesJson[pictureBox.Name]?.ToString());
            //    }
            //}
            //diagnoseSignBase64Image = settingValues.diagnoseSign;

            if (settingValues.auditSign != null && settingValues.auditSign != "")
            {
                this.auditSign.Image = CommonFuns.Base64ToImage(settingValues.auditSign);
                auditSignBase64Image = settingValues.auditSign;
            }

            if (!string.IsNullOrEmpty(user.Loginer.sign))
            {
                this.diagnoseSign.Image = CommonFuns.Base64ToImage(user.Loginer.sign);
                diagnoseSignBase64Image = user.Loginer.sign;
            }
        }

        //private class templateInfo {
        //    public string col1 { get; set; }
        //    public string col2 { get; set; }
        //    public string col3 { get; set; }
        //    public string col4 { get; set; }
        //    public templateInfo(string c1, string c2, string c3, string c4)
        //    {
        //        col1 = c1;
        //        col2 = c2;
        //        col3 = c3;
        //        col4 = c4;

        //    }
        //}

        private void ReportSetting_Load(object sender, EventArgs e)
        {
            initUI();

            if (DesignMode) return;

            loadSettings();
            //DataGridViewColumn[] cols = new DataGridViewColumn[]
            //{
            //    new DataGridViewTextBoxColumn { Name="C1", DataPropertyName="col1" },
            //    new DataGridViewTextBoxColumn { Name="C2", DataPropertyName="col2" },
            //    new DataGridViewTextBoxColumn { Name="C3", DataPropertyName="col3" },
            //    new DataGridViewTextBoxColumn { Name="C4", DataPropertyName="col4" },
            //};
            //this.dataGridView1.Columns.AddRange(cols);

            //List<templateInfo> lst = new List<templateInfo>();
            //lst.Add(new templateInfo("病历号", "姓名", "性别", "年龄"));
            //lst.Add(new templateInfo("身份证号", "出生日期", "患者编号", "病区/床号"));
            //this.dataGridView1.DataSource = new BindingCollection<templateInfo>(lst);
        }

        private void diagnoseSignUploadBtn_Click(object sender, EventArgs e)
        {
            this.saveBtn.Enabled = false;

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                using (Bitmap originalBitmap = new Bitmap(dlg.FileName))
                {
                    //Bitmap processedBitmap = MakeBackgroundTransparent(originalBitmap);

                    Bitmap transparentImage = new Bitmap(SignWidth, SignHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    using (Graphics g = Graphics.FromImage(transparentImage))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                        g.Clear(Color.Transparent);

                        g.DrawImage(originalBitmap, new Rectangle(0, 0, SignWidth, SignHeight));
                    }

                    //processedBitmap.Dispose();

                    diagnoseSign.Image = transparentImage;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        transparentImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        byte[] imageBytes = ms.ToArray();
                        diagnoseSignBase64Image = Convert.ToBase64String(imageBytes);
                    }
                }
            }

            this.saveBtn.Enabled = true;

            //OpenFileDialog dlg = new OpenFileDialog();
            //dlg.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

            //if (dlg.ShowDialog() == DialogResult.OK)
            //{
            //    diagnoseSign.Image = Image.FromFile(dlg.FileName);
            //    // 转换为Base64
            //    diagnoseSignBase64Image = CommonFuns.FileToBase64(dlg.FileName);
            //}
        }

        private void diagnoseSignCancelBtn_Click(object sender, EventArgs e)
        {
            diagnoseSign.Image = null;
            diagnoseSignBase64Image = "";
        }

        private Bitmap MakeBackgroundTransparent(Bitmap bitmap, int tolerance = 50)
        {
            if (bitmap == null || bitmap.Width == 0 || bitmap.Height == 0)
                return bitmap;

            Color backgroundColor = DetectBackgroundColor(bitmap);

            Bitmap result = new Bitmap(SignWidth, SignHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.Clear(Color.Transparent);
                g.DrawImage(bitmap, new Rectangle(0, 0, SignWidth, SignHeight));
            }

            for (int y = 0; y < result.Height; y++)
            {
                for (int x = 0; x < result.Width; x++)
                {
                    Color pixel = result.GetPixel(x, y);

                    int diffR = Math.Abs(pixel.R - backgroundColor.R);
                    int diffG = Math.Abs(pixel.G - backgroundColor.G);
                    int diffB = Math.Abs(pixel.B - backgroundColor.B);

                    if (diffR <= tolerance && diffG <= tolerance && diffB <= tolerance)
                    {
                        result.SetPixel(x, y, Color.Transparent);
                    }
                }
            }

            return result;
        }

        private Color DetectBackgroundColor(Bitmap bitmap)
        {
            List<Color> cornerColors = new List<Color>();

            cornerColors.Add(bitmap.GetPixel(0, 0));
            cornerColors.Add(bitmap.GetPixel(bitmap.Width - 1, 0));
            cornerColors.Add(bitmap.GetPixel(0, bitmap.Height - 1));
            cornerColors.Add(bitmap.GetPixel(bitmap.Width - 1, bitmap.Height - 1));

            int avgR = (int)cornerColors.Average(c => c.R);
            int avgG = (int)cornerColors.Average(c => c.G);
            int avgB = (int)cornerColors.Average(c => c.B);

            return Color.FromArgb(avgR, avgG, avgB);
        }

        private void auditSignUploadBtn_Click(object sender, EventArgs e)
        {
            this.saveBtn.Enabled = false;

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                using (Bitmap originalBitmap = new Bitmap(dlg.FileName))
                {

                    //Bitmap processedBitmap = MakeBackgroundTransparent(originalBitmap);
                    Bitmap transparentImage = new Bitmap(SignWidth, SignHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    using (Graphics g = Graphics.FromImage(transparentImage))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                        g.Clear(Color.Transparent);

                        g.DrawImage(originalBitmap, new Rectangle(0, 0, SignWidth, SignHeight));
                    }

                    //processedBitmap.Dispose();

                    auditSign.Image = transparentImage;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        transparentImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        byte[] imageBytes = ms.ToArray();
                        auditSignBase64Image = Convert.ToBase64String(imageBytes);
                    }
                }
            }

            this.saveBtn.Enabled = true;

            //OpenFileDialog dlg = new OpenFileDialog();
            //dlg.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

            //if (dlg.ShowDialog() == DialogResult.OK)
            //{
            //    auditSign.Image = Image.FromFile(dlg.FileName);
            //    // 转换为Base64
            //    auditSignBase64Image = CommonFuns.FileToBase64(dlg.FileName);
            //}
        }

        private void auditSignCancelBtn_Click(object sender, EventArgs e)
        {
            auditSign.Image = null;
            auditSignBase64Image = "";
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            string errMsg = "";

            try
            {
                reportSettingValues settingValues = CommonValues.SystemSettings.reportSets;
                var settingValuesJson = JObject.Parse(JsonConvert.SerializeObject(settingValues, SysConstants.JsonSettings));
                List<TextBoxEx> textBoxList = CommonFuns.RetriveSubControls<TextBoxEx>(this.tableLayoutPanel1);
                foreach (TextBoxEx textBox in textBoxList)
                {
                    settingValuesJson[textBox.Name] = textBox.Text;
                }
                List<ComboBox> comboboxList = CommonFuns.RetriveSubControls<ComboBox>(this.tableLayoutPanel1);
                foreach (ComboBox comboBox in comboboxList)
                {
                    settingValuesJson[comboBox.Name] = Convert.ToInt32(comboBox.SelectedValue);
                }

                settingValuesJson["diagnoseSign"] = diagnoseSignBase64Image;
                settingValuesJson["auditSign"] = auditSignBase64Image;

                string settingValuesStr = JsonConvert.SerializeObject(settingValuesJson, SysConstants.JsonSettings);
                ZoncareEntities dbEntities = ZoncareEntities.Instance;
                sysSetting sysSettingInfo = dbEntities.SysSettings.Where(o => o.id == 1).FirstOrDefault();
                if (sysSettingInfo != null)
                {
                    sysSettingInfo.reportInfo = settingValuesStr;
                }
                CommonValues.SystemSettings.reportSets = JsonConvert.DeserializeObject<reportSettingValues>(settingValuesStr);

                //更新用户签名
                user loginUser = dbEntities.User.FirstOrDefault(o => o.id == user.Loginer.id);
                if (loginUser != null)
                {
                    loginUser.sign = diagnoseSignBase64Image;
                }

                dbEntities.SaveChanges();
            }
            catch (DataException ex)
            {
                errMsg = ex.Message;
                CommonFuns.PromptError("保存失败");
                return;
            }

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

            reportSettingValues settingValues = new reportSettingValues();
            string settingValuesStr = JsonConvert.SerializeObject(settingValues, SysConstants.JsonSettings);

            ZoncareEntities dbEntities = ZoncareEntities.Instance;
            try
            {
                sysSetting sysSettingInfo = dbEntities.SysSettings.Where(o => o.id == 1).FirstOrDefault();
                if (sysSettingInfo != null)
                {
                    sysSettingInfo.reportInfo = settingValuesStr;
                }

                user loginUser = dbEntities.User.FirstOrDefault(o => o.id == user.Loginer.id);
                if (loginUser != null)
                {
                    loginUser.sign = "";
                }

                dbEntities.SaveChanges();
            }
            catch (DataException ex)
            {
                errMsg = ex.Message;
                CommonFuns.PromptError("操作失败");
                return;
            }
            CommonValues.SystemSettings.reportSets = JsonConvert.DeserializeObject<reportSettingValues>(settingValuesStr);
            this.FindForm().Close();
        }
    }
}
