using AnalysisSystem.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Zoncare.Common;
using Zoncare.Component;
using Zoncare.Component.Controls;
using Zoncare.DAL;
using Zoncare.Utilities;

namespace EcgBpAnalysis.FuncComponent
{
    public partial class RegisterForm : BaseDialogForm
    {
        private int _registerType { get; set; } //操作类型(0:登记; 1:读卡)
        private int? _deviceType;
        private string _deviceModel;

        private bool _isNewCase;
        private int? _originalID;
        private int? _checkItem;
        private examCase _originalPatient;
        private DateTime _lastValidBirthDate;
        private static readonly int[] NewDeviceTypes = { 2, 3, 4 };

        private DateTimePicker dtpStartEnd = new DateTimePicker();
        private NumericUpDown numUpDown = new NumericUpDown();

        public event Action<examCase, DeviceInfo, bool, bool> OnDataSaved;

        public RegisterForm(int? caseId = null, int? checkItem = null) : base("", "登记")
        {
            InitializeComponent();

            _checkItem = checkItem;
            initForCheckItem();

            SuspendLayout();

            this.Text = "登记";
            _registerType = 0;
            _originalID = caseId;
            if (caseId == null) { _isNewCase = true; }
        }

        public RegisterForm(examCase patientInfo, int deviceType, string deviceModel) : base("", "读卡")
        {
            InitializeComponent();

            _deviceType = deviceType;
            _deviceModel = deviceModel;

            initForCheckItem();
            SuspendLayout();

            this.Text = "读卡";
            _registerType = 1;

            if (patientInfo != null)
            {
                _originalPatient = patientInfo;
                _originalPatient.deviceType = deviceType.ToString();
            }
      
            this.cbxCheckItem.Enabled = false;
            this.chkEnterAnalysis.Visible = true;
            this.btnNext.Visible = true;
            this.btnSave.Visible = this.btnSaveAndWrite.Visible = false;
        }

