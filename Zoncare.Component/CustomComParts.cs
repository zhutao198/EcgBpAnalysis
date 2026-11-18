using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Zoncare.Component.Controls;
using Zoncare.Utilities;

namespace Zoncare.Component
{
    public delegate void ItemSelectedEventHandler(object sender, ItemSelectedEventArgs e);
    public delegate void OperationEventHandler(object sender, CancelEventArgs e);

    /// <summary>
    /// 自动填充输入组件的下拉列表(ListBox)的项
    /// </summary>
    [Serializable]
    public class ListBoxItem
    {
        public object ItemValue { get; set; }
        public string ItemText { get; set; }

        public ListBoxItem() { }

        public ListBoxItem(object value, string text)
        {
            this.ItemValue = value;
            this.ItemText = text;
        }

        public override string ToString()
        {
            return this.ItemText;
        }
    }

    /// <summary>
    /// 列表项选中事件的参数
    /// </summary>
    public class ItemSelectedEventArgs
    {
        public ListBoxItem SelectedItem { get; set; }
        public int SelectedIndex { get; set; }

        public ItemSelectedEventArgs(ListBoxItem selectedItem, int selectedIndex)
        {
            this.SelectedItem = selectedItem;
            this.SelectedIndex = selectedIndex;
        }
    }

    /// <summary>
    /// 包含输入框的组合控件
    /// </summary>
    public interface ITextInputBox
    {
        TextBoxEx InputBox { get; }

        string InputText { get; }
    }

    #region 对工具栏项目的相关扩展
    //自定义工具栏按钮
    public class CustomToolStripButton : ToolStripButton
    {
        private bool _userSelected;

        public Image InactiveImage { get; set; }
        public Image ActiveImage { get; set; }
        public Color TextColor { get; set; }
        public bool UserSelected
        {
            get => _userSelected;
            set
            {
                _userSelected = value;
                if (_userSelected == true)
                {
                    if (ActiveImage != null) Image = ActiveImage;
                }
                else
                {
                    if (InactiveImage != null) Image = InactiveImage;
                }
            }
        }

        public CustomToolStripButton()
        {

        }
        public CustomToolStripButton(string text) : base(text)
        {

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            string text = this.Text.Replace("\n", "");
            Rectangle rect = this.ContentRectangle;
            Graphics g = e.Graphics;

            if (this.DisplayStyle == ToolStripItemDisplayStyle.ImageAndText)
            {
                if (DesignMode) return;
                if (!UserSelected)
                {
                    using (SolidBrush brush = new SolidBrush(this.BackColor))
                    {
                        if (!Selected)
                        {
                            g.FillRectangle(brush, rect);
                        }
                        else
                        {
                            brush.Color = Color.FromArgb(236, 242, 255);
                            g.FillRectangle(brush, rect);
                        }
                    }
                    ForeColor = TextColor;
                }
                else
                {
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(221, 225, 229))) g.FillRectangle(brush, rect);
                    ForeColor = Color.FromArgb(73, 120, 193);
                    using (Pen p = new Pen(ForeColor, 2F))
                    {
                        g.DrawLine(p, rect.Left, Bounds.Bottom - 2, rect.Right, Bounds.Bottom - 2);
                    }
                }

                //e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                SizeF textSize = g.MeasureString(text, Font);
                RectangleF rectText = new RectangleF(0, Height - textSize.Height, Width, textSize.Height);
                //绘制图像
                if (Image != null)
                {
                    float imgRectHeight = Height - textSize.Height;
                    float imgWidth = Image.Width, imgHeight = Image.Height;
                    if (imgHeight > imgRectHeight)
                    {
                        imgHeight = imgRectHeight;
                        imgWidth = imgHeight * Image.Width / Image.Height;
                    }

                    if (Enabled)
                    {
                        g.DrawImage(Image, Width / 2 - imgWidth / 2, imgRectHeight / 2 - imgHeight / 2, imgWidth, imgHeight);
                    }
                    else
                    {
                        ControlPaint.DrawImageDisabled(g, this.Image, (int)((Width - imgWidth) / 2), (int)((imgRectHeight- imgHeight) / 2), this.BackColor);

                        //using (Image img = ToolStripRenderer.CreateDisabledImage(Image))
                        //    g.DrawImage(img, Width / 2 - imgWidth / 2, imgRectHeight / 2 - imgHeight / 2, imgWidth, imgHeight);
                    }
                }

                //绘制文本
                Color textColor = this.ForeColor;
                if (!Enabled) textColor = SystemColors.GrayText;
                using (StringFormat sf = new StringFormat())
                using (Brush brush = new SolidBrush(textColor))
                {
                    sf.Alignment = StringAlignment.Center; // 中心对齐
                    sf.LineAlignment = StringAlignment.Far; // 垂直中心对齐
                    sf.Trimming = StringTrimming.EllipsisCharacter; // 截断为省略号
                    g.DrawString(text, this.Font, brush, rectText, sf);
                }

