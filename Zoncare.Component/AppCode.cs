using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Zoncare.Common;
using Zoncare.Component.Controls;
using Zoncare.Utilities;

namespace Zoncare.Component
{
    public enum RoundStyle
    {
        None,
        Top,
        Bottom,
        Left,
        Right,
        All
    }

    public class UIFuns
    {
        /// <summary>
        /// copy the data from one object to another one of which the type is the same
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ToObject"></param>
        /// <param name="FromObject"></param>
        public static void ClonePropertyValues<T>(T FromObject, T ToObject)
        {
            Type objectType = ToObject.GetType();
            foreach (PropertyInfo pinfo in objectType.GetProperties())
            {
                if (pinfo.CanWrite) pinfo.SetValue(ToObject, pinfo.GetValue(FromObject, null), null);
            }
        }

        /// <summary>
        /// 加载控件对应于当前语言的相关文本
        /// </summary>
        /// <param name="parent">父级控件</param>
        public static void LoadControlText(Control parent)
        {
            List<Control> lstControl = CommonFuns.RetriveSubControls<Control>(parent, false, true);
            lstControl.Sort(new ControlResizeComparer(ListSortDirection.Descending)); //Form排在最前

            Font changedParentFont = null;
            foreach (Control con in lstControl)
            {
                if ((con is Label) || (con is CheckBox) || (con is RadioButton) || (con is Button)
                    || (con is GroupBox) || (con is Form))
                {
                    con.Text = CommonValues.Langs[con.Text];
                    if (changedParentFont != null)
                    {
                        con.Font = new Font(changedParentFont, con.Font.Style);
                        if (con is Button && con.Height < con.PreferredSize.Height) con.Height = con.PreferredSize.Height - 2;
                    }

                    if (con is Form || con is UserControl)
                    {
                        //if (CommonValues.LanguageID == 2052) //对中文版本应用指定的窗口字体
                        //{
                        //    changedParentFont = SetUniformFont(con as Form);
                        //}
                        //else
                        //{
                        //    changedParentFont = new Font("Calibri ", 12);
                        //}

                        FieldInfo[] fieldInfo = con.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                        for (int i = 0; i < fieldInfo.Length; i++)
                        {
                            switch (fieldInfo[i].FieldType.Name)
                            {
                                //ToolStrip toolstrip = ToolStripManager.FindToolStrip("contextMenuStrip1");
                                case "ContextMenuStrip":
                                    ContextMenuStrip contextMenuStrip = (ContextMenuStrip)fieldInfo[i].GetValue(con);
                                    foreach (ToolStripItem tsItem in contextMenuStrip.Items)
                                    {
                                        if (CommonValues.Langs.ContainsKey(tsItem.Text))
                                        {
                                            if (changedParentFont != null) tsItem.Font = new Font(changedParentFont, tsItem.Font.Style);
                                            tsItem.Text = CommonValues.Langs[tsItem.Text];
                                        }
                                        //tsItem.DropDownItems 显示文本在界面代码中处理
                                    }
                                    break;
                                case "Timer":
                                    //System.Windows.Forms.Timer timer = (System.Windows.Forms.Timer)fieldInfo[i].GetValue(con);
                                    break;
                            }
                        }
                        con.Text = CommonValues.Langs[con.Text];
                    }
                }

                if (con is TabControl)
                {
                    TabControl tabCtl = con as TabControl;
                    foreach (TabPage tp in tabCtl.TabPages)
                    {
                        if (CommonValues.Langs.ContainsKey(tp.Text))
                        {
                            if (changedParentFont != null) tp.Font = new Font(changedParentFont, tp.Font.Style);
                            tp.Text = CommonValues.Langs[tp.Text];
                        }
                    }
                }

                if (con is ToolStrip) //ToolStripLabel, ToolStripButton
                {
                    ToolStrip tsCtl = con as ToolStrip;
                    foreach (ToolStripItem tsItem in tsCtl.Items)
                    {
                        if (CommonValues.Langs.ContainsKey(tsItem.Text))
                        {
                            if (changedParentFont != null) tsItem.Font = new Font(changedParentFont, tsItem.Font.Style);
                            tsItem.Text = CommonValues.Langs[tsItem.Text];
                            tsItem.ToolTipText = CommonValues.Langs[tsItem.ToolTipText];
                        }
                    }
                }

                if (con is Chart)
                {
                    Chart chartCtl = con as Chart;
                    foreach (Series seriesItem in chartCtl.Series)
                    {
                        string legendText = seriesItem.LegendText;
                        if (CommonValues.Langs.ContainsKey(legendText))
                        {
                            if (changedParentFont != null) seriesItem.Font = new Font(changedParentFont, seriesItem.Font.Style);
                            seriesItem.LegendText = CommonValues.Langs[legendText];
                        }
                    }
                }

                //if (con is DataGridView)
                //{
                //    DataGridView dgv = con as DataGridView;
                //    foreach (DataGridViewColumn col in dgv.Columns)
                //    {
                //        string headerText = col.HeaderText;
                //        if (StaticValues.Langs.ContainsKey(headerText))
                //        {
                //            col.HeaderText = StaticValues.Langs[headerText];
                //            col.AutoSizeMode = col.AutoSizeMode;
                //        }
                //    }
                //}

                //特殊处理以校正其Anchor属性的效果
                if (con.Parent is TableLayoutPanel && (con is ComboBox || con is DateTimePicker))
                {
                    AnchorStyles originalAnchor = con.Anchor;
                    DockStyle originalDock = con.Dock;
                    if (con.Dock != DockStyle.Fill)
                    {
                        con.Dock = DockStyle.Fill;
                        con.Dock = originalDock;
                        con.Anchor = originalAnchor;
                    }
                    else
                    {
                        con.Anchor = AnchorStyles.Left;
                    }
                }

                if (changedParentFont != null) con.Font = new Font(changedParentFont, con.Font.Style);
            }
        }

