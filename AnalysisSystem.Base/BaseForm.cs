using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.Component.Controls;
using Zoncare.DAL;
using Zoncare.Utilities;

namespace AnalysisSystem.Base
{
    public partial class BaseForm : Form
    {
        protected ZoncareEntities _MyEntities;
        protected PersistDictionary<string> _langs;
        public Action Onclosed;
        public Action OnLogout;
        public bool IsLoaded { get; private set; }
        public bool IsLoggingOut { get; protected set; }
        public bool ShowAfterLoad { get; set; } = true;

        private string mainIconPath = PubFuns.GetAbsolutePath(@"images\icon\holter.ico");

        public BaseForm()
        {
            InitializeComponent();

            if (File.Exists(mainIconPath)) { this.Icon = new Icon(mainIconPath); }
        }

        private void BaseForm_Load(object sender, EventArgs e)
        {
            if (IsLoaded) return;

            //this.Visible = false;
            _langs = CommonValues.Langs;
            _MyEntities = ZoncareEntities.Instance;
            this.AutoScaleMode = AutoScaleMode.None;


            initControls();
            initWindowState();

            //if (!DesignMode)
            //{
            //    if (PubFuns.IsAttributeDefined(this, typeof(ECG_SAMPLEING), true))
            //    {
            //        try
            //        {
            //            //ECG_preprocess.Deinit();
            //            //ECG_diagnose.Deinit();
            //            //Thread.Sleep(50);
            //            ECG_preprocess.Init(CommonValues.EcgSettings);
            //            ECG_diagnose.Init(CommonValues.EcgSettings);
            //            Thread.Sleep(50);

            //            //var settings = CommonValues.EcgSettings;
            //            //int n2 = ECG_preprocess.ecgPreprocessSetFilter(CommonFuns.ConvertParaValue(settings.EcgBaselineDrift), CommonFuns.ConvertParaValue(settings.EcgFrequency), CommonFuns.ConvertParaValue(settings.EcgEMG));
            //        }
            //        catch (Exception ex)
            //        {
            //            Logger.CreateLog(ex, "心电数据采集");
            //        }
            //    }
            //}

            if (!this.Visible && ShowAfterLoad) this.Visible = true;

            IsLoaded = true;
        }

        private void BaseForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //关闭子窗口
            FormCollection childForms = Application.OpenForms;
            for (int i = childForms.Count - 1; i-- > 0;)
            {
                if (childForms[i].Owner == this) childForms[i].Close();
            }
        }

        private void BaseForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            List<ToolStripEx> toolStripList = CommonFuns.RetriveSubControls<ToolStripEx>(this);
            if (toolStripList.Count > 0)
            {
                foreach (ToolStripEx toolStrip in toolStripList)
                {
                    if (toolStrip.OriginalImages != null)
                    {
                        foreach (Image img in toolStrip.OriginalImages.Values)
                        {
                            img?.Dispose();
                        }
                    }
                    if (toolStrip.DisabledImages != null)
                    {
                        foreach (Image img in toolStrip.DisabledImages.Values)
                        {
                            img?.Dispose();
                        }
                    }
                }
            }

            //if (PubFuns.IsAttributeDefined(this, typeof(ECG_SAMPLEING), true))
            //{
            //    PubFuns.DelayDoWork(() =>
            //    {
            //        ECG_preprocess.Deinit();
            //        ECG_diagnose.Deinit();
            //        return true;
            //    }, 200, 1, null);
            //}

