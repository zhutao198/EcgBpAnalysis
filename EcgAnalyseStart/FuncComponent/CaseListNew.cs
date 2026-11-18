using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.Component.Controls;
using Zoncare.DAL;

namespace EcgBpAnalysis.FuncComponent
{
    [ToolboxItem(true)]
    public partial class CaseList : UserControl
    {
        private PersistDictionary<string> _langs;
        private ContextMenuStrip _ctxMenuStrip;
        private int? _selectedId;
        //刷新请求序号：连续触发刷新时，只有最新一次的后台查询结果会绑定到界面
        private int _refreshSeq;
        //静态信号量：所有列表的后台查询串行执行，避免多线程并发访问共享 EF 上下文（Instance）
        private static readonly SemaphoreSlim _queryGate = new SemaphoreSlim(1, 1);
        private bool _isFirstShow = true;
        private SortInfo _sortInfo; //数据列表的列排序方向
        private static object _lockObject = new object();

        public Action<examCase> UploadDataFile;
        public Action<examCase> EnterAnalysis;
        /// <summary>
        /// 刷新病例列表(object:事件源, 参数二:第几页, 参数三:是否需要重置列表复选项)
        /// </summary>
        public Action<object, int, bool> RefreshData;

        public CaseListUsage Usage { get; set; } = CaseListUsage.MainWindow;

        public enum CaseListUsage
        {
            MainWindow,
            Recycle,
            Favorite
        }

        public CaseList()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        #region 事件
        public void OnControlShown()
        {
            // 执行显示后的逻辑
            Task.Run(async () =>
            {
                await Task.Delay(100);

                //轻量 UI 初始化（列头、样式）
                this.InvokeIfRequired(() => initGrid());

                //目录扫描 + JSON 解析 + 批量写库：内部使用独立上下文（CreateIndependent），
                //可安全在后台线程执行；原先包在 Invoke 里同步跑，会阻塞 UI 数百毫秒
                try { RetrieveData(); }
                catch (Exception ex) { Logger.CreateLog(ex, "病例管理"); }

                //RefreshData 处理器内部读取界面控件构造查询后，自行移交后台取数（见 RefreshList）
                this.InvokeIfRequired(() => RefreshData?.Invoke(this, -1, true));

                _isFirstShow = false;
            });
        }

        private void CaseList_Load(object sender, EventArgs e)
        {
            _langs = CommonValues.Langs;

            if (DesignMode) return;

            init();

            if (Usage != CaseListUsage.MainWindow)
            {
                this.InvokeIfRequired(() =>
                {
                    initGrid();
                    //RefreshData?.Invoke(-1, true);
                }, true);
            }

            this.zoncarePage.PageIndexChanged += ZoncarePage_PageIndexChanged;
        }

        private void dgvData_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow curRow = this.dgvData.Rows[e.RowIndex];
            DataGridViewColumn curCol = this.dgvData.Columns[e.ColumnIndex];
            examCase bindItem = curRow.DataBoundItem as examCase;

            if (curCol.Name.Equals("statusName"))
            {
                //e.Value = SysConstants.CaseStatus[bindItem.status];
                Color foreColor = Color.FromArgb(82, 196, 26);
                if (bindItem.status == 1)
                {
                    if (!string.IsNullOrEmpty(bindItem.webSourceId)) foreColor = Color.FromArgb(220, 20, 60);
                }
                else if (bindItem.status == 2)
                {
                    foreColor = Color.FromArgb(220, 20, 60);
                }
                e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor = foreColor;
            }
            if (curCol.Name.Equals("caseNo"))
            {
                e.CellStyle.Padding = new Padding(0, 0, 20, 0);
                e.FormattingApplied = true;
            }
        }

        private void dgvData_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow curRow = this.dgvData.Rows[e.RowIndex];
            DataGridViewColumn curCol = this.dgvData.Columns[e.ColumnIndex];
            examCase bindItem = curRow.DataBoundItem as examCase;