                //上传设置按钮
                if (this.Name == "btnUpload" && this.Visible)
                {
                    Rectangle rectBtn = this.Bounds;
                    Rectangle rectSetting = new Rectangle(rectBtn.Right - 16, rectBtn.Top, 16, 16);

                    Bitmap bmp = Properties.Resources.uploadSettings;
                    g.DrawImage(bmp, rectBtn.Width - 16, 0, 16, 16);
                }
            }
        }
    }

    public class CustomToolStripDropdownButton : ToolStripDropDownButton
    {
        public CustomToolStripDropdownButton() { }
        public CustomToolStripDropdownButton(string text) : base(text)
        {

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // 获取当前的文本和矩形区域
            string text = this.Text.Replace("\n", "");
            Rectangle rect = this.ContentRectangle;

            if (this.DisplayStyle == ToolStripItemDisplayStyle.ImageAndText)
            {
                e.Graphics.FillRectangle(new SolidBrush(this.BackColor), rect);
                //e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                SizeF textSize = e.Graphics.MeasureString(text, Font);
                RectangleF rectText = new RectangleF(0, Height - textSize.Height, Width, textSize.Height);
                //绘制图像
                if (Image != null)
                {
                    float imgRectHeight = Height - textSize.Height;
                    float imgWidth = Image.Width, imgHeight = Image.Height;
                    if (imgHeight > imgRectHeight)
                    {
                        imgHeight = imgRectHeight;
                        imgWidth = imgHeight * Image.Width / Image.Height;
                    }
                    e.Graphics.DrawImage(Image, Width / 2 - imgWidth / 2, imgRectHeight / 2 - imgHeight / 2, imgWidth, imgHeight);
                }

                //绘制文本
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center; // 中心对齐
                    sf.LineAlignment = StringAlignment.Far; // 垂直中心对齐
                    sf.Trimming = StringTrimming.EllipsisCharacter; // 截断为省略号

                    e.Graphics.DrawString(text, this.Font, new SolidBrush(this.ForeColor), rectText, sf);
                }
            }
        }
    }

    //重置当前光标所在工具栏的背景色
    public class ToolStripBackcolorRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            Rectangle bounds = new Rectangle(Point.Empty, e.Item.Size);

            if (e.ToolStrip is ToolStripEx ts)
            {
                if (ts.DisabledItems.Count > 0)
                {
                    if (ts.DisabledItems.Contains(e.Item.Name))
                    {
                        //Rectangle contentRect = e.Item.ContentRectangle;
                        //Rectangle innerRect = new Rectangle(contentRect.Left + (contentRect.Width - e.Item.Image.Width) / 2, contentRect.Top ,
                        //    e.Item.Image.Width, e.Item.Image.Height);
                        //e.Graphics.FillRectangle(Brushes.LightGray, innerRect);
                        return;
                    }
                    else
                    {
                    }
                }
            }

            Point p = e.ToolStrip.PointToClient(Control.MousePosition); //Cursor.Position
            if (e.Item.Bounds.Contains(p))
            {
                if (e.Item.Selected)
                {
                    Color backColor = Color.FromArgb(221, 225, 229);
                    using (SolidBrush brush = new SolidBrush(backColor))
                    {
                        e.Graphics.FillRectangle(brush, bounds);
                        return;
                    }
                    //using (SolidBrush brush = new SolidBrush(e.Item.BackColor))
                    //{
                    //    e.Graphics.FillRectangle(brush, bounds);
                    //    using (Pen dashPen = new Pen(Color.FromArgb(73, 120, 193), 2))
                    //    {
                    //        dashPen.DashStyle = DashStyle.Dot;
                    //        Rectangle hoverRect = new Rectangle(0, 0, bounds.Width - 2, bounds.Height - 2);
                    //        e.Graphics.DrawRectangle(dashPen, bounds);
                    //    }
                    //    return;
                    //}
                }
            }

            base.OnRenderButtonBackground(e);
        }

        protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
        {
            if (e.ToolStrip is ToolStripEx ts)
            {
                if (ts.DisabledItems.Count > 0)
                {
                    if (ts.DisabledItems.Contains(e.Item.Name))
                    {
                        return;
                    }
                }
            }

            Rectangle bounds = new Rectangle(Point.Empty, e.Item.Size);
            Point p = e.ToolStrip.PointToClient(Control.MousePosition); //Cursor.Position
            if (e.Item.Selected || e.Item.Pressed)
            {
                if (e.Item.Bounds.Contains(p))
                {
                    Color backColor = PubFuns.GetColorPlus(e.Item.BackColor, 0, 30, 30, 30);
                    using (SolidBrush brush = new SolidBrush(backColor))
                    {
                        e.Graphics.FillRectangle(brush, bounds);
                    }
                }
                else
                {
                    Color backColor = PubFuns.GetColorPlus(e.Item.BackColor, 0, 15, 15, 15);
                    using (SolidBrush brush = new SolidBrush(backColor))
                    {
                        e.Graphics.FillRectangle(brush, bounds);
                    }
                }
            }
            else
            {
                base.OnRenderDropDownButtonBackground(e);
            }
        }

        //protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        //{
        //    Rectangle bounds = new Rectangle(Point.Empty, e.Item.Size);
        //    if (e.Item.Selected)
        //    {
        //        //base.OnRenderMenuItemBackground(e);
        //        e.Graphics.FillRectangle(Brushes.ForestGreen, bounds);
        //    }
        //    else
        //    {
        //        e.Graphics.FillRectangle(Brushes.LightSteelBlue, bounds);
        //    }
        //}
    }

    public class CustomToolStripRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
        {
            var splitButton = e.Item as ToolStripSplitButton;
            if (splitButton != null)
            {
                // 获取按钮整体区域
                //Rectangle bounds = new Rectangle(Point.Empty, splitButton.Size);
                Rectangle contentRect = splitButton.ContentRectangle;

                // 区分按钮部分和箭头部分
                Rectangle buttonRect = new Rectangle(contentRect.X, contentRect.Y, contentRect.Width - splitButton.ButtonBounds.Width, contentRect.Height);
                int arrowWidth = contentRect.Width - splitButton.ButtonBounds.Width;
                Rectangle arrowRect = new Rectangle(contentRect.X + contentRect.Width - arrowWidth, contentRect.Y, arrowWidth, contentRect.Height);

                // 绘制箭头部分的背景色（这里改成你想要的背景色）
                if (arrowRect != Rectangle.Empty)
                {
                    bool subItemChecked = false;
                    foreach (ToolStripItem item in splitButton.DropDownItems)
                    {
                        ToolStripMenuItem menuItem = item as ToolStripMenuItem;
                        if (menuItem != null && menuItem.Checked)
                        {
                            subItemChecked = true;
                        }
                    }
                    if (subItemChecked)
                    {
                        using (Brush brush = new SolidBrush(Color.FromArgb(177, 208, 255)))
                        {
                            e.Graphics.FillRectangle(brush, arrowRect);
                        }
                    }
                }

                // 绘制按钮部分的背景（保持默认）
                base.OnRenderSplitButtonBackground(e);

                //// 重新绘制箭头图标（因为背景被覆盖了，需要重绘箭头）
                //if (splitButton.DropDownButtonBounds != Rectangle.Empty)
                //{
                //    // 绘制箭头（使用系统箭头或自定义）
                //    DrawArrow(e.Graphics, arrowRect);
                //}
            }
            else
            {
                base.OnRenderSplitButtonBackground(e);
            }
        }
    }
    #endregion


    #region 对DataGridView的相关扩展
    public class DataGridViewActionColumn : DataGridViewColumn
    {
        private int actionGap = 6;

        /// <summary>
        /// 指定当前操作列的"操作"按钮显示方式
        /// </summary>
        [Browsable(false)]
        public List<DataGridViewEx.GridDataAction> ActionMode { get; set; }

        /// <summary>
        /// 指定当前操作列的按钮之间的间距(像素)
        /// </summary>
        [Browsable(false)]
        public int ActionGap
        {
            get { return actionGap; }
            set
            {
                actionGap = value;
                if (this.DataGridView != null)
                {
                    (this.DataGridView as DataGridViewEx).ResizeColumnWidth();
                }
            }
        }

        public DataGridViewActionColumn(List<DataGridViewEx.GridDataAction> actionMode)
        {
            DataGridViewActionCell cellTemplate = new DataGridViewActionCell() { ActionMode = actionMode };
            //if (this.CustomActionMode != ZoncareDataGrid.GridActions.NotSet)
            //    cellTemplate.ActionType = this.CustomActionMode;

            this.CellTemplate = cellTemplate;

            this.ActionMode = actionMode;
            this.HeaderText = "";
        }
    }

    public class DataGridViewActionCell : DataGridViewTextBoxCell
    {
        private RectangleData[] actionRectangle;
        private PointF cellPosition = PointF.Empty;
        private DataGridViewEx.RowActionModel actionModel;
        private ToolTip actionToolTip;
        private bool showCellToolTips; //表格原始属性(是否显示鼠标悬停单元格时的提示)
        private RectangleF previousActionRect = RectangleF.Empty;
        public List<DataGridViewEx.GridDataAction> ActionMode { get; set; } //单元格的操作类型

        public DataGridViewActionCell()
        {
            //overrided = false;
        }

        //public DataGridViewActionCell(ZoncareDataGrid.GridActions actionType)
        //{
        //    this.ActionType = actionType;
        //}

        protected override void OnDataGridViewChanged()
        {
            base.OnDataGridViewChanged();

            DataGridViewEx gv = this.DataGridView as DataGridViewEx;
            if (gv != null)
            {
                showCellToolTips = gv.ShowCellToolTips;
                actionToolTip = new ToolTip();
            }
        }

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value,
            object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);

            DataGridViewEx gv = this.DataGridView as DataGridViewEx;
            if (gv == null || gv.RowActionModels == null || gv.RowActionModels.Count <= 0) return;

            actionModel = gv.RowActionModels.Find(o => o.RowIndex == rowIndex);
            if (actionModel == null) return;

            //抹去单元格原来的背景
            graphics.SetClip(cellBounds);
            using (Brush backColorBrush = new SolidBrush(cellStyle.BackColor))
            {
                Rectangle contentBounds = this.GetContentBounds(graphics, cellStyle, rowIndex);
                graphics.FillRectangle(backColorBrush, cellBounds.X, cellBounds.Y, contentBounds.Width, contentBounds.Height);
            }

            //初始化操作显示区相关信息
            if (actionRectangle == null) { actionRectangle = new RectangleData[actionModel.Actions.Count]; }
            int i = 0;
            foreach (DataGridViewEx.GridDataAction action in actionModel.Actions)
            {
                actionRectangle[i] = new RectangleData()
                {
                    CellAction = action,
                    ToolTipText = action.DisplayText
                };
                i++;
            }

            cellPosition = new PointF(cellBounds.X, cellBounds.Y);
            int totalWidth = getActionTotalWidth(graphics, gv, cellBounds, cellStyle);
            int gap = (gv.Columns[this.ColumnIndex] as DataGridViewActionColumn).ActionGap;
            int xOffset = (cellBounds.Width - totalWidth) / 2, elementWidth = 0;
            int j = 0;
            Point location = new Point(cellBounds.X, cellBounds.Y);
            foreach (DataGridViewEx.GridDataAction action in actionModel.Actions)
            {
                location.X = cellBounds.X + xOffset;
                RectangleData rectData = actionRectangle[j];
                if (rectData.RegionFontStyle == null) rectData.RegionFontStyle = cellStyle.Font.Style; //初始化为所在单元格的字体样式

                Font textFont = new Font(cellStyle.Font, rectData.RegionFontStyle.Value);
                Color textForeColor = cellStyle.ForeColor;
                if (action.UseLinkStyle)
                {
                    textForeColor = Color.FromArgb(0, 100, 255);
                    textFont = new Font(cellStyle.Font, FontStyle.Underline);
                }
                if (action.ContentStyle != null)
                {
                    if (action.ContentStyle.Font != null) textFont = action.ContentStyle.Font;
                    if (action.ContentStyle.ForeColor != Color.Empty) textForeColor = action.ContentStyle.ForeColor;
                }
                if (action.Enabled == false) { textForeColor = SystemColors.GrayText; }

                SizeF textSize = graphics.MeasureString(action.DisplayText, textFont);
                switch (action.ActionType)
                {
                    case DataGridViewEx.RowActionType.TextButton:
                        location.Y = cellBounds.Y + (cellBounds.Height - (int)textSize.Height) / 2;
                        rectData.ActionRectangle = new RectangleF(location, textSize);

                        using (Brush brush1 = new SolidBrush(textForeColor))
                        {
                            graphics.DrawString(action.DisplayText, textFont, brush1, location.X, location.Y);
                        }
                        break;
                    case DataGridViewEx.RowActionType.ImageButton:
                        Image img = null;
                        if (action.Value is string)
                        {
                            string imgFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, action.Value.ToString());
                            if (File.Exists(imgFilePath)) { img = Image.FromFile(imgFilePath); }
                        }
                        else if (action.Value is Image)
                        {
                            img = action.Value as Image;
                        }

                        if (img != null)
                        {
                            location.Y = cellBounds.Y + (cellBounds.Height - img.Height) / 2;
                            rectData.ActionRectangle = new RectangleF(location, new SizeF(img.Width, img.Height));
                            graphics.DrawImage(img, location);
                        }
                        else
                        {
                            location.Y = cellBounds.Y + (cellBounds.Height - 16) / 2;
                            rectData.ActionRectangle = new RectangleF(location, new SizeF(16, 16));
                            graphics.DrawRectangle(Pens.SlateGray, location.X, location.Y, 16, 16);
                        }

                        //绘制提示文字
                        //Graphics gGrid = gv.CreateGraphics();
                        //Font fontTips = new Font("宋体", 9, FontStyle.Regular);
                        //if (rectData.ShowToolTipText)
                        //{
                        //    SizeF tipSize = gGrid.MeasureString(action.DisplayText, fontTips);
                        //    RectangleF tipRect = new RectangleF(location.X + img.Width / 2, location.Y + img.Height / 2, tipSize.Width, tipSize.Height + 6);
                        //    gGrid.DrawRectangle(Pens.AliceBlue, tipRect.X, tipRect.Y, tipRect.Width, tipRect.Height);
                        //    gGrid.FillRectangle(Brushes.GhostWhite, tipRect);
                        //    gGrid.DrawString(action.DisplayText, fontTips, new SolidBrush(Color.Black), tipRect);
                        //}
                        break;
                    case DataGridViewEx.RowActionType.RadioButton:
                        //RadioButtonState state = (action.Value == "true" ? RadioButtonState.CheckedNormal : RadioButtonState.UncheckedNormal);
                        //Size radioSize = RadioButtonRenderer.GetGlyphSize(graphics, state);

                        //location.Y = cellBounds.Y + (cellBounds.Height - radioSize.Height) / 2;
                        //int top = cellBounds.Y + (cellBounds.Height - (int)textSize.Height) / 2;
                        //Rectangle textBounds = new Rectangle(location.X + radioSize.Width + 1, top, (int)textSize.Width, (int)textSize.Height);
                        //rectData.ActionRectangle = new RectangleF(location, new SizeF(radioSize.Width + textBounds.Width + 1, textSize.Height));
                        //Font font2 = new Font(cellStyle.Font, rectData.RegionFontStyle.Value);
                        //RadioButtonRenderer.DrawRadioButton(graphics, location, textBounds, action.DisplayText, font2, false, state);

                        int rectHeight = (int)textSize.Height - 8;
                        location.Y = cellBounds.Y + (cellBounds.Height - rectHeight) / 2;
                        Rectangle rectRadio = new Rectangle(location, new Size(rectHeight, rectHeight)); //单选按钮的图标区域
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        using (Brush radioBrush = new SolidBrush(Color.FromArgb(0, 100, 255)))
                        {
                            using (Pen radioPen = new Pen(radioBrush))
                            {
                                if (action.Value?.ToString() == "true")
                                {
                                    RectangleF radioRectCopy = new RectangleF(location, rectRadio.Size); //矩形区域副本
                                    graphics.DrawEllipse(radioPen, radioRectCopy);

                                    radioRectCopy.Inflate(-3, -3);
                                    if (radioRectCopy.Width < 3)
                                    {
                                        radioRectCopy = new RectangleF(location.X + (rectHeight - 3) / 2f, location.Y + (rectHeight - 3) / 2f, 3, 3);
                                    }
                                    graphics.FillEllipse(radioBrush, radioRectCopy);
                                }
                                else
                                {
                                    graphics.DrawEllipse(radioPen, rectRadio);
                                }
                            }
                        }

                        int y = cellBounds.Y + (cellBounds.Height - (int)textSize.Height) / 2;
                        Rectangle rectText = new Rectangle(location.X + rectRadio.Width + 1, y, (int)textSize.Width + 1, (int)textSize.Height);
                        rectData.ActionRectangle = new RectangleF(location, new SizeF(rectRadio.Width + 1 + rectText.Width, textSize.Height));
                        using (Brush brush2 = new SolidBrush(textForeColor))
                        {
                            graphics.DrawString(action.DisplayText, textFont, brush2, rectText);
                        }
                        graphics.SmoothingMode = SmoothingMode.None; //重置, 否则单元格边框显示不正常
                        break;
                }
                elementWidth = (int)rectData.ActionRectangle.Width;
                xOffset += elementWidth + gap;
                j++;
            }
            graphics.ResetClip();
        }

        protected override void OnMouseEnter(int rowIndex)
        {
            this.DataGridView.ShowCellToolTips = false;
            base.OnMouseEnter(rowIndex);
        }

        protected override void OnMouseMove(DataGridViewCellMouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (this.actionRectangle == null) return;

            //根据鼠标指针的位置, 显示不同效果
            DataGridViewEx gv = this.DataGridView as DataGridViewEx;
            bool mouseInActionRect = false, isToolTipShown = false;
            PointF p = new PointF(this.cellPosition.X + e.X, this.cellPosition.Y + e.Y);
            for (int i = 0; i < this.actionRectangle.Length; i++)
            {
                RectangleData rectData = this.actionRectangle[i];
                DataGridViewEx.RowActionType actionType = rectData.CellAction.ActionType;
                if (rectData.ActionRectangle.Contains(p))
                {
                    //Invalidate事件会触发MouseMove事件，避免重复调用
                    if (rectData.ActionRectangle == this.previousActionRect) return;

                    this.previousActionRect = rectData.ActionRectangle;
                    if (actionType == DataGridViewEx.RowActionType.TextButton)
                    {
                        //显示下划线
                        rectData.RegionFontStyle = getActionFontStyle(rectData.CellAction, FontStyle.Underline);
                    }
                    else if (actionType == DataGridViewEx.RowActionType.ImageButton)
                    {
                        //显示提示文字
                        rectData.ShowToolTipText = true;
                        this.ToolTipText = rectData.ToolTipText;
                        if (string.IsNullOrEmpty(actionToolTip.GetToolTip(gv)))
                        {
                            this.actionToolTip.Show(
                                rectData.ToolTipText,
                                gv,
                                (int)(rectData.ActionRectangle.Left + rectData.ActionRectangle.Width / 2),
                                (int)(rectData.ActionRectangle.Top + rectData.ActionRectangle.Height),
                                1500);
                            isToolTipShown = true;
                        }

                        rectData.RegionFontStyle = FontStyle.Regular;
                    }
                    else
                    {
                        rectData.RegionFontStyle = getActionFontStyle(rectData.CellAction, FontStyle.Regular);
                    }

                    if (rectData.CellAction.Enabled)
                        gv.Cursor = Cursors.Hand;
                    else
                        gv.Cursor = Cursors.No;

                    mouseInActionRect = true;
                    break;
                }
                else
                {
                    rectData.ShowToolTipText = false;
                    rectData.RegionFontStyle = getActionFontStyle(rectData.CellAction, FontStyle.Regular);
                }
            }

            if (!mouseInActionRect)
            {
                this.actionToolTip.Hide(this.DataGridView);
                this.previousActionRect = RectangleF.Empty;
                this.DataGridView.Cursor = Cursors.Default;
            }

            this.DataGridView.InvalidateCell(this);
        }

        protected override void OnMouseLeave(int rowIndex)
        {
            base.OnMouseLeave(rowIndex);
            this.DataGridView.ShowCellToolTips = showCellToolTips;

            if (actionRectangle == null) return;

            RectangleF cellRect = new RectangleF(this.cellPosition, this.Size);
            if (this.DataGridView.ContainsFocus && cellRect.Contains(this.DataGridView.PointToClient(Cursor.Position))) { return; }

            this.previousActionRect = RectangleF.Empty;
            this.DataGridView.Cursor = Cursors.Default;

            //this.DataGridView.ShowCellToolTips = false;
            this.actionToolTip.Hide(this.DataGridView);

            for (int i = 0; i < actionRectangle.Length; i++)
            {
                actionRectangle[i].RegionFontStyle = getActionFontStyle(actionRectangle[i].CellAction, FontStyle.Regular);
                actionRectangle[i].ShowToolTipText = false;
            }

            this.DataGridView.InvalidateCell(this);
        }

        protected override void OnMouseUp(DataGridViewCellMouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (actionRectangle == null) return;

            PointF p = new PointF(this.cellPosition.X + e.X, this.cellPosition.Y + e.Y); //当前鼠标指针的位置
            for (int i = 0; i < this.actionRectangle.Length; i++)
            {
                RectangleData rectData = this.actionRectangle[i];
                string actionEventName = string.Empty;

                if (rectData.CellAction.ActionType == DataGridViewEx.RowActionType.TextButton)
                {
                    if (rectData.CellAction.Enabled && rectData.ActionRectangle.Contains(p))
                        actionEventName = "RowAction_TextButtonClick";
                }
                else if (rectData.CellAction.ActionType == DataGridViewEx.RowActionType.RadioButton)
                {
                    if (rectData.CellAction.Enabled && rectData.ActionRectangle.Contains(p))
                    {
                        if (rectData.CellAction.Value?.ToString() == "true") return;

                        DataGridViewEx gv = this.DataGridView as DataGridViewEx;
                        foreach (var rowModel in gv.RowActionModels)
                        {
                            var radioAction = rowModel.Actions.Find(a => a.ActionType == DataGridViewEx.RowActionType.RadioButton);
                            if (radioAction != null && radioAction.Key == rectData.CellAction.Key)
                            {
                                if (rowModel.RowIndex == e.RowIndex)
                                    radioAction.Value = "true";
                                else
                                    radioAction.Value = "false"; //将其他行的单选按钮状态改为未选中状态
                            }
                        }

                        this.DataGridView.InvalidateColumn(e.ColumnIndex);
                        actionEventName = "RowAction_RadioButtonClick";
                    }
                }
                else if (rectData.CellAction.ActionType == DataGridViewEx.RowActionType.ImageButton)
                {
                    if (rectData.CellAction.Enabled && rectData.ActionRectangle.Contains(p))
                        actionEventName = "RowAction_ImageButtonClick";
                }

                if (!string.IsNullOrEmpty(actionEventName)) //触发ZoncareDataGrid的行编辑事件
                {
                    DataGridViewEx gv = this.DataGridView as DataGridViewEx;

                    //Type t = Type.GetType(gv.GetType().AssemblyQualifiedName);
                    MulticastDelegate customEvents = (MulticastDelegate)gv.GetType()
                        .GetField(actionEventName, BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(gv);
                    if (customEvents != null)
                    {
                        Delegate[] delegates = customEvents.GetInvocationList();
                        foreach (Delegate dlg in delegates)
                        {
                            dlg.Method.Invoke(dlg.Target, new object[] { gv, new DataGridViewActionCellMouseEventArgs(rectData.CellAction, e) });
                        }
                    }
                    break;
                }
            }
        }

        private FontStyle getActionFontStyle(DataGridViewEx.GridDataAction action, FontStyle desiredFontStyle)
        {
            DataGridViewEx.RowActionType actionType = action.ActionType;
            FontStyle fontStyle = desiredFontStyle;

            bool fontStyleIsFixed = (action.ContentStyle?.Font != null);
            if (actionType == DataGridViewEx.RowActionType.TextButton)
            {
                if (fontStyleIsFixed) fontStyle = action.ContentStyle.Font.Style;
            }

            return fontStyle;
        }

        //获取所有操作按钮区域的总宽度(包括间隙)
        private int getActionTotalWidth(Graphics g, DataGridView gv, Rectangle cellBounds, DataGridViewCellStyle cellStyle)
        {
            int gap = (gv.Columns[this.ColumnIndex] as DataGridViewActionColumn).ActionGap;
            int elementWidth = 0, totalWidth = 0;
            int j = 0;
            Point location = new Point(cellBounds.X, cellBounds.Y);
            foreach (DataGridViewEx.GridDataAction action in actionModel.Actions)
            {
                RectangleF rectAction = RectangleF.Empty;
                RectangleData rectData = actionRectangle[j];
                if (rectData.RegionFontStyle == null) rectData.RegionFontStyle = cellStyle.Font.Style; //初始化为所在单元格的字体样式

                Font textFont = new Font(cellStyle.Font, rectData.RegionFontStyle.Value);
                if (action.ContentStyle != null)
                {
                    if (action.ContentStyle.Font != null) textFont = action.ContentStyle.Font;
                }

                SizeF textSize = g.MeasureString(action.DisplayText, textFont);
                switch (action.ActionType)
                {
                    case DataGridViewEx.RowActionType.TextButton:
                        location.Y = cellBounds.Y + (cellBounds.Height - (int)textSize.Height) / 2;
                        rectAction = new RectangleF(location, textSize);

                        break;
                    case DataGridViewEx.RowActionType.ImageButton:
                        Image img = null;
                        if (action.Value is string)
                        {
                            string imgFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, action.Value.ToString());
                            if (File.Exists(imgFilePath)) { img = Image.FromFile(imgFilePath); }
                        }
                        else if (action.Value is Image)
                        {
                            img = action.Value as Image;
                        }

                        if (img != null)
                        {
                            location.Y = cellBounds.Y + (cellBounds.Height - img.Height) / 2;
                            rectAction = new RectangleF(location, new SizeF(img.Width, img.Height));
                        }
                        else
                        {
                            location.Y = cellBounds.Y + (cellBounds.Height - 16) / 2;
                            rectAction = new RectangleF(location, new SizeF(16, 16));
                        }

                        break;
                    case DataGridViewEx.RowActionType.RadioButton:
                        int rectHeight = (int)textSize.Height - 8;
                        location.Y = cellBounds.Y + (cellBounds.Height - rectHeight) / 2;
                        Rectangle rectRadio = new Rectangle(location, new Size(rectHeight, rectHeight));

                        int y = cellBounds.Y + (cellBounds.Height - (int)textSize.Height) / 2;
                        Rectangle rectText = new Rectangle(location.X + rectRadio.Width + 1, y, (int)textSize.Width + 1, (int)textSize.Height);
                        rectAction = new RectangleF(location, new SizeF(rectRadio.Width + 1 + rectText.Width, textSize.Height));
                        break;
                }
                elementWidth = (int)rectAction.Width;
                totalWidth += elementWidth;
                location.X += elementWidth + gap;
                j++;
            }

            return totalWidth;
        }

        private class RectangleData
        {
            internal DataGridViewEx.GridDataAction CellAction { get; set; }
            internal RectangleF ActionRectangle { get; set; }
            internal bool ShowToolTipText { get; set; }
            internal string ToolTipText { get; set; }
            internal FontStyle? RegionFontStyle { get; set; } //存放文本样式(鼠标移动到按钮上时)
        }
    }

    /// <summary>
    /// 列表头复选框单元格
    /// </summary>
    public class DatagridViewCheckBoxHeaderCell : DataGridViewColumnHeaderCell
    {
        public delegate void CheckBoxClickedHandler(int colIndex, bool state);
        private Point checkBoxLocation;
        private Size checkBoxSize;
        private bool _checked = false;
        private Point _cellLocation = new Point();
        private CheckBoxState _cbState = CheckBoxState.UncheckedNormal;
        public event CheckBoxClickedHandler OnCheckBoxClicked;

        public DatagridViewCheckBoxHeaderCell()
        {
        }

        protected override bool SetValue(int rowIndex, object value)
        {
            if (value is bool)
            {
                _checked = (bool)value;
                this.DataGridView?.InvalidateCell(this);
            }
            return base.SetValue(rowIndex, value);
        }

        protected override void Paint(Graphics graphics,
            Rectangle clipBounds,
            Rectangle cellBounds,
            int rowIndex,
            DataGridViewElementStates dataGridViewElementState,
            object value,
            object formattedValue,
            string errorText,
            DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, dataGridViewElementState, value,
                formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);

            Point p = new Point();
            Size s = CheckBoxRenderer.GetGlyphSize(graphics, CheckBoxState.UncheckedNormal);
            p.X = cellBounds.Left + (cellBounds.Width - s.Width) / 2;
            p.Y = cellBounds.Top + (cellBounds.Height - s.Height) / 2;
            _cellLocation = cellBounds.Location;
            checkBoxLocation = p;
            checkBoxSize = s;
            if (_checked)
                _cbState = CheckBoxState.CheckedNormal;
            else
                _cbState = CheckBoxState.UncheckedNormal;

            CheckBoxRenderer.DrawCheckBox(graphics, checkBoxLocation, _cbState);
        }

        protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
        {
            Point p = new Point(e.X + _cellLocation.X, e.Y + _cellLocation.Y);
            if (p.X >= checkBoxLocation.X && p.X <= checkBoxLocation.X + checkBoxSize.Width
                && p.Y >= checkBoxLocation.Y && p.Y <= checkBoxLocation.Y + checkBoxSize.Height)
            {
                _checked = !_checked;
                if (OnCheckBoxClicked != null)
                {
                    OnCheckBoxClicked(e.ColumnIndex, _checked);
                    this.DataGridView.InvalidateCell(this);
                }

            }
            base.OnMouseClick(e);
        }
    }

    /// <summary>
    /// 可编辑文本单元格的文本显示样式(指定格式字符串及相应的文本域样式, 跨列使用)
    /// </summary>
    public class DataGridViewTextCellStyle : DataGridViewCellStyle
    {
        /// <summary>
        /// 为单元格指定其中文本显示样式的格式化信息
        /// </summary>
        /// <param name="cellStyle">单元格原始格式信息</param>
        /// <param name="textStyleFormat">自定义文本格式化表达式</param>
        public DataGridViewTextCellStyle(DataGridViewCellStyle cellStyle, string textStyleFormat) : base(cellStyle)
        {
            this.StyleFormatText = textStyleFormat;
            this.OriginalStyle = cellStyle;
        }

        /// <summary>
        /// 格式化字符串, 用于给单元格文本中的一个或多个连续字符指定其自定义文本样式(TextStyle)的序号
        /// e.g. "AABBCC{0:DD}EE{1:FF}GG"
        /// </summary>
        public string StyleFormatText { get; set; }

        /// <summary>
        /// 自定义文本样式集合
        /// </summary>
        public List<TextStyle> TextStyles { get; set; }

        /// <summary>
        /// 单元格原始格式
        /// </summary>
        public DataGridViewCellStyle OriginalStyle { get; private set; }

        /// <summary>
        /// 是否在单元格中显示虚线(0: 不显示; 1: 在上方显示; 2: 显示在中间; 3: 在下方显示)
        /// </summary>
        public int DashLineStyle { get; set; } = 0;

        /// <summary>
        /// 鼠标悬浮到文本所在单元格时显示的信息
        /// </summary>
        public string ToopTipText { get; set; }

        /// <summary>
        /// 左侧填充字符(e.g. 填充空格造成文本缩进效果)
        /// </summary>
        public string LeftPaddings { get; set; }

        /// <summary>
        /// 多行文本显示时是否应用不同样式以形成对比
        /// </summary>
        public bool MultiLineContrast { get; set; }

        /// <summary>
        /// 重置文本格式
        /// </summary>
        public void ResetStyle()
        {
            this.StyleFormatText = "";
            this.TextStyles.Clear();
        }

        /// <summary>
        /// 自定义文本样式
        /// </summary>
        public class TextStyle
        {
            /// <summary>
            /// 对应于文本格式表达式中的序号
            /// </summary>
            public int StyleIndex { get; set; }
            /// <summary>
            /// 文本内容
            /// </summary>
            public string Text { get; set; }
            /// <summary>
            /// 格式化文本的文字字体
            /// </summary>
            public Font TextFont { get; set; }
            /// <summary>
            /// 字体颜色
            /// </summary>
            public Color ForeColor { get; set; }
            /// <summary>
            /// 背景色
            /// </summary>
            public Color BackColor { get; set; }

            public TextStyle(DataGridViewCellStyle baseStyle)
            {
                this.TextFont = baseStyle.Font;
                this.ForeColor = baseStyle.ForeColor;
                this.BackColor = baseStyle.BackColor;
            }
        }
    }

    /// <summary>
    /// "操作列"鼠标事件参数
    /// </summary>
    public class DataGridViewActionCellMouseEventArgs
    {
        public DataGridViewEx.GridDataAction CellAction { get; set; }
        public DataGridViewCellMouseEventArgs CellMouseEventArgs { get; set; }

        public DataGridViewActionCellMouseEventArgs(DataGridViewEx.GridDataAction action, DataGridViewCellMouseEventArgs mouseEventArgs)
        {
            this.CellAction = action;
            this.CellMouseEventArgs = mouseEventArgs;
        }
    }

    #endregion

}