        public static void LoadControlsText(List<Control> controlList)
        {
            Font changedParentFont = null;
            foreach (Control con in controlList)
            {
                if ((con is Label) || (con is CheckBox) || (con is RadioButton) || (con is Button)
                    || (con is GroupBox) || (con is Form))
                {
                    con.Text = CommonValues.Langs[con.Text];
                    if (changedParentFont != null)
                    {
                        con.Font = new Font(changedParentFont, con.Font.Style);
                        if (con is Button && con.Height < con.PreferredSize.Height) con.Height = con.PreferredSize.Height - 2;
                    }

                    if (con is Form || con is UserControl)
                    {
                        //if (CommonValues.LanguageID == 2052) //对中文版本应用指定的窗口字体
                        //{
                        //    changedParentFont = SetUniformFont(con as Form);
                        //}
                        //else
                        //{
                        //    changedParentFont = new Font("Calibri ", 12);
                        //}

                        FieldInfo[] fieldInfo = con.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                        for (int i = 0; i < fieldInfo.Length; i++)
                        {
                            switch (fieldInfo[i].FieldType.Name)
                            {
                                //ToolStrip toolstrip = ToolStripManager.FindToolStrip("contextMenuStrip1");
                                case "ContextMenuStrip":
                                    ContextMenuStrip contextMenuStrip = (ContextMenuStrip)fieldInfo[i].GetValue(con);
                                    foreach (ToolStripItem tsItem in contextMenuStrip.Items)
                                    {
                                        if (CommonValues.Langs.ContainsKey(tsItem.Text))
                                        {
                                            if (changedParentFont != null) tsItem.Font = new Font(changedParentFont, tsItem.Font.Style);
                                            tsItem.Text = CommonValues.Langs[tsItem.Text];
                                        }
                                        //tsItem.DropDownItems 显示文本在界面代码中处理
                                    }
                                    break;
                                case "Timer":
                                    //System.Windows.Forms.Timer timer = (System.Windows.Forms.Timer)fieldInfo[i].GetValue(con);
                                    break;
                            }
                        }
                        con.Text = CommonValues.Langs[con.Text];
                    }
                }

                if (con is TabControl)
                {
                    TabControl tabCtl = con as TabControl;
                    foreach (TabPage tp in tabCtl.TabPages)
                    {
                        if (CommonValues.Langs.ContainsKey(tp.Text))
                        {
                            if (changedParentFont != null) tp.Font = new Font(changedParentFont, tp.Font.Style);
                            tp.Text = CommonValues.Langs[tp.Text];
                        }
                    }
                }

                if (con is ToolStrip) //ToolStripLabel, ToolStripButton
                {
                    ToolStrip tsCtl = con as ToolStrip;
                    foreach (ToolStripItem tsItem in tsCtl.Items)
                    {
                        if (CommonValues.Langs.ContainsKey(tsItem.Text))
                        {
                            if (changedParentFont != null) tsItem.Font = new Font(changedParentFont, tsItem.Font.Style);
                            tsItem.Text = CommonValues.Langs[tsItem.Text];
                            tsItem.ToolTipText = CommonValues.Langs[tsItem.ToolTipText];
                        }
                    }
                }

                if (con is Chart)
                {
                    Chart chartCtl = con as Chart;
                    foreach (Series seriesItem in chartCtl.Series)
                    {
                        string legendText = seriesItem.LegendText;
                        if (CommonValues.Langs.ContainsKey(legendText))
                        {
                            if (changedParentFont != null) seriesItem.Font = new Font(changedParentFont, seriesItem.Font.Style);
                            seriesItem.LegendText = CommonValues.Langs[legendText];
                        }
                    }
                }

                //if (con is DataGridView)
                //{
                //    DataGridView dgv = con as DataGridView;
                //    foreach (DataGridViewColumn col in dgv.Columns)
                //    {
                //        string headerText = col.HeaderText;
                //        if (StaticValues.Langs.ContainsKey(headerText))
                //        {
                //            col.HeaderText = StaticValues.Langs[headerText];
                //            col.AutoSizeMode = col.AutoSizeMode;
                //        }
                //    }
                //}

                //特殊处理以校正其Anchor属性的效果
                if (con.Parent is TableLayoutPanel && (con is ComboBox || con is DateTimePicker))
                {
                    AnchorStyles originalAnchor = con.Anchor;
                    DockStyle originalDock = con.Dock;
                    if (con.Dock != DockStyle.Fill)
                    {
                        con.Dock = DockStyle.Fill;
                        con.Dock = originalDock;
                        con.Anchor = originalAnchor;
                    }
                    else
                    {
                        con.Anchor = AnchorStyles.Left;
                    }
                }

                if (changedParentFont != null) con.Font = new Font(changedParentFont, con.Font.Style);
            }
        }

