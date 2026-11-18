using System.Collections.Generic;

namespace Zoncare.Common
{
    public class ECGMarks
    {
        public byte[] Marks { get; set; } //8字节心电文件后缀
        public string Md5 { get; set; } //32位字符串
        public int Catalog_count { get; set; }
        public ECGMarks()
        {
            Marks = new byte[8];
        }
    }
    public class ECGCatalog
    {
        public int Chapter { get; set; } //章节(编号)
        public int Pages { get; set; }//页码(地址)
    }
    public class ECGContent
    {
        public int Size { get; set; }
        public byte[] Data { get; set; }
    }
    public class ECGData
    {
        public ECGCatalog Catalog { get; set; }
        public ECGContent Content { get; set; }
        public ECGData()
        {
            Catalog = new ECGCatalog();
            Content = new ECGContent();
        }
    }
    public class ZQECGFileInfo
    {
        public ECGMarks Marks { get; set; }
        public List<ECGData> DataList { get; set; }
        public ZQECGFileInfo()
        {
            Marks = new ECGMarks();
            DataList = new List<ECGData>();
        }
    }
}
