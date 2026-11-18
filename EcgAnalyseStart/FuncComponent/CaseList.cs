using AnalysisSystem.Base;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.Component.Controls;
using Zoncare.DAL;
using Zoncare.Utilities;

namespace EcgBpAnalysis.FuncComponent
{
    [ToolboxItem(true)]
    public partial class CaseList : UserControl
    {
        private PersistDictionary<string> _langs;
        private ContextMenuStrip _ctxMenuStrip;
        private bool _usedForFavorites = false;
        private int? _selectedId;
        private bool _isFirstShow = true;
        private SortInfo _sortInfo; //数据列表的列排序方向
        private Timer _refreshTimer;
        private static object _lockObject = new object();

        public Action<examCase> UploadDataFile;
        public Action<examCase> EnterAnalysis;

        [DefaultValue(false), Category("自定义属性"), Description("是否用于病例收藏界面"), Browsable(true)]
        public bool UsedForFavorites
        {
            get => _usedForFavorites;
            set
            {
                _usedForFavorites = value;
                if (_usedForFavorites)
                {
                    this.btnCancelFavorite.Visible = true;
                    this.btnEdit.Visible = false;
                    this.btnDelete.Visible = false;
                }
                else
                {
                    this.btnCancelFavorite.Visible = false;
                    this.btnEdit.Visible = true;
                    this.btnDelete.Visible = true;
                }
            }
        }


        public CaseList()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            if (!DesignMode)
            {
                this.tlpFilter.SuspendLayout();
                this.tlpFilter.Visible = false;
            }
        }

        #region 事件
        public void OnControlShown()
        {
            this.tlpFilter.Visible = true;
            this.tlpFilter.ResumeLayout();

            // 执行显示后的逻辑
            Task.Run(async () =>
            {
                await Task.Delay(100);
                this.InvokeIfRequired(() =>
                {
                    initUI();
                    RetrieveData();
                    RefreshList();

                    if (!string.IsNullOrEmpty(CommonValues.WebCaseSource)) { _refreshTimer.Start(); }
                });
                _isFirstShow = false;
            });
        }

        private void CaseList_Load(object sender, EventArgs e)
        {
            _langs = CommonValues.Langs;

            if (DesignMode) return;

            init();
            //PubFuns.AsyncDoWork(() => { this.InvokeIfRequired(() => refreshList()); });

            registerEvents();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            RefreshList(1);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            retrieveWebCases();
            this.btnRefresh.Tag = 15;
            this.btnRefresh.Text = "15";
            this.btnRefresh.Invalidate();
        }

        //编辑
        private void btnEdit_Click(object sender, EventArgs e)
        {
            //List<int> lstSelected = getSelectedCaseId();
            List<examCase> selectedCases = (this.dgvData.DataSource as BindingCollection<examCase>).Where(o => o.IsSelected).ToList();
            if (selectedCases.Count <= 0)
            {
                ScreenCenterTip.Show("请至少选择一条记录。");
                return;
            }
            if (selectedCases.Count > 1)
            {
                ScreenCenterTip.Show("只能选中一条记录。");
                return;
            }

            examCase caseInfo = selectedCases[0];
            RegisterForm frmPatient = new RegisterForm(caseInfo.id, caseInfo.checkItem);
            frmPatient.OnDataSaved += (patInfo, deviceType, isReadCard, startAnalysis) => { RefreshList(resetCheckState: false); };
            frmPatient.ShowDialogEx(this.FindForm(), showLayer: true);
        }

        //删除
        private void btnDelete_Click(object sender, EventArgs e)
        {
            //List<int> lstSelected = getSelectedCaseId();
            List<int> lstSelected = (this.dgvData.DataSource as BindingCollection<examCase>).Where(o => o.IsSelected).Select(o => o.id).ToList();
            if (lstSelected.Count <= 0)
            {
                ScreenCenterTip.Show("请至少选择一条记录。");
                return;
            }

            CancelForm frmCancel = new CancelForm(lstSelected);
            frmCancel.OnCaseCancelled += () => { RefreshList(); };
            frmCancel.ShowDialog(this.FindForm());
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
                if (bindItem.status == 2)
                    e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor = Color.FromArgb(220, 20, 60);
                else
                    e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor = Color.FromArgb(82, 196, 26);
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

            if (curCol.Name.Equals("caseNo"))
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);
                e.Paint(e.CellBounds, DataGridViewPaintParts.ContentForeground);

