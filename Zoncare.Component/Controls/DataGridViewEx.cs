using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Zoncare.Utilities;

namespace Zoncare.Component.Controls
{
    public class DataGridViewEx : DataGridView
    {
        private List<GridDataAction> m_RowActions = new List<GridDataAction>(); //操作列[类型的集合] 
        private Color[] m_OriginalRowBackColors; //行背景色
        public delegate void RowDataActionEventHandler(object sender, DataGridViewActionCellMouseEventArgs e);

        public DataGridViewEx()
        {
            //this.SetStyle(
            //    ControlStyles.UserPaint |
            //    ControlStyles.DoubleBuffer |
            //    ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint, true);
        }

        [Description("点击操作列中的文本按钮时发生"), Category("自定义事件")]
        public event RowDataActionEventHandler RowAction_TextButtonClick;

        [Description("点击操作列中的图片按钮时发生"), Category("自定义事件")]
        public event RowDataActionEventHandler RowAction_ImageButtonClick;

        [Description("点击操作列中的单选按钮时发生"), Category("自定义事件")]
        public event RowDataActionEventHandler RowAction_RadioButtonClick;

        [DefaultValue(false), Category("自定义属性"), Description("是否显示自定义的奇偶行背景色"), Browsable(true)]
        public bool AlternateRowColor { get; set; } = false;

        [DefaultValue(false), Category("自定义属性"), Description("是否使鼠标当前悬停的行突出显示"), Browsable(true)]
        public bool HighlightRowHover { get; set; } = false;

        [DefaultValue(false), Category("自定义属性"), Description("是否显示选中行或单元格的颜色效果"), Browsable(true)]
        public bool ShowSelectionColor { get; set; } = false;

        [DefaultValue(false), Category("自定义属性"), Description("设置是否显示行号"), Browsable(true)]
        public bool ShowRowNumber { get; set; } = false;

        [DefaultValue(false), Category("自定义属性"), Description("设置是否绘制自定义的行表头和列表头背景色(蓝色调)"), Browsable(true)]
        public bool ShowCustomBackColorStyle { get; set; }

        [DefaultValue(""), Category("自定义属性"), Description("指定[操作]列的列表头文本"), Browsable(true)]
        public string ActionColumnHeaderText { get; set; }

        [DefaultValue(""), Category("自定义属性"), Description("绑定到DateTime类型属性列的文本显示格式")]
        public string CustomDateTimeFormat { get; set; }

