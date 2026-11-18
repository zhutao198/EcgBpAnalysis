namespace Zoncare.Common
{
    public class systemSettingValues
    {
        public baseSettingValues baseSets = new baseSettingValues();
        public ecgSettingValues ecgSets = new ecgSettingValues();
        public bpSettings bpSets = new bpSettings();
        public reportSettingValues reportSets = new reportSettingValues();
    }

    public class baseSettingValues
    {
        public string holterDataPath = "C:\\ZoncareData";
        public int language = 2052;
        public int dateFormat = 0;
        public string hospitalName = "";
        public string hospitalPhone = "";
        public string hospitalAddress = "";
        public string hospitalLogo = "";
    }

    public class ecgSettingValues
    {
        #region 滤波设置
        public filterSettingValues filterSets = new filterSettingValues();
        #endregion

        #region 常规设置
        public customSettingValues customSets = new customSettingValues();
        #endregion

        #region 高级设置
        public advanceSettingValues advanceSets = new advanceSettingValues();
        #endregion

        #region 12导联ST异常标准
        public STUpFallValues stUpFallSets = new STUpFallValues();
        #endregion
    }

    public class filterSettingValues
    {
        public int frequency = (CommonValues.SystemType == "ecg-bp" ? 0 : 50); //工频
        public int baseline = 60;                   //基线漂移
        public int myoelectric = 0;                 //肌电
    }

    public class customSettingValues
    {
        public int tachycardia = 120;              //心动过速(bpm)
        public int tachycardiaCalc = 4;            //心动过速计算方法心搏数(个)
        public int bradycardia = 50;               //心动过缓(bpm)
        public int bradycardiaCalc = 4;            //心动过缓计算方法心搏数
        public int longRR = 1500;                  //长RR间期(ms)
        public int includePremature = 0;           //是否包含早搏后代偿间期
        public int fastestSlowestHr = 4;           //最快/最慢心率(个)
        public int allowEctopicBeat = 1;           //是否允许异位心搏参与平均心律计算
        public int PAC = 25;                       //房早提前量(%)
        public int asystoleTime = 2000;            //停搏时间(ms)
        public int ATHr = 100;                     //房速心率(bpm)
        public int RonT = 370;                     //室早RonT(ms)
        public int minDuration = 30;               //最短持续时间(s)
        public int minAvgHr = 50;                  //最小平均心律(bpm)
        public int atrialJunctional = 50;          //房性&交界性(%)
        public int ventricularPrecent = 10;        //室性(%)
    }

    public class advanceSettingValues
    {
        public int STMode = 0;                    //ST模式
        public double STLower = 0.1;               //ST压低幅值(mV)
        public int STLowerDuration = 60;           //ST压低持续时间(s)
        public double STUpper = 0.3;               //ST提高幅值(mV)
        public int STUpperDuration = 60;           //ST提高持续时间(s)
        public int senseFailed = 200;              //感知失败(ms)
        public int outputFailed = 1000;            //输出失败(ms)
        public int takingFailed = 350;             //夺获失败(ms)
        public int analysisStep = 5;               //分析步长(min)
        public double highFrequency1 = 0.15;       //高频范围1(Hz)
        public double highFrequency2 = 0.4;        //高频范围2(Hz)
        public double lowFrequency1 = 0.04;        //低频范围1(Hz)
        public double lowFrequency2 = 0.15;        //低频范围2(Hz)
        public double lowestFrequency1 = 0.0033;   //极低频范围1(Hz)
        public double lowestFrequency2 = 0.04;     //极低频范围2(Hz)
        public double twaMinAvg = 7.6;             //TWA最小平均交替幅值(uV)
        public int twaEventDuration = 60;          //事件持续时间(s)
    }

    public class STUpFallValues
    {
        public double ILeadFall = -0.1;            //I导联下降(mV)
        public double ILeadUp = 0.1;              //I导联上升(mV)
        public double IILeadFall = -0.1;           //II导联下降(mV)
        public double IILeadUp = 0.1;             //II导联上升(mV)
        public double IIILeadFall = -0.1;          //III导联下降(mV)
        public double IIILeadUp = 0.1;            //III导联上升(mV)
        public double aVRLeadFall = -0.1;          //aVR导联下降(mV)
        public double aVRLeadUp = 0.1;            //aVR导联上升(mV)
        public double aVLLeadFall = -0.1;          //aVL导联下降(mV)
        public double aVLLeadUp = 0.1;            //aVL导联上升(mV)
        public double aVFLeadFall = -0.1;          //aVF导联下降(mV)
        public double aVFLeadUp = 0.1;            //aVF导联上升(mV)
        public double V1LeadFall = -0.1;           //V1导联下降(mV)
        public double V1LeadUp = 0.3;             //V1导联上升(mV)
        public double V2LeadFall = -0.1;           //V2导联下降(mV)
        public double V2LeadUp = 0.3;             //V2导联上升(mV)
        public double V3LeadFall = -0.1;           //V3导联下降(mV)
        public double V3LeadUp = 0.3;             //V3导联上升(mV)
        public double V4LeadFall = -0.1;           //V4导联下降(mV)
        public double V4LeadUp = 0.1;             //V4导联上升(mV)
        public double V5LeadFall = -0.1;           //V5导联下降(mV)
        public double V5LeadUp = 0.1;             //V5导联上升(mV)
        public double V6LeadFall = -0.1;           //V6导联下降(mV)
        public double V6LeadUp = 0.1;             //V6导联上升(mV)
    }

    public class reportSettingValues
    {
        public string reportTitle = "";            //报告标题
        public int displayPageNum = 0;             //报告页码是否显示
        public string acceptFlag = "";             //互认标识
        public int signMode = 0;                   //签名模式
        public int displayConclusion = 0;          //是否显示诊断结论
        public int displayConclusionType = 0;      //结论显示方式
        public string pageFoot = "";               //页脚
        public string diagnoseSign = "";        //诊断医生签名
        public string auditSign = "";           //审核医生签名
    }

    public class bpSettings
    {
        public bpDiagnoseSettingValues bpDiagnoseSets = new bpDiagnoseSettingValues();
    }

    public class bpDiagnoseSettingValues
    {
        public int minWakeSys = 90;            //收缩压 白天最小值
        public int maxWakeSys = 140;            //收缩压 白天最大值
        public int minSleepSys = 80;            //收缩压 夜间最小值
        public int maxSleepSys = 125;           //收缩压 夜间最大值
        public int minValidSys = 90;            //收缩压 24小时最小值
        public int maxValidSys = 130;           //收缩压 24小时最大值

        public int minWakeDia = 60;             //舒张压 白天最小值
        public int maxWakeDia = 90;             //舒张压 白天最大值
        public int minSleepDia = 50;            //舒张压 夜间最小值
        public int maxSleepDia = 80;            //舒张压 夜间最大值
        public int minValidDia = 60;            //舒张压 24小时最小值
        public int maxValidDia = 90;            //舒张压 24小时最大值

        public int avgSys = 130;                //24小时平均收缩压
        public int avgDia = 80;                 //24小时平均舒张压
        public int avgSysOfMorning = 135;       //早间平均收缩压
        public int avgDiaOfMorning = 85;        //早间平均舒张压
        public int avgSysOfDay = 135;           //白天平均收缩压
        public int avgDiaOfDay = 85;            //白天平均舒张压
        public int avgSysOfNight = 120;         //夜间平均收缩压
        public int avgDiaOfNight = 70;          //夜间平均舒张压

        public int maxErrorSys = 200;           //收缩压最大错误阈值
        public int minErrorSys = 70;            //收缩压最小错误阈值
        public int maxErrorDia = 140;           //舒张压最大错误阈值
        public int minErrorDia = 40;            //舒张压最小错误阈值
        public int maxErrorPr = 150;            //脉率最大错误阈值

        public string daytimeStart = "07:00";   //白天时间段起始
        public string daytimeEnd = "22:00";     //白天时间段截止

        public string unit = "mmHg";            //单位
    }
}