        /// <summary>
        /// 设置多语言通用窗口字体
        /// </summary>
        /// <param name="frm">窗体</param>
        /// <returns>窗口特性标识中指定的字体</returns>
        public static Font SetUniformFont(Form frm)
        {
            Font changedFont = null;
            CustomAttributeData customAttr = PubFuns.GetAttribute(frm, typeof(UniformWindow), true);
            if (customAttr != null)
            {
                string fontFamily = PubFuns.GetAttributeValue(customAttr, "FontFamily").ToString();
                float fontSize = Convert.ToSingle(PubFuns.GetAttributeValue(customAttr, "FontSize"));

                byte bCharSet = frm.Font.GdiCharSet;
                frm.AutoScaleMode = AutoScaleMode.None;
                changedFont = frm.Font = new Font(fontFamily, Convert.ToSingle(fontSize), FontStyle.Regular, frm.Font.Unit);
            }

            return changedFont;
        }

        /// <summary>
        /// 根据当前屏幕分辨率调整界面元素大小
        /// </summary>
        /// <param name="parentCtl">父级容器(控件)</param>
        /// <param name="minZoomRate">最小缩放比例</param>
        /// <param name="adjustParent">是否缩放父级控件</param>
        public static void SetControls(Control parentCtl, float minZoomRate = 0.75f, bool adjustParent = false)
        {
            //List<Control> lstControls = CommonFuns.RetriveSubControls<Control>(parent, false, true);
            //lstControls.Sort(new ControlResizeComparer()); //排序优先处理容器内控件
            parentCtl.SuspendLayout();
            foreach (Control ctl in parentCtl.Controls)
            {
                if (ctl.Controls.Count > 0) { SetControls(ctl); } //递归调用, 先缩放子控件, 后缩放控件本身

                setControl(ctl, minZoomRate);
            }

            if (adjustParent) setControl(parentCtl, minZoomRate);
            parentCtl.ResumeLayout();
        }

