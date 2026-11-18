using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ABPMRepository
{
    class ABPMOperator_Monitor
    {
        private SerialPort com;
        private string patientInfoString = string.Empty;
        private string[] arrRecord;

        private int recordCount;
        public int RecordCount
        {
            get { return recordCount; }
            set { recordCount = value; }
        }

        public ABPMOperator_Monitor()
        {
            bool isReady = false;
            //foreach (string portName in portList)
            //{
                com = new SerialPort("COM4");

                if (TestCommunication() == true)
                {
                    isReady = true;
                    com.DataReceived += new SerialDataReceivedEventHandler(Port_DataReceived);
                }

            if (!isReady)
            {
                MessageBox.Show("COM4串口通信失败！");
                return;
            }

            arrRecord = new string[5] {
                "2019-05-10 08:34:10 77 132 98 0 0",
                "2019-05-10 09:00:59 77 517 517 0 0",
                "2019-05-10 09:31:11 51 138 103 0 0",
                "2019-05-10 10:01:49 51 517 517 0 0",
                "2019-05-10 10:31:05 51 517 517 0 0"
            };
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string strReceive = string.Empty;
            if (com != null)
            {
                int n = com.BytesToRead;
                byte[] byteReceive = new byte[n];
                com.Read(byteReceive, 0, n);

                strReceive = Encoding.UTF8.GetString(byteReceive);
                strReceive = TrimEndLineChars(strReceive);
                if (strReceive == "AT+AMBP")
                {
                    com.WriteLine("OK\r\n");
                }
                else if (strReceive.StartsWith("AT+PATIENT="))
                {
                    com.WriteLine("OK\r\n");
                    this.patientInfoString = strReceive.Substring("AT+PATIENT=".Length);
                }
                else if (strReceive.StartsWith("AT+PATIENT?"))
                {
                    if (string.IsNullOrEmpty( this.patientInfoString))
                        com.WriteLine("");
                    else
                        com.WriteLine("PATIENT=" + this.patientInfoString + "\r\n");
                }
                else if (strReceive.StartsWith("AT+RECORD_CNT?"))
                {
                    com.WriteLine("RECORD_CNT=" + arrRecord.Length + "\r\n");
                }
                else if (strReceive.StartsWith("AT+RECORD="))
                {
                    int index = Convert.ToInt32(strReceive.Split('=')[1].TrimEnd('?'));
                    com.WriteLine("RECORD=" + arrRecord[index] + "\r\n");
                }

                Console.WriteLine("接收到的数据是: " + strReceive);
            }
        }

        private bool TestCommunication()
        {
            bool bResult = false;
            try
            {
                com.Open();

                bResult = true;
            }
            catch (UnauthorizedAccessException ex)
            {
                bResult = false;
            }
            catch (Exception ex)
            {
                bResult = false;
            }
            finally
            {
                ClosePort();
            }

            return bResult;
        }

        /// <summary>
        /// 发送数据同步接收返回结果
        /// </summary>
        /// <param name="message"></param>
        /// <param name="keepConnection"></param>
        /// <returns></returns>
        private string sendMessage(string message, bool keepConnection = false)
        {
            string strReceived = string.Empty;
            try
            {
                if (!keepConnection) com.Open();
                com.WriteLine(message);

                DateTime dtBegin = DateTime.Now;
                while (com.BytesToRead < 2)
                {
                    Thread.Sleep(1);

                    if (DateTime.Now.Subtract(dtBegin).TotalMilliseconds > 3000) //如果3秒后仍然无数据返回，则视为超时
                    {
                        return "";
                    }
                }
                Thread.Sleep(50);
                byte[] recData = new byte[com.BytesToRead];
                com.Read(recData, 0, recData.Length);

                strReceived = Encoding.UTF8.GetString(recData);
            }
            catch
            {
                return "";
            }
            finally
            {
                if (!keepConnection) ClosePort();
            }
            return strReceived;
        }

        public void OpenPort() {
            com.Open();
        }

        public void ClosePort()
        {
            if (com != null && com.IsOpen)
            {
                com.Close();
                com.Dispose();
            }
        }

        private string TrimEndLineChars(string message)
        {
            return message.TrimEnd("\r\n".ToCharArray()).ToUpper();
        }
    }
}
