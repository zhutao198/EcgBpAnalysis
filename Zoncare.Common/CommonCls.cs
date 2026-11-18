using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Zoncare.Utilities;

namespace Zoncare.Common
{
    /// <summary>
    /// 心电采集的数据点
    /// </summary>
    public struct PointD
    {
        public static readonly PointD Empty = new PointD(0, 0, false);
        public short Val { get; set; }
        public int Statu { get; set; }
        public bool IsPacing { get; set; }

        public PointD(short val, int statu)
        {
            this.Val = val;
            this.Statu = statu;
            this.IsPacing = false;
        }
        public PointD(short val, int statu, bool pacing)
        {
            this.Val = val;
            this.Statu = statu;
            this.IsPacing = pacing;
        }
        public static bool operator ==(PointD a, PointD b)
        {
            return (a.Val == b.Val);
        }
        public static bool operator !=(PointD a, PointD b)
        {
            return (a.Val != b.Val);
        }
    }

    /// <summary>
    /// 接收到的心电波形数据信息(可能包含多帧数据)
    /// </summary>
    public class EcgDataElement
    {
        /// <summary>
        /// 当前心率
        /// </summary>
        public int HR { get; set; }

        /// <summary>
        /// 各导联状态(对应于18个导联的序号)
        /// </summary>
        public bool[] LeadStatus { get; set; }

        /// <summary>
        /// 根据各帧字节数据转换的原始数据
        /// </summary>
        public List<int> RawData { get; set; }

        /// <summary>
        /// 各导联数据(不含状态数据)
        /// </summary>
        public Dictionary<int, List<PointD>> LeadData { get; set; }

        public byte Index { get; set; }

        public void Reset()
        {
            HR = 0;
            LeadStatus = new bool[18];
            RawData?.Clear();
            LeadData?.Clear();
        }
    }