        //调整单个控件的大小及位置
        private static void setControl(Control ctl, float minZoomRate = 0.75f)
        {
            float validWidthScale = Math.Max(minZoomRate, CommonValues.WidthScale);
            float validHeightScale = Math.Max(minZoomRate, CommonValues.HeightScale);

            string[] literalityTypes = new string[] {
                "Label", "ComboBox", "TextBox", "Button", "DateTimePicker", "RadioButton", "CheckBox",
                "RichTextBox", "TreeView", "ListBox", "ListView", "TabControl", "ToolStrip", "GroupBox"
            };
            if (literalityTypes.Contains(ctl.GetType().Name) || literalityTypes.Contains(ctl.GetType().BaseType.Name))
            {
                float currentSize = ctl.Font.Size * Math.Max(0.8F, validHeightScale);
                ctl.Font = new Font(ctl.Font.Name, currentSize, ctl.Font.Style, ctl.Font.Unit);
                if (!(ctl is Button))
                {
                    ctl.Width = (int)(ctl.Width * validWidthScale);
                }
            }

            if (ctl.Parent is TableLayoutPanel && (ctl is ComboBox || ctl is DateTimePicker || ctl is Button))
            {
                //特殊处理以修正其Anchor属性的效果
                AnchorStyles originalAnchor = ctl.Anchor;
                DockStyle originalDock = ctl.Dock;
                if (ctl.Dock != DockStyle.Fill)
                {
                    ctl.Dock = DockStyle.Fill;
                    ctl.Dock = originalDock;
                    ctl.Anchor = originalAnchor;
                }
                else
                {
                    ctl.Anchor = AnchorStyles.Left;
                }
            }

            if (CommonValues.WidthScale == 1 && CommonValues.HeightScale == 1) return;

            if ((ctl is TreeView) || (ctl is ListBox) || (ctl is ListView)
                || (ctl is TabControl) || (ctl is ToolStrip) || (ctl is TableLayoutPanel))
            {
                if (ctl.Dock == DockStyle.Left || ctl.Dock == DockStyle.Right)
                    ctl.Width = (int)(ctl.Width * validWidthScale);
                else if (ctl.Dock == DockStyle.Top || ctl.Dock == DockStyle.Bottom)
                    ctl.Height = (int)(ctl.Height * validHeightScale);

                if (ctl is ToolStrip ts)
                {
                    foreach (ToolStripItem tsItem in ts.Items)
                    {
                        tsItem.ImageScaling = ToolStripItemImageScaling.SizeToFit;
                        tsItem.Height = (int)(tsItem.Height * validHeightScale);
                    }
                }
            }

            if (ctl is DataGridViewEx dgvEx)
            {
                dgvEx.ResizeFont(CommonValues.HeightScale);
            }

            if (ctl is TextBoxEx tbxEx)
            {
                //tbxEx.Height = Convert.ToInt32(tbxEx.Height * validHeightScale);
                tbxEx.Height = tbxEx.GetPreferredHeight();
            }

            //Button, PictureBox, TableLayoutPanel, UserControl, Form 高度减小量最低80%
            float wScale = CommonValues.WidthScale, hScale = CommonValues.HeightScale;
            if (wScale < 0.8) wScale = 0.8f;
            if (hScale < 0.8) hScale = 0.8f;
            if (ctl is RoundButton || ctl is Button)
            {
                int preferredWidth = ctl.PreferredSize.Width;

                Button btn = ctl as Button;
                if (ctl.Anchor != (AnchorStyles.Left | AnchorStyles.Right) && ctl.Dock != DockStyle.Fill)
                {
                    if (ctl.Width >= preferredWidth || wScale > 1) //按钮宽度过小时不再继续缩小其宽度值
                    {
                        ctl.Width = (int)(ctl.Width * wScale);
                    }
                }
                ctl.Height = (int)(ctl.Height * hScale);

                if (ctl is RoundButton rBtn)
                {
                    rBtn.Radius = (int)(rBtn.Radius * validWidthScale);
                }
                else if (btn.Image != null)
                {
                    Bitmap bmp = new Bitmap(btn.Image, (int)(btn.Image.Width * CommonValues.WidthScale), (int)(btn.Image.Height * CommonValues.HeightScale));
                    btn.Image = bmp;
                }
            }

            if (ctl is CustomBorderPanel cbPanel)
            {
                cbPanel.Height = (int)(cbPanel.Height * hScale);
            }

            if (ctl is TableLayoutPanel || ctl is PictureBox || ctl is UserControl)
            {
                if (ctl.Dock != DockStyle.Fill && (ctl is PictureBox || ctl is UserControl))
                {
                    if (ctl.Dock != DockStyle.Top && ctl.Dock != DockStyle.Bottom)
                        ctl.Width = (int)(ctl.Width * wScale);
                    if (ctl.Dock != DockStyle.Left && ctl.Dock != DockStyle.Right)
                        ctl.Height = (int)(ctl.Height * hScale);
                }

                TableLayoutPanel tlp = ctl as TableLayoutPanel;
                if (tlp != null)
                {
                    tlp.SuspendLayout();
                    foreach (RowStyle rStyle in tlp.RowStyles)
                    {
                        if (rStyle.SizeType == SizeType.Absolute) rStyle.Height = rStyle.Height * hScale;
                    }
                    foreach (ColumnStyle cStyle in tlp.ColumnStyles)
                    {
                        if (cStyle.SizeType == SizeType.Absolute) cStyle.Width = cStyle.Width * wScale;
                    }
                    tlp.ResumeLayout();
                }
            }

            if (ctl is Form frm)
            {
                int oldW = frm.Width, oldH = frm.Height;
                frm.Width = (int)(frm.Width * wScale);
                frm.Height = (int)(frm.Height * hScale);

                frm.Left += (oldW - frm.Width) / 2;
                frm.Top += (oldH - frm.Height) / 2;
                //if (frm.Owner != null)
                //{
                //    frm.Left = frm.Owner.Left + (frm.Owner.Width - frm.Width) / 2;
                //    frm.Top = frm.Owner.Top + (frm.Owner.Height - frm.Height) / 2;
                //}
            }

            ctl.Margin = new Padding(
                GetNaturalFloor(ctl.Margin.Left * CommonValues.WidthScale),
                GetNaturalFloor(ctl.Margin.Top * CommonValues.HeightScale),
                GetNaturalFloor(ctl.Margin.Right * CommonValues.WidthScale),
                GetNaturalFloor(ctl.Margin.Bottom * CommonValues.HeightScale));
            ctl.Padding = new Padding(
                GetNaturalFloor(ctl.Padding.Left * CommonValues.WidthScale),
                GetNaturalFloor(ctl.Padding.Top * CommonValues.HeightScale),
                GetNaturalFloor(ctl.Padding.Right * CommonValues.WidthScale),
                GetNaturalFloor(ctl.Padding.Bottom * CommonValues.HeightScale));
        }