        [DefaultValue(null), Category("自定义属性"), Description("定义[操作]列包含的各种操作的集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<GridDataAction> DataActionMode
        {
            get { return m_RowActions; }
            set
            {
                m_RowActions = value;
                AddActionColumn(m_RowActions);

                resetAllRowActions();
            }
        }

        /// <summary>
        /// 所有行的操作列模式的集合
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<RowActionModel> RowActionModels { get; } = new List<RowActionModel>();

        /// <summary>
        /// 操作列
        /// </summary>
        [Browsable(false)]
        public DataGridViewActionColumn ActionColumn { get; private set; }

        /// <summary>
        /// 设置操作按钮所在列的宽度,没有操作列则不用设置
        /// </summary>
        [Browsable(false)]
        public int ActionColumnWidth { get; set; }

        /// <summary>
        /// 为指定的行设置操作按钮显示方式
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="actions"></param>
        public void SetRowActions(int rowIndex, List<GridDataAction> actions)
        {
            RowActionModel model = this.RowActionModels.Find(o => o.RowIndex == rowIndex);
            if (model != null)
            {
                model.Actions = actions;
                model.UserDefined = true;

                setMinWidthOfActionCol();
            }
        }

        /// <summary>
        /// 增加复选框列
        /// </summary>
        /// <param name="checkboxHeaderClicked">列表头的复选框点击事件</param>
        public DataGridViewCheckBoxColumn AddCheckBoxHeaderColumn(DatagridViewCheckBoxHeaderCell.CheckBoxClickedHandler checkboxHeaderClicked)
        {
            DatagridViewCheckBoxHeaderCell cbHeader = new DatagridViewCheckBoxHeaderCell();
            cbHeader.OnCheckBoxClicked += checkboxHeaderClicked;
            DataGridViewCheckBoxColumn chbCol = new DataGridViewCheckBoxColumn()
            {
                Name = "chk",
                HeaderText = " ",
                HeaderCell = cbHeader,
                MinimumWidth = 30
            };
            this.Columns.Add(chbCol);

            chbCol.DefaultCellStyle.NullValue = false;
            cbHeader.Value = false;
            return chbCol;
        }

        /// <summary>
        /// 增加数据操作列
        /// </summary>
        /// <param name="gridActions">操作类型</param>
        public DataGridViewActionColumn AddActionColumn(List<GridDataAction> gridActions, string columnHeaderText = "")
        {
            string headerText = "";
            if (!string.IsNullOrEmpty(columnHeaderText))
                headerText = columnHeaderText;
            else
                headerText = string.IsNullOrEmpty(this.ActionColumnHeaderText) ? "" : this.ActionColumnHeaderText;

            //不存在此类型操作列则增加
            DataGridViewActionColumn actionCol = getActionColumn();
            if (actionCol == null)
            {
                actionCol = new DataGridViewActionColumn(gridActions) { Name = "Operations", HeaderText = headerText };
                actionCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                actionCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                this.Columns.Add(actionCol);
                this.ActionColumn = actionCol;

                setMinWidthOfActionCol();
            }

            return actionCol;
        }


        #region 跨列合并单元格、表头或指定列的内容 =======================================================
        [MergableProperty(false)]
        [Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Localizable(true)]
        [Description("设置或获取需合并各行单元格的列的集合"), Browsable(true), Category("单元格合并")]
        public List<string> MergeColumnNames { get; set; } = new List<string>();

        [Description("合并的列表头背景颜色"), Browsable(true), Category("单元格合并")]
        public Color SpanColumnHeaderBackColor { get; set; } = Color.Empty;

        private Dictionary<int, List<SpanColInfo>> spanCols = new Dictionary<int, List<SpanColInfo>>();//需要合并的数据行中的列

        /// <summary>
        /// 合并列表头
        /// </summary>
        /// <param name="colIndex">列的索引</param>
        /// <param name="colCount">需要合并的列数</param>
        /// <param name="text">合并列后的文本</param>
        public void AddSpanHeader(int colIndex, int colCount, string text)
        {
            if (this.SpanColumnHeaderBackColor == Color.Empty)
                this.SpanColumnHeaderBackColor = this.ColumnHeadersDefaultCellStyle.BackColor;

            if (colCount < 2)
            {
                //throw new Exception("合并1列无意义。");
                return;
            }

            List<SpanColInfo> lstSpanHeader = new List<SpanColInfo>(colCount);
            int rCol = colIndex + colCount - 1; //同一大标题下的最后一列的索引
            if (!spanCols.ContainsKey(-1)) spanCols[-1] = new List<SpanColInfo>();
            spanCols[-1].Add(new SpanColInfo(-1, text, colIndex, rCol));
        }

        /// <summary>
        /// 合并指定的行中跨列的单元格
        /// </summary>
        public void AddSpanCol(int rowIndex, int colIndex, int colCount, string text)
        {
            int rCol = colIndex + colCount - 1;
            if (!spanCols.ContainsKey(rowIndex)) spanCols[rowIndex] = new List<SpanColInfo>();
            spanCols[rowIndex].Add(new SpanColInfo(rowIndex, text, colIndex, rCol));
        }

        /// <summary>
        /// 清除跨列合并设置项
        /// </summary>
        public void ClearSpanInfo()
        {
            spanCols.Clear();
        }

        protected override void OnColumnWidthChanged(DataGridViewColumnEventArgs e)
        {
            base.OnColumnWidthChanged(e);
            foreach (var spans in spanCols)
            {
                foreach (SpanColInfo spanInfo in spans.Value)
                {
                    if (spanInfo.RowIndex < this.Rows.Count && spanInfo.ColCellBoundFlags.ContainsKey(e.Column.DisplayIndex))
                    {
                        //this.InvalidateRow(spanInfo.RowIndex);
                        foreach (int col in spanInfo.ColCellBoundFlags.Keys)
                        {
                            this.InvalidateCell(col, spanInfo.RowIndex);
                            //this.Invalidate(this.GetCellDisplayRectangle(col, spanColInfo.RowIndex, true));
                        }
                    }
                }
            }
        }

        protected override void OnScroll(ScrollEventArgs e)
        {
            base.OnScroll(e);
            foreach (var spans in spanCols.Values)
            {
                foreach (SpanColInfo spanInfo in spans)
                {
                    if (spanInfo.ColCellBoundFlags.Count > 0)
                    {
                        if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll)// && e.Type == ScrollEventType.EndScroll)
                        {
                            foreach (int ci in spanInfo.ColCellBoundFlags.Keys)
                            {
                                this.Invalidate(this.GetCellDisplayRectangle(ci, spanInfo.RowIndex, true));
                            }
                        }
                    }
                }
            }
        }

        #endregion =====================================================================================


        /// <summary>
        /// 自定义设置每一列的宽度比例(不含[操作]列)
        /// </summary>
        /// <param name="lstWidth">每一列的宽度比例(不包含自定义的操作选项),包含元素总数为不包含操作列的列表总列数</param>
        public void ResizeColumnWidth(List<int> lstWidth = null)
        {
            int totalWidth = 0; //除[操作]列以外的列宽度权重总和
            if (lstWidth != null) totalWidth = lstWidth.Sum();

            if (this.Columns.Count > 0)
            {
                if (this.ActionColumn != null)
                {
                    setMinWidthOfActionCol();
                    if (ActionColumnWidth == 0)
                        this.ActionColumn.Width = this.ActionColumn.MinimumWidth;
                    else
                        this.ActionColumn.Width = ActionColumnWidth;
                }

                for (int i = 0; i < this.Columns.Count; i++)
                {
                    if (Columns[i] is DataGridViewActionColumn)
                    {
                        continue;
                    }
                    else
                    {
                        if (0 == totalWidth) break;

                        int realTotalWidth = this.Width - ((this.ActionColumn?.Width) ?? 0);
                        if (i < lstWidth.Count)
                        {
                            if (lstWidth[i] == 0)
                                this.Columns[i].Visible = false;
                            else
                                this.Columns[i].Width = Convert.ToInt32((float)realTotalWidth * lstWidth[i] / totalWidth - 1);
                        }
                    }
                }
            }
        }

        public void ResizeFont(float newHeight)
        {
            if (newHeight <= 0.8f) newHeight = 0.8f;

            Single sCHDCS = Convert.ToSingle(this.ColumnHeadersDefaultCellStyle.Font.Size) * newHeight;
            this.ColumnHeadersDefaultCellStyle.Font = new Font(this.ColumnHeadersDefaultCellStyle.Font.Name, sCHDCS, this.ColumnHeadersDefaultCellStyle.Font.Unit);

            Single sDCS = Convert.ToSingle(this.DefaultCellStyle.Font.Size) * newHeight;
            this.DefaultCellStyle.Font = new Font(this.DefaultCellStyle.Font.Name, sDCS, this.DefaultCellStyle.Font.Unit);

            this.ColumnHeadersHeight = Math.Max(4, (int)(this.ColumnHeadersHeight * newHeight));
            this.RowTemplate.Height = (int)(this.RowTemplate.Height * newHeight);
        }

        protected override void OnCreateControl()
        {
            this.AllowUserToAddRows = false;
            this.AllowUserToDeleteRows = false;

            //this.AllowUserToOrderColumns = true;
            this.AllowUserToResizeRows = false;

            this.EnableHeadersVisualStyles = false;
            this.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.RowHeadersVisible = false;
            this.ColumnHeadersHeight = 36;

            this.RowTemplate.Height = 30;
            //this.RowTemplate.MinimumHeight = 30;

            this.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            this.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(238, 245, 251);
            this.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.GridColor = SystemColors.GradientInactiveCaption;
            this.BackgroundColor = Color.White;
            this.DefaultCellStyle.BackColor = Color.White;
            this.DefaultCellStyle.SelectionForeColor = SystemColors.ControlText;
            this.DefaultCellStyle.SelectionBackColor = Color.Lavender; //Color.FromArgb(0, 215, 0);

            this.AutoGenerateColumns = false;

            base.OnCreateControl();
        }

        //移到单元格时的颜色
        protected override void OnCellMouseMove(DataGridViewCellMouseEventArgs e)
        {
            base.OnCellMouseMove(e);
            if (HighlightRowHover && e.RowIndex >= 0)
            {
                Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.AliceBlue;
                //Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(236, 242, 255);
                if (!ShowSelectionColor) Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Rows[e.RowIndex].DefaultCellStyle.BackColor;
            }

            
            if (this.Columns[e.ColumnIndex] is DataGridViewActionColumn) return;

            this.Cursor = Cursors.Default;
            if (e.RowIndex >= 0)
            {
                DataGridViewCell curCell = this.Rows[e.RowIndex].Cells[e.ColumnIndex];
                if (curCell is DataGridViewCheckBoxCell)
                {
                    //Rectangle cellBound = this.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                    //Point p = new Point(cellBound.X + e.X, cellBound.Y + e.Y);
                    if (curCell.ReadOnly && curCell.ContentBounds.Contains(e.X, e.Y)) this.Cursor = Cursors.No;
                }
            }
        }

        //离开时还原颜色
        protected override void OnCellMouseLeave(DataGridViewCellEventArgs e)
        {
            base.OnCellMouseLeave(e);
            if (HighlightRowHover && e.RowIndex >= 0)
            {
                if (m_OriginalRowBackColors != null && m_OriginalRowBackColors.Length > 0)
                {
                    Rows[e.RowIndex].DefaultCellStyle.BackColor = m_OriginalRowBackColors[e.RowIndex];
                    if (!ShowSelectionColor) Rows[e.RowIndex].DefaultCellStyle.SelectionBackColor = Rows[e.RowIndex].DefaultCellStyle.BackColor;
                }
            }
        }

        protected override void OnCellValueChanged(DataGridViewCellEventArgs e)
        {
            base.OnCellValueChanged(e);

            if (e.RowIndex > -1 && e.ColumnIndex > -1)
            {
                DataGridViewCellStyle cellStyle = this.Rows[e.RowIndex].Cells[e.ColumnIndex].Style;
                if (cellStyle is DataGridViewTextCellStyle)
                {
                    this.Rows[e.RowIndex].Cells[e.ColumnIndex].Style = ((DataGridViewTextCellStyle)cellStyle).OriginalStyle;
                }
            }
        }

        protected override void OnDataSourceChanged(EventArgs e)
        {
            //数据源改变后, 为生成的行初始化RowActionModels
            resetAllRowActions();

            if (ReadOnly)
            {
                bool hasCheckBoxCol = false;
                foreach (DataGridViewColumn col in Columns)
                {
                    if (col is DataGridViewCheckBoxColumn)
                    {
                        hasCheckBoxCol = true;
                        break;
                    }
                }
                if (hasCheckBoxCol)
                {
                    ReadOnly = false;
                    foreach (DataGridViewColumn col in this.Columns) { col.ReadOnly = !(col is DataGridViewCheckBoxColumn); }
                }
            }

            //此处将先行引发DataBindingComplete事件
            base.OnDataSourceChanged(e);

            this.Invalidate();
        }

        //This event is raised when the contents of the data source change 
        //or when the value of the DataSource, DataMember, or BindingContext property changes.
        protected override void OnDataBindingComplete(DataGridViewBindingCompleteEventArgs e)
        {
            if (e.ListChangedType != ListChangedType.ItemChanged)
            {
                //单双行变色, 并使最后一行为深色背景
                m_OriginalRowBackColors = new Color[this.Rows.Count];
                for (int i = 0; i < this.Rows.Count; i++)
                {
                    DataGridViewRow row = this.Rows[i];
                    if (AlternateRowColor)
                    {
                        if (this.Rows.Count % 2 == 0)
                        {
                            if (i % 2 != 0) //设置偶数行的背景色(index从0开始)
                                row.DefaultCellStyle.BackColor = Color.FromArgb(248, 249, 251); //Color.AliceBlue; //Color.FromArgb(246, 247, 249)
                            else
                                row.DefaultCellStyle.BackColor = Color.White;
                        }
                        else
                        {
                            if (i % 2 == 0)
                                row.DefaultCellStyle.BackColor = Color.FromArgb(248, 249, 251);
                            else
                                row.DefaultCellStyle.BackColor = Color.White;
                        }
                    }
                    m_OriginalRowBackColors[i] = row.DefaultCellStyle.BackColor;
                }

                ////显示行号
                //SizeF maxRowNumberSize = this.CreateGraphics().MeasureString(this.Rows.Count.ToString(), this.Font); //获取最大行号的大小
                //this.RowHeadersWidth = (int)Math.Ceiling(maxRowNumberSize.Width) + 30;
                //for (int i = 0; i < this.Rows.Count; i++)
                //{
                //    DataGridViewRow row = this.Rows[i];
                //    row.HeaderCell.Value = (i + 1).ToString();
                //}

                //设置自定义日期/时间属性列的显示格式
                if (!string.IsNullOrEmpty(CustomDateTimeFormat))
                {
                    foreach (DataGridViewColumn col in this.Columns)
                    {
                        if (col.ValueType != null)
                        {
                            if (col.ValueType.Equals(typeof(DateTime)) || (col.ValueType.GenericTypeArguments.Length > 0 && col.ValueType.GenericTypeArguments[0].Equals(typeof(DateTime))))
                            {
                                col.DefaultCellStyle.Format = CustomDateTimeFormat;
                            }
                        }
                    }
                }

                resetAllRowActions();
                this.CurrentCell = null;
            }

            base.OnDataBindingComplete(e);
        }

        protected override void OnRowsAdded(DataGridViewRowsAddedEventArgs e)
        {
            base.OnRowsAdded(e);
            return;

            if (m_RowActions.Count > 0)
            {
                if (e.RowIndex > this.RowActionModels.Count - 1) //新增的行追加到列表尾部
                {
                    for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
                    {
                        List<GridDataAction> singleRowActions = resetSingleRowActions();
                        this.RowActionModels.Add(new RowActionModel() { RowIndex = i, Actions = singleRowActions });
                    }
                }
                else //新增的行插入到列表中间
                {
                    int i = e.RowIndex;
                    while (i < e.RowIndex + e.RowCount)
                    {
                        List<GridDataAction> singleRowActions = resetSingleRowActions();
                        this.RowActionModels.Insert(i, new RowActionModel() { RowIndex = i, Actions = singleRowActions });
                        i++;
                    }
                    for (int j = i; j < this.RowActionModels.Count; j++)
                    {
                        if (j >= this.Rows.Count)
                        {
                            this.RowActionModels.RemoveRange(j, this.RowActionModels.Count - j);
                            break;
                        }
                        else
                        {
                            this.RowActionModels[j].RowIndex = j;
                        }
                    }
                }
            }
        }

        protected override void OnRowsRemoved(DataGridViewRowsRemovedEventArgs e)
        {
            base.OnRowsRemoved(e);
        }

        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            base.OnCellPainting(e);

            //合并单元格, 跨列显示, 合并列表头
            if (e.RowIndex > -1 && e.ColumnIndex > -1)
            {
                //if (this.CellBorderStyle == DataGridViewCellBorderStyle.None)
                //{
                //    e.AdvancedBorderStyle.All = DataGridViewAdvancedCellBorderStyle.None;
                //}
                drawMergeCellsOfColumn(e);
                drawColSpanCells(e);
                drawCellDashLine(e, e.Value?.ToString());
            }
            else
            {
                drawMergeColumnHeaders(e);
            }

            //绘制自定义行、列标头颜色
            if (this.ShowCustomBackColorStyle)
            {
                if (e.RowIndex == -1 || (e.ColumnIndex < 0 && e.RowIndex >= 0))
                {
                    drawColumnAndRowHeader(e);
                    e.Handled = true;
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            this.drawRowNumber(this, e.Graphics);

            if (this.ColumnHeadersBorderStyle == DataGridViewHeaderBorderStyle.None)
            {
                foreach (DataGridViewColumn column in this.Columns)
                {
                    if (column.Visible)
                    {
                        Rectangle headerRect = this.GetCellDisplayRectangle(column.Index, -1, false);
                        using (Pen borderPen = new Pen(Color.LightGray, 1))
                        {
                            // 绘制右边框
                            int spliterHeight = 20;
                            int margin = (headerRect.Height - spliterHeight) / 2;
                            e.Graphics.DrawLine(borderPen,
                                headerRect.Right - 1, headerRect.Top + margin,
                                headerRect.Right - 1, headerRect.Bottom - margin);
                        }
                    }
                }
            }
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            ResizeColumnWidth();
        }


        /// <summary>
        /// Column和Row Header绘制背景色(蓝色调) -- 逐单元格
        /// </summary>
        /// <param name="e"></param>
        private void drawColumnAndRowHeader(DataGridViewCellPaintingEventArgs e)
        {
            using (LinearGradientBrush backbrush = new LinearGradientBrush(e.CellBounds, Color.FromArgb(191, 215, 249), Color.FromArgb(159, 191, 249), LinearGradientMode.Vertical))
            {
                Rectangle border = e.CellBounds;
                border.Width -= 1;
                //填充绘制效果
                e.Graphics.FillRectangle(backbrush, border);

                //绘制Column、Row的Text信息
                e.PaintContent(e.CellBounds);

                //绘制边框
                ControlPaint.DrawBorder3D(e.Graphics, e.CellBounds, Border3DStyle.RaisedInner);
            }
        }

        /// <summary>
        /// 绘制合并的二维表头(横向合并)
        /// </summary>
        /// <param name="e"></param>
        private void drawMergeColumnHeaders(DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                if (!spanCols.ContainsKey(e.RowIndex)) return;
                SpanColInfo spanInfo = spanCols[e.RowIndex].FirstOrDefault(o => o.ColCellBoundFlags.ContainsKey(e.ColumnIndex));
                if (spanInfo == null) return;

                //画边框
                Graphics g = e.Graphics;
                e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border);

                int left = e.CellBounds.Left, top = e.CellBounds.Top + 2,
                right = e.CellBounds.Right, bottom = e.CellBounds.Bottom;

                switch (spanInfo.ColCellBoundFlags[e.ColumnIndex])
                {
                    case 1:
                        left += 2;
                        break;
                    case 2:
                        break;
                    case 3:
                        right -= 2;
                        break;
                }

                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                //画上半部分底色
                g.FillRectangle(new SolidBrush(this.SpanColumnHeaderBackColor), left, top, right - left, (bottom - top) / 2);

                //画中线
                g.DrawLine(new Pen(this.GridColor), left, (top + bottom) / 2, right, (top + bottom) / 2);

                //写小标题
                g.DrawString(e.Value + "", e.CellStyle.Font, Brushes.Black,
                    new Rectangle(left, (top + bottom) / 2, right - left, (bottom - top) / 2), sf);

                left = this.GetColumnDisplayRectangle(spanInfo.LeftCol, true).Left - 2;
                if (left < 0) left = this.GetCellDisplayRectangle(-1, -1, true).Width;

                right = this.GetColumnDisplayRectangle(spanInfo.RightCol, true).Right - 2;
                if (right < 0) right = this.Width;

                //绘制合并后的表头标题
                g.DrawString(spanInfo.Text, e.CellStyle.Font, Brushes.Black,
                    new Rectangle(left, top, right - left, (bottom - top) / 2), sf);

                ////绘制合并后的表头标题, 不显示小标题
                //g.DrawString(SpanCols[e.ColumnIndex].Text, e.CellStyle.Font, Brushes.Black, new Rectangle(left, top, right - left, e.CellBounds.Height), sf);

                e.Handled = true;

            }
        }