    /// <summary>
    /// 参数键值对
    /// </summary>
    public class KeyValuePara
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }

    /// <summary>
    /// ComboBox、ListBox等下拉列表的项
    /// </summary>
    public class ListItem
    {
        private string itemText = null;
        private object itemValue = null;

        public ListItem() { }
        public ListItem(object val, string text)
        {
            itemValue = val;
            itemText = text;
        }

        /// <summary>
        /// 获取或设置此列表项的显示文本
        /// </summary>
        public string Text
        {
            get { return this.itemText; }
            set { this.itemText = value; }
        }

        /// <summary>
        /// 获取或设置与此列表项关联的值
        /// </summary>
        public object Value
        {
            get { return this.itemValue; }
            set { this.itemValue = value; }
        }

        public int Id { get; set; }
        public string Sort { get; set; }
        public object Tag { get; set; }

        public override string ToString()
        {
            return itemText;
        }
    }

    /// <summary>
    /// 血压设备基本信息
    /// </summary>
    public class DeviceInfo
    {
        /// <summary>
        /// 设备序列号
        /// </summary>
        public string SN { get; set; }

        /// <summary>
        /// 蓝牙设备MAC地址
        /// </summary>
        public string MAC { get; set; }

        /// <summary>
        /// 设备类型(1:旧心电设备(IE90/IE95); 2:心电(8333); 3:血压(8337); 4:二合一设备(8377);)
        /// </summary>
        public int DeviceType { get; set; }

        /// <summary>
        /// 设备连接方式(USB直连:1, 蓝牙连接:2)
        /// </summary>
        public int ConnectionMode { get; set; }

        /// <summary>
        /// 设备型号(如：IE6、IE6S、IE6T、IE6Pro、IE6Ultro、IE7、IE7S、IE7T、IE7Pro、IE7Ultro等)
        /// </summary>
        public string DeviceModel { get; set; }

        /// <summary>
        /// 相关磁盘路径(直连设备)
        /// </summary>
        public string DrivePath { get; set; }

        public DeviceInfo(int deviceType, string deviceModel)
        {
            DeviceType = deviceType;
            DeviceModel = deviceModel;
        }
    }

    /// <summary>
    /// 血压戴机参数设置方案
    /// </summary>
    public class BpSettingScheme
    {
        public string start1 { get; set; } = "07:00";
        public string end1 { get; set; } = "22:00";
        public int interval1 { get; set; } = 30;
        public string start2 { get; set; } = "22:00";
        public string end2 { get; set; } = "07:00";
        public int interval2 { get; set; } = 60;
        public string start3 { get; set; } = "00:00";
        public string end3 { get; set; } = "00:00";
        public int interval3 { get; set; } = 5;
        public string start4 { get; set; } = "00:00";
        public string end4 { get; set; } = "00:00";
        public int interval4 { get; set; } = 5;
        public string start5 { get; set; } = "00:00";
        public string end5 { get; set; } = "00:00";
        public int interval5 { get; set; } = 5;

        public int systolicMax { get; set; } = 140;
        public int systolicMin { get; set; } = 90;
        public int diastolicMax { get; set; } = 90;
        public int diastolicMin { get; set; } = 60;
        public int dayAlarm { get; set; } = 1;
        public int nightAlarm { get; set; } = 1;
        public int syncTime { get; set; } = 1;
        public int pressureUnit { get; set; } = 0;
    }

    /// <summary>
    /// 设备状态
    /// </summary>
    public class DeviceStatus
    {
        public bool IsReady { get; set; }
        /// <summary>
        /// 检测到的设备类型(0: 异常; 1:旧心电设备(IE90/IE95); 2:心电(8333); 3:血压(8337); 4:二合一设备(8377);)
        /// </summary>
        public DeviceInfo DeviceInfo { get; set; }
        public string Msg { get; set; }
        public string LeadOffAlarm { get; set; }
        public string DrivePath { get; set; }
    }

    /// <summary>
    /// 降采样映射关系
    /// </summary>
    public struct SampleMapping
    {
        public int mapping { get; set; }
        public int target { get; set; }
    }

    //12导ST异常标准
    public struct AbnormalOfST
    {
        public int index { get; set; }
        public double fall { get; set; }
        public double up { get; set; }
    }

    /// <summary>
    /// 坐标轴的相关信息
    /// </summary>
    public struct AxisInfo
    {
        public AxisZQ AxisType;
        public float Length;
        public float MajorGap;
        public float MinorGap;
        public int MajorMarkerNum;
        public int MinorMarkerNum;
    }

    /// <summary>
    /// 心电导联
    /// </summary>
    public class Lead
    {
        public int Index { get; set; }
        public string Alias { get; set; }       //别名
        public string RealName { get; set; }    //实际导联名称

        public Lead() { }

        public Lead(int index, string name)
        {
            Index = index;
            RealName = name;
        }
    }

    /// <summary>
    /// 心电波形属性(纸速、增益等)
    /// </summary>
    public struct EcgAttr
    {
        public int Index { get; set; }
        public float Value { get; set; }
        public string Text { get; set; }

        public EcgAttr(int index, float value)
        {
            this.Index = index;
            this.Value = value;
            this.Text = "";
        }
    }

    /// <summary>
    /// 与语言相关的参数设置
    /// </summary>
    public struct LanguageSettings
    {
        /// <summary>
        /// 对当前语言的文本字体大小的缩放调整比例
        /// </summary>
        public float AdjustFontScale { get; set; }

        /// <summary>
        /// 心电报告中单行显示的内容高度比例系数(适当增加行间距以允许容纳两行文字)
        /// </summary>
        public float LineHeightRatio { get; set; }
    }

    /// <summary>
    /// 坐标轴
    /// </summary>
    public enum AxisZQ
    {
        AxisX,
        AxisY,
        AxisZ
    }

    /// <summary>
    /// 心电采集设备类型
    /// </summary>
    public enum EcgDeviceType
    {
        Device_unknow = -1,
        Device_iMAC = 0, // IMAC系列
        Device_collectBox = 1, // 中旗采集盒
        Device_zqSeries = 2, // ZQ系列（32位）
        Device_wifiDaq = 3, // WIFI采集盒
        Device_zqDaq = 4, // ZQ采集盒（心电一体机采集卡）
        Device_bluetoothDaq = 5, // 蓝牙采集盒（WIFI采集盒改进版）
    }

    /// <summary>
    /// 导联体系
    /// </summary>
    public enum Tissues
    {
        STANDARD,           //常规
        PARIES_POSTERIOR,   //后壁
        RIGHT_THORAX,       //右胸
        LEFT_THORAX,        //左胸(右胸后壁)
        BETWEEN_RIB,        //上一肋间
        NEXT_RIB,           //下一肋间
        FRANK,
        CABRERA
    }

    /// <summary>
    /// 波形走纸速度
    /// </summary>
    public enum EcgSpeed
    {
        EcgSpeed_5 = 0,     // 5mm/s
        EcgSpeed_6_25 = 1,  // 6.25mm/s
        EcgSpeed_10 = 2,    // 10mm/s
        EcgSpeed_12_5 = 3,  // 12.5mm/s
        EcgSpeed_25 = 4,    // 25mm/s
        EcgSpeed_50 = 5,    // 50mm/s
    };

    /// <summary>
    /// 波形增益
    /// </summary>
    public enum EcgGain
    {
        EcgGain_1_25 = 0,   // 1.25mm/mv
        EcgGain_2_5 = 1,    // 2.5mm/mv
        EcgGain_5 = 2,      // 5mm/mv
        EcgGain_10 = 3,     // 10mm/mv
        EcgGain_20 = 4,     // 20mm/mv
        EcgGain_40 = 5,     // 40mm/mv
        EcgGain_Auto = 6,   // 自动增益，默认为-10.0mm/mv
    };

    /// <summary>
    /// 心电数据导出文件的格式
    /// </summary>
    public enum EcgFormat
    {
        EcgFormat_ZQECG = 0,    // 中旗自定义格式心电文件
        EcgFormat_PDF = 1, // 中旗自定义PDF格式心电文件
        EcgFormat_PNG = 2, // 中旗自定义PNG格式心电文件
        EcgFormat_JPG = 3, // 中旗自定义JPG格式心电文件
        EcgFormat_XML = 4, // 中旗自定义XML格式心电文件
        EcgFormat_HL7 = 5, // HL7格式心电文件
        EcgFormat_DICOM = 6, // DICOM格式心电文件
        EcgFormat_PNG_W = 7, // 中旗自定义PNG格式心电文件(没有患者信息)
        EcgFormat_JPG_W = 8, // 中旗自定义JPG格式心电文件(没有患者信息)
        EcgFormat_INI = 9,   // 中旗自定义INI格式心电文件(没有波形数据)
        EcgFormat_JSON = 10, // 中旗自定义JSON格式心电文件(没有波形数据)
        EcgFormat_XML_NW = 11 // 中旗自定义XML格式心电文件(没有波形数据)
    };

    /// <summary>
    /// 导联推导类型(8芯15导, 10芯18导...)
    /// </summary>
    public enum EcgDerivation
    {
        None = 0,
        From8To15 = 1,
        From10To18 = 2
    }

    /// <summary>
    /// iMac10数据传输途径
    /// </summary>
    public enum DataCommMode
    {
        BLE = 0,
        WIRE = 1
    }

    /// <summary>
    /// 后台返回码
    /// </summary>
    public enum RETURN_CODE
    {
        OK = 200, //正常
        StatusErr = 402, //数据状态有误
        NotFound = 404, //未找到设备或数据
        FileNotFound = 407, //文件未找到
        SessionExpire = 408, //session过期
        VerfifyFail = 409, //校验失败
        MisMatching = 410, //不匹配
        Err = 500, //系统错误
        Repeat = 606 //数据重复
    }

    /// <summary>
    /// 检查类别
    /// </summary>
    public enum CheckItem
    {
        ECG = 1,
        BP = 2,
        EcgAndBp = 3
    }

    /// <summary>
    /// 病例状态
    /// </summary>
    public enum CaseStatus
    {
        Registered = 1, //已登记
        Uploaded = 2,   //已上传
        Diagnosed = 3,  //已诊断
        Reviewed = 4    //已审核
    }

    /// <summary>
    /// 发生改变的系统设置信息类别
    /// </summary>
    public enum SettingChangeType
    {
        BaseInfoChanged,
        EcgSettingsChanged,
        BPSettingsChanged,
        ReportSettingsChanged,
        AccountSettingsChanged,
        HabitualSettingsChanged
    }

    public enum Holter_File_Type
    {
        IE90_10T12L = 0,    //IE90 10芯12导
        IE90_7T13L = 1,     //IE90 7芯3导
        IE90_5T3L = 2,      //IE90 5芯3导
        OLD_8377_10T12L = 3,//8377 10芯12导
        NEW_8377_10T12L = 4,//8377二合一 10芯12导
        NEW_8377_7T12L = 5, //8377二合一 7芯12导
        NEW_8377_7T3L = 6,  //8377二合一 7芯3导
        NEW_8377_5T3L = 7   //8377二合一 5芯3导
    }

    public enum Holter_FrameData_Size
    {
        IE90_12 = 16,        //16字节
        IE90_3 = 8,          //8字节
        NEW_8377 = 28,       //28 || 28字节  v1.0
        NEW_8377_20 = 20,    //28 || 20字节  v2.0
        NEW_8377_30 = 30     //30 || 30字节  v1.1
    }

    /// <summary>
    /// 病例相关的流程处理状态
    /// </summary>
    public class CaseProcessState
    {
        /// <summary>
        /// 当前处理状态(0:正常, 400及以上:错误代码, -1:生成数据中, -2:正验证CRC)
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 获取总体进度值
        /// </summary>
        public int ProgressValue
        {
            get
            {
                int percent = 0;
                foreach (string key in SubProgresses.Keys)
                {
                    WorkProgress progress = SubProgresses[key];
                    percent += (int)(progress.weight * progress.percent);
                }

                if (percent < 1) percent = 1;
                percent = Math.Min(percent, 99);
                return percent;
            }
        }

        /// <summary>
        /// 处理过程的子流程进度
        /// </summary>
        public Dictionary<string, WorkProgress> SubProgresses { get; set; } = new Dictionary<string, WorkProgress>();

    }

    /// <summary>
    /// 数据分析或算法处理的进度
    /// </summary>
    public class WorkProgress
    {
        public string @type { get; set; }
        public string code { get; set; } = "200";
        public string message { get; set; }
        public float weight { get; set; } = 1; //总处理流程中的权重(0-1)
        public int percent { get; set; }
        public bool completed = false;
        public WorkProgress(float subWeight = 1)
        {
            weight = subWeight;
        }
    }


    #region 心电参数配置相关
    public class LeadInfo
    {
        public LeadData data { get; set; }
        public string[] props { get; set; }
        public ChannelSetting channelSetting { get; set; }
        public LeadConfig leadConfig { get; set; }
        public WaveConfig waveConfig { get; set; }
        public Dictionary<string, string[]> stOperationSetting { get; set; }
    }

    public class LeadData
    {
        public string[] I { get; set; }
        public string[] II { get; set; }
        public string[] III { get; set; }
        public string[] aVR { get; set; }
        public string[] aVL { get; set; }
        public string[] aVF { get; set; }
        public string[] V1 { get; set; }
        public string[] V2 { get; set; }
        public string[] V3 { get; set; }
        public string[] V4 { get; set; }
        public string[] V5 { get; set; }
        public string[] V6 { get; set; }
    }

    public class ChannelSetting
    {
        public int mainChannel { get; set; }
        public int secondChannel1 { get; set; }
        public int secondChannel2 { get; set; }
    }

    #endregion

    /// <summary>
    /// 对波形显示的相关设置
    /// </summary>
    public class WaveSettings
    {
        /// <summary>
        /// 走纸速度(mm/s)
        /// </summary>
        public float ScanSpeed { get; set; } = 25F;

        /// <summary>
        /// 增益(mm/mV)
        /// </summary>
        public float Gain { get; set; } = 10F;

        public int Layout_12 { get; set; } = 3;         //12导布局(默认: 6x2) 
        public int Layout_15 { get; set; } = 10;        //15导布局(默认: 5x3)
        public int Layout_18 { get; set; } = 6;         //18导布局(默认: 6x3)
        public int Layout_6 { get; set; } = 12;         //6导布局(默认: 6x1)
        public int Layout_9 { get; set; } = 13;         //9导布局(默认: 9x1)

        /// <summary>
        /// 是否显示网格
        /// </summary>
        public bool GridVisible { get; set; } = true;

        /// <summary>
        /// 网格线颜色
        /// </summary>
        [JsonIgnore]
        public Color GridColor { get; set; } = SysConstants.SAMPLE_GRID_COLOR;

        /// <summary>
        /// 波形通道宽度(mV)
        /// </summary>
        [JsonIgnore]
        public float ChannelWidth { get; set; } = 2.0f;

        public int GetLayoutIndex(int leadMode)
        {
            int layoutIdx = 3;
            switch (leadMode)
            {
                case 0:
                    layoutIdx = Layout_6;
                    break;
                case 1:
                    layoutIdx = Layout_12;
                    break;
                case 2:
                    layoutIdx = Layout_15;
                    break;
                case 3:
                    layoutIdx = Layout_18;
                    break;
                case 4:
                    layoutIdx = Layout_9;
                    break;
            }
            return layoutIdx;
        }

        public void SetLayoutIndex(int leadMode, int layoutIndex)
        {
            switch (leadMode)
            {
                case 0:
                    Layout_6 = layoutIndex;
                    break;
                case 1:
                    Layout_12 = layoutIndex;
                    break;
                case 2:
                    Layout_15 = layoutIndex;
                    break;
                case 3:
                    Layout_18 = layoutIndex;
                    break;
                case 4:
                    Layout_9 = layoutIndex;
                    break;
            }
        }
    }

    /// <summary>
    /// 对心电采集的相关设置
    /// </summary>
    public class SampleSettings
    {
        /// <summary>
        /// 导联模式(0:6导, 1:12导, 2:15导, 3:18导, 4:9导)
        /// </summary>
        public int LeadMode { get; set; } = 1;

        /// <summary>
        /// 导联体系
        /// </summary>
        public Tissues LeadSystem { get; set; } = Tissues.STANDARD;

        /// <summary>
        /// 采样时长(s)
        /// </summary>
        public int SamplingDuration { get; set; } = 10;

        /// <summary>
        /// 文件输出路径
        /// </summary>
        public string OutputPath { get; set; } = PubFuns.GetAbsolutePath("ECG-20NT\\ecgstd", true, true);

        /// <summary>
        /// 文件输出格式
        /// </summary>
        [JsonIgnore]
        public string OutputFileType { get; set; }

        /// <summary>
        /// 波形显示相关设置
        /// </summary>
        public WaveSettings WaveSettingInfo { get; set; }

        /// <summary>
        /// 相关导联
        /// </summary>
        [JsonIgnore]
        public List<Lead> RelatedLeads
        {
            get
            {
                int layoutIndex = WaveSettingInfo.GetLayoutIndex(LeadMode);
                return CommonFuns.GetRelatedLeads(LeadMode, LeadSystem, layoutIndex, SingleRhythm, ThreeRhythm1, ThreeRhythm2, ThreeRhythm3, OneLeadOnly);
            }
        }

        /// <summary>
        /// 原始数据导联
        /// </summary>
        [JsonIgnore]
        public List<Lead> RelatedRawLeads
        {
            get
            {
                int layoutIndex = WaveSettingInfo.GetLayoutIndex(LeadMode);
                return CommonFuns.GetRelatedLeads(LeadMode, LeadSystem, layoutIndex, SingleRhythm, ThreeRhythm1, ThreeRhythm2, ThreeRhythm3, OneLeadOnly, true);
            }
        }

        /// <summary>
        /// 起搏开关(0:off, 1:on)
        /// </summary>
        public int PaceOn { get; set; } = 0;

        /// <summary>
        /// 起搏检测通道
        /// </summary>
        public int PaceLeadIndex { get; set; } = 6; //C1

        /// <summary>
        /// 数据传输方式或接口类型(iMac10 蓝牙接口:0, 磁吸接口:1)
        /// </summary>
        public DataCommMode DataCommMode { get; set; } = DataCommMode.BLE;

        /// <summary>
        /// 自动诊断(0:off, 1:on)
        /// </summary>
        public int AutoDiagnose { get; set; } = 1;

        /// <summary>
        /// 导联推导类型(智能导联)
        /// </summary>
        //public EcgDerivation DerivationType { get; set; } = EcgDerivation.None;

        public int DerivateFrom8To15 { get; set; } = 0;
        public int DerivateFrom10To18 { get; set; } = 0;

        /// <summary>
        /// 单节率导联
        /// </summary>
        public int SingleRhythm { get; set; } = 0;

        public int ThreeRhythm1 { get; set; } = 0;    //三节律第一个导联索引
        public int ThreeRhythm2 { get; set; } = 1;    //三节律第二个导联索引
        public int ThreeRhythm3 { get; set; } = 2;    //三节律第三个导联索引

        /// <summary>
        /// 是否应用于单个导联的情形(如: 节律分析, 心率变异性)
        /// </summary>
        [JsonIgnore]
        public bool OneLeadOnly { get; set; }

        /// <summary>
        /// 三节率导联
        /// </summary>
        [JsonIgnore]
        public Tuple<int, int, int> ThreeRhythms
        {
            get
            {
                return new Tuple<int, int, int>(ThreeRhythm1, ThreeRhythm2, ThreeRhythm3);
            }
        }
    }

    /// <summary>
    /// 心电采集的相关设置(节律分析)
    /// </summary>
    public class SampleSettings_rhythm
    {
        /// <summary>
        /// 采样时长(s)
        /// </summary>
        public int SamplingDuration { get; set; } = 60;

        /// <summary>
        /// 文件输出路径
        /// </summary>
        public string OutputPath { get; set; } = PubFuns.GetAbsolutePath("ECG-20NT\\ecg_rhythm", true, true);

        /// <summary>
        /// 波形显示相关设置
        /// </summary>
        public WaveSettings WaveSettingInfo { get; set; }

        /// <summary>
        /// 节律分析的导联
        /// </summary>
        public int LeadIndex { get; set; } = 0;
    }

    /// <summary>
    /// 心电系统主体应用设置
    /// </summary>
    public class EcgSettings
    {
        public EcgDeviceType DeviceType { get; set; } = EcgDeviceType.Device_iMAC; //设备类型
        public string ImacIPAddress { get; set; }       //iMAC设备主机地址
        public int? ImacPort { get; set; } = 9998;      //iMAC设备通讯端口
        public string ImacSN { get; set; }              //iMAC设备Sn
        public string CommPort { get; set; }            //采集盒串口号
        public string BluetoothSN { get; set; }         //蓝牙设备Sn
        public int EcgFrequency { get; set; } = 50;         //工频
        public float EcgBaselineDrift { get; set; } = 0.3F; //基线漂移
        public int EcgEMG { get; set; } = 35;               //肌电
        public string Organization { get; set; }        //机构名称
        public string ExportPath { get; set; } = PubFuns.GetAbsolutePath("ECG-20NT\\ecgexport", true, true); //文件导出路径
        public string HisExportPath { get; set; } = PubFuns.GetAbsolutePath("ECG-20NT\\his", true, true); //his文件保存路径
        public int EcgType { get; set; } = 0;           //心电类型(默认: 标准心电)
        public int LeadMode { get; set; } = 1;          //导联模式(0:6导; 1:12导[默认]; 2:15导; 3:18导; 4:9导)
        public int DisplayMode { get; set; } = 0;       //波形显示模式(0:同步; 1:顺序)
        public EcgFormat[] FileFormats { get; set; } = new EcgFormat[] { };   //导出文件的格式
        public int FileNameType { get; set; } = 0; //导出文件的命名方式(0:默认-用日期时间命名; 1:自定义)
        public string FileNameFormat { get; set; } = "DateTime"; //自定义文件命名的格式
        public int Direction { get; set; } = 0;         //文件内容排版方向(0[默认值]:横向; 1:纵向;)
        public int? Scale { get; set; } = 100;          //缩放比例
        public float PaperSpeed { get; set; } = 25F;    //纸速
        public float Gain { get; set; } = 10F;          //增益
        public int ShowGrid { get; set; } = 1;          //是否显示网格(0:否; 1:是)

        public LeadModeInfo[] LeadModesInfo { get; set; }
    }

    /// <summary>
    /// 波形分页信息
    /// </summary>
    public class EcgPager
    {
        public int[] PageSizes { get; set; } = new int[1]; //各页的波形数据点个数
        public int PageCount { get; set; } = 1;
        public int PageIndex { get; set; }
        public Rectangle RenderRectangle { get; set; }
    }

    /// <summary>
    /// 与导联模式相关的导联体系、布局、节律导联等信息
    /// </summary>
    public class LeadModeInfo
    {
        public int LeadMode { get; set; }
        public Tissues LeadSystem { get; set; } = Tissues.STANDARD; //导联体系
        public int LayoutIndex { get; set; }
        public int SingleRhythmLead { get; set; } = 0;  //单节率导联索引
        public int ThreeRhythmLead1 { get; set; } = 0;  //三节律第一个导联索引
        public int ThreeRhythmLead2 { get; set; } = 1;  //三节律第二个导联索引
        public int ThreeRhythmLead3 { get; set; } = 2;  //三节律第三个导联索引
    }

    /// <summary>
    /// 自定义病例列表字段相关信息
    /// </summary>
    public class DataField
    {
        public string Field { get; set; }
        public string DisplayName { get; set; }
        public bool Hide { get; set; }
        public bool Sort { get; set; }
        public bool IsCustom { get; set; }
        public bool IsUsed { get; set; }
        public int WidthWeight { get; set; }
        [JsonIgnore]
        public bool IsModified { get; set; } = false;

        public DataField() { }
        public DataField(string fieldName, string displayName)
        {
            Field = fieldName;
            DisplayName = displayName;
        }
    }

    /// <summary>
    /// PropertyGrid的绑定对象的基类
    /// </summary>
    public class BasicReportInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public long Id { get; set; }//reportID
        public string Gender { get; set; }
        public int? Age { get; set; }
        public string AgeUnit { get; set; }
        public string AgeAndUnit { get; set; }//年龄+单位 列表显示
        public string Race { get; set; }//种族
        public int? Height { get; set; }
        public double? Weight { get; set; }
        public EcgFileInfo EcgFileInfo { get; set; } = new EcgFileInfo() { Waveform = new Dictionary<int, PointD[]>() };

        [Category("\t\t患者信息"), DisplayName("PID"), ReadOnlyAttribute(true), BrowsableEx(true)]
        public string PId { get; set; }
        [Category("\t\t患者信息"), DisplayName("姓名"), ReadOnlyAttribute(true), BrowsableEx(true)]
        public string Name { get; set; }//患者姓名
        [Category("\t\t患者信息"), DisplayName("性别"), TypeConverter(typeof(GenderItem)), ReadOnlyAttribute(true), BrowsableEx(true)]
        public string GenderStr { get; set; }
        [Category("\t\t患者信息"), DisplayName("年龄"), ReadOnlyAttribute(true), BrowsableEx(true)]
        public string AgeStr { get; set; }
        [Category("\t\t患者信息"), DisplayName("年龄单位"), TypeConverter(typeof(AgeUnitItem)), ReadOnlyAttribute(true), BrowsableEx(true)]
        public string AgeUnitStr { get; set; }
        [Category("\t\t患者信息"), DisplayName("科室"), ReadOnlyAttribute(true), BrowsableEx(true)]
        public string Department { get; set; }//科室
        [Category("\t\t患者信息"), DisplayName("床号"), ReadOnlyAttribute(true), BrowsableEx(true)]
        public string BedNo { get; set; }//床号
        [Category("\t\t患者信息"), DisplayName("种族"), TypeConverter(typeof(RaceItem)), ReadOnlyAttribute(true), BrowsableEx(true)]
        public string RaceStr { get; set; }
        [Category("\t\t患者信息"), DisplayName("体重"), BrowsableEx(false)]
        public string WeightStr { get; set; }
        [Category("\t\t患者信息"), DisplayName("身高"), BrowsableEx(false)]
        public string HeightStr { get; set; }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class EcgFileInfo
    {
        public Tissues LeadSystem { get; set; }
        public string FileTime { get; set; }//开始采集的时间
        public int AcquisitionDuration { get; set; } //采集时长(单位:毫秒)
        public int Frequency { get; set; }
        public double BaseLine { get; set; }
        public int Myoelect { get; set; }
        public int MachineType { get; set; }
        public Dictionary<int, PointD[]> Waveform { get; set; }//波形数据
        public int RhyIndex { get; set; }//节律分析心电图中的导联索引
        public int layoutIndex { get; set; }
        public int SingleRhythm { get; set; }
        public int ThreeRhythm1 { get; set; }
        public int ThreeRhythm2 { get; set; }
        public int ThreeRhythm3 { get; set; }
        public float Gain { get; set; }
        public float PaperSpeed { get; set; }
        public string DeviceSN { get; set; }
        public string DeviceTypeName { get; set; }
        public int PaceFlag { get; set; }
        public int SamplingRate { get; set; } = SysConstants.SAMPLE_RATE;
    }

    public class GenderItem : StringConverter
    {
        //重写GetStandardValuesSupported方法并返回true,以指示此对象支持可从列表中选取的一组标准值
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        //检索一个集合，该集合包含适用于此验证程序的数据类型的标准值集。 如果该数据类型不支持标准值集，这将返回 null
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new string[] { CommonValues.Langs["男"], CommonValues.Langs["女"], CommonValues.Langs["未知"] }); //编辑下拉框中的items
        }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;//true下拉框不可编辑
        }
    }
    public class AgeUnitItem : StringConverter
    {
        //重写GetStandardValuesSupported方法并返回true,以指示此对象支持可从列表中选取的一组标准值
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        //检索一个集合，该集合包含适用于此验证程序的数据类型的标准值集。 如果该数据类型不支持标准值集，这将返回 null
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (CommonValues.LanguageID != 1049)
            {
                return new StandardValuesCollection(new string[] { CommonValues.Langs["岁(复)"], CommonValues.Langs["月(复)"], CommonValues.Langs["天(复)"] }); //编辑下拉框中的items
            }
            else
            {
                InfCustomTypeDescriptor instance = context.Instance as InfCustomTypeDescriptor;
                BasicReportInfo rptInfo = instance.GetPropertyOwner(context.PropertyDescriptor) as BasicReportInfo;

                string[][] RusAgeUnits = new string[][] {
                    SysConstants.RusAgeUnits0.Values.ToArray(),
                    SysConstants.RusAgeUnits1.Values.ToArray(),
                    SysConstants.RusAgeUnits2.Values.ToArray(),
                };
                string[] rusAgeUnits = new string[3];
                if (string.IsNullOrEmpty(rptInfo.AgeStr) || Int32.Parse(rptInfo.AgeStr) == 0)
                {
                    rusAgeUnits = RusAgeUnits[0];
                }
                else
                {
                    int res = Math.Abs(Int32.Parse(rptInfo.AgeStr) % 10);
                    if (res == 1)
                    {
                        rusAgeUnits = RusAgeUnits[1];
                    }
                    else if (res == 2)
                    {
                        rusAgeUnits = RusAgeUnits[2];
                    }
                    else
                    {
                        rusAgeUnits = RusAgeUnits[0];
                    }
                }
                List<string> ageUnits = new List<string>(rusAgeUnits);
                ageUnits.Remove(SysConstants.RusAgeUnits0["U"]);
                return new StandardValuesCollection(ageUnits);
            }

        }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;//true下拉框不可编辑
        }
    }
    public class RaceItem : StringConverter
    {
        //重写GetStandardValuesSupported方法并返回true,以指示此对象支持可从列表中选取的一组标准值
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        //检索一个集合，该集合包含适用于此验证程序的数据类型的标准值集。 如果该数据类型不支持标准值集，这将返回 null
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new string[] { CommonValues.Langs["白人"], CommonValues.Langs["非洲"], CommonValues.Langs["东方"], CommonValues.Langs["其他"], CommonValues.Langs["未知"] }); //编辑下拉框中的items
        }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;//true下拉框不可编辑
        }
    }

}