                if (!string.IsNullOrEmpty(bindItem.webSourceId))
                {
                    Graphics g = e.Graphics;
                    int contentRectWidth = e.CellBounds.Width - e.CellStyle.Padding.Left - e.CellStyle.Padding.Right;
                    SizeF textSize = g.MeasureString(e.FormattedValue.ToString(), e.CellStyle.Font);
                    float textRight = e.CellBounds.Right - e.CellStyle.Padding.Right - (contentRectWidth - textSize.Width) / 2F;
                    float x = textRight + 2;
                    float y = e.CellBounds.Top + (e.CellBounds.Height - 16) / 2F;
                    g.DrawImage(Properties.Resources.internet, x, y, 16, 16);
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
            if (UsedForFavorites) return;

            if (e.Button == MouseButtons.Right)
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
                    RefreshList();
                    this.dgvData.Columns[colIndex].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
                }
                else
                {
                    _sortInfo.SortDirection = "desc"; //再次点击变为降序排序
                    _sortInfo.ColumnSorts[colIndex] = 0;
                    RefreshList();
                    this.dgvData.Columns[colIndex].HeaderCell.SortGlyphDirection = SortOrder.Descending;
                }
            }
        }

        private void dgvData_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            //更新序号
            BindingCollection<examCase> bindingList = this.dgvData.DataSource as BindingCollection<examCase>;
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

                if (!UsedForFavorites)
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
                frmPatient.OnDataSaved += (patInfo, deviceType, isReadCard, startAnalysis) => { RefreshList(resetCheckState: false); };
                frmPatient.ShowDialogEx(this.FindForm(), showLayer: true);
            }
        }

        private void mnuDel_Click(object sender, EventArgs e)
        {
            if (dgvData.SelectedRows.Count > 0)
            {
                examCase patientInfo = dgvData.SelectedRows[0].DataBoundItem as examCase;
                CancelForm frmCancel = new CancelForm(new List<int>() { patientInfo.id });
                frmCancel.OnCaseCancelled += () => { RefreshList(); };
                frmCancel.ShowDialog(this.FindForm());
            }
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            int countdown = Convert.ToInt32(this.btnRefresh.Tag);
            if (countdown == 0)
            {
                retrieveWebCases();
            }

            int nextCount = (countdown - 1 >= 0 ? countdown - 1 : 15);
            this.btnRefresh.Tag = nextCount;
            this.btnRefresh.Text = nextCount.ToString();
            this.btnRefresh.Invalidate();
        }

        private void ZoncarePage_PageIndexChanged(object sender, EventArgs e)
        {
            try
            {
                RefreshList(zoncarePage.PageIndex, false);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "病例管理");
            }
        }

        #endregion

        private void init()
        {
            _refreshTimer = new Timer();
            _refreshTimer.Interval = 1000; // 每秒检查一次
            _refreshTimer.Tick += refreshTimer_Tick;

            _sortInfo = new SortInfo();

            _ctxMenuStrip = new ContextMenuStrip() { ShowImageMargin = false };
            ToolStripMenuItem mnuEdit = new ToolStripMenuItem(_langs["编辑"], null, mnuEdit_Click);
            _ctxMenuStrip.Items.Add(mnuEdit);

            ToolStripMenuItem mnuDel = new ToolStripMenuItem(_langs["删除"], null, mnuDel_Click);
            _ctxMenuStrip.Items.Add(mnuDel);
        }

        private void initUI()
        {
            CommonFuns.InitCommboBoxItems(this.cbxSex, SysConstants.Genders, true, "", "全部");
            CommonFuns.InitCommboBoxItems(this.cbxCheckItem, SysConstants.CheckItems, true, -1, "全部");
            if (CommonValues.SystemType == "ecg")
            {
                this.cbxCheckItem.SelectedValue = 1;
                this.cbxCheckItem.Enabled = false;
            }
            else if (CommonValues.SystemType == "bp")
            {
                this.cbxCheckItem.SelectedValue = 2;
                this.cbxCheckItem.Enabled = false;
            }

            if (string.IsNullOrEmpty(CommonValues.WebCaseSource))
            {
                this.btnRefresh.Visible = false;
            }

            CommonFuns.InitCommboBoxItems(this.cbxAgeUnit, SysConstants.AgeUnits);
            CommonFuns.InitCommboBoxItems(this.cbxStatus, SysConstants.CaseStatus, true, -1, "全部");

            string dateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat];
            this.dtpFromDate.CustomFormat = this.dtpToDate.CustomFormat = dateFormat;
            this.dtpFromDate.Value = Convert.ToDateTime(DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd") + " 00:00:00");
            this.dtpFromDate.Checked = false;
            this.dtpToDate.Value = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59");
            this.dtpToDate.Checked = false;

            initGrid();
        }

        private void initGrid()
        {
            //复选框
            this.dgvData.AutoGenerateColumns = false;
            this.dgvData.ColumnHeadersHeight = 32;
            this.dgvData.ActionColumnHeaderText = _langs["操作"];
            setGridStyle();

            BindingCollection<examCase> bindingList = new BindingCollection<examCase>(new List<examCase>(), this);
            this.dgvData.DataSource = bindingList;

            this.dgvData.CellContentClick += dgvData_CellContentClick;
            this.dgvData.DataBindingComplete += dgvData_DataBindingComplete;
        }

        private void setGridStyle()
        {
            this.dgvData.Columns.Clear();

            string dateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat];
            this.dgvData.CustomDateTimeFormat = dateFormat + " HH:mm:ss";

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

            this.dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", DataPropertyName = "id", Visible = false });

            DataGridViewTextBoxColumn indexCol = new DataGridViewTextBoxColumn { Name = "Index", HeaderText = _langs["序号"], DataPropertyName = "DisplayIndex" };
            indexCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            indexCol.SortMode = DataGridViewColumnSortMode.NotSortable;
            this.dgvData.Columns.Add(indexCol);

            //用户自定义需显示的列
            List<DataField> fieldList = new List<DataField>();
            if (UsedForFavorites)
            {
                string[] fields = new string[] { "caseNo", "statusName", "name", "sexName", "ageName", "checkItemName", "appointmentTime" };
                foreach (string fieldName in fields)
                {
                    DataField fld = new DataField();
                    fld.field = fieldName;
                    fld.displayName = examCase.ColMap[fieldName];
                    fieldList.Add(fld);
                }
                if (CommonValues.SystemType == "ecg")
                    fieldList.Add(new DataField() { field = "holterReportConclusion", displayName = examCase.ColMap["holterReportConclusion"] });
                else if (CommonValues.SystemType == "bp")
                    fieldList.Add(new DataField() { field = "bpReportConclusion", displayName = examCase.ColMap["bpReportConclusion"] });
                else
                    fieldList.Add(new DataField() { field = "holterReportConclusion", displayName = examCase.ColMap["holterReportConclusion"] });
            }
            else
            {
                fieldList = examCase.GetDisplayDataFields();

                DataField fldCheckItem = fieldList.FirstOrDefault(f => f.field == "checkItemName");
                if (fldCheckItem != null)
                {
                    if (CommonValues.SystemName == "ecg" || CommonValues.SystemName == "bp") fldCheckItem.hide = true;
                }
            }

            foreach (DataField fieldInfo in fieldList)
            {
                DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn()
                {
                    Name = fieldInfo.field,
                    HeaderText = fieldInfo.displayName,
                    DataPropertyName = fieldInfo.field
                };
                if (fieldInfo.hide) col.Visible = false;
                if (fieldInfo.displayName != "dataTypeName") col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                this.dgvData.Columns.Add(col);
            }
            //this.dgvData.Columns["ExaminationTime"].DefaultCellStyle.FormatProvider = CultureInfo.InvariantCulture;

            if (!UsedForFavorites)
            {
                this.dgvData.DataActionMode = new List<DataGridViewEx.GridDataAction>()
                {
                    new DataGridViewEx.GridDataAction(){ Key="upload", ActionType=DataGridViewEx.RowActionType.ImageButton, DisplayText=_langs["上传"], Value=Properties.Resources.upload },
                    new DataGridViewEx.GridDataAction(){ Key="analyse", ActionType=DataGridViewEx.RowActionType.ImageButton, DisplayText=_langs["分析"], Value=Properties.Resources.analyseIcon },
                };
            }
            _sortInfo.ColumnSorts = new int[this.dgvData.Columns.Count];
        }

        private void retrieveWebCases()
        {
            var reqParas = new List<KeyValuePair<string, string>>();
            reqParas.Add(new KeyValuePair<string, string>("hospitalCode", CommonValues.HospitalCode));

            Task.Run(() =>
            {
                lock (_lockObject)
                {
                    using (ZoncareEntities dbEntities = ZoncareEntities.CreateIndependent())
                    {
                        try
                        {
                            string resp = HttpService.GetWebRequest(CommonValues.WebCaseSource + "/holter/client/patient/list", "GET", reqParas, out string errMsg);
                            if (!string.IsNullOrEmpty(resp))
                            {
                                JObject jo = JObject.Parse(resp);
                                if (jo["code"].ToString() == "0")
                                {
                                    string dataString = jo["data"]["rows"].ToString();
                                    List<examCase> lstCase = JsonConvert.DeserializeObject<List<examCase>>(dataString);
                                    var sourceIds = lstCase.Select(o => o.id.ToString());
                                    var existingIds = dbEntities.ExamCases.AsNoTracking().Where(p => sourceIds.Contains(p.webSourceId)).Select(p => p.webSourceId);
                                    //List<int> missingIds = sourceIds.Except(existingIds).ToList();

                                    List<examCase> newCases = new List<examCase>(lstCase.Count);
                                    for (int i = 0; i < lstCase.Count; i++)
                                    {
                                        examCase caseInfo = lstCase[i];
                                        if (existingIds.Contains(caseInfo.id.ToString())) continue;

                                        caseInfo.webSourceId = caseInfo.id.ToString(); ;
                                        caseInfo.id = 0;
                                        caseInfo.recordTime = null;
                                        caseInfo.uploadTime = null;
                                        caseInfo.status = 1;
                                        newCases.Add(caseInfo);
                                    }

                                    if (newCases.Count > 0)
                                    {
                                        //先进行批量新增
                                        dbEntities.ChangeTracker.AutoDetectChangesEnabled = false;
                                        dbEntities.ExamCases.AddRange(newCases);
                                        dbEntities.SaveChanges();

                                        foreach (examCase caseInfo in newCases)
                                        {
                                            string rootPath = AppCode.CreateCaseDirectory(caseInfo);
                                            AppCode.SaveArchiveInfo(caseInfo, null);
                                        }
                                        dbEntities.ChangeTracker.DetectChanges();
                                        dbEntities.SaveChanges();

                                        this.InvokeIfRequired(() => RefreshList(), true);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.CreateLog(ex, "病例管理");
                        }
                        finally
                        {
                            dbEntities.ChangeTracker.AutoDetectChangesEnabled = true;
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 检索刷新当前存储目录中数据相关的病例信息
        /// </summary>
        public void RetrieveData()
        {
            if (UsedForFavorites) return;

            using (ZoncareEntities dbEntities = ZoncareEntities.CreateIndependent())
            {
                string rootPath = CommonValues.SystemSettings.baseSets.holterDataPath;
                DirectoryInfo dirInfo = new DirectoryInfo(rootPath);
                if (!dirInfo.Exists) return;

                foreach (DirectoryInfo dirCase in dirInfo.GetDirectories())
                {
                    using (var tran = dbEntities.Database.BeginTransaction())
                    {
                        try
                        {
                            bool redirected = false;
                            examCase caseInfo = dbEntities.ExamCases.FirstOrDefault(o => o.filePath.Replace("/", "\\") == dirCase.FullName);
                            if (caseInfo == null)
                            {
                                //导入新的病例信息
                                string archivePath = Path.Combine(dirCase.FullName, "archiver.info");
                                FileInfo archiveInfo = new FileInfo(archivePath);
                                if (archiveInfo.Exists)
                                {
                                    string archiveContent = File.ReadAllText(archivePath);
                                    JObject jo = JObject.Parse(archiveContent);
                                    //患者信息
                                    examCase newCase = jo["ecg"].ToObject<examCase>();
                                    newCase.id = 0;
                                    newCase.updateBy = user.Loginer.userName;
                                    newCase.updateTime = DateTime.Now;
                                    newCase.fileDirectory = rootPath;
                                    newCase.filePath = dirCase.FullName;
                                    dbEntities.ExamCases.Add(newCase);
                                    int ret1 = dbEntities.SaveChanges();

                                    //血压数据
                                    //(jo["bpDetail"] as JObject).ContainsKey
                                    JToken bpList = jo["bpDetail"]["list"];
                                    if (bpList != null)
                                    {
                                        JArray arr = bpList as JArray;
                                        for (int i = 0; i < arr.Count; i++)
                                        {
                                            bpDetail bp = arr[i].ToObject<bpDetail>();
                                            bp.id = 0;
                                            bp.caseId = newCase.id;
                                            if (bp.OriginalTime == null)
                                            {
                                                DateTime originalTime = DateTime.ParseExact(bp.time, SysConstants.DATETIME_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
                                                bp.OriginalTime = originalTime;
                                            }
                                            dbEntities.BpDetails.Add(bp);
                                        }
                                    }
                                    int ret2 = dbEntities.SaveChanges();
                                    redirected = ret1 > 0;
                                }
                                Logger.CreateLog($"源文件{dirCase.FullName}, 数据读取{(redirected ? "成功" : "失败")}", "病例管理");
                            }
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            Logger.CreateLog(ex, "病例管理");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 刷新病例列表
        /// </summary>
        /// <param name="pageIndex">当前列表页码</param>
        /// <param name="resetCheckState">是否需重置多选状态</param>
        /// <param name="resetGridStyle">是否需要重置列表头及表格样式</param>
        public void RefreshList(int pageIndex = -1, bool resetCheckState = true, bool resetGridStyle = false)
        {
            int pageSize = zoncarePage.PageSize;
            if (pageIndex <= 0)
                pageIndex = zoncarePage.PageIndex;
            else
                zoncarePage.PageIndex = pageIndex;

            if (resetGridStyle) { setGridStyle(); }

            string patientName = this.tbxName.Text.Trim();
            try
            {
                IQueryable<examCase> query = getQuery();
                List<examCase> listData = null;

                int? sortCol = _sortInfo.SortColumn;
                string sortColName = string.Empty;
                if (sortCol != null) sortColName = this.dgvData.Columns[sortCol.Value].DataPropertyName;

                if (!string.IsNullOrEmpty(patientName) || sortColName == "name")
                {
                    var queriedData = queryDataFromMemory(query, patientName, sortColName);
                    zoncarePage.RecordCount = queriedData.Count();
                    //排序
                    if (sortColName == "name")
                    {
                        if (_sortInfo.SortDirection == "asc")
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
                    zoncarePage.RecordCount = query.Count();

                    //排序
                    if (sortCol == null)
                        query = query.OrderByDescending(o => o.updateTime).ThenBy(o => o.status);
                    else
                        query = DbFuns.ApplyDynamicSorting(query, this.dgvData.Columns[sortCol.Value].DataPropertyName, _sortInfo.SortDirection);

                    listData = (from s in query
                                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                                select s).ToList();
                }

                BindingCollection<examCase> bindingList = new BindingCollection<examCase>(listData, this);
                this.dgvData.DataSource = bindingList;

                if (_isFirstShow || resetGridStyle)
                {
                    //this.dgvData.AutoResizeColumns();
                    this.dgvData.ResizeColumnWidth(new List<int>() { 8, 0, 8, 10, 15, 20, 15, 15, 15, 30, 15 });
                }
                if (resetCheckState) { this.dgvData.Columns[0].HeaderCell.Value = false; }
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
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "病例管理");
            }
        }

        //由于文本加密，涉及到患者姓名的筛选或排序需在内存中进行
        private IEnumerable<examCase> queryDataFromMemory(IQueryable<examCase> query, string patientName = "", string sortCol = "")
        {
            var sourceData = query.AsEnumerable();
            if (!string.IsNullOrEmpty(patientName)) sourceData = sourceData.Where(o => o.name.Contains(patientName));

            zoncarePage.RecordCount = sourceData.Count();
            //排序
            if (string.IsNullOrEmpty(sortCol))
            {
                sourceData = sourceData.OrderBy(o => o.status).ThenByDescending(o => o.uploadTime);
            }
            else
            {
                if (sortCol != "name")
                    sourceData = DbFuns.ApplyDynamicSorting(sourceData.AsQueryable(), sortCol, _sortInfo.SortDirection);
            }

            return sourceData;
        }

        private void registerEvents()
        {
            this.tbxFromAge.KeyPress += tbxAge_KeyPress;
            this.tbxToAge.KeyPress += tbxAge_KeyPress;
            this.zoncarePage.PageIndexChanged += ZoncarePage_PageIndexChanged;
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

        //构造查询
        private IQueryable<examCase> getQuery()
        {
            //DateTime dtStart = Convert.ToDateTime(dtpStartTime.Value.ToString("yyyy-MM-dd") + " 00:00:00");
            //DateTime dtEnd = Convert.ToDateTime(dtpEndTime.Value.ToString("yyyy-MM-dd") + " 23:59:59");
            string rootPath = CommonValues.SystemSettings.baseSets.holterDataPath;

            IQueryable<examCase> query = ZoncareEntities.Instance.ExamCases.AsNoTracking().Where(o => o.fileDirectory.Replace("/", "\\") == rootPath && o.delFlag == 0);

            string caseNo = this.tbxCaseNo.Text.Trim();
            if (!string.IsNullOrEmpty(caseNo))
            {
                query = query.Where(o => o.caseNo.Contains(caseNo));
            }

            if (this.cbxSex.SelectedIndex > 0)
            {
                query = query.Where(o => this.cbxSex.SelectedValue.Equals(o.sex));
            }
            if (this.cbxCheckItem.SelectedIndex > 0)
            {
                query = query.Where(o => o.checkItem.Equals(this.cbxCheckItem.SelectedValue));
            }

            if (this.dtpFromDate.Checked)
            {
                DateTime fromTime = this.dtpFromDate.Value.Date;
                query = query.Where(o => o.appointmentTime >= fromTime);
            }
            if (this.dtpToDate.Checked)
            {
                DateTime toTime = this.dtpToDate.Value.Date.AddDays(1);
                query = query.Where(o => o.appointmentTime < toTime);
            }

            string ageUnit = this.cbxAgeUnit.SelectedValue.ToString();
            if (!string.IsNullOrEmpty(this.tbxFromAge.Text.Trim()))
            {
                int fromAge = CommonFuns.GetAgeDays(Convert.ToInt32(this.tbxFromAge.Text), ageUnit);
                query = query.Where(o => o.ageDays >= fromAge);
            }
            if (!string.IsNullOrEmpty(this.tbxToAge.Text.Trim()))
            {
                int toAge = CommonFuns.GetAgeDays(Convert.ToInt32(this.tbxToAge.Text), ageUnit);
                query = query.Where(o => o.ageDays < toAge);
            }

            if (this.cbxStatus.SelectedIndex > 0)
            {
                query = query.Where(o => o.status.Equals(this.cbxStatus.SelectedValue));
            }
            string conclusion = this.tbxConclusion.Text.Trim();
            if (!string.IsNullOrEmpty(conclusion))
            {
                query = query.Where(o => o.holterReportConclusion.Contains(conclusion) || o.bpReportConclusion.Contains(conclusion));
            }

            return query;
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