        /// <summary>
        /// 绘制数据行中跨列的单元格
        /// </summary>
        /// <param name="e"></param>
        private void drawColSpanCells(DataGridViewCellPaintingEventArgs e)
        {
            if (!spanCols.ContainsKey(e.RowIndex)) return;
            SpanColInfo spanInfo = spanCols[e.RowIndex].FirstOrDefault(o => o.ColCellBoundFlags.ContainsKey(e.ColumnIndex));
            if (spanInfo == null) return;

            //画边框
            e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border);

            Graphics g = e.Graphics;
            int left = e.CellBounds.Left, top = e.CellBounds.Top + e.CellStyle.Padding.Top + 1,
                right = e.CellBounds.Right - 1, bottom = e.CellBounds.Bottom - e.CellStyle.Padding.Bottom;

            StringFormat sf = new StringFormat()
            {
                Alignment = StringAlignment.Near,       //水平向左对齐
                LineAlignment = StringAlignment.Center,    //垂直向下对齐
                FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.MeasureTrailingSpaces,
                Trimming = StringTrimming.EllipsisCharacter
            };

            if (e.CellStyle.Alignment == (e.CellStyle.Alignment &
                (DataGridViewContentAlignment.TopCenter | DataGridViewContentAlignment.MiddleCenter | DataGridViewContentAlignment.BottomCenter)))
            {
                sf.Alignment = StringAlignment.Center; //水平居中
            }
            if (e.CellStyle.Alignment == (e.CellStyle.Alignment &
                (DataGridViewContentAlignment.MiddleLeft | DataGridViewContentAlignment.MiddleCenter | DataGridViewContentAlignment.MiddleRight)))
            {
                sf.LineAlignment = StringAlignment.Center; //垂直居中
            }