        //获取小于或等于指定浮点数的最大自然数
        public static int GetNaturalFloor(float fVal)
        {
            int result = (int)Math.Floor(fVal);
            if (fVal > 0 && result == 0) result = 1;
            return result;
        }
    }

    public class ScreenCenterTip
    {
        // 最大宽度限制（防止过宽）
        private const int MaxWidth = 400;
        // 最小宽度
        private const int MinWidth = 150;
        // 高度固定
        private const int Height = 36;
        // 内边距
        private const int Padding = 20;
        // 提示停留时长(毫秒)
        private const int ShowDuration = 1600;

        private static Font tipTextFont = new Font("Microsoft YaHei UI", 10.5F); //"Microsoft Sans Serif" , , FontStyle.Bold

        /// <summary>
        /// 屏幕居中显示一条轻提示，ShowDuration 毫秒后自动关闭。
        /// 注意：WinForms 的 Timer 依赖“创建窗口的那个线程”的消息泵来派发 WM_TIMER，
        /// 若在没有消息泵的线程（如 Task.Run/线程池线程）上创建 Form 和 Timer，
        /// Tick 永远不会触发，提示窗就不会自动关闭。
        /// 因此这里统一保证：窗口一定在拥有消息泵的线程上创建。
        /// </summary>
        public static void Show(string message)
        {
            string msgText = CommonValues.Langs[message];

            // 当前线程已有消息泵（正常的UI线程），就地显示，由现有消息循环派发定时器
            if (Application.MessageLoop)
            {
                ShowAndAutoClose(msgText, false);
                return;
            }

            // 当前线程没有消息泵（线程池/后台线程），另起一个带消息泵的 STA 线程来显示
            System.Threading.Thread tipThread = new System.Threading.Thread(() =>
            {
                try
                {
                    ShowAndAutoClose(msgText, true);
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex, "屏幕居中提示");
                }
            });
            tipThread.IsBackground = true;
            tipThread.SetApartmentState(System.Threading.ApartmentState.STA);
            tipThread.Start();
        }

