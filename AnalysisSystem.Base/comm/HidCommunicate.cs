using HidLibrary;
using System.Linq;
using System.Text;

namespace AnalysisSystem.Base.comm
{
    class HidCommunicate
    {
        private const int VendorId = 0x2E88;
        private const int ProductId = 0x4601;
        private HidDevice _device;

        public bool ConnectToDevice()
        {
            bool deviceConnected = false;
            _device = HidDevices.Enumerate(VendorId, ProductId).FirstOrDefault();
            if (_device != null)
            {
                // 设备连接成功
                _device.OpenDevice();
                _device.Inserted += DeviceInserted;
                _device.Removed += DeviceRemoved;
                _device.MonitorDeviceEvents = true;

                //while (true)
                //{
                //    var report = _device.ReadReport();
                //    Console.WriteLine(BitConverter.ToString(report.Data));
                //}

                deviceConnected = true;
            }

            return deviceConnected;
        }

        public void CloseDevice()
        {
            _device?.CloseDevice();
        }

        private void DeviceInserted()
        {
            // 处理设备插入事件
        }

        private void DeviceRemoved()
        {
            // 处理设备拔出事件
        }

        public bool WriteData(byte[] data)
        {
            HidReport rpt = _device.CreateReport();
            rpt.Data = data;

            return _device.WriteReport(rpt);
        }

        public string GetManufacturerString()
        {
            string manufacturer = string.Empty;
            bool readOK = _device.ReadManufacturer(out byte[] data);
            if (readOK)
            {
                manufacturer = Encoding.UTF8.GetString(data).Replace("\0", "");
            }

            return manufacturer;
        }
    }
}