            left = this.GetColumnDisplayRectangle(spanInfo.LeftCol, false).Left;
            if (left < 0) left = this.GetCellDisplayRectangle(-1, -1, false).Width;

            right = this.GetColumnDisplayRectangle(spanInfo.RightCol, false).Right - 1;
            if (right < 0) right = this.Width;

            //绘制合并后的单元格文本
            RectangleF textArea = new RectangleF(left, top, right - left, bottom - top);
            using (Brush backColorBrush = new SolidBrush(e.CellStyle.BackColor))
            {
                g.FillRectangle(backColorBrush, left, top, right - left, bottom - top); //抹掉当前行相邻合并单元格的文本，并覆盖掉中间的网格线
            }

            //g.SetClip(textArea);
            //g.SmoothingMode = SmoothingMode.HighQuality;
            //g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            string toolTipText = "";
            float charPos = left + 1;
            RectangleF textRect = RectangleF.Empty;
            if (e.CellStyle is DataGridViewTextCellStyle)
            {
                DataGridViewTextCellStyle cellTextStyle = (DataGridViewTextCellStyle)e.CellStyle;
                DataGridViewCellStyle originalStyle = cellTextStyle.OriginalStyle;

                if (!string.IsNullOrEmpty(cellTextStyle.StyleFormatText))
                {
                    string realText = cellTextStyle.LeftPaddings;
                    Regex regexStyle = new Regex(@"{\d+:.+?}");
                    int offset = 0; //字符位置

                    using (Brush bUnmatch = new SolidBrush(e.CellStyle.ForeColor))
                    {
                        foreach (Match match in regexStyle.Matches(cellTextStyle.StyleFormatText))
                        {
                            string[] styleContent = match.Value.Trim("{}".ToArray()).Split(new char[] { ':' }, 2);
                            int styleIndex = Convert.ToInt32(styleContent[0]);
                            DataGridViewTextCellStyle.TextStyle textStyle = cellTextStyle.TextStyles.Find(o => o.StyleIndex == styleIndex);
                            if (textStyle == null) textStyle = new DataGridViewTextCellStyle.TextStyle(e.CellStyle);

                            string unMatchedStr = cellTextStyle.StyleFormatText.Substring(offset, match.Index - offset);
                            offset = match.Index + match.Value.Length;

                            //绘制未匹配上的字符(未自定义文本样式)
                            SizeF textSize = g.MeasureString(unMatchedStr, originalStyle.Font, new SizeF(0, 0), sf);
                            textRect = new RectangleF(charPos, top, textSize.Width, bottom - top);
                            g.DrawString(unMatchedStr, originalStyle.Font, bUnmatch, textRect, sf);
                            realText += unMatchedStr;
                            charPos += textSize.Width;

                            using (Brush brushMatched = new SolidBrush(textStyle.ForeColor))
                            {
                                //绘制匹配上的字符(自定义文本样式)
                                textSize = g.MeasureString(styleContent[1], textStyle.TextFont, new SizeF(0, 0), sf);
                                textRect = new RectangleF(charPos, top, textSize.Width, bottom - top);
                                g.DrawString(styleContent[1], textStyle.TextFont, brushMatched, textRect, sf);
                                realText += styleContent[1];
                                charPos += textSize.Width;
                            }
                        }

                        //绘制剩余的字符
                        if (offset < cellTextStyle.StyleFormatText.Length)
                        {
                            string unMatchedStr = cellTextStyle.StyleFormatText.Substring(offset);
                            SizeF textSize = g.MeasureString(unMatchedStr, originalStyle.Font, new SizeF(0, 0), sf);
                            textRect = new RectangleF(charPos, top, right - charPos, bottom - top);
                            g.DrawString(unMatchedStr, originalStyle.Font, bUnmatch, textRect, sf);
                            realText += unMatchedStr;
                            charPos += textSize.Width;
                        }
                    }
                    spanInfo.Text = realText;
                }
                else
                {
                    spanInfo.Text = cellTextStyle.LeftPaddings + spanInfo.Text;
                    using (Brush foreColorBrush = new SolidBrush(originalStyle.ForeColor))
                    {
                        SizeF textSize = g.MeasureString(spanInfo.Text, originalStyle.Font, new SizeF(0, 0), sf);
                        g.DrawString(spanInfo.Text, originalStyle.Font, foreColorBrush, textArea, sf);
                        charPos += textSize.Width;
                    }
                }

                //显示自定义的提示信息
                if (!string.IsNullOrEmpty(cellTextStyle.ToopTipText))
                {
                    this.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = toolTipText = cellTextStyle.ToopTipText;
                }
            }
            else
            {
                using (Brush foreColorBrush = new SolidBrush(e.CellStyle.ForeColor))
                {
                    SizeF textSize = g.MeasureString(spanInfo.Text, e.CellStyle.Font, new SizeF(0, 0), sf);
                    g.DrawString(spanInfo.Text, e.CellStyle.Font, foreColorBrush, textArea, sf);
                    charPos += textSize.Width;
                }
            }

