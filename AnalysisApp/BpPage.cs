using AnalysisApp.Commons;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.DAL;

namespace AnalysisApp
{
    public partial class BpAnalysisPage : BaseUserControl
    {
        private bpDiagnoseSettingValues settingValues = CommonValues.SystemSettings.bpSets.bpDiagnoseSets;
        private DateTime DayTimeStart = DateTime.Now;
        private DateTime DayTimeEnd = DateTime.Now;
        private ZoncareEntities _dbEntities;
        private static readonly string[] ErrorCodes = { "496", "497", "498", "499", "500" };

        public BpAnalysisPage()
        {
            InitializeComponent();
        }

        public override async Task RefreshData(bool resetUI = false)
        {
            await base.RefreshData(resetUI);
            loadPageData();
            if (resetUI)
            {
                this.tabControl1.SelectedIndex = 0;
                this.drawEcgCommonWave1.Visible = AnaValues.CurrentCase.checkItem == 3;
            }
        }

        #region 事件
        private void BpAnalysisPage_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                _dbEntities = ZoncareEntities.Instance;

                initCtl();

                loadPageData();

                Console.WriteLine("血压分析界面已加载");
            }
        }

        private void dgvBP_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridViewColumn col = this.dgvBP.Columns[e.ColumnIndex];
            DataGridViewRow row = this.dgvBP.Rows[e.RowIndex];
            BPDataInfo rowData = row.DataBoundItem as BPDataInfo;
            bool isExcluded = row.Cells["dataStatus"].Value?.ToString() == "0";
            if (isExcluded)
            {
                e.CellStyle.BackColor = Color.Gainsboro;
                e.CellStyle.ForeColor = Color.DimGray;
                e.CellStyle.SelectionBackColor = Color.Gainsboro;
                e.CellStyle.SelectionForeColor = Color.DimGray;
            }
            else
            {
                // 恢复默认颜色
                //row.DefaultCellStyle.BackColor = dgvBP.DefaultCellStyle.BackColor;
                //row.DefaultCellStyle.ForeColor = dgvBP.DefaultCellStyle.ForeColor;
                //row.DefaultCellStyle.SelectionForeColor = row.DefaultCellStyle.ForeColor;

                e.CellStyle.BackColor = row.DefaultCellStyle.BackColor;
                e.CellStyle.ForeColor = row.DefaultCellStyle.ForeColor;
                e.CellStyle.SelectionForeColor = row.DefaultCellStyle.ForeColor;
            }

            DateTime dateTime = DateTime.Parse(row.Cells["bpDateTime"].Value?.ToString());
            double sBP = rowData.SBP;
            double dBP = rowData.DBP;
            double mAP = rowData.MAP;
            int pulse = (int)row.Cells["pulse"].Value;
            string sBPStr = sBP.ToString();
            string dBPStr = dBP.ToString();
            string mAPStr = mAP.ToString();
            string pulseStr = pulse.ToString();
            double maxWakeSys = settingValues.maxWakeSys;
            double minWakeSys = settingValues.minWakeSys;
            double maxSleepSys = settingValues.maxSleepSys;
            double minSleepSys = settingValues.minSleepSys;
            double maxErrorSys = settingValues.maxErrorSys;
            double minErrorSys = settingValues.minErrorSys;

            double maxWakeDia = settingValues.maxWakeDia;
            double minWakeDia = settingValues.minWakeDia;
            double maxSleepDia = settingValues.maxSleepDia;
            double minSleepDia = settingValues.minSleepDia;
            double maxErrorDia = settingValues.maxErrorDia;
            double minErrorDia = settingValues.minErrorDia;

            int maxErrorPr = settingValues.maxErrorPr;
            if (settingValues.unit == "kPa")
            {
                sBPStr = CommonFuns.mmHg2KPa(Convert.ToInt32(sBP), 2).ToString("F2");
                dBPStr = CommonFuns.mmHg2KPa(Convert.ToInt32(dBP), 2).ToString("F2");
                mAPStr = CommonFuns.mmHg2KPa(Convert.ToInt32(mAP), 2).ToString("F2");

                //maxWakeSys = CommonFuns.mmHg2KPa(settingValues.maxWakeSys, 2);
                //minWakeSys = CommonFuns.mmHg2KPa(settingValues.minWakeSys, 2);
                //maxSleepSys = CommonFuns.mmHg2KPa(settingValues.maxSleepSys, 2);
                //minSleepSys = CommonFuns.mmHg2KPa(settingValues.minSleepSys, 2);
                //maxErrorSys = CommonFuns.mmHg2KPa(settingValues.maxErrorSys, 2);
                //minErrorSys = CommonFuns.mmHg2KPa(settingValues.minErrorSys, 2);

                //maxWakeDia = CommonFuns.mmHg2KPa(settingValues.maxWakeDia, 2);
                //minWakeDia = CommonFuns.mmHg2KPa(settingValues.minWakeDia, 2);
                //maxSleepDia = CommonFuns.mmHg2KPa(settingValues.maxSleepDia, 2);
                //minSleepDia = CommonFuns.mmHg2KPa(settingValues.minSleepDia, 2);
                //maxErrorDia = CommonFuns.mmHg2KPa(settingValues.maxErrorDia, 2);
                //minErrorDia = CommonFuns.mmHg2KPa(settingValues.minErrorDia, 2);
            }

            bool isElevated = false, isDecreased = false;
            if (col.Name == "SBP")
            {
                if (sBP > maxErrorSys || sBP < minErrorSys)
                {
                    sBPStr = sBPStr + " E";
                    e.CellStyle.ForeColor = Color.Red;
                }
                else
                {
                    bool bDay = CalcBPInfo.IsInTimeRange(dateTime, TimeSpan.FromHours(DayTimeStart.Hour), TimeSpan.FromHours(DayTimeEnd.Hour));
                    if (bDay)
                    {
                        if (sBP > maxWakeSys)
                        {
                            sBPStr = sBPStr + " ↑";
                            isElevated = true;
                        }
                        if (sBP < minWakeSys)
                        {
                            sBPStr = sBPStr + " ↓";
                            isDecreased = true;
                        }
                    }
                    else
                    {
                        if (sBP > maxSleepSys)
                        {
                            sBPStr = sBPStr + " ↑";
                            isElevated = true;
                        }
                        if (sBP < minSleepSys)
                        {
                            sBPStr = sBPStr + " ↓";
                            isDecreased = true;
                        }
                    }
                }
                if (e.Value != null) { e.Value = sBPStr; }
                if (isElevated || isDecreased) { e.CellStyle.ForeColor = ColorTranslator.FromHtml("#ffb800"); }
            }
            else if (col.Name == "DBP")
            {
                if (dBP > maxErrorDia || dBP < minErrorDia)
                {
                    dBPStr = dBPStr + " E";
                    e.CellStyle.ForeColor = Color.Red;
                }
                else
                {
                    bool bDay = CalcBPInfo.IsInTimeRange(dateTime, TimeSpan.FromHours(DayTimeStart.Hour), TimeSpan.FromHours(DayTimeEnd.Hour));
                    if (bDay)
                    {
                        if (dBP > maxWakeDia)
                        {
                            dBPStr = dBPStr + " ↑";
                            isElevated = true;
                        }
                        if (dBP < minWakeDia)
                        {
                            dBPStr = dBPStr + " ↓";
                            isDecreased = true;
                        }
                    }
                    else
                    {
                        if (dBP > maxSleepDia)
                        {
                            dBPStr = dBPStr + " ↑";
                            isElevated = true;
                        }
                        if (dBP < minSleepDia)
                        {
                            dBPStr = dBPStr + " ↓";
                            isDecreased = true;
                        }
                    }
                }
                if (e.Value != null) { e.Value = dBPStr; }
                if (isElevated || isDecreased) { e.CellStyle.ForeColor = ColorTranslator.FromHtml("#ffb800"); }
            }
            else if (col.Name == "MAP")
            {
                if (e.Value != null) { e.Value = mAPStr; }
            }
            else if (col.Name == "Pulse")
            {
                if (pulse > maxErrorPr)
                {
                    pulseStr = pulseStr + " E";
                    e.CellStyle.ForeColor = Color.Red;
                }
                if (e.Value != null) { e.Value = pulseStr; }
            }

            if (!isExcluded)
            {
                e.CellStyle.SelectionForeColor = e.CellStyle.ForeColor;
            }
        }

        private void dgvBP_CellContentClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) { return; }

            try
            {
                DataGridView dgv = sender as DataGridView;
                DataGridViewRow clickedRow = dgv.Rows[e.RowIndex];
                DataGridViewCell curCell = clickedRow.Cells[e.ColumnIndex];
                if (curCell is DataGridViewCheckBoxCell)
                {
                    if (!curCell.ReadOnly)
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
                        dgv.InvalidateCell(e.ColumnIndex, e.RowIndex);
                    }
                }
                else if (dgv.Columns[e.ColumnIndex].Name == "notes")
                {
                    dgv.BeginEdit(false);
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "血压数据分析");
            }
        }

        private void dgvBP_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (this.dgvBP.Columns[e.ColumnIndex].Name != "notes") return;

            DataGridViewRow row = this.dgvBP.Rows[e.RowIndex];
            DataGridViewCell cell = row.Cells[e.ColumnIndex];
            string newValue = e.FormattedValue?.ToString() ?? "";
            if (CommonValues.LanguageID == 2052)
            {
                if (newValue.Length > 4)
                {
                    ScreenCenterTip.Show("最多只能输入4个字符");
                    cell.Tag = new CellState() { IsValidValue = false };
                    this.dgvBP.CancelEdit();
                    return;
                }
            }

            BPDataInfo rowData = row.DataBoundItem as BPDataInfo;
            cell.Tag = new CellState()
            {
                IsValidValue = true,
                OldValue = rowData.Notes,
                NewValue = newValue
            };
        }

        private void dgvBP_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = this.dgvBP.Rows[e.RowIndex];
            BPDataInfo rowData = row.DataBoundItem as BPDataInfo;
            DataGridViewColumn col = this.dgvBP.Columns[e.ColumnIndex];
            DataGridViewCell cell = row.Cells[e.ColumnIndex];

            if (col.Name == "notes")
            {
                CellState state = cell.Tag as CellState;
                //更新到数据库
                if (state != null)
                {
                    if (state.IsValidValue)
                    {
                        if (state.NewValue != state.OldValue)
                        {
                            var bpDataRecord = _dbEntities.BpDetails.FirstOrDefault(o => o.id == rowData.Id);
                            bpDataRecord.notes = rowData.Notes;
                            _dbEntities.SaveChanges();
                        }
                    }
                    else
                    {
                        BeginInvoke(new MethodInvoker(() =>
                        {
                            this.dgvBP.CurrentCell = this.dgvBP.Rows[e.RowIndex].Cells[0];
                        }));
                    }
                }
            }
            cell.Tag = null;
        }

        private void dgvBP_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in this.dgvBP.Rows)
            {
                //隐藏失败重测之前的一条测量错误记录
                BPDataInfo rowData = row.DataBoundItem as BPDataInfo;
                if (rowData.MeasureType == 1)
                {
                    if (row.Index > 0)
                    {
                        DataGridViewRow previousRow = this.dgvBP.Rows[row.Index - 1];
                        BPDataInfo previousData = previousRow.DataBoundItem as BPDataInfo;
                        if (!string.IsNullOrEmpty(previousData.ErrorCode) && previousData.ErrorCode != "0")
                        {
                            previousRow.Visible = false;
                        }
                    }
                }

                //隐藏空采集产生的数据与无效错误数据
                if ("6".Equals(rowData.ErrorCode) || "7".Equals(rowData.ErrorCode) || ErrorCodes.Contains(rowData.ErrorCode))
                {
                    row.Visible = false;
                }

                if (rowData.DataStatus == 0)
                {
                    if (rowData.SBP == 0 || rowData.DBP == 0 || rowData.MAP == 0)
                    {
                        row.Cells[0].ReadOnly = true;
                    }
                }
            }

            if (e.ListChangedType == System.ComponentModel.ListChangedType.Reset)
            {
                if (this.dgvBP.Rows.Count > 0) this.dgvBP.CurrentCell = this.dgvBP.FirstDisplayedCell;
            }
            this.dgvBP.Invalidate();
        }

        private void dgvBP_SelectionChanged(object sender, EventArgs e)
        {
            if (this.dgvBP.SelectedRows.Count == 0)
            {
                //清空血压波形
                return;
            }

            BPDataInfo bpData = dgvBP.SelectedRows[0].DataBoundItem as BPDataInfo;
            this.bpWaveChart1.BpDataRecord = bpData;
            if (this.bpWaveChart1.IsLoaded)
            {
                this.bpWaveChart1.LoadWave();
            }
            if (this.drawEcgCommonWave1.IsLoaded)
            {
                TimeSpan span = bpData.bpDateTime.Subtract(AnaValues.holterConfig.StartDateTime);
                int sample = (int)(span.TotalSeconds * AnaValues.holterConfig.SampleRate);
                if (sample < AnaValues.holterConfig.Total)
                {
                    this.drawEcgCommonWave1.OnWaveformStartTimeChanged(sample);
                }
            }
        }

        private void notes_TextChanged(object sender, EventArgs e)
        {
            RichTextBox richTextBox = sender as RichTextBox;
            var bpTrendNotesJson = new JObject();
            bpTrendNotesJson["notes"] = richTextBox.Text;
            string bpInfoFilePath = "";
            if (!string.IsNullOrEmpty(AnaValues.RootPath))
            {
                bpInfoFilePath = Path.Combine(AnaValues.RootPath, AnaValues.BPInfoFileName);
            }
            // 将bpTrendNotesJson写入bpInfoFilePath
            try
            {
                string jsonString = bpTrendNotesJson.ToString();
                File.WriteAllText(bpInfoFilePath, jsonString, Encoding.UTF8);
                //ScreenCenterTip.Show("保存成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"写入文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //恢复
        private void btnRestore_Click(object sender, EventArgs e)
        {
            setDataStatus(1);
            this.drawBpTrendWave1.UpdateDataRefresh();
            this.bpHistogramControl1.UpdateDataRefresh();
            this.bpPieChatControl1.UpdateDataRefresh();
            this.bpFitPlotControl1.UpdateDataRefresh();
            AnaValues.DataSourceState = 2;
        }

        //排除
        private void btnDiscard_Click(object sender, EventArgs e)
        {
            setDataStatus(0);
            this.drawBpTrendWave1.UpdateDataRefresh();
            this.bpHistogramControl1.UpdateDataRefresh();
            this.bpPieChatControl1.UpdateDataRefresh();
            this.bpFitPlotControl1.UpdateDataRefresh();
            AnaValues.DataSourceState = 2;

        }

        //比较
        private void btnCompare_Click(object sender, EventArgs e)
        {
            BpCompareForm.Instance.Show(AnaValues.CurrentCase.name, this.FindForm());

        }

        #endregion


        public override void InitUI()
        {
            base.InitUI();
            initGrid();
        }

        private void initCtl()
        {


            this.bpWaveChart1.OnEnsureDataUpdate += (sbp, dbp, map) =>
            {
                if (this.dgvBP.SelectedRows.Count > 0)
                {
                    BPDataInfo rowData = this.dgvBP.SelectedRows[0].DataBoundItem as BPDataInfo;
                    bpDetail dbData = _dbEntities.BpDetails.FirstOrDefault(o => o.id == rowData.Id);
                    if (dbData != null)
                    {
                        dbData.sysPressure = sbp * 1000;
                        dbData.diaPressure = dbp * 1000;
                        dbData.mapPressure = map * 1000;
                        _dbEntities.SaveChanges();

                        AnaValues.CurrentBpDetails = _dbEntities.BpDetails.AsNoTracking().Where(o => o.caseId == AnaValues.CurrentCase.id).ToList();
                        loadPageData();
                    }
                }
            };
        }

        private void initGrid()
        {
            // 设置表格基本样式
            CommonFuns.InitDataGridStyles(this.dgvBP);
            this.dgvBP.ColumnHeadersHeight = 30;
            this.dgvBP.RowTemplate.Height = 26;

            //复选框
            DataGridViewCheckBoxColumn col0 = this.dgvBP.AddCheckBoxHeaderColumn((colIndex, state) =>
            {
                this.dgvBP.EndEdit();
                this.dgvBP.Rows.OfType<DataGridViewRow>().ToList().ForEach(row =>
                {
                    if (!row.Cells[colIndex].ReadOnly)
                    {
                        row.Cells[colIndex].Value = state;
                        BPDataInfo rowData = row.DataBoundItem as BPDataInfo;
                        rowData.IsSelected = state;
                    }
                });
            });
            col0.DataPropertyName = "IsSelected";
            col0.MinimumWidth = 30;

            DataGridViewColumn[] cols = new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name="date", HeaderText="日期", DataPropertyName="Date" },
                new DataGridViewTextBoxColumn { Name="time", HeaderText="时间", DataPropertyName="Time" },
                new DataGridViewTextBoxColumn { Name="SBP", HeaderText="收缩压", DataPropertyName="SBP" },
                new DataGridViewTextBoxColumn { Name="DBP", HeaderText="舒张压", DataPropertyName="DBP" },
                new DataGridViewTextBoxColumn { Name="MAP", HeaderText="平均动脉压", DataPropertyName="MAP" },
                new DataGridViewTextBoxColumn { Name="pulse", HeaderText="脉率", DataPropertyName="Pulse" },
                new DataGridViewTextBoxColumn { Name="motionState", HeaderText="状态", DataPropertyName="MotionState" },
                //new DataGridViewTextBoxColumn { Name="errorCode", HeaderText="错误代码", DataPropertyName="ErrorCode" },
                new DataGridViewTextBoxColumn { Name="notes", HeaderText="备注", DataPropertyName="Notes", ReadOnly=false },
                new DataGridViewTextBoxColumn { Name="dataStatus", HeaderText="数据状态", DataPropertyName="DataStatus", Visible=false },
                new DataGridViewTextBoxColumn { Name="bpDateTime", HeaderText="时间日期", DataPropertyName="bpDateTime", Visible=false },
            };
            foreach (DataGridViewColumn col in cols)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                if (col.Name != "notes") col.ReadOnly = true;
            }
            this.dgvBP.Columns.AddRange(cols);

            // 为每列单独设置样式（覆盖默认）
            foreach (DataGridViewColumn col in this.dgvBP.Columns)
            {
                col.HeaderCell.Style.BackColor = Color.LightSteelBlue;
            }

            this.dgvBP.CellFormatting += dgvBP_CellFormatting;
            this.dgvBP.CellMouseClick += dgvBP_CellContentClick;
            this.dgvBP.SelectionChanged += dgvBP_SelectionChanged;
            this.dgvBP.CellValidating += dgvBP_CellValidating;
            this.dgvBP.CellEndEdit += dgvBP_CellEndEdit;
            this.dgvBP.DataBindingComplete += dgvBP_DataBindingComplete;
        }

        private void initData()
        {
            this.dgvBP.DataSource = null;

            BindingCollection<BPDataInfo> bindingBPList = new BindingCollection<BPDataInfo>(CalcBPInfo.GetBPDataList(), this);
            // 绑定数据到表格
            this.dgvBP.DataSource = bindingBPList;

            //if (CalcBPInfo.settingValues.unit == "kPa")
            //{
            //    this.dgvBP.Columns["SBP"].HeaderText = "收缩压(kPa)";
            //    this.dgvBP.Columns["DBP"].HeaderText = "舒张压(kPa)";
            //    this.dgvBP.Columns["MAP"].HeaderText = "平均压(kPa)";
            //}
            //else
            //{
            //    this.dgvBP.Columns["SBP"].HeaderText = "收缩压(mmHg)";
            //    this.dgvBP.Columns["DBP"].HeaderText = "舒张压(mmHg)";
            //    this.dgvBP.Columns["MAP"].HeaderText = "平均压(mmHg)";
            //}

        }

        private void setDataStatus(int status)
        {
            List<int> lstSelected = (this.dgvBP.DataSource as BindingCollection<BPDataInfo>).Where(o => o.IsSelected).Select(o => o.Id).ToList();
            if (lstSelected.Count <= 0)
            {
                ScreenCenterTip.Show("请至少选择一条记录。");
                return;
            }

            bool statusChanged = false;
            List<bpDetail> records = _dbEntities.BpDetails.Where(o => lstSelected.Contains(o.id)).ToList();
            foreach (bpDetail record in records)
            {
                if (record.dataStatus != status)
                {
                    record.dataStatus = status;
                    statusChanged = true;
                }
            }

            if (statusChanged)
            {
                _dbEntities.SaveChanges();

                AnaValues.CurrentBpDetails = _dbEntities.BpDetails.AsNoTracking().Where(o => o.caseId == AnaValues.CurrentCase.id).ToList();

                initData();
            }
        }

        private void loadPageData()
        {
            settingValues = CommonValues.SystemSettings.bpSets.bpDiagnoseSets;

            DayTimeStart = DateTime.Parse(settingValues.daytimeStart);
            DayTimeEnd = DateTime.Parse(settingValues.daytimeEnd);

            initData();

            this.drawBpTrendWave1.UpdateDataRefresh();
            this.bpHistogramControl1.InitRadioCheckStatus();
            this.bpPieChatControl1.InitRadioCheckStatus();
            this.bpFitPlotControl1.UpdateDataRefresh();

            this.notes.Text = CalcBPInfo.GetNotes();
        }

        private class CellState
        {
            public bool IsValidValue { get; set; }
            public string OldValue { get; set; }
            public string NewValue { get; set; }
        }
    }
}
