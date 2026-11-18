using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace Zoncare.Common
{
    public class SysConstants
    {
        /// <summary>
        /// 日期格式
        /// </summary>
        public const string DATE_FORMAT = "yyyy-MM-dd";

        /// <summary>
        /// 日期时间格式
        /// </summary>
        public const string DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// 程序闲置超时时长(秒)
        /// </summary>
        public const int TIMEOUT_SECONDS = 36000; // 闲置10小时后超时

        /// <summary>
        /// 程序最佳宽度
        /// </summary>
        public const float BEST_WIDTH = 1920;

        /// <summary>
        /// 程序最佳高度
        /// </summary>
        public const float BEST_HEIGHT = 1040;

        /// <summary>
        /// 心电图网格每一小格(1毫米)对应的像素数 [1mm = 3.7795px]
        /// </summary>
        public const float PIX_SCALE = 3.7795f;

        /// <summary>
        /// 采样率
        /// </summary>
        public const int SAMPLE_RATE = 1000;

        /// <summary>
        /// 诊断测量算法产生的无效值
        /// </summary>
        public const int INVALID_MEASURE = -32768;

        /// <summary>
        /// 导出图片的默认大小(单位:像素)
        /// </summary>
        public static readonly Size EXPORT_SIZE_A4 = new Size(3508, 2479); //基于A4纸张按300DPI换算的大小(11.69*300, 8.27*300)

        public static Size EXPORT_SIZE = new Size(3508, 2479);

        /// <summary>
        /// 采集的心电波形网格线颜色
        /// </summary>
        public static readonly Color SAMPLE_GRID_COLOR = Color.FromArgb(60, 60, 60);

        /// <summary>
        /// 心电报告中网格线颜色
        /// </summary>
        public static readonly Color PRINT_GRID_COLOR = Color.FromArgb(150, 150, 150);

        /// <summary>
        /// 心电报告标题字体大小
        /// </summary>
        public static float TitleFontSize = 55f;

        /// <summary>
        /// 心电报告内容字体大小
        /// </summary>
        public static float ContentFontSize = 31.5f;

        /// <summary>
        /// 心电报告底部参数信息字体大小
        /// </summary>
        public static float ParaFontSize = 27f;

        /// <summary>
        /// 程序可解析的时间格式
        /// </summary>
        public static readonly string[] DATETIME_FORMATS = new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss:fff" };

        /// <summary>
        /// 导出文件名称的可能组成部分
        /// </summary>
        public static readonly string[] FileNameParts = new string[] { "PID", "Name", "DateTime", "Date", "Time" };

        /// <summary>
        /// 程序内导联别称
        /// </summary>
        public static readonly string[] LeadAlias = new string[] { "I", "II", "III", "aVR", "aVL", "aVF", "V1", "V2", "V3", "V4", "V5", "V6", "V7", "V8", "V9", "V3R", "V4R", "V5R" };

        public static readonly string[] LeadAlias_ADV = new string[] { "X", "Y", "Z", "X-Y", "X-Z", "Z-Y" };

        public static readonly string[] IMacTypes = new string[] { "NONE", "iMac1200", "iMac1800", "iMac300", "iMac120", "iMac12", "iMac1800A", "iMac1500", "iMac1500C" };

        /// <summary>
        /// 8377设备型号(动态心电血压二合一)
        /// </summary>
        public static readonly string[] DeviceModel8377 = new string[] { "iE10", "iE20", "iE30", "iE15", "iE25" };

        /// <summary>
        /// 8337设备型号(动态血压)
        /// </summary>
        public static readonly string[] DeviceModel8337 = new string[] { "iE6", "iE6S", "iE6T", "iE6Pro", "iE7", "iE7S", "iE7T", "iE7Pro", "VIS9" };

        /// <summary>
        /// 8333设备型号(动态心电)
        /// </summary>
        public static readonly string[] DeviceModel8333 = new string[] { "iE4", "iE4s", "iE4pro", "iE5", "iE5s", "iE5pro" };


        public static readonly Dictionary<int, string> Locales = new Dictionary<int, string>()
        {
            {2052, "zh" },  //中文
            {1033, "en" },  //英文
            {1049, "ru" },  //俄语
        };

        public static readonly PersistDictionary<int> CheckItems = new PersistDictionary<int>(true)
        {
            { 1, "动态心电" },
            { 2, "动态血压" },
            { 3, "心电和血压" }
        };

        public static readonly PersistDictionary<string> Genders = new PersistDictionary<string>(true)
        {
            {"M", "男" },
            {"F", "女" },
            {"U", "未知" }
        };

        public static readonly PersistDictionary<int> PatientSource = new PersistDictionary<int>(true)
        {
            { 1, "门诊" },
            { 2, "住院" },
            { 3, "体检" },
            { 4, "其他" }
        };

        public static readonly PersistDictionary<string> AgeUnits = new PersistDictionary<string>(true)
        {
            { "Y", "岁" },
            { "M", "月" },
            { "D", "天" }
        };

        public static readonly PersistDictionary<int> RoleTypes = new PersistDictionary<int>(true)
        {
            { 1, "管理员" },
            { 2, "诊断医生" },
            { 3, "审核医生" },
            { 4, "护士" }
        };
      
        //俄语3-0结尾的岁、月、天
        public static readonly Dictionary<string, string> RusAgeUnits0 = new Dictionary<string, string>()
        {
            { "Y","лет"},
            { "M","месяцев" },
            { "D","днів"},
            { "U","Неизвестный"}
        };
        //俄语1结尾的岁、月、天
        public static readonly Dictionary<string, string> RusAgeUnits1 = new Dictionary<string, string>()
        {
            { "Y","год"},
            { "M","месяц" },
            { "D","день"},
            { "U","Неизвестный"}
        };
        //俄语2结尾的岁、月、天
        public static readonly Dictionary<string, string> RusAgeUnits2 = new Dictionary<string, string>()
        {
            { "Y","года"},
            { "M","месяца" },
            { "D","дні"},
            { "U","Неизвестный"}
        };
        //病例状态
        public static readonly PersistDictionary<int> CaseStatus = new PersistDictionary<int>(true)
        {
            { 1, "已登记" },
            { 2, "未分析" },
            { 3, "已诊断" },
            { 4, "已审核" }
        };

        //导联模式
        public static readonly PersistDictionary<EcgDeviceType> SampleDevices = new PersistDictionary<EcgDeviceType>(true)
        {
            { EcgDeviceType.Device_iMAC, "iMAC系列"}, //iMAC系列心电图机
            { EcgDeviceType.Device_bluetoothDaq, "蓝牙采集盒"}, //掌上心电采集仪
        };

        //导联模式
        public static PersistDictionary<int> LeadModes = new PersistDictionary<int>(true)
        {
            { 0, "6导"},
            { 4, "9导" },
            { 1, "12导"},
            { 2, "15导"},
            { 3, "18导"}
        };

        //导联模式与导联数关系映射
        public static readonly Dictionary<int, int> LeadModeMap = new Dictionary<int, int>()
        {
            { 0, 6 },
            { 1, 12 },
            { 2, 15 },
            { 3, 18 },
            { 4, 9 }
        };

        //导联体系
        public static readonly PersistDictionary<Tissues> LeadSystems = new PersistDictionary<Tissues>(true)
        {
            {Tissues.STANDARD, "常规"},
            {Tissues.PARIES_POSTERIOR, "后壁"},
            {Tissues.RIGHT_THORAX, "右胸"},
            {Tissues.LEFT_THORAX, "右胸后壁"},
            {Tissues.BETWEEN_RIB, "上一肋间"},
            {Tissues.NEXT_RIB, "下一肋间"},
            //{Tissues.FRANK, "FRANK"},
            {Tissues.CABRERA, "CABRERA"}
        };

        //心电波形分析类型
        public static readonly PersistDictionary<int> EcgTypes = new PersistDictionary<int>(true)
        {
            {0, "标准心电"},
            {1, "向量心电"},
            {2, "时间向量"},
            {3, "心室晚电位"},
            {4, "频谱心电"},
            {5, "QT离散度"},
            {6, "心率变异性"},
            {7, "节律分析"}
        };

        //种族
        public static readonly PersistDictionary<string> Races = new PersistDictionary<string>(true)
        {
            {"C", "白人"},
            {"N", "非洲"},
            {"M", "东方"},
            {"O", "其他"},
            {"U", "未知"}
        };

        //纸速
        public static readonly PersistDictionary<float> PaperSpeeds = new PersistDictionary<float>(true)
        {
            { 5F, "5mm/s"},
            { 6.25F, "6.25mm/s"},
            { 10F, "10mm/s"},
            { 12.5F, "12.5mm/s"},
            { 25F, "25mm/s"},
            { 50F, "50mm/s"}
        };

        //增益
        public static readonly PersistDictionary<float> Gains = new PersistDictionary<float>(true)
        {
            { 2.5F, "2.5mm/mV"},
            { 5F, "5mm/mV"},
            { 10F, "10mm/mV"},
            { 20F, "20mm/mV"},
            { 40F, "40mm/mV"}
        };
        public static readonly PersistDictionary<float> Gains_Plus = new PersistDictionary<float>(true)
        {
            { 1.25F, "1.25mm/mv"},
            { 2.5F, "2.5mm/mv"},
            { 5F, "5mm/mv"},
            { 10F, "10mm/mv"},
            { 20F, "20mm/mv"},
            { 40F, "40mm/mv"}
        };


        //心电波形布局
        public static readonly Dictionary<int, string> Layouts = new Dictionary<int, string>()
        {
            { 0, "3x4" },
            { 1, "3x4+1" },
            { 2, "3x4+3" },
            { 3, "6x2" },
            { 4, "6x2+1" },
            { 5, "12x1" },
            { 6, "6x3" },
            { 7, "6x3+1" },
            { 8, "9x2" },
            { 9, "9x2+1" },
            { 10, "5x3" },
            { 11, "5x3+1" },
            { 12, "6x1" },
            { 13, "9x1" },
            { 14, "6+3" },
            { 15, "3x3" },
            { 16, "3x3+1" }
        };

        //常规心电 6导, 12导, 15导, 18导相关的导联
        public static readonly Dictionary<int, string> ECG_LT_STANDARD = new Dictionary<int, string>()
        {
            { 0, "I,II,III,aVR,aVL,aVF" },
            { 1, "I,II,III,aVR,aVL,aVF,V1,V2,V3,V4,V5,V6" },
            { 2, "I,II,III,aVR,aVL,aVF,V1,V2,V3,V4,V5,V6,V3R,V4R,V5R" },
            { 3, "I,II,III,aVR,aVL,aVF,V1,V2,V3,V4,V5,V6,V7,V8,V9,V3R,V4R,V5R" },
            { 4, "I,II,III,aVR,aVL,aVF,V1,V3,V5" },
            { 5, "CH1,CH2,CH3" }
        };

        //12导的不同体系导联
        public static readonly Dictionary<Tissues, string> ECG_LT_SYSTEM = new Dictionary<Tissues, string>()
        {
            { Tissues.STANDARD, "I,II,III,aVR,aVL,aVF,V1,V2,V3,V4,V5,V6" },
            //{ LeadSystem.STANDARD, "I,II,III,aVR,aVL,aVF,V1,V2,V3,V4,V5,V6,V7,V8,V9,V3R,V4R,V5R" },
            { Tissues.PARIES_POSTERIOR, "I,II,III,aVR,aVL,aVF,V1,V2,V3,V7,V8,V9" },
            { Tissues.RIGHT_THORAX, "I,II,III,aVR,aVL,aVF,V1,V2,V3,V3R,V4R,V5R" },
            { Tissues.LEFT_THORAX, "I,II,III,aVR,aVL,aVF,V3R,V4R,V5R,V7,V8,V9" },
            { Tissues.BETWEEN_RIB, "I,II,III,aVR,aVL,aVF,V`1,V`2,V`3,V`4,V`5,V`6" },
            { Tissues.NEXT_RIB, "I,II,III,aVR,aVL,aVF,V.1,V.2,V.3,V.4,V.5,V.6" },
            { Tissues.FRANK, "I,II,III,aVR,aVL,aVF,I,E,C,A,M,H" },
            { Tissues.CABRERA, "aVL,I,-aVR,II,aVF,III,V1,V2,V3,V4,V5,V6" }
        };

        //工频
        public static readonly PersistDictionary<int> ECG_FREQUENCY = new PersistDictionary<int>(true)
        {
             { 0, "OFF" },
             { 50, "50Hz" },
             { 60, "60Hz" }
        };

        //基线漂移
        //public static readonly PersistDictionary<float> ECG_BASELINE = new PersistDictionary<float>(true)
        //{
        //     { 0, "OFF" },
        //     { 0.01F, "0.01Hz" },
        //     { 0.05F, "0.05Hz" },
        //     { 0.3F, "0.3Hz" },
        //     { 0.6F, "0.6Hz" }
        //};
        public static readonly PersistDictionary<int> ECG_BASELINE = new PersistDictionary<int>(true)
        {
             { 0, "OFF" },
             { 60, "ON" }
        };

        //肌电
        //public static readonly PersistDictionary<int> ECG_MYOELECTRICITY = new PersistDictionary<int>(true)
        //{
        //     { 0, "OFF" },
        //     { 25, "25Hz" },
        //     { 35, "35Hz" },
        //     { 45, "45Hz" },
        //     { 75, "75Hz" },
        //     { 100, "100Hz" },
        //     { 150, "150Hz" },
        //     { 250, "250Hz" },
        //     { 350, "350Hz" }
        //};
        public static readonly PersistDictionary<int> ECG_EMG = new PersistDictionary<int>(true)
        {
             { 0, "OFF" },
             { 45, "ON" }
        };

        //心电文件导出格式
        public static readonly PersistDictionary<EcgFormat> ECG_FORMAT = new PersistDictionary<EcgFormat>(true)
        {
            { EcgFormat.EcgFormat_PDF, "PDF" },
            { EcgFormat.EcgFormat_PNG, "PNG" },
            { EcgFormat.EcgFormat_JPG, "JPG" },
            { EcgFormat.EcgFormat_XML, "XML" },
            { EcgFormat.EcgFormat_HL7, "HL7" },
            { EcgFormat.EcgFormat_DICOM, "DICOM" },
            { EcgFormat.EcgFormat_PNG_W, "PNG-波形" },
            { EcgFormat.EcgFormat_JPG_W, "JPG-波形" },
            { EcgFormat.EcgFormat_INI, "INI" },
            { EcgFormat.EcgFormat_JSON, "JSON" },
            { EcgFormat.EcgFormat_XML_NW, "XML-无波形" }
        };

        public static readonly PersistDictionary<int> SAMPLE_DURATION = new PersistDictionary<int>(true)
        {
            { 10, "10s" },
            { 20, "20s" },
            { 30, "30s" },
            { 40, "40s" },
            { 50, "50s" },
            { 60, "1分钟" },
            { 120, "2分钟" },
            { 180, "3分钟" },
            { 240, "4分钟" },
            { 300, "5分钟" },
            { 600, "10分钟" },
            { 900, "15分钟" },
            { 1800, "30分钟" }
        };

        public static readonly PersistDictionary<int> ADV_SAMPLE_DURATION = new PersistDictionary<int>(true)
        {
            { 10, "10s" },
            { 30, "30s" },
            { 60, "1分钟" },
            { 120, "2分钟" },
            { 180, "3分钟" }
        };

        public static readonly PersistDictionary<int> OPEN_SWITCH = new PersistDictionary<int>(true)
        {
            { 0, "off" },
            { 1, "on" }
        };

        //iMac10数据传输途径(蓝牙/磁吸接口)
        public static readonly PersistDictionary<DataCommMode> DATA_COMM_MODE = new PersistDictionary<DataCommMode>(true)
        {
            { DataCommMode.BLE, "蓝牙接口" },
            { DataCommMode.WIRE, "磁吸接口" }
        };

        //iMac10起搏检测通道映射(常规12导序号与通道名称对应关系)
        public static readonly Dictionary<int, string> PACE_DETECT_MAP = new Dictionary<int, string>()
        {
            { 0, "C1" },
            { 1, "C2" },
            { 6, "C3" },
            { 7, "C4" },
            { 8, "C5" },
            { 9, "C6" },
            { 10, "C7" },
            { 11, "C8" }
        };

        //心电图机各型号(zqecg - ECG_NO_MACHINE_TYPE)对应关系
        public static readonly Dictionary<int, string> MACHINE_MODELS = new Dictionary<int, string>() {
            { 9, "ECGNT20" },
            { 10, "8111" },
            { 11, "8113/8114" },
        };

        //心电文件中采样率映射关系
        public static readonly Dictionary<int, int> SampleRates = new Dictionary<int, int>() {
            { 128, 125 },
            { 256, 250 },
            { 512, 500 },
            { 1024, 1000 }
        };

        /// <summary>
        /// 应用内采样率以及降采样映射关系
        /// </summary>
        public static readonly Dictionary<int, SampleMapping> SampleMaps = new Dictionary<int, SampleMapping>() {
            { 128, new SampleMapping{ mapping=125, target=125} },
            { 125, new SampleMapping{ mapping=125, target=125} },
            { 250, new SampleMapping{ mapping=250, target=125} },
            { 256, new SampleMapping{ mapping=250, target=125} },
            { 512, new SampleMapping{ mapping=500, target=125} },
            { 500, new SampleMapping{ mapping=500, target=125} },
            { 1000, new SampleMapping{ mapping=1000, target=125} },
            { 1024, new SampleMapping{ mapping=1000, target=125} }
        };

        //设置的日期显示格式对应的实际格式
        public static readonly Dictionary<int, string> DateFormats = new Dictionary<int, string>() {
            { 0, "yyyy-MM-dd" },
            { 1, "dd-MM-yyyy" }
        };

        public static JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            DateFormatString = "yyyy-MM-dd HH:mm:ss",
            Formatting = Formatting.Indented
        };
    }

    public class CommonValues
    {
        /// <summary>
        /// 软件运行状态(true: 正常 / false: 失败)
        /// </summary>
        public static bool SoftRunStatus = true;

        /// <summary>
        /// 加密狗状态(-1：未找到加密狗 / 0：加密狗连接正常 / 1：加密狗通信异常 / 2：加密狗通信正常)
        /// </summary>
        public static int SoftDogStatus = -1;

        /// <summary>
        /// 是否已初始化加密狗(启动检测并绑定事件)
        /// </summary>
        public static bool SoftDogInitialized = false;

        /// <summary>
        /// 界面显示的宽度比例
        /// </summary>
        public static float WidthScale = 1;

        /// <summary>
        /// 界面显示的高度比例
        /// </summary>
        public static float HeightScale = 1;

        /// <summary>
        /// 系统类型(ecg:心电; bp:血压; ecg-bp:二合一)
        /// </summary>
        public static string SystemType = "ecg";

        /// <summary>
        /// 本医院编码
        /// </summary>
        public static string HospitalCode = "";

        /// <summary>
        /// 从网络接收病例分析请求的服务地址
        /// </summary>
        public static string WebCaseSource = "";

        /// <summary>
        /// 当前操作系统GPU是否可用
        /// </summary>
        public static bool IsGpuAvailable = true;

        /// <summary>
        /// 日志文件的存储路径
        /// </summary>
        public static string LogPath = string.Empty;

        /// <summary>
        /// 程序访问的站点基地址
        /// </summary>
        public static string BaseURL = string.Empty;

        /// <summary>
        /// 当前语言区域标识代码
        /// </summary>
        public static int LanguageID = 2052;

        /// <summary>
        /// 串口通讯将使用的波特率(iMac10)
        /// </summary>
        public static int CommBaudRate = 921600;

        /// <summary>
        /// 蓝牙串口使用过的波特率
        /// </summary>
        public static Dictionary<string, int> BluetoothBaudRates = new Dictionary<string, int>();

        /// <summary>
        /// 系统内心电相关设置
        /// </summary>
        public static EcgSettings EcgSettings = new EcgSettings();

        /// <summary>
        /// 系统设置
        /// </summary>
        public static systemSettingValues SystemSettings = new systemSettingValues();

        /// <summary>
        /// 本系统名称
        /// </summary>
        public static string SystemName;

        /// <summary>
        /// 波形数据
        /// </summary>
        public static List<List<float>> EcgWaveData = null;

        /// <summary>
        /// iMAC心电图机的主机地址
        /// </summary>
        public static string ZoncareImacIP;

        /// <summary>
        /// 蓝牙掌上采集仪的通讯串口
        /// </summary>
        public static string ZoncareBluetoothCommPort;

        /// <summary>
        /// 对应于当前系统语言的词条集
        /// </summary>
        public static PersistDictionary<string> Langs = new PersistDictionary<string>();

        /// <summary>
        /// 与各种语言相关的报告参数设置信息
        /// </summary>
        public static Dictionary<int, LanguageSettings> LangsSettings = new Dictionary<int, LanguageSettings>();

        /// <summary>
        /// 用于后台高级功能分析进程通讯消息同步
        /// </summary>
        public static Mutex CrossProcessMutex;

        /// <summary>
        /// 主线程的同步上下文对象
        /// </summary>
        public static SynchronizationContext MainTreadContext;

        /// <summary>
        /// 保存已启动的控制台进程,key:进程的ID,value:进程
        /// </summary>
        public static Dictionary<int, Process> BackProcesses = new Dictionary<int, Process>();

        /// <summary>
        /// 保存控制台返回的消息,key:guid,value:返回的消息
        /// </summary>
        public static Dictionary<string, string> ProcessMessages = new Dictionary<string, string>();


        #region 系统功能控制变量
        /// <summary>
        /// 是否检查上传的文件的CRC
        /// </summary>
        public static bool VerifyFileCRC = true;

        /// <summary>
        /// 是否降采样
        /// </summary>
        public static bool ReduceSampleAble = false;

        /// <summary>
        /// 采集心电数据时是否需要检查数据丢包情况
        /// </summary>
        public static bool IsCheckPacketLoss = false;

        /// <summary>
        /// 是否在自动诊断时输出诊断参数
        /// </summary>
        public static bool IsOutputDiagParameters = true;

        /// <summary>
        /// 是否在自动诊断时输出明尼苏达码
        /// </summary>
        public static bool IsOutputMinnesotaCode = true;

        /// <summary>
        /// 是否在自动诊断时输出诊断结论
        /// </summary>
        public static bool IsOutputDiagResult = true;


        /// <summary>
        /// 采集心电数据完成时是否提示可能的导联反接
        /// </summary>
        public static bool IsPromptLeadReversed = false;

        /// <summary>
        /// 采集过程超过10秒后停止采集是否保留波形数据
        /// </summary>
        public static bool IsReserveDataWhenStop = false;

        /// <summary>
        /// 进入采集界面后是否显示滤波延迟引起的直线
        /// </summary>
        public static bool IsReserveFilterLine = true;

        /// <summary>
        /// 是否支持磁吸接口通讯(针对iMac10)
        /// </summary>
        public static bool IsMagneticInterfaceEnabled = true;


        /// <summary>
        /// 登录界面是否展示历史登录用户名
        /// </summary>
        public static bool IsRememberLoginAccount = true;

        /// <summary>
        /// 打印报告前是否预览(true 预览，false不预览）
        /// </summary>
        public static bool IsPreviewBeforePrint = false;

        /// <summary>
        /// 打印前是否显示打印机参数(true显示，false不显示)
        /// </summary>
        public static bool IsShowParamBeforePrint = false;

        /// <summary>
        /// 是否记录运行日志
        /// </summary>
        public static bool IsSaveRuntimeLog = true;

        /// <summary>
        /// 是否保存操作日志(true 显示，false不显示)
        /// </summary>
        public static bool IsSaveOperateLog = true;

        /// <summary>
        /// 删除报告时是否删除文件
        /// </summary>
        public static bool IsDeleteFile = false;

        /// <summary>
        /// 采集完成后是否自动导出
        /// </summary>
        public static bool IsAutoExport = false;

        /// <summary>
        /// 是否启用心电文件上传功能
        /// </summary>
        public static bool IsUploadEnabled = false;

        /// <summary>
        /// 是否启用报告显示模式切换功能(同步/顺序)
        /// </summary>
        public static bool IsDisplayModeEnabled = false;

        /// <summary>
        /// 导出心电报告时是否导出所有采集的心电波形(仅支持标准心电)
        /// </summary>
        public static bool IsExportAllWaves = false; //2023.10 jinj

        /// <summary>
        /// 标准心电报告中签名的位置(0:右上方; 1:右下方)
        /// </summary>
        public static int SignaturePosition = 1; //2023.10 jinj

        /// <summary>
        /// 进入系统的验证方式(0:加密狗验证; 1:注册码验证)
        /// </summary>
        public static int VerificationMode = 0;

        /// <summary>
        /// 报告波形背景网格线样式(0:网格点; 1:网格线)
        /// </summary>
        public static int ReportGridStyle = 0;

        /// <summary>
        /// 导出时是否将报告保存到指定的HIS相关文件目录
        /// </summary>
        public static bool IsSaveToHIS = false;

        /// <summary>
        /// 是否允许修改PID
        /// </summary>
        public static bool IsPidChangeable = false;

        /// <summary>
        /// 报告网格颜色(1mm)
        /// </summary>
        public static Color ReportGridColor1 = Color.FromArgb(180, 60, 60);
        /// <summary>
        /// 报告网格颜色(5mm)
        /// </summary>
        public static Color ReportGridColor5 = Color.FromArgb(240, 120, 120);
        /// <summary>
        /// 报告背景颜色
        /// </summary>
        public static Color ReportBackgroundColor = Color.White;
        /// <summary>
        /// 报告中心电波形颜色
        /// </summary>
        public static Color ReportWaveColor = Color.Black;

        /// <summary>
        /// 机构LOGO文件名称
        /// </summary>
        public static string OrganizationLogo = string.Empty;

        /// <summary>
        /// 主界面点击刷新按钮时将进行心电数据文件检索的目录
        /// </summary>
        public static string DirectoryToRefresh = string.Empty;

        /// <summary>
        /// 是否自动刷新病例数据列表
        /// </summary>
        public static bool IsAutoRefresh = false;

        /// <summary>
        /// 是否支持9导采集模式
        /// </summary>
        public static bool SupportLeads9 = false;

        /// <summary>
        /// 心电文件上传的远端地址(中网)
        /// </summary>
        public static string UploadServerAddress = string.Empty;

        /// <summary>
        /// 与第三方对接的服务地址
        /// </summary>
        public static string ThirdpartyAddress = string.Empty;

        /// <summary>
        /// 心电报告地区互认的缩写文字(e.g. 黑HR/牡HR)
        /// </summary>
        public static string RegionalRecognitionAbbr = string.Empty;

        /// <summary>
        /// 心电报告地区互认的相关描述
        /// </summary>
        public static string RegionalRecognition = string.Empty;

        /// <summary>
        /// 值为空时字符串表示文本
        /// </summary>
        public static string DefaultEmptyValueText = "";

        /// <summary>
        /// 起搏信号及其标识增强的幅度值(mm)
        /// </summary>
        public static float PaceBoostHeight = 2.0F;

        /// <summary>
        /// 高级功能设置(true:已开通; false:未开通)
        /// </summary>
        public static Dictionary<string, bool> AdvancedFeatureSettings = new Dictionary<string, bool>()
        {
            {"VCG",false },//向量心电
            {"FCG",false },//频谱心电
            {"TVCG",false },//时间向量心电
            {"HRV",false },//心率变异性
            {"VLP",false },//心室晚电位
            {"QTD",false },//QT离散度
            {"RhythmAnalysis",false },//节律分析
        };

        #endregion

    }

}