            Onclosed?.Invoke();
        }

        private void BaseForm_Shown(object sender, EventArgs e)
        {
            if (!this.DesignMode)
            {
                if (_MyEntities == null) _MyEntities = ZoncareEntities.Instance;

                this.SuspendLayout();
                //替换对应于表格各列表头文本的词条
                List<DataGridView> lstDataGridView = CommonFuns.RetriveSubControls<DataGridView>(this, false, false);
                foreach (DataGridView dgv in lstDataGridView)
                {
                    DataGridViewColumn lastVisibleCol = null;
                    foreach (DataGridViewColumn col in dgv.Columns)
                    {
                        string headerText = col.HeaderText;
                        if (CommonValues.Langs.ContainsKey(headerText))
                        {
                            col.HeaderText = CommonValues.Langs[headerText];
                            col.AutoSizeMode = col.AutoSizeMode;
                        }
                        if (col.Visible) lastVisibleCol = col;
                    }
                }

                List<ComboBox> lstCombobox = CommonFuns.RetriveSubControls<ComboBox>(this, false, false);
                foreach (ComboBox cbx in lstCombobox)
                {
                    //调整下拉框中的下拉选项宽度
                    cbx.DropDownWidth = CommonFuns.GetMaxWidthOfComboxItems(cbx, 0);

                    //当文本长度超出文本显示区域时, 显示其长文本提示
                    cbx.SelectedIndexChanged += cbx_SelectedIndexChanged;
                    cbx_SelectedIndexChanged(cbx, null);

                    //针对禁用的或无选中项的下拉框处理其提示信息
                    Control parentCtl = cbx.Parent;
                    if (parentCtl != null)
                    {
                        parentCtl.MouseMove += parentCtl_MouseMove;
                        parentCtl.MouseLeave += parentCtl_MouseLeave;
                    }

                    ZoncareComboBox zqCombobox = cbx as ZoncareComboBox;
                    if (zqCombobox != null) zqCombobox.IsShown = true;
                }

                this.ResumeLayout();
            }
        }

        //初始化控件显示
        private void initControls()
        {
            //指定分页控件的语言词条
            List<ZoncarePage> lstPageCtl = CommonFuns.RetriveSubControls<ZoncarePage>(this);
            foreach (ZoncarePage pager in lstPageCtl)
            {
                pager.Langs = CommonValues.Langs;
            }

            //获取工具栏各项的图标
            List<ToolStrip> lstToolStrip = CommonFuns.RetriveSubControls<ToolStrip>(this);
            foreach (ToolStrip ts in lstToolStrip)
            {
                //ts.LayoutStyle = ToolStripLayoutStyle.Flow;
                //using (Graphics g = ts.CreateGraphics())  // Graphics.FromHwnd(IntPtr.Zero)
                {
                    foreach (ToolStripItem item in ts.Items)
                    {
                        if (item is ToolStripSeparator) continue;
                        item.AutoSize = false;
                        if (CommonValues.LanguageID == 2052)
                        {
                            item.Width = 78;
                        }
                        else
                        {
                            item.Width = 102;

                            ////调整工具栏项使其不至于太宽
                            //using (StringFormat sf = StringFormat.GenericTypographic)
                            //{
                            //    sf.FormatFlags = StringFormatFlags.NoWrap;
                            //    sf.Trimming = StringTrimming.Word;

                            //    Rectangle destRect = new Rectangle(item.ContentRectangle.X, item.ContentRectangle.Y, Math.Min(item.ContentRectangle.Width, 120), item.ContentRectangle.Height);

                            //    int maxTextLength = CommonFuns.GetMaxLengthOfTextArea(g, item.Text, item.Font, destRect, sf, out float maxLineWidth);
                            //    if (item.Text.Length > maxTextLength) item.Text = item.Text.Substring(0, maxTextLength) + "\n" + item.Text.Substring(maxTextLength);
                            //}
                        }

                        if (item.Image == null && !string.IsNullOrEmpty(item.Name))
                        {
                            string imgName = item.Name.Substring(3).ToLower();
                            item.Image = CommonFuns.GetImageFromFile(PubFuns.GetBaseDirectory() + "\\images\\" + imgName + ".png", false, false, 32, 32);
                        }
                        int preferredHeight = item.Height;
                        if (item.Width < 60)
                        {
                            item.Width = 60;
                            //item.Height = preferredHeight;
                        }
                    }
                }
            }

            if (CommonValues.LanguageID != 2052)
            {
                List<RoundButton> listBtns = CommonFuns.RetriveSubControls<RoundButton>(this);
                foreach (RoundButton btn in listBtns)
                {
                    if (btn.Name == "btnBrowse" && btn.Text == "...") continue;
                    if (this.Name == "LoginForm") continue;//登录界面的按钮不自动调整宽度

                    Control parentCtl = CommonFuns.GetSpecifiedParent(btn, typeof(ZoncarePage));
                    if (parentCtl == null)
                    {
                        btn.AutoSize = true;
                        btn.MinimumSize = new Size(84, 0);
                        btn.MaximumSize = new Size(0, 28);
                    }
                }
                this.Hide();
            }

            //窗体加载后，对窗体实现多语言处理
            if (!this.DesignMode)
            {
                UIFuns.LoadControlText(this);
            }

            //使窗口自适应操作系统当前分辨率
            if (CommonValues.WidthScale != 1 || CommonValues.HeightScale != 1)
            {
                UIFuns.SetControls(this, adjustParent: true);
            }
        }

        //初始化窗体显示
        private void initWindowState()
        {
            if (this.Owner != null &&
               (this.StartPosition == FormStartPosition.CenterScreen || this.StartPosition == FormStartPosition.CenterParent))
            {
                this.Left = this.Owner.Left + (this.Owner.ClientSize.Width - this.ClientSize.Width) / 2;
                this.Top = this.Owner.Top + (this.Owner.ClientSize.Height - this.ClientSize.Height) / 2;
            }

            string iconPath = Application.StartupPath + @"\images\icon\popup.ico";
            if (this.Name == "UserManagement" || this.Name == "SoftwareSettings")
            {
                if (File.Exists(mainIconPath)) { this.Icon = new Icon(mainIconPath); }

                //this.TopMost = true;
                ////Thread.Sleep(1);
                ////Application.DoEvents();
                //this.TopMost = false;
                //this.ShowInTaskbar = true;
                //this.BringToFront();
            }
            else if (this.WindowState != FormWindowState.Maximized && this.Name != "LoginForm")
            {
                if (File.Exists(iconPath)) { this.Icon = new Icon(iconPath); }
            }
            else
            {
                if (File.Exists(mainIconPath)) { this.Icon = new Icon(mainIconPath); }
            }
        }


        #region 下拉框处理
        private void cbx_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender == null) return;

            ComboBox cbx = sender as ComboBox;
            if (cbx.Enabled && shouldShowTip(cbx))
            {
                this.toolTip1.SetToolTip(cbx, cbx.Text);
            }
            else
            {
                this.toolTip1.SetToolTip(cbx, "");
            }
        }

        private void parentCtl_MouseMove(object sender, MouseEventArgs e)
        {
            bool tipShown = false;
            Control parent = sender as Control;
            Control subCtl = parent.GetChildAtPoint(e.Location);
            if (subCtl != null)
            {
                if (subCtl is ComboBox && subCtl.Visible)
                {
                    ComboBox cbx = subCtl as ComboBox;
                    if (shouldShowTip(cbx))
                    {
                        if (string.IsNullOrEmpty(toolTip1.GetToolTip(cbx)))
                        {
                            toolTip1.Show(cbx.Text, cbx, cbx.Width / 2, cbx.Height + 1);
                            toolTip1.Tag = cbx;
                        }
                        tipShown = true;
                    }
                }
            }

            if (!tipShown && toolTip1.Tag != null)
            {
                Control ctrl = toolTip1.Tag as Control;
                toolTip1.Hide(ctrl);
            }
        }

        private void parentCtl_MouseLeave(object sender, EventArgs e)
        {
            if (toolTip1.Tag != null)
            {
                ComboBox cbx = toolTip1.Tag as ComboBox;
                //if (cbx.ClientRectangle.Contains(cbx.PointToClient(Cursor.Position))) { return; }

                toolTip1.Hide(cbx);
            }
        }

        private bool shouldShowTip(ComboBox cbx)
        {
            bool needShowTip = false;
            Size itemTextSize;
            int textAreaWidth;
            using (Graphics g = cbx.CreateGraphics())
            {
                g.PageUnit = GraphicsUnit.Pixel;
                itemTextSize = TextRenderer.MeasureText(g, cbx.Text, cbx.Font);
            }

            textAreaWidth = (cbx.DropDownStyle == ComboBoxStyle.Simple ? cbx.Width - 2 : cbx.Width - 16);
            if (itemTextSize.Width > textAreaWidth)
            {
                needShowTip = true;
            }

            return needShowTip;
        }
        #endregion

    }
}