            //根据单元格内文本长度控制提示信息的显示
            if (string.IsNullOrEmpty(toolTipText))
            {
                if (charPos > textArea.Width)
                {
                    this.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = spanInfo.Text;
                }
                else
                {
                    this.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = "";
                }
            }

            //g.ResetClip();
            e.Handled = true; //指示当前单元格已经绘制完毕不需继续绘制(若系统继续绘制则会覆盖自行绘制的效果)
        }

        /// <summary>
        /// 绘制当前数据列中需合并的各行单元格
        /// </summary>
        /// <param name="e"></param>
        private void drawMergeCellsOfColumn(DataGridViewCellPaintingEventArgs e)
        {
            if (this.MergeColumnNames == null || !this.MergeColumnNames.Contains(this.Columns[e.ColumnIndex].Name)) return;

            //if (e.CellStyle.Alignment == DataGridViewContentAlignment.NotSet) { e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; }
            using (Brush gridBrush = new SolidBrush(this.GridColor))
            {
                int cellwidth;
                //上面相同的行数
                int upRows = 0;
                //下面相同的行数
                int downRows = 0;
                //总行数
                int count = 0;
                if (e.RowIndex > -1)
                {
                    cellwidth = e.CellBounds.Width;
                    Pen gridLinePen = new Pen(gridBrush);

                    string curValue = e.Value == null ? "" : e.Value.ToString().Trim();
                    if (!string.IsNullOrEmpty(curValue))
                    {
                        #region 获取下面的行数
                        for (int i = e.RowIndex; i < this.Rows.Count; i++)
                        {
                            if (this.Rows[i].Cells[e.ColumnIndex].Value.ToString().Equals(curValue))
                            {
                                //this.Rows[i].Cells[e.ColumnIndex].Selected = this.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected;
                                downRows++;
                                if (e.RowIndex != i)
                                {
                                    cellwidth = cellwidth < this.Rows[i].Cells[e.ColumnIndex].Size.Width ? cellwidth : this.Rows[i].Cells[e.ColumnIndex].Size.Width;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        #endregion

                        #region 获取上面的行数
                        for (int i = e.RowIndex; i >= 0; i--)
                        {
                            if (this.Rows[i].Cells[e.ColumnIndex].Value.ToString().Equals(curValue))
                            {
                                //this.Rows[i].Cells[e.ColumnIndex].Selected = this.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected;
                                upRows++;
                                if (e.RowIndex != i)
                                {
                                    cellwidth = cellwidth < this.Rows[i].Cells[e.ColumnIndex].Size.Width ? cellwidth : this.Rows[i].Cells[e.ColumnIndex].Size.Width;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        #endregion
                        count = downRows + upRows - 1;
                        if (count < 2) { return; }
                    }

                    using (SolidBrush backBrush = new SolidBrush(e.CellStyle.BackColor),
                        fontBrush = new SolidBrush(e.CellStyle.ForeColor))
                    {
                        if (this.Rows[e.RowIndex].Selected)
                        {
                            backBrush.Color = e.CellStyle.SelectionBackColor;
                            fontBrush.Color = e.CellStyle.SelectionForeColor;
                        }
                        //以背景色填充
                        e.Graphics.FillRectangle(backBrush, e.CellBounds);
                        //绘制字符串
                        paintingMergedText(e, fontBrush, cellwidth, upRows, downRows, count);
                    }

                    //画下边线
                    if (downRows == 1)
                    {
                        e.Graphics.DrawLine(gridLinePen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right - 1, e.CellBounds.Bottom - 1);
                        count = 0;
                    }
                    // 画右边线
                    e.Graphics.DrawLine(gridLinePen, e.CellBounds.Right - 1, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Bottom);

                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// 绘制单元格内的虚线(在表格无网格线的情形下)
        /// </summary>
        /// <param name="e"></param>
        /// <param name="text"></param>
        private void drawCellDashLine(DataGridViewCellPaintingEventArgs e, string text)
        {
            string displayText = e.Value?.ToString();

            if (spanCols.ContainsKey(e.RowIndex))
            {
                SpanColInfo spanInfo = spanCols[e.RowIndex].FirstOrDefault(o => o.ColCellBoundFlags.ContainsKey(e.ColumnIndex));
                if (spanInfo != null) { displayText = spanInfo.Text; }
            }

            if (e.CellStyle is DataGridViewTextCellStyle)
            {
                DataGridViewTextCellStyle cellTextStyle = (DataGridViewTextCellStyle)e.CellStyle;
                if (cellTextStyle.DashLineStyle == 0) return;

                //绘制虚线 - 连接线/底部虚线(表格单元格无边框时使用)
                using (Pen pen = new Pen(Color.DarkGray, 0.5f))
                {
                    pen.DashPattern = new float[] { 1.5F, 4F, 1.5F, 4F };
                    switch (cellTextStyle.DashLineStyle)
                    {
                        case 1: //在单元格上方显示虚线
                            e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Top + 3, e.CellBounds.Right, e.CellBounds.Top + 3);
                            break;
                        case 2: //在单元格中间显示虚线
                            string cellText = cellTextStyle.LeftPaddings + displayText;

                            SizeF textSize = e.Graphics.MeasureString(cellText, cellTextStyle.OriginalStyle.Font, new SizeF(0, 0));
                            e.Graphics.DrawLine(pen, e.CellBounds.Left + textSize.Width + 6, e.CellBounds.Top + e.CellBounds.Height / 2, e.CellBounds.Right - 3, e.CellBounds.Top + e.CellBounds.Height / 2);
                            break;
                        case 3: //在单元格下方显示虚线
                            e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Bottom - 3, e.CellBounds.Right, e.CellBounds.Bottom - 3);
                            break;
                    }
                }
                e.Handled = true;
            }
        }

        /// <summary>
        /// //绘制行号
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="g"></param>
        private void drawRowNumber(DataGridView dgv, Graphics g)
        {
            if (!this.DesignMode)
            {
                if (!(this.RowHeadersVisible && this.ShowRowNumber)) return;

                int iRowCount;
                if (dgv.DataSource == null)
                {
                    iRowCount = -1;
                }
                else
                {  //获取总记录数
                    iRowCount = dgv.BindingContext[RuntimeHelpers.GetObjectValue(dgv.DataSource), dgv.DataMember].Count;
                }
                if (iRowCount <= 0) return;

                SizeF maxRowNumberSize = g.MeasureString(iRowCount.ToString(), dgv.Font); //获取最大行号的大小
                int iRowHeaderWidth = (int)Math.Ceiling(maxRowNumberSize.Width) + 16;
                if (iRowHeaderWidth > dgv.RowHeadersWidth)
                {
                    dgv.RowHeadersWidth = iRowHeaderWidth;
                }

                int dataAreaTop = dgv.ColumnHeadersHeight; //数据行开始呈现的顶部位置
                for (int i = 0; i <= iRowCount - 1; i++)
                {
                    Rectangle rectRow = dgv.GetRowDisplayRectangle(i, false); //获取显示行的矩形区域
                    if (rectRow.Top < dataAreaTop)
                    {
                        continue; //不绘制被挡住区域(滚动条以下)
                    }

                    float topValue = rectRow.Top + (float)(rectRow.Height - maxRowNumberSize.Height) / 2;
                    float fontWidth = g.MeasureString((i + 1).ToString(), dgv.Font).Width;
                    g.DrawString((i + 1).ToString(), dgv.Font, new SolidBrush(dgv.ForeColor), this.RowHeadersWidth - fontWidth - 2, topValue); //行号右对齐显示
                }
            }
        }

        //绘制列中需合并的单元格的显示文本
        private void paintingMergedText(DataGridViewCellPaintingEventArgs e, SolidBrush fontBrush, int cellwidth, int UpRows, int DownRows, int count)
        {
            string cellText = e.Value.ToString();
            int fontheight = (int)e.Graphics.MeasureString(cellText, e.CellStyle.Font).Height;
            int fontwidth = (int)e.Graphics.MeasureString(cellText, e.CellStyle.Font).Width;
            int cellheight = e.CellBounds.Height;

            if (e.CellStyle.Alignment == DataGridViewContentAlignment.BottomCenter)
            {
                e.Graphics.DrawString(cellText, e.CellStyle.Font, fontBrush, e.CellBounds.X + (cellwidth - fontwidth) / 2, e.CellBounds.Y + cellheight * DownRows - fontheight);
            }
            else if (e.CellStyle.Alignment == DataGridViewContentAlignment.BottomLeft)
            {
                e.Graphics.DrawString(cellText, e.CellStyle.Font, fontBrush, e.CellBounds.X, e.CellBounds.Y + cellheight * DownRows - fontheight);
            }
            else if (e.CellStyle.Alignment == DataGridViewContentAlignment.BottomRight)
            {
                e.Graphics.DrawString(cellText, e.CellStyle.Font, fontBrush, e.CellBounds.X + cellwidth - fontwidth, e.CellBounds.Y + cellheight * DownRows - fontheight);
            }
            else if (e.CellStyle.Alignment == DataGridViewContentAlignment.MiddleCenter)
            {
                e.Graphics.DrawString(cellText, e.CellStyle.Font, fontBrush, e.CellBounds.X + (cellwidth - fontwidth) / 2, e.CellBounds.Y - cellheight * (UpRows - 1) + (cellheight * count - fontheight) / 2);
            }
            else if (e.CellStyle.Alignment == DataGridViewContentAlignment.MiddleLeft)
            {
                e.Graphics.DrawString(cellText, e.CellStyle.Font, fontBrush, e.CellBounds.X, e.CellBounds.Y - cellheight * (UpRows - 1) + (cellheight * count - fontheight) / 2);
            }
            else if (e.CellStyle.Alignment == DataGridViewContentAlignment.MiddleRight)
            {
                e.Graphics.DrawString(cellText, e.CellStyle.Font, fontBrush, e.CellBounds.X + cellwidth - fontwidth, e.CellBounds.Y - cellheight * (UpRows - 1) + (cellheight * count - fontheight) / 2);
            }
            else if (e.CellStyle.Alignment == DataGridViewContentAlignment.TopCenter)
            {
                e.Graphics.DrawString(cellText, e.CellStyle.Font, fontBrush, e.CellBounds.X + (cellwidth - fontwidth) / 2, e.CellBounds.Y - cellheight * (UpRows - 1));
            }
            else if (e.CellStyle.Alignment == DataGridViewContentAlignment.TopLeft)
            {
                e.Graphics.DrawString(cellText, e.CellStyle.Font, fontBrush, e.CellBounds.X, e.CellBounds.Y - cellheight * (UpRows - 1));
            }
            else if (e.CellStyle.Alignment == DataGridViewContentAlignment.TopRight)
            {
                e.Graphics.DrawString(cellText, e.CellStyle.Font, fontBrush, e.CellBounds.X + cellwidth - fontwidth, e.CellBounds.Y - cellheight * (UpRows - 1));
            }
            else
            {
                e.Graphics.DrawString(cellText, e.CellStyle.Font, fontBrush, e.CellBounds.X + (cellwidth - fontwidth) / 2, e.CellBounds.Y - cellheight * (UpRows - 1) + (cellheight * count - fontheight) / 2);
            }
        }

        //设置"操作"列的宽度(最小值)
        private void setMinWidthOfActionCol()
        {
            if (this.ActionColumn == null) return;

            float minTextWidth = 2;
            using (Graphics g = this.CreateGraphics())
            {
                int headerW = this.ActionColumn.HeaderCell.PreferredSize.Width;
                if (headerW > minTextWidth) minTextWidth = headerW;

                foreach (RowActionModel model in this.RowActionModels)
                {
                    float w = 0;
                    foreach (GridDataAction action in model.Actions)
                    {
                        switch (action.ActionType)
                        {
                            case RowActionType.TextButton:
                                w += g.MeasureString(action.DisplayText, this.Font).Width;
                                break;
                            case RowActionType.ImageButton:
                                int wth = 16;
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

                                if (img != null) wth = img.Width;

                                w += wth;
                                break;
                            case RowActionType.RadioButton:
                                RadioButtonState state = (action.Value?.ToString() == "true" ? RadioButtonState.CheckedNormal : RadioButtonState.UncheckedNormal);
                                Size radioSize = RadioButtonRenderer.GetGlyphSize(g, state);
                                w += radioSize.Width + 1 + g.MeasureString(action.DisplayText, this.Font).Width + 1; //与绘制此复选按钮的宽度相同
                                break;
                        }
                        w += this.ActionColumn.ActionGap; //+各操作按钮之间的间隙
                    }
                    if (minTextWidth < w) minTextWidth = w;
                }
            }
            this.ActionColumn.MinimumWidth = (int)minTextWidth;
        }

        //获取"操作"列
        private DataGridViewActionColumn getActionColumn()
        {
            foreach (DataGridViewColumn col in this.Columns)
            {
                if (col is DataGridViewActionColumn)
                {
                    return col as DataGridViewActionColumn;
                }
            }
            return null;
        }

        //基于设置的RowActionMode, 重置所有行的操作模式集合
        private void resetAllRowActions()
        {
            this.RowActionModels.Clear();
            if (m_RowActions != null && m_RowActions.Count > 0)
            {
                for (int i = 0; i < this.Rows.Count; i++)
                {
                    DataGridViewRow row = this.Rows[i];

                    List<GridDataAction> singleRowActions = resetSingleRowActions();
                    this.RowActionModels.Add(new RowActionModel() { RowIndex = row.Index, Actions = singleRowActions });
                }
            }
        }

        private List<GridDataAction> resetSingleRowActions()
        {
            List<GridDataAction> singleRowActions = new List<GridDataAction>();
            if (m_RowActions != null && m_RowActions.Count > 0)
            {
                foreach (GridDataAction srcAction in m_RowActions)
                {
                    //GridDataAction action = PubFuns.DeepClone(srcAction);
                    GridDataAction action = new GridDataAction();
                    action.ActionType = srcAction.ActionType;
                    action.ContentStyle = (srcAction.ContentStyle != null ? new DataGridViewCellStyle(srcAction.ContentStyle) : null);
                    action.DisplayText = srcAction.DisplayText;
                    action.Enabled = srcAction.Enabled;
                    action.Key = srcAction.Key;
                    action.UseLinkStyle = srcAction.UseLinkStyle;
                    action.Value = srcAction.Value;
                    singleRowActions.Add(action);
                }
            }
            return singleRowActions;
        }

        /// <summary>
        /// 表格某一行中“操作”列的操作模式(文本/图标/单选按钮)
        /// </summary>
        [Serializable]
        public class RowActionModel
        {
            public int RowIndex { get; set; }
            public List<GridDataAction> Actions { get; set; }

            [DefaultValue(false)]
            public bool UserDefined { get; set; }

            public RowActionModel() { }
        }

        /// <summary>
        /// "操作"列中一个具体的操作
        /// </summary>
        [Serializable]
        public class GridDataAction
        {
            /// <summary>
            /// 指定Action的标识
            /// </summary>
            public string Key { get; set; }

            /// <summary>
            /// 操作的显示类型(文本/图标/单选按钮)
            /// </summary>
            public RowActionType ActionType { get; set; }

            /// <summary>
            /// 指定的Action文本样式
            /// </summary>
            public DataGridViewCellStyle ContentStyle { get; set; }

            /// <summary>
            /// 是否应用链接样式(蓝色字体带下划线)
            /// </summary>
            public bool UseLinkStyle { get; set; }

            /// <summary>
            /// 对于图片显示方式, 表示鼠标悬浮时的提示信息; 
            /// 对其它显示方式则为普通关联文本;
            /// </summary>
            public string DisplayText { get; set; }

            /// <summary>
            /// 对于图标显示方式, 该值为图像或图像文件路径(相对路径); 
            /// 对于RadioButton, 等同于其属性值Checked的字符串形式("true" or "false");
            /// </summary>
            public object Value { get; set; }

            /// <summary>
            /// 是否禁用
            /// </summary>
            public bool Enabled { get; set; } = true;
        }

        public enum RowActionType
        {
            TextButton = 1,
            ImageButton = 2,
            RadioButton = 3
        }

        private class SpanColInfo //跨列的信息
        {
            public SpanColInfo(int rowIndex, string text, int leftCol, int rightCol)
            {
                this.RowIndex = rowIndex;
                this.Text = text;
                this.LeftCol = leftCol;
                this.RightCol = rightCol;

                this.ColCellBoundFlags = new Dictionary<int, int>();
                for (int i = leftCol; i <= rightCol; i++)
                {
                    int flag = 2;
                    if (i == leftCol) { flag = 1; }
                    else if (i == rightCol) { flag = 3; }

                    this.ColCellBoundFlags[i] = flag;
                }
            }

            public int RowIndex;
            public string Text; //跨列显示的文本
            public int LeftCol; //对应左边列的索引
            public int RightCol; //对应右边列的索引

            //跨列涉及的所有各列位置标记，用于调整合并区域最左、最右侧单元格边距
            //key:列序号, value:(1:左，2中，3右)
            public Dictionary<int, int> ColCellBoundFlags;
        }
    }

}