using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace ABPMRepository
{
    public partial class BluetoothConnection : Form
    {
        private ABPMOperator op_ABPM;
        private System.Timers.Timer conn_Timer = new System.Timers.Timer(500);
        private int m_angle = 0; //从 x 轴到弧线的起始点沿顺时针方向度量的角（以度为单位）

        public string SelectedBluetooth { get; private set; } //远端MAC地址或 "0"(本地直连)
        public string ErrorMessage { get; private set; }

        public BluetoothConnection(ABPMOperator op)
        {
            InitializeComponent();
            op_ABPM = op;

            this.picConn.GetType().GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .SetValue(picConn, true, null);
        }

        private void BluetoothConnection_Load(object sender, EventArgs e)
        {
            Thread thdFillBle = new Thread(new ThreadStart(retrieveBluetooth)); //创建新的线程
            thdFillBle.Start();

            this.conn_Timer.Elapsed += new ElapsedEventHandler(conn_Timer_Elapsed);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cbxBluetooth_DropDown(object sender, EventArgs e)
        {
            //List<BluetoothInformation> dataSource = cbxBluetooth.DataSource as List<BluetoothInformation>;
            //dataSource.RemoveAt(0);
            //cbxBluetooth.DataSource = dataSource;
            //SendKeys.Send("{F4}");
        }

        private void cbxBluetooth_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!cbxBluetooth.ContainsFocus) return;
            if (cbxBluetooth.SelectedValue == null) return;
            if (cbxBluetooth.SelectedValue.ToString() == "" || cbxBluetooth.SelectedValue.ToString() == "0") return;

            //conn_Timer.Enabled = true;
            picConn.Image = ZqBPFuns.ShowLoadingImage();
            m_angle = 0;

            string errMsg = string.Empty;
            bool connectable = op_ABPM.TryConnectDevice(out errMsg, true, cbxBluetooth.SelectedValue.ToString());
            //conn_Timer.Enabled = false;
            picConn.Image = null;

            picConn.Refresh();
            using (Graphics graphics = picConn.CreateGraphics())
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.FillEllipse(Brushes.White, 0, 0, picConn.Width, picConn.Height);
                if (connectable)
                {
                    graphics.DrawString("✔", new Font("宋体", 11, FontStyle.Bold), Brushes.LimeGreen, 0, 0);
                    this.SelectedBluetooth = cbxBluetooth.SelectedValue.ToString();
                    Thread.Sleep(100);
                    this.Close();
                }
                else
                {
                    graphics.DrawString("✖", new Font("宋体", 11, FontStyle.Bold), Brushes.Red, 0, 0);
                    this.SelectedBluetooth = "";
                }
            }
        }

        private void conn_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (picConn.InvokeRequired)
            {
                //Action refreshConnAction = new Action(refreshConnStatus);
                Action refreshConnStatus = () =>
                {
                    if (conn_Timer.Enabled)
                    {
                        picConn.Refresh();
                    }
                };
                if (this.IsHandleCreated) picConn.Invoke(refreshConnStatus); //not BeginInvoke -- BeginInvoke不能实时刷新
            }

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

        private void retrieveBluetooth()
        {
            if (cbxBluetooth.InvokeRequired)
            {
                var fillBleAction = new Action(retrieveBluetooth);
                if (this.IsHandleCreated) cbxBluetooth.BeginInvoke(fillBleAction);
            }
            else
            {
                cbxBluetooth.ValueMember = "BluetoothMac";
                cbxBluetooth.DisplayMember = "BluetoothName";
                cbxBluetooth.Text = "正在搜索蓝牙设备...";

                List<BluetoothInformation> lstBluetoothSource = op_ABPM.GetBluetooths();
                if (lstBluetoothSource.Count <= 0)
                {
                    cbxBluetooth.Items.Add(new BluetoothInformation { BluetoothMAC = "0", BluetoothName = "没有发现可用的蓝牙设备!" });
                    cbxBluetooth.SelectedIndex = 0;
                    this.ErrorMessage = "没有发现可用的蓝牙设备";
                    this.Close();
                }
                else
                {
                    //BluetoothInformation usedItem = lstBluetoothSource.Find(o => o.IsUsed == true);
                    lstBluetoothSource.Insert(0, new BluetoothInformation { BluetoothMAC = "0", BluetoothName = "---请选择蓝牙设备---" });
                    cbxBluetooth.DataSource = lstBluetoothSource;
                    //if (usedItem != null)
                    //    cbxBluetooth.SelectedItem = usedItem;
                    //else
                    //    cbxBluetooth.SelectedIndex = 0;

                    cbxBluetooth.Enabled = true;
                    this.ErrorMessage = "";
                }
            }
        }

    }
}
