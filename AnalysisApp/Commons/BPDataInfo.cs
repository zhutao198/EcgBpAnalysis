using System;

namespace AnalysisApp.Commons
{
    public class BPHourListInfo
    {
        public string Date { get; set; } = "";          //日期
        public string Time { get; set; } = "";          //时间
        public double SBP { get; set; } = 0;               //收缩压
        public double DBP { get; set; } = 0;               //舒张压
        public double MAP { get; set; } = 0;               //平均动脉压
        public int Pulse { get; set; } = 0;             //舒张压
        public int Count { get; set; } = 0;             //每小时记录个数
    }

    public class BPDataInfo
    {
        public bool IsSelected { get; set; } = false;               //是否选中
        public int Id { get; set; } = 0;                            //id号
        public DateTime bpDateTime { get; set; } = DateTime.Now;    //日期时间
        public string Date { get; set; } = "";                      //日期
        public string Time { get; set; } = "";                      //时间
        public double SBP { get; set; } = 0;                        //收缩压
        public double DBP { get; set; } = 0;                        //舒张压
        public double MAP { get; set; } = 0;                        //平均动脉压
        public double RPP { get; set; } = 0;                        //RPP
        public int Pulse { get; set; } = 0;                         //脉率
        public int DataStatus { get; set; } = 0;                    //数据状态
        public string MotionState { get; set; } = "";               //运动状态
        public int Status { get; set; } = 0;                        //状态
        public string ErrorCode { get; set; } = "";                 //错误代码
        public string Notes { get; set; } = "";                     //备注
        public int MeasureType { get; set; } = 0;                   //测量类型
        public bool bDay { get; set; } = false;                     //是否白天
    }

    public class BPDetailData
    {
        public string StartTime { get; set; } = "";         //开始时间
        public string EndTime { get; set; } = "";           //结束时间
        public int Count { get; set; } = 0;                 //记录个数
        public int EffectiveCount { get; set; } = 0;        //有效记录个数
        public double EffectiveCountRate { get; set; } = 0; //有效记录个数占比
        public int ErrorNum { get; set; } = 0;              //错误记录个数
        public double ErrorNumPercent { get; set; } = 0;    //错误记录个数占比        
        public double AASI { get; set; } = 0;               //AASI
        public double Correl { get; set; } = 0;             //相关性
        public string RelationShip { get; set; } = "";      //关系函数
        public string ZyjlType { get; set; } = "";          //昼夜节律类型
        public string MorningSbpAvg { get; set; } = "";         //晨间收缩压平均值
        public string MorningDbpAvg { get; set; } = "";         //晨间舒张压平均值
        public string MorningPre { get; set; } = "";            //晨峰血压
        public string TotalTime { get; set; } = "";            //总监测时间

        public int SbpGtLimit { get; set; } = 0;            //大于收缩压临界值个数
        public double SbpLoadNum { get; set; } = 0;         //大于收缩压临界值个数占比
        public double SbpMedian { get; set; } = 0;             //收缩压中值
        public double SbpPercentage { get; set; } = 0;      //收缩压百分比 = (白天收缩压平均值-夜间收缩压平均值) / 白天收缩压平均值;
        public string SbpMin { get; set; } = "";                //收缩压最小值
        public string SbpMinDbp { get; set; } = "";             //收缩压最小值对应的舒张压
        public string SbpMinTime { get; set; } = "";        //收缩压最小值发生时间
        public string SbpAvg { get; set; } = "";                //收缩压平均值
        public string SbpMax { get; set; } = "";                //收缩压最大值
        public string SbpMaxDbp { get; set; } = "";             //收缩压最大值对应的舒张压
        public string SbpMaxTime { get; set; } = "";        //收缩压最大值发生时间
        public string SbpBzc { get; set; } = "";             //收缩压标准差
        public string SbpByxs { get; set; } = "";            //收缩压变异系数

        public int DbpGtLimit { get; set; } = 0;            //大于舒张压临界值个数
        public double DbpLoadNum { get; set; } = 0;         //大于舒张压临界值占比
        public double DbpMedian { get; set; } = 0;             //舒张压中值
        public double DbpPercentage { get; set; } = 0;      //舒张压百分比 = (白天舒张压平均值-夜间舒张压平均值) / 白天舒张压平均值;
        public string DbpMin { get; set; } = "";                //舒张压最小值
        public string DbpMinTime { get; set; } = "";        //舒张压最小值发生时间
        public string DbpAvg { get; set; } = "";                //舒张压平均值
        public string DbpMax { get; set; } = "";                //舒张压最大值
        public string DbpMaxTime { get; set; } = "";        //舒张压最大值发生时间
        public string DbpBzc { get; set; } = "";             //舒张压标准差
        public string DbpByxs { get; set; } = "";            //舒张压变异系数

        public string MapMin { get; set; } = "";             //平均动脉压最小值
        public string MapAvg { get; set; } = "";             //平均动脉压平均值
        public string MapMax { get; set; } = "";             //平均动脉压最大值
        public string MapBzc { get; set; } = "";             //平均动脉压标准差
        public string MapByxs { get; set; } = "";            //平均动脉压变异系数

        public int PulseMedian { get; set; } = 0;           //脉搏中值
        public int PulseMin { get; set; } = 0;              //脉搏最小值
        public string PulseMinTime { get; set; } = "";      //脉搏最小值发生时间
        public string PulseAvg { get; set; } = "";              //脉搏平均值
        public int PulseMax { get; set; } = 0;              //脉搏最大值
        public string PulseMaxTime { get; set; } = "";      //脉搏最大值发生时间
        public string PulseBzc { get; set; } = "";           //脉搏标准差
        public string PulseByxs { get; set; } = "";          //脉搏变异系数

    }

    public class BPSummaryData
    {
        public BPDetailData bpDayData = new BPDetailData();
        public BPDetailData bpNightData = new BPDetailData();
        public BPDetailData bpAllData = new BPDetailData();
    }

    public class BPCompareInfo
    {
        public string Type { get; set; } = "";
        public string All { get; set; } = "";
        public string Day { get; set; } = "";
        public string Night { get; set; } = "";
    }

    /// <summary>
    /// 脉搏波(血压波形)数据
    /// </summary>
    public class PulseWaveData
    {
        /// <summary>
        /// 原始信号dc值,单位为压力值
        /// </summary>
        public double[] dc { get; set; }

        /// <summary>
        /// 原始信号ac值,单位为幅度
        /// </summary>
        public double[] ac { get; set; }

        /// <summary>
        /// 台阶dc值数组(每个台阶的压力值)
        /// </summary>
        public double[] stepDc { get; set; }

        /// <summary>
        /// 台阶dc值数组长度
        /// </summary>
        public int[] stepDcLen { get; set; }

        /// <summary>
        /// 台阶放气各区间开始和结束位置
        /// </summary>
        public GroupInterval[] stepGroups { get; set; }

        /// <summary>
        /// 信号长度/个数
        /// </summary>
        public int len { get; set; }

        public PulseWaveData(double[] arrDC, double[] arrAC, double[] arrStep, int[] arrStepLen, int dataLen)
        {
            dc = arrDC;
            ac = arrAC;
            stepDc = arrStep;
            stepDcLen = arrStepLen;
            len = dataLen;
        }
    }

    public static class BPTrendFlag
    {
        public static bool bBp { get; set; } = true;
        public static bool bPulse { get; set; } = true;
        public static bool bAvgBp { get; set; } = true;
        public static bool bRpp { get; set; } = true;
        public static bool bError { get; set; } = true;
    }
}