            if (curCol.Name.Equals("Index") || curCol.Name.Equals("caseNo"))
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                e.Paint(e.CellBounds, DataGridViewPaintParts.ContentForeground);

                Graphics g = e.Graphics;
                int contentRectWidth = e.CellBounds.Width - e.CellStyle.Padding.Left - e.CellStyle.Padding.Right;
                SizeF textSize = g.MeasureString(e.FormattedValue.ToString(), e.CellStyle.Font);
                float textLeft = e.CellBounds.Left + e.CellStyle.Padding.Left + (contentRectWidth - textSize.Width) / 2F;
                float textRight = e.CellBounds.Right - e.CellStyle.Padding.Right - (contentRectWidth - textSize.Width) / 2F;

                if (curCol.Name.Equals("Index") && Usage == CaseListUsage.MainWindow)
                {
                    if (bindItem.collectCases != null && bindItem.collectCases.Count > 0)
                    {
                        float x = textLeft - 16 - 2;
                        float y = e.CellBounds.Top + (e.CellBounds.Height - 16) / 2F;
                        e.Graphics.DrawImage(Properties.Resources.collect, x, y, 16, 16);
                    }
                }
                else if (curCol.Name.Equals("caseNo"))
                {
                    if (!string.IsNullOrEmpty(bindItem.webSourceId))
                    {
                        float x = textRight + 2;
                        float y = e.CellBounds.Top + (e.CellBounds.Height - 16) / 2F;
                        g.DrawImage(Properties.Resources.internet, x, y, 16, 16);
                    }
                }
                e.Handled = true;
            }
        }

        private void dgvData_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (e.RowIndex >= 0)
            {
                try
                {
                    DataGridViewCell curCell = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    examCase rowData = dgv.Rows[e.RowIndex].DataBoundItem as examCase;
                    if (e.ColumnIndex == 0) //复选框列
                    {
                        if (curCell is DataGridViewCheckBoxCell)
                        {
                            if (curCell.Value == null)
                            {
                                curCell.Value = true;
                            }
                            else
                            {
                                bool isChecked = (bool)curCell.Value;
                                if (isChecked)
                                {
                                    curCell.Value = false;
                                    dgv.Columns[0].HeaderCell.Value = false;
                                }
                                else
                                {
                                    curCell.Value = true;
                                }
                            }
                            //rowData.IsSelected = (bool)curCell.Value;

                            dgv.InvalidateCell(e.ColumnIndex, e.RowIndex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex, "病例管理");
                }
            }
        }

        private void dgvData_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (Usage != CaseListUsage.MainWindow) return;

            if (e.Button == MouseButtons.Right && _ctxMenuStrip != null)
            {
                //bool isMulti = (dgvData.SelectedRows.Count > 1);
                //foreach (ToolStripItem item in _ctxMenuStrip.Items)
                //{
                //    if (item != toolDelete && item != toolConvert) item.Visible = !isMulti;
                //}

                DataGridViewRow curRow = dgvData.Rows[e.RowIndex]; //dgvData.SelectedRows[0];
                foreach (DataGridViewRow row in dgvData.Rows)
                {
                    if (row.Index == e.RowIndex)
                    {
                        row.Cells[0].Value = true;
                        row.Cells[0].Selected = true;
                    }
                    else
                    {
                        row.Cells[0].Value = false;
                    }
                }

                _selectedId = Convert.ToInt32(curRow.Cells["Id"].Value);
                _ctxMenuStrip.Show(MousePosition.X, MousePosition.Y);
            }
        }

        private void dgvData_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.RowIndex >= 0)
            {
                examCase bindItem = this.dgvData.Rows[e.RowIndex].DataBoundItem as examCase;
                _selectedId = bindItem.id;
            }
        }

        private void dgvData_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.RowIndex >= 0)
            {
                //int caseId = Convert.ToInt32(this.dgvData.Rows[e.RowIndex].Cells["Id"].Value);
                examCase bindItem = this.dgvData.Rows[e.RowIndex].DataBoundItem as examCase;
                if (bindItem.status >= 2 || !string.IsNullOrEmpty(bindItem.webSourceId))
                {
                    EnterAnalysis?.Invoke(bindItem);
                }
            }
        }

        private void dgvData_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int colIndex = e.ColumnIndex;

            if (dgvData.Columns[colIndex].SortMode != DataGridViewColumnSortMode.NotSortable)
            {
                _sortInfo.SortColumn = colIndex;

                if (_sortInfo.ColumnSorts[colIndex] == 0)
                {
                    _sortInfo.SortDirection = "asc"; //指定升序排列
                    _sortInfo.ColumnSorts[colIndex] = 1;
                    RefreshData?.Invoke(sender, -1, false);
                    this.dgvData.Columns[colIndex].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
                }
                else
                {
                    _sortInfo.SortDirection = "desc"; //再次点击变为降序排序
                    _sortInfo.ColumnSorts[colIndex] = 0;
                    RefreshData?.Invoke(sender, -1, false);
                    this.dgvData.Columns[colIndex].HeaderCell.SortGlyphDirection = SortOrder.Descending;
                }
            }
        }

        private void dgvData_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            //更新序号
            //BindingCollection<examCase> bindingList = this.dgvData.DataSource as BindingCollection<examCase>;
            int i = (this.zoncarePage.PageIndex - 1) * this.zoncarePage.PageSize;
            //foreach (examCase caseInfo in bindingList)
            //{
            //    caseInfo.DisplayIndex = ++i;
            //}

            if (_sortInfo.SortColumn != null)
            {
                DataGridViewColumnHeaderCell headerCell = this.dgvData.Columns[_sortInfo.SortColumn.Value].HeaderCell;
                if (_sortInfo.SortDirection == "asc")
                    headerCell.SortGlyphDirection = SortOrder.Ascending;
                else
                    headerCell.SortGlyphDirection = SortOrder.Descending;
            }

            //移除与病例状态不符的操作图标
            foreach (DataGridViewRow row in this.dgvData.Rows)
            {
                DataGridViewEx.RowActionModel model = this.dgvData.RowActionModels.Find(o => o.RowIndex == row.Index);
                examCase caseInfo = this.dgvData.Rows[row.Index].DataBoundItem as examCase;

                if (Usage == CaseListUsage.MainWindow)
                {
                    if (caseInfo.status > 1 || !string.IsNullOrEmpty(caseInfo.webSourceId))
                        model.Actions.RemoveAll(o => o.Key == "upload");
                    else
                        model.Actions.RemoveAll(o => o.Key == "analyse");
                }
                caseInfo.DisplayIndex = ++i;
            }
            this.dgvData.Invalidate();
        }

        private void dgvData_RowAction_ImageButtonClick(object sender, DataGridViewActionCellMouseEventArgs e)
        {
            int rowIndex = e.CellMouseEventArgs.RowIndex;
            try
            {
                examCase patientInfo = this.dgvData.Rows[rowIndex].DataBoundItem as examCase;
                if (e.CellAction.Key == "upload")
                {
                    UploadDataFile?.Invoke(patientInfo);
                }
                else if (e.CellAction.Key == "analyse")
                {
                    EnterAnalysis?.Invoke(patientInfo);
                }

                this.dgvData.InvalidateRow(rowIndex);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "病例管理");
            }
        }

        private void dgvData_SelectionChanged(object sender, EventArgs e)
        {
            //if (!this.dgvData.ContainsFocus) return;

            //if (this.dgvData.SelectedRows.Count > 0)
            //{
            //    DataGridViewRow curRow = this.dgvData.SelectedRows[0];
            //    _selectedId = Convert.ToInt32(curRow.Cells["Id"].Value);
            //}
        }

        private void tbxAge_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                return;
            }
        }

        private void mnuEdit_Click(object sender, EventArgs e)
        {
            if (dgvData.SelectedRows.Count > 0)
            {
                examCase caseInfo = dgvData.SelectedRows[0].DataBoundItem as examCase;
                RegisterForm frmPatient = new RegisterForm(caseInfo.id, caseInfo.checkItem);
                frmPatient.OnDataSaved += (patInfo, deviceType, isReadCard, startAnalysis) => { RefreshData?.Invoke(sender, -1, false); };
                frmPatient.ShowDialogEx(this.FindForm(), showLayer: true);
            }
        }

        private void mnuDel_Click(object sender, EventArgs e)
        {
            if (dgvData.SelectedRows.Count > 0)
            {
                examCase patientInfo = dgvData.SelectedRows[0].DataBoundItem as examCase;
                CancelForm frmCancel = new CancelForm(new List<int>() { patientInfo.id });
                frmCancel.OnCaseCancelled += () =>
                {
                    RefreshData?.Invoke(sender, -1, true);
                    ResetGridSelection();
                };
                frmCancel.ShowDialog(this.FindForm());
            }
        }

        private void mnuFavorite_Click(object sender, EventArgs e)
        {
            if (dgvData.SelectedRows.Count > 0)
            {
                examCase caseInfo = dgvData.SelectedRows[0].DataBoundItem as examCase;
                if (ZoncareEntities.Instance.CollectCases.Any(o => o.ecgId == caseInfo.id && o.userId == user.Loginer.id))
                {
                }
                else
                {
                    collectCase favour = new collectCase()
                    {
                        ecgId = caseInfo.id,
                        userId = user.Loginer.id,
                        collectTime = DateTime.Now
                    };
                    ZoncareEntities.Instance.CollectCases.Add(favour);
                    ZoncareEntities.Instance.SaveChanges();
                }
                RefreshData?.Invoke(sender, -1, false);
                ScreenCenterTip.Show("收藏成功");
            }
        }

        private void ZoncarePage_PageIndexChanged(object sender, EventArgs e)
        {
            try
            {
                RefreshData?.Invoke(sender, zoncarePage.PageIndex, false);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "病例管理");
            }
        }

        #endregion

        private void init()
        {
            _sortInfo = new SortInfo();

            if (Usage == CaseListUsage.MainWindow)
            {
                _ctxMenuStrip = new ContextMenuStrip() { ShowImageMargin = false };
                ToolStripMenuItem mnuEdit = new ToolStripMenuItem(_langs["编辑"], null, mnuEdit_Click);
                _ctxMenuStrip.Items.Add(mnuEdit);

                ToolStripMenuItem mnuDel = new ToolStripMenuItem(_langs["删除"], null, mnuDel_Click);
                _ctxMenuStrip.Items.Add(mnuDel);

                ToolStripMenuItem mnuFavorite = new ToolStripMenuItem(_langs["收藏"], null, mnuFavorite_Click);
                _ctxMenuStrip.Items.Add(mnuFavorite);
            }
        }

        private void initGrid()
        {
            this.dgvData.ReadOnly = true;
            this.dgvData.AutoGenerateColumns = false;
            this.dgvData.ColumnHeadersHeight = 32;
            this.dgvData.ActionColumnHeaderText = _langs["操作"];

            string dateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat];
            this.dgvData.CustomDateTimeFormat = dateFormat + " HH:mm:ss";

            setGridColumns();

            BindingCollection<examCase> bindingList = new BindingCollection<examCase>(new List<examCase>(), this);
            this.dgvData.DataSource = bindingList;

            this.dgvData.CellContentClick += dgvData_CellContentClick;
            this.dgvData.DataBindingComplete += dgvData_DataBindingComplete;
        }

        private void setGridColumns()
        {
            this.dgvData.Columns.Clear();

            //第一列(复选框)
            DataGridViewCheckBoxColumn col0 = this.dgvData.AddCheckBoxHeaderColumn((colIndex, state) =>
            {
                this.dgvData.EndEdit();
                this.dgvData.Rows.OfType<DataGridViewRow>().ToList().ForEach(row =>
                {
                    row.Cells[colIndex].Value = state;
                    examCase rowData = row.DataBoundItem as examCase;
                    rowData.IsSelected = state;
                });
            });
            col0.Width = 60;
            col0.DataPropertyName = "IsSelected";
            //col0.FalseValue = false;
            //col0.ValueType = typeof(bool);

            //第二列
            this.dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", DataPropertyName = "id", Visible = false });
            //第三列
            DataGridViewTextBoxColumn indexCol = new DataGridViewTextBoxColumn { Name = "Index", HeaderText = _langs["序号"], DataPropertyName = "DisplayIndex" };
            indexCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            indexCol.SortMode = DataGridViewColumnSortMode.NotSortable;
            this.dgvData.Columns.Add(indexCol);

            List<DataField> fieldList = new List<DataField>();
            if (Usage == CaseListUsage.MainWindow)
            {
                //获取自定义需显示的列
                fieldList = examCase.GetDisplayDataFields();
                DataField fldCheckItem = fieldList.FirstOrDefault(f => f.Field == "checkItemName");
                if (fldCheckItem != null)
                {
                    if (CommonValues.SystemName == "ecg" || CommonValues.SystemName == "bp") fldCheckItem.Hide = true;
                }
            }
            else if (Usage == CaseListUsage.Recycle)
            {
                string[] fields = new string[] { "caseNo", "name", "applyTime", "appointmentTime", "cancelReason" };
                foreach (string fieldName in fields)
                {
                    DataField fld = new DataField(fieldName, examCase.ColMap[fieldName]);
                    fieldList.Add(fld);
                }
            }
            else if (Usage == CaseListUsage.Favorite)
            {
                string[] fields = new string[] { "caseNo", "statusName", "name", "sexName", "ageName", "checkItemName", "appointmentTime" };
                foreach (string fieldName in fields)
                {
                    DataField fld = new DataField();
                    fld.Field = fieldName;
                    fld.DisplayName = examCase.ColMap[fieldName];
                    fieldList.Add(fld);
                }
                if (CommonValues.SystemType == "ecg")
                    fieldList.Add(new DataField() { Field = "holterReportConclusion", DisplayName = examCase.ColMap["holterReportConclusion"] });
                else if (CommonValues.SystemType == "bp")
                    fieldList.Add(new DataField() { Field = "bpReportConclusion", DisplayName = examCase.ColMap["bpReportConclusion"] });
                else
                    fieldList.Add(new DataField() { Field = "holterReportConclusion", DisplayName = examCase.ColMap["holterReportConclusion"] });
            }

            foreach (DataField fieldInfo in fieldList)
            {
                DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn()
                {
                    Name = fieldInfo.Field,
                    HeaderText = fieldInfo.DisplayName,
                    DataPropertyName = fieldInfo.Field.ToLower().Contains("reportconclusion") ? "reportConclusion" : fieldInfo.Field
                };
                if (fieldInfo.Hide) col.Visible = false;
                if (fieldInfo.DisplayName != "dataTypeName") col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                this.dgvData.Columns.Add(col);
            }
            //this.dgvData.Columns["ExaminationTime"].DefaultCellStyle.FormatProvider = CultureInfo.InvariantCulture;

            if (Usage == CaseListUsage.MainWindow)
            {
                this.dgvData.DataActionMode = new List<DataGridViewEx.GridDataAction>()
                {
                    new DataGridViewEx.GridDataAction(){ Key="upload", ActionType=DataGridViewEx.RowActionType.ImageButton, DisplayText=_langs["上传"], Value=Properties.Resources.upload },
                    new DataGridViewEx.GridDataAction(){ Key="analyse", ActionType=DataGridViewEx.RowActionType.ImageButton, DisplayText=_langs["分析"], Value=Properties.Resources.analyseIcon },
                };
            }

            _sortInfo.ColumnSorts = new int[this.dgvData.Columns.Count];
        }

        /// <summary>
        /// 检索刷新当前存储目录中数据相关的病例信息
        /// </summary>
        public void RetrieveData()
        {
            if (Usage != CaseListUsage.MainWindow) return;

            string rootPath = CommonValues.SystemSettings.baseSets.holterDataPath;
            DirectoryInfo dirInfo = new DirectoryInfo(rootPath);
            if (!dirInfo.Exists) return;

            using (ZoncareEntities dbEntities = ZoncareEntities.CreateIndependent())
            {
                // 阶段1：一次查询载入已入库路径，替代 N 次全表扫描
                HashSet<string> existPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (string path in dbEntities.ExamCases.Select(o => o.filePath).ToList())
                {
                    if (!string.IsNullOrEmpty(path)) existPaths.Add(path.Replace("/", "\\"));
                }

                // 阶段2：纯文件解析，不开事务、不占锁，单目录异常只影响自己
                List<examCase> insertCases = new List<examCase>();
                Dictionary<string, List<bpDetail>> insertBps =
                    new Dictionary<string, List<bpDetail>>(StringComparer.OrdinalIgnoreCase);

                DirectoryInfo[] caseDirs;
                try { caseDirs = dirInfo.GetDirectories(); }
                catch (Exception ex) { Logger.CreateLog(ex, "病例管理"); return; }

                foreach (DirectoryInfo dirCase in caseDirs)
                {
                    string fullPath = dirCase.FullName;
                    if (existPaths.Contains(fullPath)) continue;   // 已在库，直接跳过

                    try
                    {
                        FileInfo archiveInfo = new FileInfo(Path.Combine(fullPath, "archiver.info"));
                        if (!archiveInfo.Exists) continue;

                        JObject jo = JObject.Parse(File.ReadAllText(archiveInfo.FullName));

                        examCase newCase = jo["ecg"].ToObject<examCase>();
                        newCase.id = 0;
                        newCase.updateBy = user.Loginer.userName;
                        newCase.updateTime = DateTime.Now;
                        newCase.fileDirectory = rootPath;
                        newCase.filePath = fullPath;

                        List<bpDetail> bps = new List<bpDetail>();
                        JArray arr = jo["bpDetail"]?["list"] as JArray;   // 原写法 bpDetail 缺失会 NRE
                        if (arr != null)
                        {
                            for (int i = 0; i < arr.Count; i++)
                            {
                                bpDetail bp = arr[i].ToObject<bpDetail>();
                                bp.id = 0;
                                if (bp.OriginalTime == null)
                                    bp.OriginalTime = DateTime.ParseExact(bp.time, SysConstants.DATETIME_FORMAT,
                                        CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
                                bps.Add(bp);
                            }
                        }

                        insertCases.Add(newCase);
                        insertBps[fullPath] = bps;
                        existPaths.Add(fullPath);
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog($"源文件{fullPath}, 数据读取失败", "病例管理");
                        Logger.CreateLog(ex, "病例管理");
                    }
                }

                if (insertCases.Count == 0) return;

                // 阶段3：分批写库，每批一个短事务
                const int batchSize = 50;
                for (int offset = 0; offset < insertCases.Count; offset += batchSize)
                {
                    List<examCase> batch = insertCases.GetRange(offset, Math.Min(batchSize, insertCases.Count - offset));
                    using (var tran = dbEntities.Database.BeginTransaction())
                    {
                        try
                        {
                            dbEntities.ExamCases.AddRange(batch);
                            dbEntities.SaveChanges();               // 回填自增 id

                            foreach (examCase c in batch)
                                foreach (bpDetail bp in insertBps[c.filePath]) { bp.caseId = c.id; dbEntities.BpDetails.Add(bp); }
                            dbEntities.SaveChanges();
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            DetachAll(dbEntities);                  // 清掉半成品实体，避免影响后续批次
                            Logger.CreateLog(ex, "病例管理");
                            continue;
                        }
                    }
                    foreach (examCase c in batch) Logger.CreateLog($"源文件{c.filePath}, 数据读取成功", "病例管理");
                }
            }
        }

        // EF Core 2.2 没有 ChangeTracker.Clear()，需手动脱轨
        private static void DetachAll(ZoncareEntities dbEntities)
        {
            foreach (var entry in dbEntities.ChangeTracker.Entries().ToList())
                entry.State = EntityState.Detached;
        }


        /// <summary>
        /// 刷新病例列表
        /// </summary>
        /// <param name="query">构造的相关查询</param>
        /// <param name="patientName">用户过滤查询结果的患者姓名</param>
        /// <param name="pageIndex">当前列表页码</param>
        /// <param name="resetCheckState">是否需重置多选状态</param>
        /// <param name="resetGridStyle">是否需要重置列表头及表格样式</param>
        public void RefreshList(IQueryable<examCase> query, string patientName = "", int pageIndex = -1, bool resetCheckState = true, bool resetGridStyle = false)
        {
            //UI 线程：仅做状态快照与轻量设置，实际数据查询移交后台线程，避免阻塞界面
            int pageSize = zoncarePage.PageSize;
            if (pageIndex <= 0)
                pageIndex = zoncarePage.PageIndex;
            else
                zoncarePage.PageIndex = pageIndex;

            if (resetGridStyle) { setGridColumns(); }

            int? sortCol = _sortInfo.SortColumn;
            string sortColName = string.Empty;
            if (sortCol != null)
            {
                sortColName = this.dgvData.Columns[sortCol.Value].DataPropertyName;
            }
            string sortDirection = _sortInfo.SortDirection;
            bool needResize = _isFirstShow || resetGridStyle;
            bool fromMemory = !string.IsNullOrEmpty(patientName) || sortColName == "name";
            int seq = ++_refreshSeq;

            Task.Run(() =>
            {
                List<examCase> listData = null;
                int recordCount = 0;

                try
                {
                    //串行化后台查询，避免并发访问共享 EF 上下文
                    _queryGate.Wait();

                    if (fromMemory)
                    {
                        var queriedData = queryDataFromMemory(query, patientName, sortColName, sortDirection);
                        recordCount = queriedData.Count();
                        //排序
                        if (sortColName == "name")
                        {
                            if (sortDirection == "asc")
                                listData = queriedData.OrderBy(o => o.name)
                                     .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                            else
                                listData = queriedData.OrderByDescending(o => o.name)
                                     .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                        }
                        else
                        {
                            listData = queriedData
                                .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                        }
                    }
                    else
                    {
                        recordCount = query.Count();
                        //排序
                        if (sortCol == null)
                            query = query.OrderByDescending(o => o.appointmentTime).ThenByDescending(o => o.id);
                        else
                            query = DbFuns.ApplyDynamicSorting(query, sortColName, sortDirection);

                        listData = (from s in query
                                    .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                                    select s).ToList();
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex, "病例管理");
                }
                finally
                {
                    _queryGate.Release();
                }

                //读取数据完成后回UI线程绑定
                try
                {
                    if (IsDisposed || !IsHandleCreated) return;
                    this.BeginInvoke(new Action(() =>
                    {
                        if (seq != _refreshSeq) return; //已有更新的刷新请求，丢弃本次过期数据
                        BindListData(listData, recordCount, needResize, resetCheckState);
                    }));
                }
                catch (ObjectDisposedException) { }
            });
        }

        /// <summary>
        /// 在 UI 线程上绑定后台查询结果（由 RefreshList 取数完成后调用）
        /// </summary>
        private void BindListData(List<examCase> listData, int recordCount, bool needResize, bool resetCheckState)
        {
            zoncarePage.RecordCount = recordCount;

            if (listData == null) return; //后台查询失败，保持现有列表内容

            BindingCollection<examCase> bindingList = new BindingCollection<examCase>(listData, this);
            this.dgvData.DataSource = bindingList;

            if (needResize)
            {
                if (Usage == CaseListUsage.MainWindow)
                {
                    //this.dgvData.AutoResizeColumns();
                    this.dgvData.ResizeColumnWidth(new List<int>() { 8, 0, 8, 10, 15, 20, 15, 15, 15, 30, 15 });
                }
                else if (Usage == CaseListUsage.Recycle)
                {
                    this.dgvData.ResizeColumnWidth(new List<int>() { 4, 0, 4, 10, 10, 10, 10, 30 });
                }
                else if (Usage == CaseListUsage.Favorite)
                {
                    this.dgvData.ResizeColumnWidth(new List<int>() { 8, 0, 8, 10, 10, 15, 10, 10, 10, 15, 30 });
                }
            }
            if (resetCheckState)
            {
                this.dgvData.Columns[0].HeaderCell.Value = false;
            }
            if (_selectedId.HasValue)
            {
                foreach (DataGridViewRow dr in this.dgvData.Rows)
                {
                    examCase bindItem = dr.DataBoundItem as examCase;
                    if (bindItem.id == _selectedId)
                    {
                        this.dgvData.CurrentCell = dr.Cells[0];
                        //bindItem.IsSelected = true;
                        break;
                    }
                }
            }
            else
            {
                if (this.dgvData.Rows.Count > 0) this.dgvData.CurrentCell = this.dgvData.Rows[0].Cells[0];
            }
        }

        /// <summary>
        /// 获取选择的病例
        /// </summary>
        /// <returns></returns>
        public List<examCase> GetSelectedCases()
        {
            if (this.dgvData.DataSource != null)
                return (this.dgvData.DataSource as BindingCollection<examCase>).Where(o => o.IsSelected).ToList();
            else
                return new List<examCase>();
        }

        /// <summary>
        /// 重置列表选中
        /// </summary>
        public void ResetGridSelection()
        {
            _selectedId = null;
        }

        //由于文本加密，涉及到患者姓名的筛选或排序需在内存中进行。
        //纯数据方法，可在后台线程执行；RecordCount 由调用方回 UI 线程后设置
        private IEnumerable<examCase> queryDataFromMemory(IQueryable<examCase> query, string patientName = "", string sortCol = "", string sortDirection = "asc")
        {
            var sourceData = query.AsEnumerable();
            if (!string.IsNullOrEmpty(patientName)) sourceData = sourceData.Where(o => o.name.Contains(patientName));

            //排序
            if (string.IsNullOrEmpty(sortCol))
            {
                sourceData = sourceData.OrderBy(o => o.status).ThenByDescending(o => o.uploadTime);
            }
            else
            {
                if (sortCol != "name")
                    sourceData = DbFuns.ApplyDynamicSorting(sourceData.AsQueryable(), sortCol, sortDirection);
            }

            return sourceData;
        }

        private List<int> getSelectedCaseId()
        {
            List<int> lstSelected = new List<int>();
            foreach (DataGridViewRow row in dgvData.Rows)
            {
                if (row.Index < 0) continue;

                DataGridViewCell firstCell = row.Cells[0];
                if (firstCell is DataGridViewCheckBoxCell && firstCell.Value is bool)
                {
                    if ((bool)firstCell.Value)
                    {
                        examCase caseInfo = row.DataBoundItem as examCase;
                        lstSelected.Add(caseInfo.id);
                    }
                }
            }
            return lstSelected;
        }


        private class SortInfo
        {
            //数据列表的所有列序集合(0:升序, 1:降序)
            public int[] ColumnSorts { get; set; }
            //当前排序的列索引
            public int? SortColumn { get; set; }
            //当前排序方向(asc/desc)
            public string SortDirection { get; set; }
        }

    }
}
