using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace ABPMRepository
{
    partial class DeviceSettings : Form
    {
        private DateTimePicker dtp = new DateTimePicker();
        private NumericUpDown updown = new NumericUpDown();
        private ABPMOperator op_ABPM;
        private DataTable originalData;
        private bool scaningBluetooth = false;
        private System.Timers.Timer conn_Timer = new System.Timers.Timer(500);
        private int m_angle = 0; //从 x 轴到弧线的起始点沿顺时针方向度量的角（以度为单位）
        private readonly Regex m_regOK = new Regex(@"^@OK((\r\n)|$)", RegexOptions.Multiline);
        public bool OperationSucceeded = false;
        public string SelectedBluetooth { get; private set; } //远端MAC地址或 "0"(本地直连)

        public DeviceSettings()
        {
            InitializeComponent();
        }
        public DeviceSettings(ABPMOperator op)
        {
            InitializeComponent();
            op_ABPM = op;
        }

        private void DeviceSettings_Load(object sender, EventArgs e)
        {
            dtp.Format = DateTimePickerFormat.Custom;
            dtp.CustomFormat = "HH:mm";
            dtp.ShowUpDown = true;
            dtp.Visible = false;

            updown.Minimum = 0;
            updown.Maximum = 120;
            updown.Increment = 5;
            updown.Value = 10;
            updown.Visible = false;

            this.dtpSystemTime.Enabled = false;
            this.dtpSystemTime.Width += 1;
            this.conn_Timer.Elapsed += new ElapsedEventHandler(conn_Timer_Elapsed);

            initBasicData();
            refreshData();
        }

        #region grid events
        private void gridTimeSettings_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (!ContainsFocus) { return; }

            DataGridViewColumn col = gridTimeSettings.Columns[e.ColumnIndex];
            if (col is DataGridViewComboBoxColumn && col.Name == "TimeType")
            {
                //gridTimeSettings.BeginEdit(false);
                SendKeys.Send("{F4}");

                DataGridViewComboBoxEditingControl comboBoxEditingControl = gridTimeSettings.EditingControl as DataGridViewComboBoxEditingControl;
                if (comboBoxEditingControl != null)
                {
                    //comboBoxEditingControl.DroppedDown = true;
                    if (gridTimeSettings.CurrentCell.Value == null || gridTimeSettings.CurrentCell.Value == DBNull.Value)
                    {
                        comboBoxEditingControl.SelectedIndex = -1;
                    }
                }
            }
            else if (col.Name == "StartTime" || col.Name == "EndTime")
            {
                Rectangle rect = gridTimeSettings.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                dtp.Size = new Size(rect.Width, rect.Height);
                dtp.Location = new Point(rect.X, rect.Y);
                dtp.Visible = true;

                if (gridTimeSettings.CurrentCell.Value != null && gridTimeSettings.CurrentCell.Value != DBNull.Value)
                {
                    DateTime dtCell, dtToday = DateTime.Today; ;
                    bool blnIsDate = DateTime.TryParseExact(gridTimeSettings.CurrentCell.Value.ToString(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dtCell);
                    if (true == blnIsDate) dtp.Text = gridTimeSettings.CurrentCell.Value.ToString(); //dtp.Value = new DateTime(dtToday.Year, dtToday.Month, dtToday.Day, dtCell.Hour, dtCell.Minute, 0);
                }
            }
            else if (col.Name == "TimeInterval")
            {
                Rectangle rect = gridTimeSettings.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                updown.Size = new Size(rect.Width, rect.Height);
                updown.Location = new Point(rect.X, rect.Y);
                updown.Visible = true;

                decimal? cellValue = ZqBPFuns.ConvertToDecimal(gridTimeSettings.CurrentCell.Value);
                if (cellValue != null) updown.Value = cellValue.Value;
            }
            else
            {
                //dtp.Visible = false;
                //updown.Visible = false;
            }
        }

        private void gridTimeSettings_CellLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (!ContainsFocus) { return; }

            dtp.Visible = false;
            updown.Visible = false;

            //object originalValue = originalData.Rows[e.RowIndex][e.ColumnIndex];
            //DataGridViewComboBoxEditingControl comboBoxEditingControl = gridTimeSettings.EditingControl as DataGridViewComboBoxEditingControl;
            //if (comboBoxEditingControl == null)
            //{
            //    if (gridTimeSettings.CurrentCell.Value.ToString() == originalValue.ToString())
            //    {
            //        if (!commitEdits)
            //            gridTimeSettings.CancelEdit();
            //    }
            //    else
            //    {
            //        gridTimeSettings.CommitEdit(DataGridViewDataErrorContexts.Commit);
            //        commitEdits = true;
            //    }
            //}
            //else
            //{
            //    if (comboBoxEditingControl.SelectedValue.ToString() == originalValue.ToString())
            //    {
            //        if (!commitEdits)
            //            gridTimeSettings.CancelEdit();
            //    }
            //    else
            //    {
            //        gridTimeSettings.CommitEdit(DataGridViewDataErrorContexts.Commit);
            //        commitEdits = true;
            //    }
            //}
        }

        private void gridTimeSettings_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (dtp.Visible)
                {
                    gridTimeSettings.CurrentCell.Value = dtp.Text.ToString();
                    dtp.Visible = false;
                }
                else if (updown.Visible)
                {
                    gridTimeSettings.CurrentCell.Value = updown.Value.ToString();
                    updown.Visible = false;
                }
            }
        }

        private void gridTimeSettings_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            dtp.Visible = false;
            updown.Visible = false;
        }

        private void gridTimeSettings_Scroll(object sender, ScrollEventArgs e)
        {
            dtp.Visible = false;
            updown.Visible = false;
        }
        #endregion

        #region button events
        private void btnProgramming_Click(object sender, EventArgs e)
        {
            foreach (Control ctl in this.gpThreshold.Controls)
            {
                if (ctl is TextBox)
                {
                    string errText = this.errorProvider1.GetError(ctl);
                    if (!string.IsNullOrEmpty(errText))
                    {
                        TextBox inputCtl = ctl as TextBox;
                        inputCtl.SelectionStart = 0;
                        inputCtl.SelectionLength = inputCtl.Text.Length;
                        inputCtl.Focus();
                        MessageBox.Show("请先确认输入的参数信息无误.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }

            OperationSucceeded = false;
            this.btnProgramming.Enabled = false;
            try
            {
                while (scaningBluetooth) //等蓝牙扫描过程结束再进行后续过程, 避免串口占用
                {
                    Application.DoEvents();
                }

                string errCode = string.Empty;
                string remoteMac = (cbxBluetooth.SelectedValue == null ? "" : cbxBluetooth.SelectedValue.ToString());
                bool isReady = op_ABPM.TryConnectDevice(out errCode, !string.IsNullOrEmpty(remoteMac), remoteMac);
                if (!isReady)
                {
                    if (this.SelectedBluetooth != null)
                    {
                        this.SelectedBluetooth = "";
                        this.picConn.Invalidate();
                    }
                    throw new Exception("设备通信连接失败!");
                }

                op_ABPM.OpenPort();
                DataTable dt = gridTimeSettings.DataSource as DataTable;
                DataTable dtChanged = dt.GetChanges();
                SettingScheme settingInfo = saveSettingScheme(null);
                //if (dtChanged != null)
                //{
                foreach (TimePeriod timePeriod in settingInfo.TimePeriods)
                {
                    string rowData = string.Join(",", timePeriod.TimeType.ToString(), timePeriod.StartTime, timePeriod.EndTime, timePeriod.TimeInterval);

                    //if (rowData.Replace(",", "") != "")
                    //{
                    //    if (string.IsNullOrEmpty(timeType))
                    //    {
                    //        MessageBox.Show($"第{timeSpanIndex + 1}行未选择时间类型", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    //        return;
                    //    }
                    //    if (string.IsNullOrEmpty(startTime))
                    //    {
                    //        MessageBox.Show($"第{timeSpanIndex + 1}行未输入开始时间", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    //        return;
                    //    }
                    //    if (string.IsNullOrEmpty(endTime))
                    //    {
                    //        MessageBox.Show($"第{timeSpanIndex + 1}行未输入结束时间", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    //        return;
                    //    }
                    //    if (string.IsNullOrEmpty(timeInterval))
                    //    {
                    //        MessageBox.Show($"第{timeSpanIndex + 1}行未输入时间间隔", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    //        return;
                    //    }
                    //}

                    string result = op_ABPM.SetTimeSpan(string.Concat(timePeriod.TimeSpanIndex, ",", rowData), true);

                    //if (result.ToUpper() != "@OK" && !result.ToUpper().StartsWith("@OK\r\n"))
                    if (!m_regOK.IsMatch(result.ToUpper()))
                        throw new Exception("时间分配信息写入出错!");
                }

                dt.AcceptChanges();
                //}

                string frontPanelData = string.Concat(settingInfo.KeyboardEnabled, ",", settingInfo.DisplayEnabled);
                string alarmData = string.Concat(settingInfo.AlarmInDay, ",", settingInfo.AlarmInNight);
                string strPressureThreshold = string.Join(",", settingInfo.MaxPressure, settingInfo.MaxSystolic, settingInfo.MinSystolic, settingInfo.MaxDiastolic, settingInfo.MinDiastolic);
                string opResult = op_ABPM.SetParameters(string.Join(",", settingInfo.DeviceTime, frontPanelData, alarmData, settingInfo.BPunit.ToString(), strPressureThreshold), true);
                if (!m_regOK.IsMatch(opResult.ToUpper()))
                {
                    throw new Exception("参数信息写入出错!");
                }
                else
                {
                    //写入虚拟病人信息至血压计, 此过程将清除设备上的历史血压数据
                    string strPatient = string.Join(",", "100001", "F", "default", 0, "Y", 16, "001", 2);
                    string inputPatientResult = op_ABPM.InputPatient(strPatient, true);
                    if (!m_regOK.IsMatch(inputPatientResult.ToUpper()))
                    {
                        throw new Exception("参数信息写入出错!");
                    }
                }

                OperationSucceeded = true;
                saveSettingScheme(settingInfo);
                MessageBox.Show("参数设置成功.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ZqBPFuns.ShowErrors(ex);
            }
            finally
            {
                op_ABPM.ClosePort();
                this.btnProgramming.Enabled = true;
                //if (true == succeeded)
                //{
                //    this.DialogResult = DialogResult.OK;
                //}
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            OperationSucceeded = true;
            this.Close();
        }
        #endregion

        #region additional component events
        private void dtp_ValueChanged(object sender, EventArgs e)
        {
            if (dtp.ContainsFocus)
                gridTimeSettings.CurrentCell.Value = dtp.Text.ToString();
        }

        private void updown_ValueChanged(object sender, EventArgs e)
        {
            if (updown.ContainsFocus)
            {
                gridTimeSettings.CurrentCell.Value = updown.Value.ToString();
            }
        }

        private void updown_LostFocus(object sender, EventArgs e)
        {
            decimal timeGap = updown.Value;
            while (timeGap % 5 != 0)
            {
                timeGap++;
            }
            updown.Value = (timeGap > 0 ? timeGap : 5);

            gridTimeSettings.CurrentCell.Value = updown.Value.ToString();
        }

        private void txtNumeric_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b' && !Char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtNumeric_Validated(object sender, EventArgs e)
        {
            int iMin = 0, iMax = 0;
            TextBox inputCtl = sender as TextBox;
            switch (inputCtl.Name)
            {
                case "txtMaxPressure":
                    iMin = 120;
                    iMax = 280;
                    break;
                case "txtMaxSystolic":
                    iMin = 100;
                    iMax = 200;
                    break;
                case "txtMinSystolic":
                    iMin = 60;
                    iMax = 100;
                    break;
                case "txtMaxDiastolic":
                    iMin = 90;
                    iMax = 180;
                    break;
                case "txtMinDiastolic":
                    iMin = 30;
                    iMax = 70;
                    break;
                default:
                    return;
            }
            if (inputCtl.Text != null && inputCtl.Text != "") //判断TextBox的内容不为空，如果不判断会导致后面的非数字对比异常
            {
                if (int.Parse(inputCtl.Text) < iMin || int.Parse(inputCtl.Text) > iMax)
                {
                    this.errorProvider1.SetError(inputCtl, $"输入的值应在{iMin}至{iMax}的范围之间");
                }
                else
                {
                    this.errorProvider1.SetError(inputCtl, string.Empty);
                }
            }
        }

        private void chbSysTimeSync_CheckedChanged(object sender, EventArgs e)
        {
            if (chbSysTimeSync.Checked)
            {
                this.dtpSystemTime.Value = DateTime.Now;
                this.dtpSystemTime.Enabled = false;
            }
            else
            {
                this.dtpSystemTime.Enabled = true;
            }
        }

        private void cbxBluetooth_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!cbxBluetooth.ContainsFocus) return;

            //conn_Timer.Enabled = true;
            m_angle = 0;
            this.SelectedBluetooth = null;
            picConn.Refresh();

            picConn.Image = ZqBPFuns.ShowLoadingImage();
            string errCode = string.Empty;
            bool connectable = op_ABPM.TryConnectDevice(out errCode, true, cbxBluetooth.SelectedValue.ToString());
            //conn_Timer.Enabled = false;
            picConn.Image = null;

            if (connectable)
                this.SelectedBluetooth = cbxBluetooth.SelectedValue.ToString();
            else
                this.SelectedBluetooth = "";
            picConn.Invalidate();
        }

        private void picConn_Paint(object sender, PaintEventArgs e)
        {
            if (this.SelectedBluetooth == null) return;

            Graphics g = e.Graphics;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.FillEllipse(Brushes.White, 0, 0, picConn.Width, picConn.Height);
            if ("" != this.SelectedBluetooth)
            {
                //MessageBox.Show(errMsg, "连接结果", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                g.DrawString("✔", new Font("宋体", 11, FontStyle.Bold), Brushes.LimeGreen, 0, 0);
            }
            else
            {
                g.DrawString("✖", new Font("宋体", 11, FontStyle.Bold), Brushes.Red, 0, 0);
            }
        }

        private void conn_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (picConn.InvokeRequired)
            {
                Action refreshConnStatus = () =>
                {
                    if (conn_Timer.Enabled)
                    {
                        picConn.Refresh();
                    }
                };
                if (this.IsHandleCreated) picConn.Invoke(refreshConnStatus);
            }

            //画一个加载过程状态的动态图
            if (m_angle >= 360) m_angle = 0;
            int r = picConn.Width / 2; //半径
            float centerX = picConn.Width / 2, centerY = picConn.Height / 2; //圆心位置

            using (Graphics graphics = picConn.CreateGraphics())
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                ////描三个点(角度相隔15度)
                //for (int i = 0; i < 3; i++)
                //{
                //    double radian = (m_angle + 15 * i) * Math.PI / 180; //弧度
                //    int colorDepth = 30 * i;
                //    graphics.DrawString(
                //        ".",
                //        new Font("宋体", 12, FontStyle.Regular),
                //        new SolidBrush(Color.FromArgb(colorDepth, colorDepth, colorDepth)),
                //        centerX + (float)(r * Math.Cos(radian)),
                //        centerY + (float)(r * Math.Sin(radian)));
                //}
                using (Pen p = new Pen(Color.LimeGreen, 1))
                {
                    //设置起止点线帽
                    p.StartCap = LineCap.Round;
                    p.EndCap = LineCap.ArrowAnchor;
                    p.LineJoin = LineJoin.Round;
                    graphics.DrawArc(p, 0, 0, picConn.Width - 4, picConn.Height - 4, m_angle, m_angle + 45);
                }
            }
            m_angle += 45;
        }
        #endregion

        private void initBasicData()
        {
            List<dynamic> lstUnit = new List<dynamic>() {
                new { value = "0", text = "mmHg" },
                new { value = "1", text = "kPa" },
            };
            cbxUnit.ValueMember = "value";
            cbxUnit.DisplayMember = "text";
            cbxUnit.DataSource = lstUnit;

            cbxKeyboard.ValueMember = "value";
            cbxKeyboard.DisplayMember = "text";
            cbxKeyboard.DataSource = new List<dynamic>() {
                new { value = "1", text = "启用" },
                new { value = "0", text = "停用" }
            };

            cbxDisplay.ValueMember = "value";
            cbxDisplay.DisplayMember = "text";
            cbxDisplay.DataSource = new List<dynamic>() {
                new { value = "1", text = "启用" },
                new { value = "0", text = "停用" }
            };

            cbxDaytime.ValueMember = "value";
            cbxDaytime.DisplayMember = "text";
            cbxDaytime.DataSource = new List<dynamic>() {
                new { value = "1", text = "启用" },
                new { value = "0", text = "停用" }
            };
            cbxNight.ValueMember = "value";
            cbxNight.DisplayMember = "text";
            cbxNight.DataSource = new List<dynamic>() {
                new { value = "1", text = "启用" },
                new { value = "0", text = "停用" }
            };

            List<dynamic> lstDayOrNight = new List<dynamic>() {
                new { value = 0, text = "白天" },
                new { value = 1, text = "夜间" },
                new { value = 2, text = "特殊一" },
                new { value = 3, text = "特殊二" },
                new { value = 4, text = "特殊三" },
            };

            gridTimeSettings.Columns.Clear();
            DataGridViewColumn[] colums = new DataGridViewColumn[] {
                new DataGridViewComboBoxColumn { Name="TimeType", HeaderText="时间类型", DataPropertyName="TimeType", ValueMember="value", DisplayMember="text", DataSource=lstDayOrNight, ReadOnly=true },
                new DataGridViewTextBoxColumn { Name="StartTime", HeaderText="开始", DataPropertyName="StartTime", ReadOnly=true, SortMode=DataGridViewColumnSortMode.NotSortable },
                new DataGridViewTextBoxColumn { Name="EndTime", HeaderText="结束", DataPropertyName="EndTime", ReadOnly=true, SortMode=DataGridViewColumnSortMode.NotSortable },
                new DataGridViewTextBoxColumn { Name="TimeInterval", HeaderText="时间间隔", DataPropertyName="TimeInterval", ReadOnly=true, SortMode=DataGridViewColumnSortMode.NotSortable },
                new DataGridViewTextBoxColumn { Name="TimeSpanIndex", DataPropertyName="TimeSpanIndex", Visible=false },
            };
            gridTimeSettings.Columns.AddRange(colums);

            gridTimeSettings.Controls.Add(dtp);
            dtp.Visible = false;
            dtp.ValueChanged += new EventHandler(dtp_ValueChanged);

            gridTimeSettings.Controls.Add(updown);
            updown.Visible = false;
            //updown.ValueChanged += new EventHandler(updown_ValueChanged);
            updown.LostFocus += new EventHandler(updown_LostFocus);

            foreach (Control ctl in this.gpThreshold.Controls)
            {
                if (ctl is TextBox) //限制文本框输入只能为数字
                {
                    TextBox tbx = ctl as TextBox;
                    tbx.KeyPress += new KeyPressEventHandler(txtNumeric_KeyPress);
                    tbx.Validated += new EventHandler(txtNumeric_Validated);
                }
            }
        }

        private void refreshData()
        {
            try
            {
                //op_ABPM.OpenPort();
                SettingScheme userSettingInfo = readSettingScheme();
                TimePeriod[] periodInfo = userSettingInfo.TimePeriods;
                this.txtMaxPressure.Text = userSettingInfo.MaxPressure;
                this.txtMaxSystolic.Text = userSettingInfo.MaxSystolic;
                this.txtMinSystolic.Text = userSettingInfo.MinSystolic;
                this.txtMaxDiastolic.Text = userSettingInfo.MaxDiastolic;
                this.txtMinDiastolic.Text = userSettingInfo.MinDiastolic;
                this.cbxDaytime.SelectedValue = userSettingInfo.AlarmInDay.ToString();
                this.cbxNight.SelectedValue = userSettingInfo.AlarmInNight.ToString();
                this.cbxUnit.SelectedValue = userSettingInfo.BPunit.ToString();
                this.cbxKeyboard.SelectedValue = userSettingInfo.KeyboardEnabled.ToString();
                this.cbxDisplay.SelectedValue = userSettingInfo.DisplayEnabled.ToString();
                this.chbSysTimeSync.Checked = (userSettingInfo.SyncWithSysTime == 1);

                DataTable dtTimeSpan = new DataTable();
                dtTimeSpan.Columns.AddRange(new DataColumn[] {
                    new DataColumn("TimeSpanIndex"),
                    new DataColumn("TimeType", typeof(int)),
                    new DataColumn("StartTime", typeof(string)),
                    new DataColumn("EndTime", typeof(string)),
                    new DataColumn("TimeInterval", typeof(string)),
                });
                for (int i = 0; i < 5; i++) //rows
                {
                    DataRow dr = dtTimeSpan.NewRow();
                    dr["TimeSpanIndex"] = i;
                    dr["TimeType"] = periodInfo[i].TimeType;
                    dr["StartTime"] = periodInfo[i].StartTime;
                    dr["EndTime"] = periodInfo[i].EndTime;
                    dr["TimeInterval"] = periodInfo[i].TimeInterval;

                    //string spanData = op_ABPM.GetTimeSpan(i);
                    //if (!string.IsNullOrWhiteSpace(spanData))
                    //{
                    //    string[] arrTimeSpan = spanData.Split(',');
                    //    if (string.IsNullOrWhiteSpace(arrTimeSpan[1])) //TimeType colunm is null, means this time span is not set
                    //    {
                    //        dr[0] = i;
                    //    }
                    //    else
                    //    {
                    //        for (int j = 0; j < 5; j++) //colunms
                    //        {
                    //            if (j == 1) //时间类型
                    //                if (!(new string[] { "0", "1", "2" }).Contains(arrTimeSpan[j])) dr[j] = "0";
                    //            else
                    //                dr[j] = arrTimeSpan[j];
                    //        }
                    //    }
                    //}
                    dtTimeSpan.Rows.Add(dr);
                }

                gridTimeSettings.DataSource = dtTimeSpan;
                this.originalData = dtTimeSpan.Copy();
                dtTimeSpan.AcceptChanges();

                //string parameters = op_ABPM.GetParameters(true);
                //string[] arrPara = parameters.Split(',');
                //if (arrPara.Length == 6)
                //{
                //    this.dtpSystemTime.Value = Convert.ToDateTime(arrPara[0]);
                //    this.cbxKeyboard.SelectedValue = arrPara[1];
                //    this.cbxDisplay.SelectedValue = arrPara[2];
                //    this.cbxDaytime.SelectedValue = arrPara[3];
                //    this.cbxNight.SelectedValue = arrPara[4];
                //    this.cbxUnit.SelectedValue = arrPara[5];
                //}

                cbxBluetooth.ValueMember = "BluetoothMac";
                cbxBluetooth.DisplayMember = "BluetoothName";
                cbxBluetooth.Text = "正在搜索蓝牙设备...";
                Thread thdFillBle = new Thread(new ThreadStart(delegate { retrieveBluetooth(null); })); //创建新的线程
                thdFillBle.IsBackground = true;
                thdFillBle.Start();

                gridTimeSettings.CurrentCell = null;
                this.btnCancel.Focus();
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                //op_ABPM.ClosePort();
            }
        }

        private void retrieveBluetooth(List<BluetoothInformation> lstBluttooths)
        {
            if (cbxBluetooth.InvokeRequired)
            {
                scaningBluetooth = true;
                List<BluetoothInformation> lstBluetoothSource = op_ABPM.GetBluetooths();
                scaningBluetooth = false;

                var fillBleAction = new Action<List<BluetoothInformation>>(retrieveBluetooth);
                if (this.IsHandleCreated) cbxBluetooth.Invoke(fillBleAction, lstBluetoothSource);
            }
            else
            {
                if (lstBluttooths.Count <= 0)
                {
                    cbxBluetooth.Items.Add(new BluetoothInformation { BluetoothMAC = "0", BluetoothName = "未发现可用的蓝牙设备！" });
                    cbxBluetooth.SelectedIndex = 0;
                }
                else
                {
                    BluetoothInformation usedItem = lstBluttooths.Find(o => o.IsUsed == true);
                    lstBluttooths.Insert(0, new BluetoothInformation { BluetoothMAC = "0", BluetoothName = "---不使用蓝牙连接---" });
                    cbxBluetooth.DataSource = lstBluttooths;
                    //if (usedItem != null)
                    //    cbxBluetooth.SelectedItem = usedItem;
                    //else
                    //    cbxBluetooth.SelectedIndex = 0;

                    cbxBluetooth.Enabled = true;
                }
            }
        }

        private SettingScheme readSettingScheme()
        {
            SettingScheme userSettings = new SettingScheme();

            string settingFilePath = AppDomain.CurrentDomain.BaseDirectory + "ABPM_Setting.xml";
            if (File.Exists(settingFilePath))
            {
                XmlSerializer serializer = new XmlSerializer(userSettings.GetType());
                FileStream stream = new FileStream(settingFilePath, FileMode.Open);
                userSettings = (SettingScheme)serializer.Deserialize(stream);
                stream.Close();
            }
            else
            {
                userSettings = new SettingScheme()
                {
                    MaxPressure = "280",
                    MaxSystolic = "140",
                    MinSystolic = "90",
                    MaxDiastolic = "90",
                    MinDiastolic = "60",
                    AlarmInDay = 1,
                    AlarmInNight = 1,
                    SyncWithSysTime = 1,
                    BPunit = 0,
                    KeyboardEnabled = 1,
                    DisplayEnabled = 1
                };
                userSettings.TimePeriods = new TimePeriod[] {
                    new TimePeriod{ TimeSpanIndex=0, TimeType=0, StartTime = "07:30", EndTime ="22:30", TimeInterval="10" },
                    new TimePeriod{ TimeSpanIndex=1, TimeType=1, StartTime = "22:30", EndTime ="07:30", TimeInterval="15" },
                    new TimePeriod{ TimeSpanIndex=2, TimeType=2, StartTime = "00:00", EndTime ="00:00", TimeInterval="5" },
                    new TimePeriod{ TimeSpanIndex=3, TimeType=3, StartTime = "00:00", EndTime ="00:00", TimeInterval="5" },
                    new TimePeriod{ TimeSpanIndex=4, TimeType=4, StartTime = "00:00", EndTime ="00:00", TimeInterval="5" }
                };
            }

            return userSettings;
        }

        private SettingScheme saveSettingScheme(SettingScheme settings = null)
        {
            SettingScheme userSettings = null;
            if (settings != null)
            {
                userSettings = settings;
                //save to XML document
                XmlSerializer serializer = new XmlSerializer(userSettings.GetType());
                XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces(
                    new XmlQualifiedName[] {
                        new XmlQualifiedName(string.Empty, "aa")
                });

                string settingFilePath = AppDomain.CurrentDomain.BaseDirectory + "ABPM_Setting.xml";
                TextWriter writer = new StreamWriter(settingFilePath);
                serializer.Serialize(writer, userSettings, namespaces);
                writer.Close();
            }
            else
            {
                userSettings = new SettingScheme()
                {
                    KeyboardEnabled = Convert.ToInt32(cbxKeyboard.SelectedValue),
                    DisplayEnabled = Convert.ToInt32(cbxDisplay.SelectedValue),
                    AlarmInDay = Convert.ToInt32(cbxDaytime.SelectedValue),
                    AlarmInNight = Convert.ToInt32(cbxNight.SelectedValue),
                    SyncWithSysTime = (this.chbSysTimeSync.Checked ? 1 : 0),
                    BPunit = Convert.ToInt32(cbxUnit.SelectedValue)
                };
                DataTable dt = gridTimeSettings.DataSource as DataTable;
                userSettings.TimePeriods = new TimePeriod[dt.Rows.Count];
                int i = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    TimePeriod periodInfo = new TimePeriod()
                    {
                        TimeSpanIndex = Convert.ToInt32(dr["TimeSpanIndex"]),
                        StartTime = dr["StartTime"].ToString(),
                        EndTime = dr["EndTime"].ToString()
                    };
                    int.TryParse(dr["TimeInterval"].ToString(), out int nInterval);
                    if (nInterval < 5) nInterval = 5;
                    periodInfo.TimeInterval = nInterval.ToString();

                    periodInfo.TimeType = Convert.ToInt32(dr["TimeType"]);
                    //if (periodInfo.TimeType > 2) periodInfo.TimeType = 2;

                    userSettings.TimePeriods[i] = periodInfo;
                    i++;
                }

                userSettings.MaxPressure = txtMaxPressure.Text.Length == 0 ? "0" : txtMaxPressure.Text;
                userSettings.MaxSystolic = txtMaxSystolic.Text.Length == 0 ? "0" : txtMaxSystolic.Text;
                userSettings.MinSystolic = txtMinSystolic.Text.Length == 0 ? "0" : txtMinSystolic.Text;
                userSettings.MaxDiastolic = txtMaxDiastolic.Text.Length == 0 ? "0" : txtMaxDiastolic.Text;
                userSettings.MinDiastolic = txtMinDiastolic.Text.Length == 0 ? "0" : txtMinDiastolic.Text;

                userSettings.DeviceTime = this.dtpSystemTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                if (this.chbSysTimeSync.Checked)
                    userSettings.DeviceTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }

            return userSettings;
        }

        //处理本窗体内回车命令键事件
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                if (this.gridTimeSettings.IsCurrentCellInEditMode)
                {
                    //DataGridViewComboBoxEditingControl comboBoxEditingControl = gridTimeSettings.EditingControl as DataGridViewComboBoxEditingControl;
                    //if (comboBoxEditingControl != null)
                    //{
                    //    gridTimeSettings.CurrentCell.Value = comboBoxEditingControl.SelectedValue;
                    //}
                    gridTimeSettings.EndEdit();
                    return true;
                }
                else
                {
                    if (this.dtp.Focused)
                    {
                        gridTimeSettings.CurrentCell.Value = dtp.Text;
                        dtp.Visible = false;
                    }
                    else if (this.updown.Focused)
                    {
                        gridTimeSettings.CurrentCell.Value = updown.Value.ToString();
                        updown.Visible = false;
                    }
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void WndProc(ref Message msg)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_CLOSE = 0xF060;

            //从右上角关闭窗体时触发
            if (msg.Msg == WM_SYSCOMMAND && ((int)msg.WParam == SC_CLOSE))
            {
                OperationSucceeded = true;
            }
            base.WndProc(ref msg);
        }
    }

    /// <summary>
    /// 配置方案
    /// </summary>
    public class SettingScheme
    {
        [XmlArray("TimeSpanInfo"), XmlArrayItem("row")]
        public TimePeriod[] TimePeriods { get; set; }

        public string MaxPressure { get; set; }
        public string MaxSystolic { get; set; }
        public string MinSystolic { get; set; }
        public string MaxDiastolic { get; set; }
        public string MinDiastolic { get; set; }
        public int AlarmInDay { get; set; }
        public int AlarmInNight { get; set; }
        public int SyncWithSysTime { get; set; }
        [XmlIgnore]
        public string DeviceTime { get; set; }
        public int BPunit { get; set; }
        public int KeyboardEnabled { get; set; }
        public int DisplayEnabled { get; set; }
    }

    /// <summary>
    /// 时间区间及其血压采集间隔
    /// </summary>
    public class TimePeriod
    {
        public int TimeSpanIndex { get; set; }
        public int TimeType { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string TimeInterval { get; set; }
    }
}