        #region 事件
        private void RegisterForm_Load(object sender, EventArgs e)
        {
            try
            {
                init();

                string dateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat];
                this.dtpBirthday.ValueChanged += (o, arg) => { this.dtpBirthday.CustomFormat = dateFormat; checkBirthDate(); };
                this.dtpBirthday.MouseUp += (o, arg) => { checkBirthDate(); };
                //this.dtpBirthday.CloseUp += (o, arg) => { convertBirthToAge(this.dtpBirthday.Value); };
                this.dtpApplyTime.ValueChanged += (o, arg) => { this.dtpApplyTime.CustomFormat = dateFormat + " HH:mm:ss"; };

                this.tbxCaseNo.Focus();
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "患者信息");
            }
        }

        private void RegisterForm_Shown(object sender, EventArgs e)
        {
            foreach (Control ctl in tlpThreshold.Controls)
            {
                if (ctl is Label lbl) { lbl.Text += "："; }
            }
            foreach (Control ctl in tlpControl.Controls)
            {
                if (ctl is Label lbl) { lbl.Text += "："; }
            }

            ResumeLayout(true);
            BeginInvoke(new MethodInvoker(loadData));
        }

        //保存患者信息
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!verifyPatientInfo()) return;

            examCase srcPatient = null;
            if (!_isNewCase || _registerType == 1) { srcPatient = _originalPatient; }

            try
            {
                examCase patient = getPatientInfo(srcPatient);
                bool isSuccess = savePatientInfo(patient);
                if (isSuccess)
                {
                    ScreenCenterTip.Show("操作成功");
                    OnDataSaved?.Invoke(patient, null, false, false);
                    Close();
                }
                else
                {
                    CommonFuns.PromptError("操作失败");
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "患者信息");
                CommonFuns.PromptError("操作失败");
            }
        }

        //保存并写入设备
        private async void btnSaveAndWrite_Click(object sender, EventArgs e)
        {
            int[] arrNewDeviceType = { 2, 3, 4 };

            if (!verifyPatientInfo()) return;

            int checkItem = Convert.ToInt32(this.cbxCheckItem.SelectedValue);
            DeviceStatus statu = await AppCode.CheckDeviceStatus(checkItem);
            if (!statu.IsReady)
            {
                ScreenCenterTip.Show("未检测到设备或设备通讯失败");
                return;
            }

            int deviceType = statu.DeviceInfo.DeviceType;
            bool isNewDevice = arrNewDeviceType.Contains(deviceType);
            bool result = false;
            try
            {
                examCase srcPatient = null;
                if (!_isNewCase || _registerType == 1) { srcPatient = _originalPatient; }

                examCase patient = getPatientInfo(srcPatient);
                if (checkItem == 1) //心电
                {
                    if (isNewDevice)
                    {
                        if (CommonFuns.PromptMessage("信息写入设备后将清除监测仪内的历史数据，确认继续该操作吗？", 1) != DialogResult.OK) return;
                    }
                    result = savePatientInfo(patient);
                    if (result == true)
                    {
                        if (isNewDevice)
                        {
                            AppCode.WriteHolterSettingFile(statu.DrivePath, patient, "On");
                            AppCode.ClearDeviceData(statu.DrivePath, checkItem);
                        }
                    }
                }
                else
                {
                    if (!verifyBpParameters()) return;
                    if (CommonFuns.PromptMessage("戴机参数写入设备后将清除监测仪内的历史数据，确认继续该操作吗？", 1) != DialogResult.OK) return;

                    BpSettingScheme bpSettings = saveBpSettingScheme(deviceType); //保存血压戴机设置参数

                    result = savePatientInfo(patient);
                    if (result == true)
                    {
                        if (checkItem == 2) //血压
                        {
                            //if (deviceType == 2)
                            //{
                            //    AppCode.SetBpMonitorParameters(bpSettings, out string errMsg);
                            //    if (!string.IsNullOrEmpty(errMsg))
                            //    {
                            //        ScreenCenterTip.Show(errMsg);
                            //        return;
                            //    }
                            //}
                            if (deviceType == 3 || deviceType == 4)
                            {
                                AppCode.WriteNibpSettingFile(statu.DrivePath, patient, bpSettings);
                            }
                        }
                        else if (checkItem == 3) //二合一
                        {
                            AppCode.WriteHolterSettingFile(statu.DrivePath, patient, "On");
                            AppCode.WriteNibpSettingFile(statu.DrivePath, patient, bpSettings);
                        }
                        AppCode.ClearDeviceData(statu.DrivePath, checkItem);
                    }
                }

                if (result == true)
                {
                    if (isNewDevice)
                    {
                        //更新设备用途
                        AppCode.ModifyDeviceUsage((patient.checkItem - 1).ToString(), statu.DrivePath);
                        //同步设备时间
                        if (this.chkSyncTime.Checked)
                        {
                            string nowTimeString = DateTime.Now.ToString("yyyyMMddHHmmss");
                            if (deviceType == 3) nowTimeString = "a" + nowTimeString;
                            AppCode.SetDeviceInfo(nowTimeString);
                        }
                    }

                    //更新所登记病例的相关设备类型
                    using (var db = ZoncareEntities.CreateIndependent())
                    {
                        var dbCase = db.ExamCases.FirstOrDefault(o => o.id == patient.id);
                        if (dbCase != null) { dbCase.deviceType = deviceType.ToString(); db.SaveChanges(); }
                    }

                    OnDataSaved?.Invoke(patient, statu.DeviceInfo, false, false);
                    ScreenCenterTip.Show("操作成功");
                    Close();
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "患者信息");
            }
            finally
            {
                if (result == false) { CommonFuns.PromptError("操作失败"); }
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (_registerType == 0) //登记
            {
                int curIndex = this.tabNavigation.SelectedIndex;
                if (curIndex == 0)
                {
                    if (!verifyPatientInfo()) return;
                    this.btnSaveAndWrite.Visible = true;
                    this.btnNext.Text = _langs["上一步"];
                }
                else
                {
                    this.btnSaveAndWrite.Visible = false;
                    this.btnNext.Text = _langs["下一步"];
                }
                this.tabNavigation.SwitchTabPage(curIndex ^ 1);
            }
            else //读卡
            {
                if (!verifyPatientInfo()) return;

                examCase srcPatient = null;
                if (!_isNewCase || _registerType == 1) { srcPatient = _originalPatient; }

                examCase patient = getPatientInfo(srcPatient);
                bool savedPatient = savePatientInfo(patient, _deviceType);
                if (savedPatient)
                {
                    this.Hide();
                    OnDataSaved?.Invoke(patient, new DeviceInfo(_deviceType.Value, _deviceModel), true, this.chkEnterAnalysis.Checked);
                    this.Close();
                }
                else
                {
                    CommonFuns.PromptError("操作失败");
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cbxCheckItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            int checkItem = Convert.ToInt32(this.cbxCheckItem.SelectedValue);
            this.btnSaveAndWrite.Visible = false;
            if (_isNewCase && checkItem == 1)
            {
                this.btnSaveAndWrite.Visible = true;
            }

            this.lblSyncTime.Visible = false;
            this.chkSyncTime.Visible = false;
            if (checkItem > 0)
            {
                if (checkItem == 1 || _registerType == 1)
                {
                    this.tabNavigation.ApplyHeaderlessStyle = true;
                    this.tabNavigation.SwitchTabPage(0);
                }
                else
                {
                    this.tabNavigation.ApplyStepStyle = true;
                    this.btnNext.Visible = true;
                }

                this.btnSaveAndWrite.Enabled = false;
                int assignedDeviceType = -1;
                if (_originalPatient != null && _originalPatient.deviceType != null)
                {
                    assignedDeviceType = Convert.ToInt32(_originalPatient.deviceType);
                    _deviceType = assignedDeviceType;
                }

                AppCode.CheckDeviceStatus(checkItem).ContinueWith(t =>
                {
                    DeviceStatus statu = t.Result;
                    if (statu.IsReady)
                    {
                        bool isNewDevice = (NewDeviceTypes.Contains(assignedDeviceType) || NewDeviceTypes.Contains(statu.DeviceInfo.DeviceType));
                        if (_deviceType == null) _deviceType = statu.DeviceInfo.DeviceType;
                        if (string.IsNullOrEmpty(_deviceModel)) _deviceModel = statu.DeviceInfo.DeviceModel;

                        this.InvokeIfRequired(() =>
                        {
                            if ((checkItem == 1 && isNewDevice) || (checkItem > 1 && _isNewCase))
                            {
                                this.btnSaveAndWrite.Enabled = true;
                                if (isNewDevice && !string.IsNullOrEmpty(statu.LeadOffAlarm))
                                {
                                    this.lblLeadOff.Visible = this.cbxLeadOffAlarm.Visible = true;
                                    this.cbxLeadOffAlarm.SelectedValue = statu.LeadOffAlarm;
                                }
                            }
                            //设备时间显示
                            if (checkItem > 1 || isNewDevice)
                            {
                                this.lblSyncTime.Visible = true;
                                this.chkSyncTime.Visible = true;
                            }
                        });
                    }
                });
            }
        }

        private void cbxAgeUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ("Y".Equals(this.cbxAgeUnit.SelectedValue))
            {
                this.tbxAge.MinValue = 0;
                this.tbxAge.MaxValue = 220;
            }
            else if ("M".Equals(this.cbxAgeUnit.SelectedValue))
            {
                this.tbxAge.MinValue = 1;
                this.tbxAge.MaxValue = 12;
            }
            else if ("D".Equals(this.cbxAgeUnit.SelectedValue))
            {
                this.tbxAge.MinValue = 1;
                this.tbxAge.MaxValue = 366;
            }
            this.tbxAge.MaxLength = this.tbxAge.MaxValue.ToString().Length;
            SelectNextControl(this.tbxAge, true, true, true, true);
        }

        private void dgvData_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow curRow = this.dgvData.Rows[e.RowIndex];
            DataGridViewColumn curCol = this.dgvData.Columns[e.ColumnIndex];
            if (curCol.Name.Equals("TimeInterval"))
            {
                e.Value = e.Value + _langs["分钟"];
            }
        }

        private void dgvData_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (!ContainsFocus) { return; }

            DataGridViewColumn col = dgvData.Columns[e.ColumnIndex];
            if (col.Name == "StartTime" || col.Name == "EndTime")
            {
                Rectangle rect = dgvData.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                dtpStartEnd.Size = new Size(rect.Width, rect.Height);
                dtpStartEnd.Location = new Point(rect.X, rect.Y);
                dtpStartEnd.Visible = true;

                if (dgvData.CurrentCell.Value != null && dgvData.CurrentCell.Value != DBNull.Value)
                {
                    DateTime dtCell, dtToday = DateTime.Today; ;
                    bool isDate = DateTime.TryParseExact(dgvData.CurrentCell.Value.ToString(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dtCell);
                    if (isDate) dtpStartEnd.Text = dgvData.CurrentCell.Value.ToString();
                }
            }
            else if (col.Name == "TimeInterval")
            {
                Rectangle rect = dgvData.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                numUpDown.Size = new Size(rect.Width, rect.Height);
                numUpDown.Location = new Point(rect.X, rect.Y);
                numUpDown.Visible = true;

                decimal? cellValue = PubFuns.ConvertToDecimal(dgvData.CurrentCell.Value);
                if (cellValue != null) numUpDown.Value = cellValue.Value;
            }
        }

        private void dgvData_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (ContainsFocus)
            {
                dtpStartEnd.Visible = false;
                numUpDown.Visible = false;
            }
        }

        private void dgvData_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (dtpStartEnd.Visible)
                {
                    dgvData.CurrentCell.Value = dtpStartEnd.Text.ToString();
                    dtpStartEnd.Visible = false;
                }
                else if (numUpDown.Visible)
                {
                    dgvData.CurrentCell.Value = numUpDown.Value.ToString();
                    numUpDown.Visible = false;
                }
            }
        }

        private void dgvData_Scroll(object sender, ScrollEventArgs e)
        {
            dtpStartEnd.Visible = false;
            numUpDown.Visible = false;
        }
        #endregion


        private void init()
        {
            initUI();
            initGrid();

            //已上传的病例不允许再修改检查类别
            if (_originalID != null && _originalPatient == null)
            {
                _originalPatient = _MyEntities.ExamCases.First(o => o.id == _originalID.Value);
                if (_originalPatient.status >= 2)
                {
                    this.cbxCheckItem.Enabled = false;
                }
            }

            this.dtpStartEnd.ValueChanged += (o, e) =>
            {
                if (dtpStartEnd.ContainsFocus) dgvData.CurrentCell.Value = dtpStartEnd.Text;
            };
            this.numUpDown.ValueChanged += (o, e) =>
            {
                if (numUpDown.ContainsFocus) dgvData.CurrentCell.Value = numUpDown.Value.ToString();
            };
            this.numUpDown.Validating += (o, e) =>
            {
                int timeGap = (int)numUpDown.Value;
                while (timeGap % 5 != 0) { timeGap++; }
                numUpDown.Value = (timeGap > 0 ? timeGap : 5);
                dgvData.CurrentCell.Value = numUpDown.Value.ToString();
            };
        }

        private void initUI()
        {
            dtpStartEnd.Format = DateTimePickerFormat.Custom;
            dtpStartEnd.CustomFormat = "HH:mm";
            dtpStartEnd.ShowUpDown = true;
            dtpStartEnd.Visible = false;

            numUpDown.Maximum = 120;
            numUpDown.Increment = 5;
            numUpDown.Value = 10;
            numUpDown.TextAlign = HorizontalAlignment.Center;
            numUpDown.Visible = false;

            this.dtpBirthday.MaxDate = DateTime.Today;
            this.dtpBirthday.Checked = false;
            this.dtpBirthday.CustomFormat = " ";
            _lastValidBirthDate = this.dtpBirthday.Value;

            this.dtpApplyTime.CustomFormat = " ";

            CommonFuns.InitCommboBoxItems(this.cbxIdType, SysConstants.PatientSource);
            this.cbxIdType.SelectedIndex = -1;
            this.cbxIdType.SelectedIndexChanged += (o, arg) => { this.cbxIdType.Invalidate(); };

            //CommonFuns.InitCommboBoxItems(this.cbxCheckItem, SysConstants.CheckItems, true, -1);
            this.cbxCheckItem.SelectedIndexChanged += cbxCheckItem_SelectedIndexChanged;
            initForCheckItem(false);

            CommonFuns.InitCommboBoxItems(this.cbxSex, SysConstants.Genders, true, "");
            CommonFuns.InitCommboBoxItems(this.cbxAgeUnit, SysConstants.AgeUnits);
            this.cbxAgeUnit.SelectedIndexChanged += cbxAgeUnit_SelectedIndexChanged;
            cbxAgeUnit_SelectedIndexChanged(null, null);

            //CommonFuns.InitCommboBoxItems(this.cbxApplyDept, new PersistDictionary<string>(), true, "");
            CommonFuns.InitCommboBoxItems(this.cbxCheckDept, new PersistDictionary<string>(), true, "");

            CommonFuns.InitCommboBoxItems(this.cbxHasPacemaker, new Dictionary<int, string>() {
                { 0, CommonValues.Langs["无"]},
                { 1, CommonValues.Langs["有"]}
            }, false);

            CommonFuns.InitCommboBoxItems(this.cbxLeadOffAlarm, new PersistDictionary<string>() {
                { "Off", CommonValues.Langs["OFF"]},
                { "On", CommonValues.Langs["ON"]}
            }, false);

            foreach (Control ctl in tlpControl.Controls)
            {
                if (ctl is RadioButton rd)
                {
                    rd.CheckedChanged += (o, arg) =>
                    {
                        if (rd.Checked)
                            rd.ForeColor = Color.FromArgb(22, 183, 119);
                        else
                            rd.ForeColor = Color.Black;
                    };
                }
            }
        }

        private void initForCheckItem(bool isFirst = true)
        {
            if (isFirst)
            {
                CommonFuns.InitCommboBoxItems(this.cbxCheckItem, SysConstants.CheckItems, true, -1);
            }

            this.cbxCheckItem.SelectedValue = -1;
            if (CommonValues.SystemType == "ecg")
            {
                this.cbxCheckItem.Enabled = false;
                this.cbxCheckItem.SelectedValue = 1;
                this.tabNavigation.ApplyHeaderlessStyle = true;
            }
            else if (CommonValues.SystemType == "bp")
            {
                this.cbxCheckItem.Enabled = false;
                this.cbxCheckItem.SelectedValue = 2;
                this.tabNavigation.ApplyStepStyle = true;
            }
            else if (CommonValues.SystemType == "ecg-bp")
            {
                if (_deviceType == 1 || _deviceType == 2)
                    this.cbxCheckItem.SelectedValue = 1;
                else if (_deviceType == 3)
                    this.cbxCheckItem.SelectedValue = 2;
                else
                    this.cbxCheckItem.SelectedValue = 3;

                if (_checkItem == null || _checkItem == 1)
                    this.tabNavigation.ApplyHeaderlessStyle = true;
                else
                    this.tabNavigation.ApplyStepStyle = true;
            }
        }

        private void initGrid()
        {
            this.dgvData.Controls.Add(dtpStartEnd);
            this.dgvData.Controls.Add(numUpDown);

            DataGridViewTextBoxColumn col1 = new DataGridViewTextBoxColumn() { Name = "TimeType", HeaderText = "时间类型", DataPropertyName = "TimeType" };
            col1.SortMode = DataGridViewColumnSortMode.NotSortable;
            col1.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            col1.HeaderCell.Style.Padding = new Padding(10, 0, 0, 0);
            col1.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            this.dgvData.Columns.Add(col1);

            DataGridViewTextBoxColumn col2 = new DataGridViewTextBoxColumn() { Name = "StartTime", HeaderText = "开始", DataPropertyName = "StartTime" };
            col2.SortMode = DataGridViewColumnSortMode.NotSortable;
            col2.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dgvData.Columns.Add(col2);

            DataGridViewTextBoxColumn col3 = new DataGridViewTextBoxColumn() { Name = "EndTime", HeaderText = "结束", DataPropertyName = "EndTime" };
            col3.SortMode = DataGridViewColumnSortMode.NotSortable;
            col3.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dgvData.Columns.Add(col3);

            DataGridViewTextBoxColumn col4 = new DataGridViewTextBoxColumn() { Name = "TimeInterval", HeaderText = "时间间隔", DataPropertyName = "TimeInterval" };
            col4.SortMode = DataGridViewColumnSortMode.NotSortable;
            col4.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dgvData.Columns.Add(col4);

            DataGridViewTextBoxColumn col5 = new DataGridViewTextBoxColumn() { Name = "TimeSpanIndex", DataPropertyName = "TimeSpanIndex", Visible = false };
            col5.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            this.dgvData.Columns.Add(col5);

        }

        private void loadData()
        {
            if (_originalPatient != null)
            {
                List<TextBoxEx> textInputs = CommonFuns.RetriveSubControls<TextBoxEx>(this.tlpPatient);
                Type dataType = _originalPatient.GetType();
                foreach (PropertyInfo prop in dataType.GetProperties())
                {
                    foreach (TextBoxEx tbx in textInputs)
                    {
                        string tbxName = tbx.Name.Substring(3).ToLower();
                        if (tbxName == prop.Name.ToLower())
                        {
                            tbx.Text = prop.GetValue(_originalPatient)?.ToString();
                            break;
                        }
                    }
                }

                this.tbxRemark.Text = _originalPatient.remark;
                this.tbxMedication.Text = _originalPatient.medication;
                this.tbxClinicalDiagnosis.Text = _originalPatient.clinicalDiagnosis;

                if (_originalPatient.idType != null)
                    this.cbxIdType.SelectedValue = _originalPatient.idType.Value;

                if (CommonValues.SystemType == "ecg")
                    this.cbxCheckItem.SelectedValue = 1;
                else if (CommonValues.SystemType == "bp")
                    this.cbxCheckItem.SelectedValue = 2;
                else
                    this.cbxCheckItem.SelectedValue = _originalPatient.checkItem;

                this.cbxSex.SelectedValue = _originalPatient.sex;
                this.cbxAgeUnit.SelectedValue = string.IsNullOrEmpty(_originalPatient.ageUnit) ? "Y" : _originalPatient.ageUnit;
                //this.cbxApplyDept.SelectedValue = _originalPatient.applyDept ?? "";
                this.tbxApplyDept.Text = _originalPatient.applyDept;
                this.cbxCheckDept.SelectedValue = _originalPatient.checkDept ?? "";
                this.cbxHasPacemaker.SelectedValue = _originalPatient.hasPacemaker;

                string dateFormat = SysConstants.DateFormats[CommonValues.SystemSettings.baseSets.dateFormat];
                if (!string.IsNullOrEmpty(_originalPatient.birthday))
                {
                    this.dtpBirthday.CustomFormat = dateFormat;
                    this.dtpBirthday.Value = DateTime.ParseExact(_originalPatient.birthday, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);
                }

                if (_originalPatient.applyTime.HasValue)
                {
                    this.dtpApplyTime.CustomFormat = dateFormat + " HH:mm:ss";
                    this.dtpApplyTime.Value = _originalPatient.applyTime.Value;
                }
            }

            loadBpSettings();
        }

        private void loadBpSettings()
        {
            BpSettingScheme bpSettings = getBpSettingScheme();
            this.txtMaxSystolic.Text = bpSettings.systolicMax.ToString();
            this.txtMinSystolic.Text = bpSettings.systolicMin.ToString();
            this.txtMaxDiastolic.Text = bpSettings.diastolicMax.ToString();
            this.txtMinDiastolic.Text = bpSettings.diastolicMin.ToString();

            this.rbDayEnable.Checked = bpSettings.dayAlarm == 1;
            this.rbNightEnable.Checked = bpSettings.nightAlarm == 1;
            this.rbUnit1.Checked = bpSettings.pressureUnit == 0;
            this.chkSyncTime.Checked = bpSettings.syncTime == 1;

            DataTable dtTimeSpan = new DataTable();
            dtTimeSpan.Columns.AddRange(new DataColumn[] {
                new DataColumn("TimeSpanIndex"),
                new DataColumn("TimeType", typeof(string)),
                new DataColumn("StartTime", typeof(string)),
                new DataColumn("EndTime", typeof(string)),
                new DataColumn("TimeInterval", typeof(int)),
            });
            DataRow dr0 = dtTimeSpan.NewRow();
            dr0["TimeSpanIndex"] = 0;
            dr0["TimeType"] = _langs["白天"];
            dr0["StartTime"] = bpSettings.start1;
            dr0["EndTime"] = bpSettings.end1;
            dr0["TimeInterval"] = bpSettings.interval1;
            dtTimeSpan.Rows.Add(dr0);

            DataRow dr1 = dtTimeSpan.NewRow();
            dr1["TimeSpanIndex"] = 1;
            dr1["TimeType"] = _langs["夜间"];
            dr1["StartTime"] = bpSettings.start2;
            dr1["EndTime"] = bpSettings.end2;
            dr1["TimeInterval"] = bpSettings.interval2;
            dtTimeSpan.Rows.Add(dr1);

            DataRow dr2 = dtTimeSpan.NewRow();
            dr2["TimeSpanIndex"] = 2;
            dr2["TimeType"] = _langs["特殊一"];
            dr2["StartTime"] = bpSettings.start3;
            dr2["EndTime"] = bpSettings.end3;
            dr2["TimeInterval"] = bpSettings.interval3;
            dtTimeSpan.Rows.Add(dr2);

            DataRow dr3 = dtTimeSpan.NewRow();
            dr3["TimeSpanIndex"] = 3;
            dr3["TimeType"] = _langs["特殊二"];
            dr3["StartTime"] = bpSettings.start4;
            dr3["EndTime"] = bpSettings.end4;
            dr3["TimeInterval"] = bpSettings.interval4;
            dtTimeSpan.Rows.Add(dr3);

            DataRow dr4 = dtTimeSpan.NewRow();
            dr4["TimeSpanIndex"] = 4;
            dr4["TimeType"] = _langs["特殊三"];
            dr4["StartTime"] = bpSettings.start5;
            dr4["EndTime"] = bpSettings.end5;
            dr4["TimeInterval"] = bpSettings.interval5;
            dtTimeSpan.Rows.Add(dr4);

            this.dgvData.DataSource = dtTimeSpan;
            this.dgvData.CurrentCell = null;
        }

        //根据生日获取年龄信息
        private void checkBirthDate()
        {
            if (this.dtpBirthday.Checked)
            {
                this.tbxAge.Enabled = this.cbxAgeUnit.Enabled = false;

                DateTime dtpValue = this.dtpBirthday.Value;
                if (!string.IsNullOrWhiteSpace(this.dtpBirthday.CustomFormat))
                {
                    if (dtpValue != _lastValidBirthDate)
                    {
                        Console.WriteLine($"old birthday:{_lastValidBirthDate}, new birthday:{dtpValue}");
                        (int age, string ageUnit) = CommonFuns.ResolveAgeFromBirthDay(dtpValue);
                        this.tbxAge.Text = age.ToString();
                        this.cbxAgeUnit.SelectedValue = ageUnit;
                    }
                }
                _lastValidBirthDate = this.dtpBirthday.Value;
            }
            else
            {
                this.tbxAge.Enabled = true;
                this.cbxAgeUnit.Enabled = true;
            }
        }

        private examCase getPatientInfo(examCase source)
        {
            examCase patient = null;
            if (source != null)
                patient = source;
            else
                patient = new examCase();

            List<TextBoxEx> textInputs = CommonFuns.RetriveSubControls<TextBoxEx>(this.tlpPatient);
            Type dataType = patient.GetType();
            foreach (PropertyInfo prop in dataType.GetProperties())
            {
                foreach (TextBoxEx tbx in textInputs)
                {
                    string tbxName = tbx.Name.Substring(3).ToLower();
                    if (tbxName == prop.Name.ToLower())
                    {
                        PubFuns.SetPropertyValue(patient, prop, tbx.Text.Trim());
                        break;
                    }
                }
            }

            patient.remark = this.tbxRemark.Text;
            patient.medication = this.tbxMedication.Text;
            patient.clinicalDiagnosis = this.tbxClinicalDiagnosis.Text;
            patient.idType = (this.cbxIdType.SelectedIndex == -1 ? null : (int?)Convert.ToInt32(this.cbxIdType.SelectedValue));

            int checkItem = Convert.ToInt32(this.cbxCheckItem.SelectedValue);
            patient.checkItem = checkItem;
            patient.sex = this.cbxSex.SelectedValue.ToString();
            patient.ageUnit = this.cbxAgeUnit.SelectedValue.ToString();
            //patient.applyDept = this.cbxApplyDept.SelectedValue?.ToString();
            patient.applyDept = this.tbxApplyDept.Text.Trim();
            patient.checkDept = this.cbxCheckDept.SelectedValue?.ToString();
            patient.hasPacemaker = Convert.ToInt32(this.cbxHasPacemaker.SelectedValue ?? 0);

            if (patient.age.HasValue) patient.ageDays = CommonFuns.GetAgeDays(patient.age.Value, patient.ageUnit);

            if (this.dtpBirthday.Checked && this.dtpBirthday.CustomFormat != "")
                patient.birthday = this.dtpBirthday.Value.ToString(SysConstants.DATE_FORMAT);
            else
                patient.birthday = string.Empty;

            if (this.dtpApplyTime.Checked && this.dtpApplyTime.CustomFormat != " ")
                patient.applyTime = this.dtpApplyTime.Value;
            else
                patient.applyTime = null;

            DateTime nowTime = DateTime.Now;
            patient.updateTime = nowTime;
            patient.updateBy = user.Loginer.userName;
            patient.leadAlarm = this.cbxLeadOffAlarm.SelectedValue?.ToString();

            if (patient.createTime == null) patient.createTime = nowTime;

            return patient;
        }

        private bool savePatientInfo(examCase patient, int? deviceType = null)
        {
            Stopwatch sw = Stopwatch.StartNew();

            bool result = AppCode.SaveCaseInfo(_registerType, patient, _isNewCase, patient.updateTime.Value, deviceType);

            string opTitle = (_registerType == 0 ? "患者信息登记" : "读卡") + (result ? "成功" : "失败");
            OperateLog.logParam(opTitle, patient, sw.ElapsedMilliseconds, user.Loginer.userName);

            return result;
        }

        private bool verifyPatientInfo()
        {
            string caseNo = this.tbxCaseNo.Text.Trim();
            if (string.IsNullOrEmpty(caseNo))
            {
                ScreenCenterTip.Show("请输入病历号");
                this.tbxCaseNo.Focus();
                return false;
            }
            string patientName = this.tbxName.Text.Trim();
            if (string.IsNullOrEmpty(patientName))
            {
                ScreenCenterTip.Show("请输入患者姓名");
                this.tbxName.Focus();
                return false;
            }
            int checkItem = Convert.ToInt32(this.cbxCheckItem.SelectedValue);
            if (checkItem == -1)
            {
                ScreenCenterTip.Show("请选择检查项目");
                this.cbxCheckItem.Focus();
                return false;
            }

            if (!PubFuns.IsValidIdCard(this.tbxIdcard.Text))
            {
                ScreenCenterTip.Show("身份证格式错误");
                this.tbxIdcard.Focus();
                return false;
            }
            if (!PubFuns.IsValidPhone(this.tbxPhone.Text))
            {
                ScreenCenterTip.Show("手机号格式不正确，请重输入");
                this.tbxPhone.Focus();
                return false;
            }
            return true;
        }

        private bool verifyBpParameters()
        {
            DataTable dt = dgvData.DataSource as DataTable;
            string start1 = dt.Rows[0]["StartTime"].ToString();
            string end1 = dt.Rows[0]["EndTime"].ToString();
            string start2 = dt.Rows[1]["StartTime"].ToString();
            string end2 = dt.Rows[1]["EndTime"].ToString();

            if (!(end1 == start2 && start1 == end2))
            {
                ScreenCenterTip.Show("白天和夜间时间分配不能存在交叉或间隔");
                return false;
            }

            string start3 = dt.Rows[2]["StartTime"].ToString(), end3 = dt.Rows[2]["EndTime"].ToString();
            string start4 = dt.Rows[3]["StartTime"].ToString(), end4 = dt.Rows[3]["EndTime"].ToString();
            string start5 = dt.Rows[4]["StartTime"].ToString(), end5 = dt.Rows[4]["EndTime"].ToString();
            if (checkTimeTntersection(start3, end3, start4, end4) ||
                checkTimeTntersection(start4, end4, start5, end5) ||
                checkTimeTntersection(start3, end3, start5, end5))
            {
                ScreenCenterTip.Show("特殊时间段不能存在重叠，请检查");
                return false;
            }

            string psUnit = this.rbUnit1.Checked ? "mmHg" : "kPa";
            (int systolicMax, int systolicMin, int diastolicMax, int diastolicMin) = getThresholds(psUnit);
            if (systolicMin > systolicMax || diastolicMin > diastolicMax)
            {
                ScreenCenterTip.Show("最小阈值不能超过最大阈值");
                return false;
            }

            return true;
        }

        #region 血压戴机参数设置
        private BpSettingScheme getBpSettingScheme()
        {
            BpSettingScheme userSettings = null;

            string settingFilePath = PubFuns.GetAbsolutePath("config\\bpConfig.json");
            if (File.Exists(settingFilePath))
            {
                string content = File.ReadAllText(settingFilePath);
                JObject jo = JObject.Parse(content);

                if (_deviceType == 3)
                    userSettings = jo["ZQ8337"].ToObject<BpSettingScheme>();
                else if (_deviceType == 4)
                    userSettings = jo["ZQ8377"].ToObject<BpSettingScheme>();
            }

            if (userSettings == null)
            {
                userSettings = new BpSettingScheme();
            }

            return userSettings;
        }

        private BpSettingScheme saveBpSettingScheme(int deviceType)
        {
            BpSettingScheme settings = new BpSettingScheme()
            {
                dayAlarm = this.rbDayEnable.Checked ? 1 : 0,
                nightAlarm = this.rbNightEnable.Checked ? 1 : 0,
                pressureUnit = this.rbUnit1.Checked ? 0 : 1,
                syncTime = this.chkSyncTime.Checked ? 1 : 0
            };

            //bool useMmgh = deviceType == 2;
            bool useMmgh = false;
            (int sysMax, int sysMin, int diaMax, int diaMin) = getThresholds(settings.pressureUnit == 0 ? "mmHg" : "kPa", useMmgh);
            settings.systolicMax = sysMax;
            settings.systolicMin = sysMin;
            settings.diastolicMax = diaMax;
            settings.diastolicMin = diaMin;

            DataTable dt = dgvData.DataSource as DataTable;
            settings.start1 = dt.Rows[0]["StartTime"].ToString();
            settings.end1 = dt.Rows[0]["EndTime"].ToString();
            int.TryParse(dt.Rows[0]["TimeInterval"].ToString(), out int interval1);
            settings.interval1 = interval1 < 5 ? 5 : interval1;

            settings.start2 = dt.Rows[1]["StartTime"].ToString();
            settings.end2 = dt.Rows[1]["EndTime"].ToString();
            int.TryParse(dt.Rows[1]["TimeInterval"].ToString(), out int interval2);
            settings.interval2 = interval2 < 5 ? 5 : interval2;

            settings.start3 = dt.Rows[2]["StartTime"].ToString();
            settings.end3 = dt.Rows[2]["EndTime"].ToString();
            int.TryParse(dt.Rows[2]["TimeInterval"].ToString(), out int interval3);
            settings.interval3 = interval3 < 5 ? 5 : interval3;

            settings.start4 = dt.Rows[3]["StartTime"].ToString();
            settings.end4 = dt.Rows[3]["EndTime"].ToString();
            int.TryParse(dt.Rows[3]["TimeInterval"].ToString(), out int interval4);
            settings.interval4 = interval4 < 5 ? 5 : interval4;

            settings.start5 = dt.Rows[4]["StartTime"].ToString();
            settings.end5 = dt.Rows[4]["EndTime"].ToString();
            int.TryParse(dt.Rows[4]["TimeInterval"].ToString(), out int interval5);
            settings.interval5 = interval5 < 5 ? 5 : interval5;

            string strBPSet = JsonConvert.SerializeObject(settings, SysConstants.JsonSettings);
            string fileContent = "";
            string settingFilePath = PubFuns.GetAbsolutePath("config\\bpConfig.json");
            if (File.Exists(settingFilePath))
            {
                string content = File.ReadAllText(settingFilePath);
                JObject jo = JObject.Parse(content);
                if (_deviceType == 3)
                    jo["ZQ8337"] = JToken.FromObject(settings);
                else if (_deviceType == 4)
                    jo["ZQ8377"] = JToken.FromObject(settings);
                fileContent = jo.ToString(Formatting.Indented);
            }
            else
            {
                dynamic obj = new ExpandoObject();
                if (_deviceType == 3)
                {
                    obj.ZQ8337 = settings;
                    obj.ZQ8377 = "";
                }
                else if (_deviceType == 4)
                {
                    obj.ZQ8337 = "";
                    obj.ZQ8377 = settings;
                }
                fileContent = JsonConvert.SerializeObject(obj, SysConstants.JsonSettings);
            }
            File.WriteAllText(settingFilePath, fileContent);

            return settings;
        }

        //检查两段时间是否有交集
        private bool checkTimeTntersection(string start1, string end1, string start2, string end2)
        {
            int TimeToMinutes(string time)
            {
                var parts = time.Split(':');
                return int.Parse(parts[0]) * 60 + int.Parse(parts[1]);
            }

            // 将时间转换为分钟，并处理跨天情况
            int startTime1 = TimeToMinutes(start1);
            int endTime1 = TimeToMinutes(end1) + (string.Compare(end1, start1) < 0 ? 24 * 60 : 0);
            int startTime2 = TimeToMinutes(start2);
            int endTime2 = TimeToMinutes(end2) + (string.Compare(end2, start2) < 0 ? 24 * 60 : 0);

            if ((startTime2 >= endTime1 && endTime2 <= startTime1 + 24 * 60) ||
                (startTime1 >= endTime2 && endTime1 <= startTime2 + 24 * 60))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private (int sysMax, int sysMin, int diaMax, int diaMin) getThresholds(string psUnit, bool userMmgh = false)
        {
            int systolicMax = 140;
            if (txtMaxSystolic.Text.Length > 0)
            {
                systolicMax = Convert.ToInt32(txtMaxSystolic.Text);
                if (psUnit == "kPa" && userMmgh)
                {
                    systolicMax = CommonFuns.kPa2mmHg(Convert.ToDouble(txtMaxSystolic.Text));
                }
            }

            int systolicMin = 90;
            if (txtMinSystolic.Text.Length > 0)
            {
                systolicMin = Convert.ToInt32(txtMinSystolic.Text);
                if (psUnit == "kPa" && userMmgh)
                {
                    systolicMin = CommonFuns.kPa2mmHg(Convert.ToDouble(txtMinSystolic.Text));
                }
            }

            int diastolicMax = 90;
            if (txtMaxDiastolic.Text.Length > 0)
            {
                diastolicMax = Convert.ToInt32(txtMaxDiastolic.Text);
                if (psUnit == "kPa" && userMmgh)
                {
                    diastolicMax = CommonFuns.kPa2mmHg(Convert.ToDouble(txtMaxDiastolic.Text));
                }
            }

            int diastolicMin = 60;
            if (txtMinDiastolic.Text.Length > 0)
            {
                diastolicMin = Convert.ToInt32(txtMinDiastolic.Text);
                if (psUnit == "kPa" && userMmgh)
                {
                    diastolicMin = CommonFuns.kPa2mmHg(Convert.ToDouble(txtMinDiastolic.Text));
                }
            }

            return (systolicMax, systolicMin, diastolicMax, diastolicMin);
        }
        #endregion

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; //双缓存
                return cp;
            }
        }

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        var cp = base.CreateParams;
        //        // 移除 WS_CLIPCHILDREN 样式
        //        // 这会允许父控件在子控件的区域上进行绘制，从而避免出现“空洞”
        //        cp.Style &= ~0x02000000;
        //        return cp;
        //    }
        //}
    }
}
