using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ABPMRepository
{
    /// <summary>
    /// 动态血压监测仪串行数据处理
    /// </summary>
    public class ABPMDataManager
    {
        private static ABPMOperatorBle operater;

        /// <summary>
        /// 扫描设备
        /// </summary>
        /// <returns>蓝牙设备的MAC地址列表</returns>
        public async static Task<List<string>> ScanDevice()
        {
            List<string> lst = null;
            if (operater == null || !operater.IsReady)
            {
                if (!string.IsNullOrEmpty(await IsConnectable(true)))
                {
                    return null;
                }
            }

            lst = await operater.GetBleDeviceList();

            return lst;
        }

        /// <summary>
        /// 连接蓝牙设备
        /// </summary>
        /// <param name="mac">要连接的蓝牙设备MAC地址</param>
        /// <returns>1:连接成功; 其它:连接失败</returns>
        public async static Task<int> ConnectDevice(string mac)
        {
            if (operater == null || !operater.IsReady)
            {
                if (!string.IsNullOrEmpty(await IsConnectable(true)))
                {
                    return 0;
                }
            }

            int bond = await operater.BoundDevice(mac);
            return bond;
        }

        /// <summary>
        /// 断开蓝牙连接
        /// </summary>
        /// <returns></returns>
        public async static Task<int> DisconnectDevice()
        {
            if (operater == null || !operater.IsReady)
            {
                if (!string.IsNullOrEmpty(await IsConnectable(true)))
                {
                    return 0;
                }
            }

            int bond = await operater.UnBoundDevice();
            return bond;
        }


        public async static Task<int> CheckConnectedMac(string mac)
        {
            if (operater == null || !operater.IsReady)
            {
                if (!string.IsNullOrEmpty(await IsConnectable(true)))
                {
                    return 0;
                }
            }

            int bond = await operater.CheckBondMac(mac);
            return bond;
        }

        /// <summary>
        /// 获取设备的型号
        /// </summary>
        /// <returns></returns>
        public async static Task<string> GetDeviceModel()
        {
            string modelInfo = string.Empty;
            if (operater == null || !operater.IsReady)
            {
                if (!string.IsNullOrEmpty(await IsConnectable(true)))
                {
                    return null;
                }
            }

            string deviceModel = await operater.GetDeviceModel();
            if (!string.IsNullOrEmpty(deviceModel))
            {
                modelInfo = deviceModel;
            }

            return modelInfo;
        }

        /// <summary>
        /// 同步系统时间
        /// </summary>
        /// <returns></returns>
        public async static Task<int> SyncTime(string timeString)
        {
            string modelInfo = string.Empty;
            if (operater == null || !operater.IsReady)
            {
                if (!string.IsNullOrEmpty(await IsConnectable(true)))
                {
                    return 0;
                }
            }

            int result = await operater.SyncSystemTime(timeString);
            return result;
        }


        /// <summary>
        /// 获取数据文件信息
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>文件大小及CRC信息</returns>
        public async static Task<ZqFileInfo> FetchFileInfo(string fileName)
        {
            if (operater == null || !operater.IsReady)
            {
                if (!string.IsNullOrEmpty(await IsConnectable(true)))
                {
                    return null;
                }
            }

            ZqFileInfo fi = null;
            string fileInfo = await operater.GetFileInfo(fileName);
            if (!string.IsNullOrEmpty(fileInfo))
            {
                string[] info = fileInfo.Split(',');
                fi = new ZqFileInfo()
                {
                    FileName = fileName,
                    FileSize = Convert.ToInt32(info[1]),
                    CRC = info[2]
                };
            }

            return fi;
        }

        /// <summary>
        /// 写入文件信息
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async static Task<int> WriteFileText(string fileName, string fileText)
        {
            if (operater == null || !operater.IsReady)
            {
                if (!string.IsNullOrEmpty(await IsConnectable(true)))
                {
                    return 0;
                }
            }

            return await operater.SetFileInfo(fileName, fileText);
        }


        /// <summary>
        /// 根据指定的信息生成文件
        /// </summary>
        /// <param name="fullName">文件全路径</param>
        /// <param name="fileName">文件名</param>
        /// <param name="fileLength">文件大小</param>
        /// <returns>所生成文件的内容(字节)</returns>
        public async static Task<byte[]> SaveFile(ZqFileInfo fileInfo, string savePath = "", int tryTimes = 1)
        {
            if (operater == null || !operater.IsReady)
            {
                if (!string.IsNullOrEmpty(await IsConnectable(true)))
                {
                    return null;
                }
            }

            byte[] content = null;
            int oneSize = 1024;
            //byte[] content = new byte[fileLength];
            //for (int i = 0; i < fileLength; i += oneSize)
            //{
            //    byte[] bys = await operater.GetFileData(fileName, i, oneSize);
            //    if (bys != null)
            //    {
            //        Buffer.BlockCopy(bys, 0, content, i, bys.Length);
            //        await Task.Delay(300);
            //    }
            //    else
            //    {
            //        return null;
            //    }
            //}

            for (int i = 0; i < tryTimes; i++)
            {
                await Task.Delay(50);
                content = await operater.GetFileData(fileInfo, 0);
                if (content != null)
                {
                    if (!string.IsNullOrEmpty(savePath))
                    {
                        using (FileStream filestream = new FileStream(savePath, FileMode.Create))
                        {
                            filestream.Write(content, 0, content.Length);
                            filestream.Close();
                        }
                    }
                    break;
                }
            }

            return content;
        }

        /// <summary>
        /// 清除监测仪内的历史血压数据
        /// </summary>
        /// <returns></returns>
        public async static Task<int> ClearData()
        {
            if (operater == null || !operater.IsReady)
            {
                if (!string.IsNullOrEmpty(await IsConnectable(true)))
                {
                    return 0;
                }
            }

            return await operater.ClearData();
        }

        /// <summary>
        /// 本地串口通讯通道是否正常
        /// </summary>
        /// <param name="forRefresh">是否重新发现并连接</param>
        /// <returns></returns>
        public static async Task<string> IsConnectable(bool forRefresh = false)
        {
            string errCode = "", errMsg = "";

            if (forRefresh)
            {
                operater?.ClosePort();
            }

            ABPMOperatorBle bleOperater = new ABPMOperatorBle();
            errCode = await bleOperater.TryConnectDevice(1);
            if (!string.IsNullOrEmpty(errCode))
            {
                if ("401" == errCode)
                {
                    errMsg = "没有检测到本机通信串口！";
                }
                else if ("402" == errCode)
                {
                    errMsg = "设备通信连接失败！";
                }
                else
                {
                    errMsg = "设备通信连接失败！";
                }
            }
            else
            {
                operater = bleOperater;
            }

            return errCode;
        }

        /// <summary>
        /// 检查数据帧有效性
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool CheckPackData(byte[] content)
        {
            bool result = false;
            int len = content.Length;
            if (len > 4)
            {
                if (content[0] == 0xAA && content[1] == 0xAA && content[2] == 0xAA && content[3] == 0xAA)
                {
                    byte[] idxBytes = new byte[] { content[4], content[5] };
                    ushort idx = BitConverter.ToUInt16(ZqBPFuns.GetBySystemEndian(idxBytes), 0);

                    byte[] lenBytes = new byte[] { content[6], content[7] };
                    ushort dataLen = BitConverter.ToUInt16(ZqBPFuns.GetBySystemEndian(lenBytes), 0);
                    byte[] arrData = new byte[dataLen];
                    Buffer.BlockCopy(content, 8, arrData, 0, arrData.Length);

                    //CRC16校验
                    byte[] crcData = new byte[dataLen + 4];
                    Buffer.BlockCopy(content, 4, crcData, 0, crcData.Length);
                    ushort crc16 = ZqBPFuns.ComputeCRC16_CCITT(crcData);
                    byte[] crcBytes = new byte[] { content[len - 2], content[len - 1] };
                    if (BitConverter.ToUInt16(ZqBPFuns.GetBySystemEndian(crcBytes), 0) == crc16)
                    {
                        result = true;
                    }
                }
            }

            return result;
        }
    }

    public class ZqFileInfo
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int FileSize { get; set; }
        public string CRC { get; set; }

        public ZqFileInfo() { }
        public ZqFileInfo(string fileName)
        {
            FileName = fileName;
        }
        public ZqFileInfo(string fileName, string filePath)
        {
            FileName = fileName;
            FilePath = filePath;
        }

    }

    public class ZqPatientInfo
    {
        public ZqPatientInfo() { }

        public string id { get; set; } //patient ID
        public string name { get; set; }
        public string sex { get; set; }
        public string age { get; set; }
        public string AgeUnit { get; set; }
        public string birthday { get; set; }
        public string height { get; set; }
        public string weight { get; set; }
        public string dept { get; set; }
        public string bed { get; set; }
        public int? dataId { get; set; }
    }
}
