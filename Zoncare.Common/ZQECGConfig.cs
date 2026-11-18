namespace Zoncare.Common
{
    public class ZQECGConfig
    {
        public static string ECG_NO_FILE = "0x0000";
        public static string ECG_NO_FILE_NAME = "0x0001"; // 文件名 没用!!!
        public static string ECG_NO_MACHINE_TYPE = "0x0002"; // 机器类型-- string
        public static string ECG_NO_MACHINE_SOFTWARE_VERSION = "0x0003"; // 软件版本
        // char
        // *
        public static string ECG_NO_MACHINE_SOFTWARE_TIME = "0x0004"; // 软件时间
        // char
        // *
        public static string ECG_NO_BASE = "0x0100"; // 心电文件说明
        public static string ECG_NO_BASE_SN = "0x0101"; // 心电图机的 SN char *
        public static string ECG_NO_BASE_VERSION = "0x0102"; // 文件版本 char *
        public static string ECG_NO_BASE_TYPE = "0x0103"; // 类型 string
        // 标准心电图 (0);
        // 向量心电图(1);
        // 时间向量心电图(2)
        // 晚心位 (3)
        // 频谱(4)
        // QT离散度(5)
        // 心率变异性(6)
        // 节律分析(7)

        public static string ECG_NO_BASE_TIME = "0x0104"; // 时间 struct tm time
        public static string ECG_NO_BASE_LEAD = "0x0105"; // 导联数 string
        public static string ECG_NO_BASE_SAMPLE = "0x0106"; // 采样率 string
        public static string ECG_NO_BASE_SECOND = "0x0107"; // 数据总时长 ms string
        public static string ECG_NO_BASE_PACE = "0x0108"; // Pace 信号 没用!!!

        public static string ECG_NO_BASE_LEAD_SYSTEM = "0x0109"; // 导联体系
        // LT_SYSTEM
        // 类型
        public static string ECG_NO_BASE_LEAD_NAME = "0x010A"; // 体系名 char
        // lead_name[12][5];
        // 类型
        public static string ECG_NO_BASE_CUSTOM_TYPE = "0x0110"; // 自定义类型
        // (char *)
        // HIS 厂家名称
        public static string ECG_NO_BASE_CUSTOM_DATA = "0x0111"; // 自定义内容
        // (char *)
        // 用于暂存 网络获取 json
        // 数据中患者信息

        public static string ECG_NO_BASE_SPEED = "0x0112";//纸速，存储的内容为 (float) [5|6.25|12.5|25|50]  mm/s
        public static string ECG_NO_BASE_GAIN = "0x0113";//肢导的增益，  存储的内容为 (float) [1.25|2.5|5|10|20|40] 
        public static string ECG_NO_BASE_LAYOUT = "0x0114";	//布局，  存储的内容为 (char *) ["3x4"|"3x4+1"|"3x4+3"|"6x2"|"6x2+1"|"12x1"|"6x3"|"6x3+1"|"9x2"|"9x2+1"|"5x3"|"5x3+1"]
        public static string ECG_NO_BASE_WAVEFORM_POSITION = "0x0115";  //位置，  浏览时显示的位置，单位 ms

        public static string ECG_NO_BASE_RECORD_ORDER = "0x0116";   //记录顺序			0: 同步	   1: 顺序
        public static string ECG_NO_BASE_SINGLE_RHYTHM = "0x0117";  //单节律	，在12、15、18导模式下都会使用
        public static string ECG_NO_BASE_THREE_RHYTHM1 = "0x0118";  //三节律的第一导，目前只在12导模式用到
        public static string ECG_NO_BASE_THREE_RHYTHM2 = "0x0119";  //三节律的第二导，目前只在12导模式用到
        public static string ECG_NO_BASE_THREE_RHYTHM3 = "0x012A";	//三节律的第三导，目前只在12导模式用到
        public static string ECG_NO_BASE_HEART_POSTION = "0x012B";	//0：左位心，1右位心

        public static string ECG_NO_BASE_AUTO_GAIN_SIGN = "0x012C";//自动增益的标记          0：不是自动增益， 1： 是自动增益
        public static string ECG_NO_BASE_GAIN_PRECORDIAL = "0x012D";    //胸导的增益，存储的内容为		 (float) [1.25|2.5|5|10|20|40] 
        public static string ECG_NO_BASE_MACHINE_TYPE_NAME = "0x012E";  //设备类型名，区别于 机器类型, 类型名是可修改

        // ...............................................
        public static string ECG_NO_PATIENT = "0x0200"; // 患者信息
        public static string ECG_NO_PATIENT_ID = "0x0201"; // 患者ID char *
        public static string ECG_NO_PATIENT_NAME = "0x0202"; // 患者姓名 char *
        public static string ECG_NO_PATIENT_GENDER = "0x0203"; // 患者性别 M
        // （male）是男的
        // F（female ）是女的
        // char
        public static string ECG_NO_PATIENT_BIRTHDAY = "0x0204"; // 患者生日
        // struct tm
        // time 没填入，默认值 ： 60
        // 岁
        public static string ECG_NO_PATIENT_WEIGHT = "0x0205"; // 患者体重 string
        public static string ECG_NO_PATIENT_HEIGHT = "0x0206"; // 患者身高 string
        public static string ECG_NO_PATIENT_AGE = "0x0207"; // 患者年龄 string
        public static string ECG_NO_PATIENT_AGE_UNIT = "0x0208"; // 年龄的单位 Y
        // (years)
        // M(months) D(days)
        // char
        public static string ECG_NO_PATIENT_RACE = "0x0209"; // 种族 C N M O U
        // 白人 非洲 东方
        // 其他 未知 char
        public static string ECG_NO_PATIENT_DRUG1 = "0x020A"; // 用药1 1
        public static string ECG_NO_PATIENT_DRUG2 = "0x020B"; // 用药2 255 用药
        // DRUG
        // 需输入两个值，且至少有一个有效值输入，缺省值255
        public static string ECG_NO_PATIENT_CDRUG1 = "0x020C"; // 用药2
        public static string ECG_NO_PATIENT_CDRUG2 = "0x020D"; // 用药2 先前诊断
        public static string ECG_NO_PATIENT_QTC_TYPE = "0x020E"; // 算法
        // QTcHodge(默认)
        // QTcHodge
        // /QTcBazett/
        // QTcFridericia
        // /QTcFramingham

        public static string ECG_NO_PATIENT_DEPARTMENTS = "0x020F"; // 科室 char
        // * 界面
        // lineEdit
        // 建议加长度限制
        public static string ECG_NO_PATIENT_BED_NUMBER = "0x0210"; // 床号 char
        // * 界面
        // lineEdit
        // 建议加长度限制

        // ...............................................
        public static string ECG_NO_DOCTOR = "0x0300"; // 医师 char *
        public static string ECG_NO_DOCTOR_REQUEST = "0x0301"; // 送检医生 char *
        public static string ECG_NO_DOCTOR_DIAGNOSIS = "0x0301"; // 诊断医生 char
        // *
        // ...............................................
        public static string ECG_NO_RECORD = "0x0400";
        public static string ECG_NO_RECORD_BASELINE = "0x0401"; // 基线漂移滤波器 如
        // 0.05Hz
        // 0.15Hz 0.25Hz
        // 0.5Hz ADS(-1)
        // float
        public static string ECG_NO_RECORD_MYOELECT = "0x0402"; // 肌电滤波 如 25;
        // 35; 45;
        // 75; 100; 150Hz
        // string
        public static string ECG_NO_RECORD_FREQUENCY = "0x0403"; // 工频滤波器 如 AC
        // [50Hz
        // 60Hz] string
        // ...............................................
        public static string ECG_NO_MEASURE = "0x0500";
        public static string ECG_NO_MEASURE_HR = "0x0501"; // HR : 80 bpm
                                                           // string
        public static string ECG_NO_MEASURE_PR = "0x0502"; // PR : 143 ms
                                                           // string
        public static string ECG_NO_MEASURE_QRS = "0x0503"; // QRS : 98 ms
                                                            // string
        public static string ECG_NO_MEASURE_QT = "0x0504";
        public static string ECG_NO_MEASURE_QTC = "0x0505"; // QT/QTC :
        // 370/426 ms
        // string
        public static string ECG_NO_MEASURE_P = "0x0506";
        public static string ECG_NO_MEASURE_QRS_ = "0x0507";
        public static string ECG_NO_MEASURE_T = "0x0508"; // P/QRS/T :
                                                          // 25/66/49 string
        public static string ECG_NO_MEASURE_RV5 = "0x0509";
        public static string ECG_NO_MEASURE_SV1 = "0x050A"; // RV5/SV1 :
        // 1.601/-1.441
        // mV RV5+SV1 : 3.042 mV
        // float
        public static string ECG_NO_MEASURE_AVERAGE = "0x050B"; // 平均模板参数
        // ECG_AVERAGE
        public static string ECG_NO_MEASURE_ST = "0x050C"; // ST段，距阵， char *
        public static string ECG_NO_MEASURE_P_INTERVAL = "0x050E";	//P 间期
        // ...............................................
        public static string ECG_NO_RESULT = "0x0600";
        public static string ECG_NO_RESULT_MINNESOTA = "0x0601"; // 明尼苏达码 char
                                                                 // *
        public static string ECG_NO_RESULT_CODE = "0x0602"; // 诊断结果编号 char *
        public static string ECG_NO_RESULT_ADVICE = "0x0603"; // 医嘱 char *

        public static string ECG_NO_RESULT_CLASSIFY = "0x0604"; // 分类基准
        public static string ECG_NO_RESULT_NOTE = "0x0605"; // 注意事项

        // ...............................................
        public static string ECG_NO_WAVEFORM = "0x0700"; // 波形
        public static string ECG_NO_WAVEFORM_DATA = "0x0701"; // 波形数据 short *
        public static string ECG_NO_WAVEFORM_SIZE = "0x0702"; // 波形大小 string
        // ...............................................
        // 综合测度 Comprehensive Measure
        public static string ECG_NO_C_MEASURE = "0x0A00"; // 综合测度
        public static string ECG_NO_C_MEASURE_P_RT = "0x0A01"; // P环运行时间; 如
        // 94毫秒
        // short
        public static string ECG_NO_C_MEASURE_QRS_RT = "0x0A02"; // QRS环运行时间 如
        // 84毫秒
        // short
        public static string ECG_NO_C_MEASURE_ST_T_RT = "0x0A03"; // ST-T环运行时间
        // 如
        // 284毫秒 short
        public static string ECG_NO_C_MEASURE_P_MAX_A = "0x0A04"; // P环最大振幅
        // 如：0.057
        // 毫伏 float
        public static string ECG_NO_C_MEASURE_QRS_MAX_A = "0x0A05"; // QRS环最大振幅
        // 如：2.110 毫伏
        // float
        public static string ECG_NO_C_MEASURE_ST_T_MAX_A = "0x0A06"; // T环最大振幅
        // 如：0.881 毫伏
        // float
        // 投影面测量参数
        public static string ECG_NO_C_PLANE_MEASURE = "0x0B00"; // 投影面测量参数
        public static string ECG_NO_C_PLANE_MEASURE_X_Y = "0x0B01"; // 额面F（X-Y）
                                                                    // Plane_Measure
        public static string ECG_NO_C_PLANE_MEASURE_X_Z = "0x0B02"; // 横面H（X-Z）
        // Plane_Measure
        public static string ECG_NO_C_PLANE_MEASURE_Z_Y = "0x0B03"; // 右侧面S（Z-Y）
        // Plane_Measure
        // QRS起始 40 ms 向量
        public static string ECG_NO_C_PLANE_MEASURE_F_4 = "0x0B04"; // 额面F(X-Y)
        // QRS_Vector
        // F[4] 10ms
        // 20ms 30ms
        // 40ms
        public static string ECG_NO_C_PLANE_MEASURE_H_4 = "0x0B05"; // 横面H(X-Z)
        // QRS_Vector
        // H[4] 10ms
        // 20ms 30ms
        // 40ms
        public static string ECG_NO_C_PLANE_MEASURE_S_4 = "0x0B06"; // 右侧面S(Z-Y)
        // QRS_Vector
        // S[4] 10ms
        // 20ms 30ms
        // 40ms
        // P;Q;S;T的位置
        public static string ECG_NO_C_VCG_PQST_POS = "0x0B07"; // P;QRS;T环的起始结束位置
        // 类型：为数组short
        // pos[5]

        public static string ECG_NO_C_VCG_STJ_VEC = "0x0B08"; // STJ向量
        // 类型：为数组QRS_Vector
        // stjVec[3]
        // ST-T起始 20;40;60;80 ms 向量
        public static string ECG_NO_C_PLANE_MEASURE_ST_T_F_4 = "0x0B09"; // 额面F(X-Y)
        // QRS_Vector
        // ST_T_F[4]
        // 20ms 40ms
        // 60ms 80ms
        public static string ECG_NO_C_PLANE_MEASURE_ST_T_H_4 = "0x0B0A"; // 横面H(X-Z)
        // QRS_Vector
        // ST_T_H[4]
        // 20ms 40ms
        // 60ms 80ms
        public static string ECG_NO_C_PLANE_MEASURE_ST_T_S_4 = "0x0B0B"; // 右侧面S(Z-Y)
        // QRS_Vector
        // ST_T_S[4]
        // 20ms 40ms
        // 60ms 80ms
        // ...............................................
        // QTD
        public static string ECG_NO_C_QTD = "0x0C00";
        public static string ECG_NO_C_QTD_Q = "0x0C01"; // ECG_NO_BASE_LEAD *
                                                        // sizeof(string)
        public static string ECG_NO_C_QTD_T = "0x0C02"; // ECG_NO_BASE_LEAD *
        // sizeof(string)
        public static string ECG_NO_C_QTD_ARG = "0x0C03"; // QTD 参数

        // ...............................................
        // VLP
        public static string ECG_NO_C_VLP = "0x0D00";
        public static string ECG_NO_C_VLP_ARG = "0x0D01"; // sizeof(VLP_ARG)
        // VLP_ARG结构体

        // RHY
        public static string ECG_NO_C_RHY = "0x0E00";
        public static string ECG_NO_C_RHYINDEX = "0x0E01"; // 节律分析(节律分析导联索引)
        // HRV
        public static string ECG_NO_C_HRV = "0x0F00"; // 心率变异性
        public static string ECG_NO_C_HRV_ARG = "0x0F01"; // 心率变异性指标参数
        public static string ECG_NO_C_HRV_ARPT = "0x0F02"; // 心率变异性AR模型功率普参数
        //写入zqecg文件的必填项
        public static string[] EcgNos = new string[]
        {
            ZQECGConfig.ECG_NO_PATIENT_ID,
            ZQECGConfig.ECG_NO_BASE_VERSION,
            ZQECGConfig.ECG_NO_BASE_TYPE,
            ZQECGConfig.ECG_NO_BASE_TIME,
            ZQECGConfig.ECG_NO_BASE_SAMPLE,
            ZQECGConfig.ECG_NO_BASE_LEAD,
            ZQECGConfig.ECG_NO_BASE_LEAD_SYSTEM,
            ZQECGConfig.ECG_NO_RECORD_BASELINE,
            ZQECGConfig.ECG_NO_RECORD_MYOELECT,
            ZQECGConfig.ECG_NO_RECORD_FREQUENCY,
            ZQECGConfig.ECG_NO_BASE_SPEED,
            ZQECGConfig.ECG_NO_BASE_GAIN,
            ZQECGConfig.ECG_NO_BASE_GAIN_PRECORDIAL,
            ZQECGConfig.ECG_NO_BASE_AUTO_GAIN_SIGN,
            ZQECGConfig.ECG_NO_BASE_WAVEFORM_POSITION,
            ZQECGConfig.ECG_NO_BASE_RECORD_ORDER,
            ZQECGConfig.ECG_NO_BASE_SINGLE_RHYTHM,
            ZQECGConfig.ECG_NO_BASE_THREE_RHYTHM1,
            ZQECGConfig.ECG_NO_BASE_THREE_RHYTHM2,
            ZQECGConfig.ECG_NO_BASE_THREE_RHYTHM3,
            ZQECGConfig.ECG_NO_BASE_HEART_POSTION,
            ZQECGConfig.ECG_NO_BASE_PACE,
            ZQECGConfig.ECG_NO_MACHINE_TYPE,
            ZQECGConfig.ECG_NO_MACHINE_SOFTWARE_VERSION,
            ZQECGConfig.ECG_NO_MACHINE_SOFTWARE_TIME,
            ZQECGConfig.ECG_NO_PATIENT_NAME,
            ZQECGConfig.ECG_NO_PATIENT_GENDER,
            ZQECGConfig.ECG_NO_PATIENT_AGE,
            ZQECGConfig.ECG_NO_PATIENT_AGE_UNIT,
            ZQECGConfig.ECG_NO_PATIENT_WEIGHT,
            ZQECGConfig.ECG_NO_PATIENT_HEIGHT,
            ZQECGConfig.ECG_NO_PATIENT_DRUG1,
            ZQECGConfig.ECG_NO_PATIENT_DRUG2,
            ZQECGConfig.ECG_NO_PATIENT_CDRUG1,
            ZQECGConfig.ECG_NO_PATIENT_CDRUG2,
            ZQECGConfig.ECG_NO_MEASURE_HR,
            ZQECGConfig.ECG_NO_MEASURE_PR,
            ZQECGConfig.ECG_NO_MEASURE_QRS,
            ZQECGConfig.ECG_NO_MEASURE_QT,
            ZQECGConfig.ECG_NO_MEASURE_QTC,
            ZQECGConfig.ECG_NO_MEASURE_P,
            ZQECGConfig.ECG_NO_MEASURE_QRS_,
            ZQECGConfig.ECG_NO_MEASURE_T,
            ZQECGConfig.ECG_NO_MEASURE_RV5,
            ZQECGConfig.ECG_NO_MEASURE_SV1,
            ZQECGConfig.ECG_NO_BASE_SECOND,
            ZQECGConfig.ECG_NO_WAVEFORM_DATA,
            ZQECGConfig.ECG_NO_WAVEFORM_SIZE
        };
    }
}