        /// <summary>
        /// 创建提示窗并定时关闭
        /// </summary>
        /// <param name="msgText">提示内容</param>
        /// <param name="createMessageLoop">true：当前线程没有消息泵，用 Application.Run 建立消息泵</param>
        private static void ShowAndAutoClose(string msgText, bool createMessageLoop)
        {
            Form form = CreateTipForm(msgText);

            Timer timer = new Timer { Interval = ShowDuration };
            timer.Tick += (sender, e) =>
            {
                timer.Stop();
                timer.Dispose();
                if (!form.IsDisposed) form.Close();
            };
            timer.Start();

            if (createMessageLoop)
            {
                try
                {
                    // 为该线程建立消息泵，Form 关闭后消息循环退出
                    Application.Run(form);
                }
                finally
                {
                    timer.Dispose();
                    if (!form.IsDisposed) form.Dispose();
                }
            }
            else
            {
                form.FormClosed += (sender, e) => { form.Dispose(); };
                form.Show();
                form.Update();
            }
        }

        /// <summary>
        /// 构造提示窗体
        /// </summary>
        private static Form CreateTipForm(string msgText)
        {
            // 创建一个Form作为容器
            Form form = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                ShowInTaskbar = false,
                TopMost = true,
                //Opacity = 0.9,
                BackColor = Color.FromArgb(74, 122, 196),
                ForeColor = Color.White,
                AutoScaleMode = AutoScaleMode.None,
            };

            // 添加标签
            Label label = new Label
            {
                Text = msgText,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = tipTextFont,
                ForeColor = Color.White,
                AutoSize = true,
                MaximumSize = new Size(MaxWidth - Padding * 2, 0),
                //BackColor = Color.Transparent,  // 让标签背景透明
            };

            // 添加标签到窗体
            form.Controls.Add(label);
            label.PerformLayout();

            // 先计算标签所需尺寸
            Size labelPreferredSize = label.GetPreferredSize(new Size(MaxWidth - Padding * 2, int.MaxValue));

            // 设置窗体大小
            int formWidth = Math.Max(MinWidth, labelPreferredSize.Width + Padding * 2);
            form.Size = new Size(formWidth + 2, Height);

            // 设置标签位置（居中）
            label.Location = new Point(
                (form.Width - labelPreferredSize.Width) / 2,
                (form.Height - labelPreferredSize.Height) / 2
            );

            // 计算屏幕中心位置
            form.Location = new Point(
                Screen.PrimaryScreen.WorkingArea.Width / 2 - form.Width / 2,
                Screen.PrimaryScreen.WorkingArea.Height / 2 - form.Height / 2
            );

