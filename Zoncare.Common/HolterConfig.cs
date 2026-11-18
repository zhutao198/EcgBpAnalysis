using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Zoncare.Common
{
    /// <summary>
    /// Holter配置信息类
    /// </summary>
    public class HolterConfig
    {
        [JsonProperty("fileType")]
        public int FileType { get; set; }

        [JsonProperty("fileList")]
        public List<string> FileList { get; set; } = new List<string>();

        [JsonProperty("fileListRange")]
        public List<List<long>> FileListRange { get; set; } = new List<List<long>>();

        [JsonProperty("startTime")]
        public string StartTime { get; set; } = string.Empty;

        [JsonProperty("days")]
        public int Days { get; set; }

        [JsonProperty("sampleRate")]
        public int SampleRate { get; set; }

        [JsonProperty("sampleBits")]
        public int? SampleBits { get; set; }

        [JsonProperty("lead")]
        public int Lead { get; set; }

        [JsonProperty("adScale")]
        public double? AdScale { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("analysisSetting")]
        public AnalysisSetting AnalysisSetting { get; set; } = new AnalysisSetting();

        [JsonProperty("paramsSetting")]
        public ParamsSetting ParamsSetting { get; set; } = new ParamsSetting();

        [JsonProperty("leadConfig")]
        public LeadConfig LeadConfig { get; set; } = new LeadConfig();

        [JsonProperty("stSetting")]
        public StSetting StSetting { get; set; } = new StSetting();

        [JsonProperty("stOperationSetting")]
        public Dictionary<string, List<object>> StOperationSetting { get; set; } = new Dictionary<string, List<object>>();

        [JsonProperty("waveConfig")]
        public WaveConfig WaveConfig { get; set; } = new WaveConfig();

        [JsonProperty("fileConversion")]
        public bool FileConversion { get; set; }

        [JsonProperty("fileConversionTime")]
        public string FileConversionTime { get; set; } = string.Empty;

        [JsonProperty("priorAnalysis")]
        public bool PriorAnalysis { get; set; }

        [JsonProperty("priorAnalysisTime")]
        public string PriorAnalysisTime { get; set; } = string.Empty;

        [JsonProperty("startReanalysis")]
        public bool StartReanalysis { get; set; }

        [JsonProperty("startReanalysisTime")]
        public string StartReanalysisTime { get; set; } = string.Empty;

        [JsonProperty("startReanalysisFinishTime")]
        public string StartReanalysisFinishTime { get; set; } = string.Empty;

        [JsonProperty("analysisHrv")]
        public bool AnalysisHrv { get; set; }

        [JsonProperty("analysisHrvTime")]
        public string AnalysisHrvTime { get; set; } = string.Empty;

        [JsonProperty("channel")]
        public int Channel { get; set; }

        [JsonProperty("hw")]
        public HardwareConfig Hw { get; set; } = new HardwareConfig();

        /// <summary>
        /// 从文件加载配置信息
        /// </summary>
        /// <param name="filePath">配置文件路径</param>
        /// <returns>Holter配置对象</returns>
        public static HolterConfig LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"配置文件不存在: {filePath}");
            }

            string jsonContent = File.ReadAllText(filePath);

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            return JsonConvert.DeserializeObject<HolterConfig>(jsonContent, settings) ??
                   throw new InvalidOperationException("配置文件格式错误");
        }

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        /// <param name="filePath">保存路径</param>
        public void SaveToFile(string filePath)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };

            string jsonContent = JsonConvert.SerializeObject(this, settings);
            File.WriteAllText(filePath, jsonContent);
        }

        /// <summary>
        /// 获取主要导联配置
        /// </summary>
        [JsonIgnore]
        public int MainChannel => AnalysisSetting?.MainChannel ?? 1;

        /// <summary>
        /// 获取次要导联1配置
        /// </summary>
        [JsonIgnore]
        public int SecondChannel1 => AnalysisSetting?.SecondChannel1 ?? 6;

        /// <summary>
        /// 获取次要导联2配置
        /// </summary>
        [JsonIgnore]
        public int SecondChannel2 => AnalysisSetting?.SecondChannel2 ?? 10;

        /// <summary>
        /// 获取总记录时长（秒）
        /// </summary>
        [JsonIgnore]
        public int TotalSeconds => (int)((double)Total / SampleRate);

        /// <summary>
        /// 获取记录开始时间的DateTime对象
        /// </summary>
        [JsonIgnore]
        public DateTime StartDateTime => DateTime.TryParse(StartTime, out var dt) ? dt : DateTime.Today;
    }

    /// <summary>
    /// 分析设置
    /// </summary>
    public class AnalysisSetting
    {
        [JsonProperty("mainChannel")]
        public int MainChannel { get; set; } = 1;

        [JsonProperty("secondChannel1")]
        public int SecondChannel1 { get; set; } = 6;

        [JsonProperty("secondChannel2")]
        public int SecondChannel2 { get; set; } = 10;

        [JsonProperty("qrsSimilarityThreshold")]
        public int QrsSimilarityThreshold { get; set; } = 80;

        [JsonProperty("qrsRefractoryPeriod")]
        public int QrsRefractoryPeriod { get; set; } = 280;

        [JsonProperty("stOffset")]
        public int StOffset { get; set; } = 60;

        [JsonProperty("jOffset")]
        public int JOffset { get; set; } = 20;

        [JsonProperty("baseOffset")]
        public int BaseOffset { get; set; } = 40;

        [JsonProperty("frequency")]
        public int Frequency { get; set; }

        [JsonProperty("baseline")]
        public int Baseline { get; set; } = 60;

        [JsonProperty("myoelectric")]
        public int Myoelectric { get; set; }

        [JsonProperty("paceAnalysisEnable")]
        public bool PaceAnalysisEnable { get; set; }

        [JsonProperty("paceEventEnable")]
        public bool PaceEventEnable { get; set; }

        [JsonProperty("paceAnalysisType")]
        public string PaceAnalysisType { get; set; } = "DDD";

        [JsonProperty("paceMethod")]
        public int PaceMethod { get; set; }

        [JsonProperty("pace2beat")]
        public int Pace2beat { get; set; } = 91;

        [JsonProperty("beat2pace")]
        public int Beat2pace { get; set; } = 790;

        //是否启用逸搏分析
        [JsonProperty("EbAnalysisEnable")]
        public bool EbAnalysisEnable { get; set; } = false;

        [JsonProperty("checkHrUpperLimit")]
        public int CheckHrUpperLimit { get; set; } = 120;

        [JsonProperty("checkHrLowerLimit")]
        public int CheckHrLowerLimit { get; set; } = 63;

        [JsonProperty("sensitivity")]
        public int Sensitivity { get; set; }

        [JsonProperty("disturbDelEnable")]
        public bool DisturbDelEnable { get; set; }

        [JsonProperty("multichannelEnable")]
        public bool MultichannelEnable { get; set; } = true;

        [JsonProperty("startSample")]
        public long StartSample { get; set; }

        [JsonProperty("endSample")]
        public long EndSample { get; set; }

        [JsonProperty("invalidInterval")]
        public List<object> InvalidInterval { get; set; } = new List<object>();
    }

    /// <summary>
    /// 参数设置
    /// </summary>
    public class ParamsSetting
    {
        [JsonProperty("p6")]
        public TachycardiaSetting P6 { get; set; } = new TachycardiaSetting();

        [JsonProperty("p7")]
        public TachycardiaSetting P7 { get; set; } = new TachycardiaSetting();

        [JsonProperty("p8")]
        public LongRRSetting P8 { get; set; } = new LongRRSetting();

        [JsonProperty("p14")]
        public HeartRateSetting P14 { get; set; } = new HeartRateSetting();

        [JsonProperty("p15")]
        public STSettingConfig P15 { get; set; } = new STSettingConfig();

        [JsonProperty("p16")]
        public PaceSetting P16 { get; set; } = new PaceSetting();

        [JsonProperty("p17")]
        public HeartRateVariabilitySetting P17 { get; set; } = new HeartRateVariabilitySetting();

        [JsonProperty("p18")]
        public AsystoleSetting P18 { get; set; } = new AsystoleSetting();

        [JsonProperty("p19")]
        public PrematureAtrialSetting P19 { get; set; } = new PrematureAtrialSetting();

        [JsonProperty("p20")]
        public BigeminySetting P20 { get; set; } = new BigeminySetting();

        [JsonProperty("p21")]
        public STStandardSetting P21 { get; set; } = new STStandardSetting();

        [JsonProperty("sspeed")]
        public AtrialTachycardiaSetting Sspeed { get; set; } = new AtrialTachycardiaSetting();

        [JsonProperty("vront")]
        public RonTSetting Vront { get; set; } = new RonTSetting();

        [JsonProperty("af")]
        public AtrialFibrillationSetting Af { get; set; } = new AtrialFibrillationSetting();

        [JsonProperty("eb")]
        public EscapeBeatSetting Eb { get; set; } = new EscapeBeatSetting();

        [JsonProperty("twa")]
        public TWASetting Twa { get; set; } = new TWASetting();
    }

    /// <summary>
    /// 心动过缓设置
    /// </summary>
    public class TachycardiaSetting
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("hr1")]
        public int Hr1 { get; set; }

        [JsonProperty("frequency")]
        public int Frequency { get; set; }
    }

    /// <summary>
    /// 长RR间期设置
    /// </summary>
    public class LongRRSetting
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("rr")]
        public int Rr { get; set; }

        [JsonProperty("includePrematureBeat")]
        public int IncludePrematureBeat { get; set; }
    }

    /// <summary>
    /// 心率设置
    /// </summary>
    public class HeartRateSetting
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("frequency")]
        public int Frequency { get; set; }

        [JsonProperty("AllowEctopicBeat")]
        public int AllowEctopicBeat { get; set; }
    }

    /// <summary>
    /// ST段设置
    /// </summary>
    public class STSettingConfig
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("low")]
        public double Low { get; set; }

        [JsonProperty("low_duration")]
        public int LowDuration { get; set; }

        [JsonProperty("high")]
        public double High { get; set; }

        [JsonProperty("high_duration")]
        public int HighDuration { get; set; }

        [JsonProperty("stMode")]
        public int StMode { get; set; }
    }

    /// <summary>
    /// 起搏设置
    /// </summary>
    public class PaceSetting
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("sense_failed")]
        public int SenseFailed { get; set; }

        [JsonProperty("taking_failed")]
        public int TakingFailed { get; set; }

        [JsonProperty("output_failed")]
        public int OutputFailed { get; set; }
    }

    /// <summary>
    /// 心率变异性设置
    /// </summary>
    public class HeartRateVariabilitySetting
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("step")]
        public int Step { get; set; }

        [JsonProperty("high1")]
        public double High1 { get; set; }

        [JsonProperty("high2")]
        public double High2 { get; set; }

        [JsonProperty("low1")]
        public double Low1 { get; set; }

        [JsonProperty("low2")]
        public double Low2 { get; set; }

        [JsonProperty("very_low1")]
        public double VeryLow1 { get; set; }

        [JsonProperty("very_low2")]
        public double VeryLow2 { get; set; }
    }

    /// <summary>
    /// 停搏设置
    /// </summary>
    public class AsystoleSetting
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("rr")]
        public int Rr { get; set; }
    }

    /// <summary>
    /// 房早设置
    /// </summary>
    public class PrematureAtrialSetting
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("before")]
        public int Before { get; set; }
    }

    /// <summary>
    /// 二联三联律设置
    /// </summary>
    public class BigeminySetting
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("refValue")]
        public int RefValue { get; set; }
    }

    /// <summary>
    /// 12导ST异常标准设置
    /// </summary>
    public class STStandardSetting
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("standardValue")]
        public List<STStandardValue> StandardValue { get; set; } = new List<STStandardValue>();
    }

    /// <summary>
    /// ST标准值
    /// </summary>
    public class STStandardValue
    {
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("fall")]
        public double Fall { get; set; }

        [JsonProperty("up")]
        public double Up { get; set; }
    }

    /// <summary>
    /// 房速设置
    /// </summary>
    public class AtrialTachycardiaSetting
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("hr")]
        public int Hr { get; set; }
    }

    /// <summary>
    /// 室早RonT设置
    /// </summary>
    public class RonTSetting
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("rr")]
        public int Rr { get; set; }
    }

    /// <summary>
    /// 房颤设置
    /// </summary>
    public class AtrialFibrillationSetting
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("min_duration")]
        public int MinDuration { get; set; }

        [JsonProperty("min_avg_hr")]
        public int MinAvgHr { get; set; }
    }

    /// <summary>
    /// 逸搏设置
    /// </summary>
    public class EscapeBeatSetting
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("spercent")]
        public int Spercent { get; set; }

        [JsonProperty("vpercent")]
        public int Vpercent { get; set; }
    }

    /// <summary>
    /// TWA分析设置
    /// </summary>
    public class TWASetting
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("min")]
        public double Min { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }
    }

    /// <summary>
    /// 导联配置
    /// </summary>
    public class LeadConfig
    {
        [JsonProperty("chestOrg")]
        public List<string> ChestOrg { get; set; } = new List<string>();

        [JsonProperty("chestLatest")]
        public List<string> ChestLatest { get; set; } = new List<string>();

        [JsonProperty("lfLatest")]
        public List<string> LfLatest { get; set; } = new List<string>();

        [JsonProperty("lfOrg")]
        public List<string> LfOrg { get; set; } = new List<string>();
    }

    /// <summary>
    /// ST设置
    /// </summary>
    public class StSetting
    {
        [JsonProperty("showGrid")]
        public bool ShowGrid { get; set; } = true;

        [JsonProperty("showThreshold")]
        public bool ShowThreshold { get; set; }
    }

    /// <summary>
    /// 波形配置
    /// </summary>
    public class WaveConfig
    {
        [JsonProperty("overview")]
        public List<int> Overview { get; set; } = new List<int>();

        [JsonProperty("basicview")]
        public List<int> Basicview { get; set; } = new List<int>();
    }

    /// <summary>
    /// 硬件配置
    /// </summary>
    public class HardwareConfig
    {
        [JsonProperty("line")]
        public int Line { get; set; }

        [JsonProperty("lead")]
        public int Lead { get; set; }
    }
}