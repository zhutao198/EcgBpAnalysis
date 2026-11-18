using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.Component.Controls;
using Zoncare.DAL;

namespace SystemSettings
{
    public partial class UserManagement : UserControl
    {
        /// <summary>内置管理员账号，不允许删除</summary>
        private const int SUPER_ADMIN_ID = 1;

        private PersistDictionary<string> _langs;
        private bool _isFirstShow = true;
        private int? _selectedId;

        public UserManagement()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
        }

        private void UserManagement_Load(object sender, EventArgs e)
        {
            _langs = CommonValues.Langs;
            try
            {
                initUI();

                if (!DesignMode)
                {
                    refreshList(1);
                }
                _isFirstShow = false;
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "系统设置");
            }
        }

        private void dgvData_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (dgv == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;

            //只处理复选框列
            if (dgv.Columns[e.ColumnIndex].Name != "chk") return;

            try
            {
                DataGridViewCheckBoxCell curCell = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewCheckBoxCell;
                if (curCell == null) return;

                if (curCell.Value == null)
                {
                    curCell.Value = true;
                }
                else if ((bool)curCell.Value)
                {
                    curCell.Value = false;
                    //取消勾选任意一行后，表头复选框同步为未选中
                    dgv.Columns["chk"].HeaderCell.Value = false;
                }
                else
                {
                    curCell.Value = true;
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "系统设置");
            }
        }

        private void dgvData_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.RowIndex >= 0 && e.RowIndex < this.dgvData.Rows.Count)
            {
                user bindItem = this.dgvData.Rows[e.RowIndex].DataBoundItem as user;
                if (bindItem != null) _selectedId = bindItem.id;
            }
        }

        private void dgvData_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                //更新序号
                int i = (this.zoncarePage.PageIndex - 1) * this.zoncarePage.PageSize;

                foreach (DataGridViewRow row in this.dgvData.Rows)
                {
                    user userInfo = row.DataBoundItem as user;
                    if (userInfo == null) continue;

                    userInfo.DisplayIndex = ++i;

                    //内置管理员账号不允许删除
                    if (userInfo.id == SUPER_ADMIN_ID)
                    {
                        DataGridViewEx.RowActionModel model = this.dgvData.RowActionModels.Find(o => o.RowIndex == row.Index);
                        model?.Actions?.RemoveAll(o => o.Key == "del");
                    }
                }

                this.dgvData.Invalidate();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "系统设置");
            }
        }

        private void dgvData_RowAction_ImageButtonClick(object sender, DataGridViewActionCellMouseEventArgs e)
        {
            int rowIndex = e.CellMouseEventArgs.RowIndex;
            if (rowIndex < 0 || rowIndex >= this.dgvData.Rows.Count) return;

            try
            {
                user userInfo = this.dgvData.Rows[rowIndex].DataBoundItem as user;
                if (userInfo == null) return;

                if (e.CellAction.Key == "edit")
                {
                    UserDetailForm frmDetail = new UserDetailForm(userInfo.id);
                    frmDetail.OnUserInfoSaved += (userId) =>
                    {
                        _selectedId = userId;
                        refreshList();
                    };
                    frmDetail.ShowDialogEx(this.FindForm(), showLayer: true);
                }
                else if (e.CellAction.Key == "del")
                {
                    deleteUsers(new List<user>() { userInfo });
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "系统设置");
            }
            finally
            {
                if (rowIndex < this.dgvData.Rows.Count) this.dgvData.InvalidateRow(rowIndex);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            //搜索条件变化后从第一页开始显示
            refreshList(1);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            UserDetailForm frmDetail = new UserDetailForm(null);
            frmDetail.OnUserInfoSaved += (userId) =>
            {
                //新增的用户id最大，排在最后一页，直接跳转到末页以便定位
                _selectedId = userId;
                refreshList(int.MaxValue);
            };
            frmDetail.ShowDialogEx(this.FindForm(), showLayer: true);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                BindingCollection<user> bindingList = this.dgvData.DataSource as BindingCollection<user>;
                if (bindingList == null) return;

                List<user> lstSelected = bindingList.Where(o => o.IsSelected).ToList();
                if (lstSelected.Count <= 0)
                {
                    ScreenCenterTip.Show("请至少选择一条记录。");
                    return;
                }

                deleteUsers(lstSelected);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "系统设置");
            }
        }

        private void OnItemSelectStateChanged(user curItem)
        {
            //内置管理员账号不允许删除
            this.btnDelete.Enabled = true;
            BindingCollection<user> bindingList = this.dgvData.DataSource as BindingCollection<user>;
            foreach (user item in bindingList)
            {
                if (item.id == SUPER_ADMIN_ID && item.IsSelected)
                {
                    this.btnDelete.Enabled = false;
                    break;
                }
            }
        }

        private void initUI()
        {
            //多语言文本
            this.label1.Text = _langs["用户名"];
            this.btnSearch.Text = _langs["搜索"];
            this.btnAdd.Text = _langs["新增"];
            this.btnDelete.Text = _langs["删除"];
            this.zoncarePage.Langs = _langs;

            this.tbxUserName.KeyPress += (sender, e) =>
            {
                if (e.KeyChar == 13)
                {
                    e.Handled = true;
                    refreshList(1);
                }
            };

            this.zoncarePage.PageIndexChanged += ZoncarePage_PageIndexChanged;

            initGrid();
        }

        private void ZoncarePage_PageIndexChanged(object sender, EventArgs e)
        {
            try
            {
                refreshList(this.zoncarePage.PageIndex);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "系统设置");
            }
        }

        private void initGrid()
        {
            //Load事件可能被多次触发，避免重复添加列
            if (this.dgvData.Columns.Count > 0) return;

            this.dgvData.AutoGenerateColumns = false;
            this.dgvData.ColumnHeadersHeight = 32;
            this.dgvData.ActionColumnHeaderText = _langs["操作"];

            DataGridViewCheckBoxColumn col0 = this.dgvData.AddCheckBoxHeaderColumn((colIndex, state) =>
            {
                this.dgvData.EndEdit();
                this.dgvData.Rows.OfType<DataGridViewRow>().ToList().ForEach(row =>
                {
                    row.Cells[colIndex].Value = state;
                    user rowData = row.DataBoundItem as user;
                    if (rowData != null) rowData.IsSelected = state;
                });
            });
            col0.Width = 60;
            col0.DataPropertyName = "IsSelected";

            this.dgvData.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", DataPropertyName = "id", Visible = false });

            DataGridViewTextBoxColumn indexCol = new DataGridViewTextBoxColumn { Name = "Index", HeaderText = _langs["序号"], DataPropertyName = "DisplayIndex" };
            indexCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            indexCol.SortMode = DataGridViewColumnSortMode.NotSortable;
            this.dgvData.Columns.Add(indexCol);

            //其余列
            DataGridViewColumn[] cols = new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name="UserName", HeaderText=_langs["用户名"], DataPropertyName="userName" },
                new DataGridViewTextBoxColumn { Name="RealName",  HeaderText=_langs["姓名"], DataPropertyName="realname" },
                new DataGridViewTextBoxColumn { Name="DeptName", HeaderText=_langs["科室"], DataPropertyName="deptName" },
                new DataGridViewTextBoxColumn { Name="RoleName", HeaderText=_langs["角色"], DataPropertyName="roleName" }
            };
            foreach (DataGridViewColumn col in cols)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            this.dgvData.Columns.AddRange(cols);

            this.dgvData.DataActionMode = new List<DataGridViewEx.GridDataAction>()
            {
                new DataGridViewEx.GridDataAction(){ Key="edit", ActionType=DataGridViewEx.RowActionType.ImageButton, DisplayText=_langs["编辑"], Value=Properties.Resources.edit },
                new DataGridViewEx.GridDataAction(){ Key="del", ActionType=DataGridViewEx.RowActionType.ImageButton, DisplayText=_langs["删除"], Value=Properties.Resources.delete },
            };

            BindingCollection<user> bindingList = new BindingCollection<user>(new List<user>(), this);
            this.dgvData.DataSource = bindingList;

            this.dgvData.CellContentClick += dgvData_CellContentClick;
            this.dgvData.DataBindingComplete += dgvData_DataBindingComplete;
        }

        /// <summary>
        /// 逻辑删除用户
        /// </summary>
        /// <param name="users">待删除的用户</param>
        /// <returns>是否执行了删除</returns>
        private bool deleteUsers(List<user> users)
        {
            if (users == null || users.Count == 0) return false;

            //内置管理员账号不允许删除(列表中已禁用其[删除]操作，此处为兜底)
            if (users.Any(o => o.id == SUPER_ADMIN_ID)) return false;

            if (CommonFuns.PromptMessage("确定删除吗？", 1) != DialogResult.OK) return false;

            Stopwatch watcher = Stopwatch.StartNew();
            ZoncareEntities dbEntities = ZoncareEntities.Instance;

            List<int> ids = users.Select(o => o.id).ToList();
            List<user> storedItems = dbEntities.User.Where(u => ids.Contains(u.id)).ToList();
            foreach (user item in storedItems)
            {
                item.delFlag = 1;
                item.updateTime = DateTime.Now;
                item.updateBy = user.Loginer?.userName;
            }
            dbEntities.SaveChanges();

            OperateLog.logParam("删除用户信息", ids, watcher.ElapsedMilliseconds, user.Loginer?.userName);

            _selectedId = null;
            refreshList();

            return true;
        }

        private void refreshList(int pageIndex = -1)
        {
            try
            {
                int pageSize = this.zoncarePage.PageSize;
                if (pageIndex <= 0) pageIndex = this.zoncarePage.PageIndex;

                var query = ZoncareEntities.Instance.User.AsNoTracking().Where(u => u.delFlag == null || u.delFlag == 0);
                string userName = this.tbxUserName.Text.Trim();
                if (!string.IsNullOrEmpty(userName))
                {
                    query = query.Where(o => o.userName.Contains(userName) || (o.realname != null && o.realname.Contains(userName)));
                }

                int recordCount = query.Count();
                this.zoncarePage.RecordCount = recordCount;

                //删除末页最后一条记录后，自动回退到有效页码
                int pageCount = Math.Max(1, (recordCount + pageSize - 1) / pageSize);
                if (pageIndex > pageCount) pageIndex = pageCount;
                if (pageIndex < 1) pageIndex = 1;

                //分页查询必须指定排序，否则Skip/Take的结果顺序不稳定
                List<user> dataList = query.OrderBy(o => o.id)
                    .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

                BindingCollection<user> bindingList = new BindingCollection<user>(dataList, this);
                foreach (user item in bindingList) { item.OnSelectStateChanged += OnItemSelectStateChanged; }

                //页码需在绑定数据前更新，DataBindingComplete中依赖页码计算序号
                this.zoncarePage.PageIndex = pageIndex;
                this.dgvData.DataSource = bindingList;

                //数据源已变化，表头复选框重置为未选中
                if (this.dgvData.Columns.Contains("chk")) this.dgvData.Columns["chk"].HeaderCell.Value = false;

                if (_isFirstShow)
                {
                    this.dgvData.ResizeColumnWidth(new List<int>() { 4, 0, 4, 10, 10, 10, 10 });
                }

                restoreSelection();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "系统设置");
            }
        }

        /// <summary>
        /// 定位到之前操作的行
        /// </summary>
        private void restoreSelection()
        {
            if (!_selectedId.HasValue) return;

            foreach (DataGridViewRow dr in this.dgvData.Rows)
            {
                user bindItem = dr.DataBoundItem as user;
                if (bindItem != null && bindItem.id == _selectedId.Value)
                {
                    this.dgvData.CurrentCell = dr.Cells["chk"];
                    break;
                }
            }

            _selectedId = null;
        }
    }
}