            return form;
        }
    }

    public class GraphicsPathHelper
    {
        /// <summary>
        /// 建立带有圆角样式的路径
        /// </summary>
        /// <param name="rect">用来建立路径的矩形</param>
        /// <param name="radius">圆角半径</param>
        /// <param name="style">圆角矩形类型</param>
        /// <param name="correctionVal">绘制边框线时需要的校正值</param>
        /// <returns></returns>
        public static GraphicsPath CreatePath(Rectangle rect, int radius, RoundStyle style, int correctionVal = 0)
        {
            GraphicsPath path = new GraphicsPath();

            switch (style)
            {
                case RoundStyle.None:
                    path.AddRectangle(rect);
                    break;
                case RoundStyle.All:
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(rect.Right - radius - correctionVal, rect.Y, radius, radius, 270, 90);
                    path.AddArc(rect.Right - radius - correctionVal, rect.Bottom - radius - correctionVal, radius, radius, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - radius - correctionVal, radius, radius, 90, 90);
                    break;
                case RoundStyle.Left:
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddLine(rect.Right - correctionVal, rect.Y, rect.Right - correctionVal, rect.Bottom - correctionVal);
                    path.AddArc(rect.X, rect.Bottom - radius - correctionVal, radius, radius, 90, 90);
                    break;
                case RoundStyle.Right:
                    path.AddArc(rect.Right - radius - correctionVal, rect.Y, radius, radius, 270, 90);
                    path.AddArc(rect.Right - radius - correctionVal, rect.Bottom - radius - correctionVal, radius, radius, 0, 90);
                    path.AddLine(rect.X, rect.Bottom - correctionVal, rect.X, rect.Y);
                    break;
                case RoundStyle.Top:
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(rect.Right - radius - correctionVal, rect.Y, radius, radius, 270, 90);
                    path.AddLine(rect.Right - correctionVal, rect.Bottom - correctionVal, rect.X, rect.Bottom - correctionVal);
                    break;
                case RoundStyle.Bottom:
                    path.AddArc(rect.Right - radius - correctionVal, rect.Bottom - radius - correctionVal, radius, radius, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - radius - correctionVal, radius, radius, 90, 90);
                    path.AddLine(rect.X, rect.Y, rect.Right - correctionVal, rect.Y);
                    break;
            }
            path.CloseFigure();

            return path;
        }

        /// <summary>
        /// 根据矩形及圆角半径大小获取圆角矩形的路径
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static GraphicsPath GetRoundedRectanglePath(Rectangle rect, float radius, int correctionVal = 0)
        {
            GraphicsPath path = new GraphicsPath();

            if (rect.Width <= 0 || rect.Height <= 0) return path;

            radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2);

            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            float diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter - correctionVal, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter - correctionVal, rect.Bottom - diameter - correctionVal, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter - correctionVal, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        public static GraphicsPath GetRoundedRectanglePath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (rect.Width <= 0 || rect.Height <= 0) return path;

            radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2f);
            if (radius <= 0) { path.AddRectangle(rect); return path; }

            float d = radius * 2f;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

    }

    public class ModalDialogWithEnabledMinMax
    {
        private static NativeWindow _ownerWindow;
        private static bool _isModalActive = false;

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        private const uint SC_CLOSE = 0xF060;
        private const uint MF_GRAYED = 0x00000001;
        private const uint MF_ENABLED = 0x00000000;

        public static void Show(Form owner, Form dialog)
        {
            // 1. 保存父窗口句柄
            _ownerWindow = new NativeWindowWrapper(owner);
            _isModalActive = true;

            // 2. 可选：禁用父窗口的关闭按钮（防止绕过模态对话框）
            IntPtr hMenu = GetSystemMenu(owner.Handle, false);
            EnableMenuItem(hMenu, SC_CLOSE, MF_GRAYED);

            // 3. 显示模态对话框
            dialog.FormClosed += (s, e) =>
            {
                _isModalActive = false;
                EnableMenuItem(hMenu, SC_CLOSE, MF_ENABLED);
            };
            dialog.Show(owner);
        }

        private class NativeWindowWrapper : NativeWindow
        {
            public NativeWindowWrapper(Form form)
            {
                AssignHandle(form.Handle);
            }

            protected override void WndProc(ref Message m)
            {
                const int WM_SYSCOMMAND = 0x0112;
                const int SC_MINIMIZE = 0xF020;
                const int SC_MAXIMIZE = 0xF030;
                const int SC_RESTORE = 0xF120;

                if (_isModalActive && m.Msg == WM_SYSCOMMAND)
                {
                    int cmd = m.WParam.ToInt32() & 0xFFF0;
                    if (cmd == SC_MINIMIZE || cmd == SC_MAXIMIZE) //|| cmd == SC_RESTORE
                    {
                        // 允许最小化/最大化/恢复
                        base.WndProc(ref m);
                        return;
                    }
                    // 阻止其他系统命令（如关闭）
                    return;
                }

                base.WndProc(ref m);
            }
        }
    }

}
