/**
* @file			aecg_common.h
* @brief		动态心电公共接口
* @details		动态心电公共接口
* @author		袁艳
* @date			2022-07-26 11:01:16
* @version		1.0.1.1
* @copyright    Copyright (c) 2018-2021  武汉中旗生物医疗电子有限公司
* @attention
* <ul>
*	<li>注意1</li>
*	<li>注意2</li>
* </ul>
* @par	       修改日志
* <table>
* 	<tr>
*		<th>日期</th>
*		<th>版本</th>
*		<th>作者</th>
*		<th>修改内容</th>
*	</tr>
* 	<tr>
*		<td>2022-04-24</td>
*		<td>1.0.1.1</td>
*		<td>袁艳</td>
*		<td>创建初始版本</td>
*	</tr>
* 	<tr>
*		<td>2022-07-26</td>
*		<td>1.0.1.2</td>
*		<td>袁艳</td>
*		<td>创建初始版本</td>
*	</tr>
* </table>
*/

#ifndef AECG_COMMON_H
#define AECG_COMMON_H

#ifdef __cplusplus
extern "C"
{
#endif

typedef enum {
    AEcgNone                                    = 0,  ///< 未知
    AEcgVersion                                 = 1,  ///< 获取库版本信息
    AecgInit                                    = 2,  ///< 库初始化
    AecgDeinit                                  = 3,  ///< 库资源释放
    AecgSetConfig                               = 4,  ///< 重新配置动态心电，必须立即生效
    AecgLoadData                                = 5,  ///< 加载已分析的病例文件，为后续其他操作加载数据到内存
    AecgReanalyze                               = 6,  ///< 开始分析
    AecgDetectAF                                = 7,  ///< 房颤分析
    AecgAnalyzeST                               = 8,  ///< ST事件分析
    AecgCalculateHRV                            = 9,  ///< 心率变异性分析
    AecgReanalyzeSegment                        = 10, ///< 区间重分析（R波和心搏类型）
    AecgReanalyzePACSegment                     = 11, ///< 区间房早重分析
    AecgAnalyzeTWA                              = 12, ///< 计算给定信号起点后的1min数据的TWA
    AecgAnalyzeQTC                              = 13, ///< 计算主导联的QT和QTc值
    AecgReclassifySimilarBeatsInSubTemplate     = 14, ///< 子模板下与该QRS形态相似的QRS波，归类为指定的模板类型
    AecgReclusterSubTemplatesByLead             = 15, ///< 依据指定的心搏类型模板，重新划分子模板
    AecgMergeTemplates                          = 16, ///< 合并子模板
    AecgModifyBeats                             = 17, ///< 修改指定位置的心搏类型
    AecgDeleteBeats                             = 18, ///< 删除指定位置的心搏类型
    AecgInsertBeat                              = 19, ///< 指定位置精确插入心搏
    AecgInsertBeatBefore                        = 20, ///< 在参考心搏之前插入心搏
    AecgInsertBeatAfter                         = 21, ///< 在参考心搏之后插入心搏 
    AecgAddAFEvent                              = 22, ///< 新增房颤片段
    AecgDeleteAFEvents                          = 23, ///< 批量删除房扑房颤
    AecgAddSTEvent                              = 24, ///< 新增ST事件
    AecgDeleteSTEvents                          = 25, ///< 删除ST事件
    AecgGetQRSInfo                              = 26, ///< 查询波型位置信息
    AecgGetLeadQuality                          = 27, ///< 获取导联的信号质量评分
    AecgMarkSegmentUnanalyzable                 = 28, ///< 设置为非分析区间
    AecgMarkSegmentAnalyzable                   = 29, ///< 设置为可分析区间
    AecgGetBeatsInfo                            = 30, ///< 获取所有心拍信息
    AecgGetStxInfo                              = 31, ///< 获取ST的测量值
    AecgGetStEventInfo                          = 32, ///< 获取ST事件信息
    AecgGetSegmentUnanalyzable                  = 33, ///< 获取不可分析片段
    AecgGetHrvPsd                               = 34, ///< 获取HRV的功率谱数据
    AecgGetHrvTimeDomain                        = 35, ///< 获取HRV的5分钟和1小时的时域数据
    AecgGetHrvFreqDomain                        = 36, ///< 获取HRV的频域数据
    AecgGetQtcInfo                              = 37, ///< 获取QTc计算结果数据
    AecgGetAfEvent                              = 38, ///< 获取房扑/房颤片段数据
    AecgGetSegmentsWave                         = 39, ///< 获取指定导联多个片段的波形数据
    AecgGetNextQrsInfo                          = 40, ///< 获取主导联指定采样点后的第一个波形qrs信息
    AecgGetPacemakerEvaluationInfo              = 41, ///< 获取起搏器评估信息
    AecgGetPaceSimilarity                       = 42, ///< 获取起搏相似性信息
    AecgSaveAnalyzeData                         = 43, ///< 保存分析结果数据（退出病例时调用）
} AEcgFuncType;

typedef enum {
    AEcgDebugFlagDefault                         = 0,   ///< 默认不启用调试标志
    AEcgDebugFlagBeatDetect                      = 1,   ///< 记录beatDetect子库接口的输入输出数据
    AEcgDebugFlagBeatTemplate                    = 2,   ///< 记录beat_template子库接口的输入输出数据
    AEcgDebugFlagSpecialRhythmAnalysis           = 3,   ///< 记录special_rhythm_analysis子库接口的输入输出数据
    AEcgDebugFlagWavePoint                       = 4,   ///< 记录wavePoint子库接口的输入输出数据
    AEcgDebugFlagHrvMeas                         = 5,   ///< 记录hrv_meas子库接口的输入输出数据
    AEcgDebugFlagSignalQuality                   = 6,   ///< 记录signal_quality子库接口的输入输出数据
    AEcgDebugFlagPaceAnalysis                    = 7,   ///< 记录pace_analysis子库接口的输入输出数据
    AEcgDebugFlagPaceRemove                      = 8,   ///< 记录pace_remove子库接口的输入输出数据
    AEcgDebugFlagEcgFilter                       = 9,   ///< 记录ecg_filter子库接口的输入输出数据
    AEcgDebugFlagSTSegmentAnalysis              = 10,   ///< 记录st_segment_analysis子库接口的输入输出数据
    AEcgDebugFlagTwa                            = 11,   ///< 记录twa子库接口的输入输出数据
    AEcgDebugFlagReviseWavePoint                = 12,   ///< 记录revise_wave_point子库接口的输入输出数据
    AEcgDebugFlagRemoveAbnormalPacing           = 13,   ///< 记录remove_abnormal_pacing子库接口的输入输出数据
} AEcgDebugFlag;


typedef void (*analysisProcess)(const void *handle, AEcgFuncType type, int percent);


/**
* @struct	AEcgService
* @brief    服务分析模块回调设置
* @details  服务分析模块回调设置
*/
typedef struct {
    analysisProcess func; ///< 服务分析模块回调函数指针：函数名，进度条
    const void *handle;  ///< 服务分析模块数据处理接口指针
} AEcgService;

/**
* @struct	InvalidInterval
* @brief    不分析的区间
* @details  不分析的区间
*/
typedef struct {
    int start; ///< 起始位置（采样点）
    int end;   ///< 终止位置
} InvalidInterval;

/**
* @struct	AEcgWaveInfo
* @brief    波形信息
* @details  波形信息
*/
typedef struct {
    int qrsStart;  ///< QRS波起始位置
    int qrspeak;   ///< QRS波峰位置
    int qrsEnd;    ///< QRS波终止位置
}AEcgWaveInfo;

/**
* @struct	TWaveAlternansInfo
* @brief    T波电交替信息结构体
* @details  T波电交替信息结构体
*/
typedef struct {
    double twaVoltage; ///< 交替电压（单位：uv）
    double twaPower;   ///< 交替功率（单位：uv）
    double twaRatio;   ///< 交替率（0~100的浮点数）
    int twa;           ///< T波电交替（单位：uv）
} TWaveAlternansInfo;

/**
* @struct	AnalysisSetting
* @brief    分析配置
* @details  分析配置
*/
typedef struct {
    int mainChannel;            ///< 主通道：I,II,III,aVR,aVL,aVF,V1,V2,V3,V4,V5,V6索引值，从0开始
    int auxiliaryChannel1;      ///< 副通道1：I,II,III,aVR,aVL,aVF,V1,V2,V3,V4,V5,V6索引值，从0开始
    int auxiliaryChannel2;      ///< 副通道2：I,II,III,aVR,aVL,aVF,V1,V2,V3,V4,V5,V6索引值，从0开始
    int qrsRefractoryPeriod;    ///< QRS不应期（单位：ms）
    int qrsSimilarityThreshold; ///< QRS重分析时相似度百分比阈值（80 ==> 80%）
    int stOffset;               ///< ST点偏移，表示 ST 点相对于 J 点的偏移量（单位：ms）
    int jOffset;                ///< J点偏移（单位：ms）
    int baseOffset;             ///< 基线点偏移（单位：ms）
    int powerFrequency;         ///< 工频: OFF(0), 50Hz, 60Hz
    int baseline;               ///< 基线漂移(数值提升100倍): OFF(0), 0.01Hz(1), 0.05Hz(5), 0.3Hz(30), 0.6Hz(60)
    int myoelectric;            ///< 肌电: OFF(0), 25Hz(25), 35Hz(35), 45Hz(45), 75Hz(75), 100Hz(100), 150Hz(150), 250Hz(250), 350Hz(350)
    int paceAnalysisEnable;     ///< 是否起搏分析
    int paceEventEnable;        ///< 是否起搏事件分析
    char paceAnalysisType[64];  ///< 起搏器类型     todo 待配置
    int paceMethod;             ///< 起搏方式：0(单腔起搏)，1(双腔起搏)，2(三腔起搏)
    int pace2beat;              ///< 起搏钉到心搏（单位：ms）
    int beat2pace;              ///< 心搏到起搏钉（单位：ms）
    int checkHrUpperLimit;      ///< 检查心率上限（单位：bmp）
    int checkHrLowerLimit;      ///< 检查心率下限（单位：bmp）
    int artifactSensitivity;    ///< 伪差剔除灵敏度：0(无), 1(高), 2(中), 3(低) todo 待配置
    int disturbDelEnable;       ///< 智能干扰删除 todo 待配置
    int multichannelEnable;     ///< 多通道       todo 待配置
    int startSample;            ///< 开始分析起始点
    int endSample;              ///< 开始分析结束点
    InvalidInterval *interval;  ///< 标注不分析的区间
    int intervalNum;            ///< 标注不分析的区间长度
    int ronTMaxRRInterval;      ///< 室早R on T最大RR间期（单位：ms)
    int sPercent;               ///< 房早逸博滞后百分比（0~100）
    int vPercent;               ///< 室早逸博滞后百分比（0~100）
} AnalysisSetting;

/**
* @struct	LeadConfig
* @brief    导联配置
* @details  导联配置
*/
typedef struct {
    int limbLead[3];  ///< 肢导联接线顺序    1(R), 2(L), 3(F)
    int chestLead[6]; ///< 胸导联接线顺序    1(V1), 2(V2), 3(V3), 4(V4), 5(V5), 6(V6)
} LeadConfig;

/**
* @struct	STReanalysisConfig
* @brief    ST 分析配置
* @details  ST 分析配置
*/
typedef struct {
    int stMode;               ///< ST 模式（默认0）：0(绝对), 1(相对)
    double stDepressionAmp;   ///< ST压低幅值（单位：mv）
    int stDepressionDuration; ///< ST持续压低时间（单位：秒）
    double stElevationAmp;    ///< ST抬高幅值（单位：mv）
    int stElevationDuration;  ///< ST持续抬高时间（单位：秒）
} STReanalysisConfig;

/**
* @struct	HrvAnalysisConfig
* @brief    心率变异性分析配置
* @details  心率变异性分析配置
*/
typedef struct {
    int step;                  ///< 分析步长（单位：秒） 300
    double highFreqFloor;      ///< 高频范围起始值（单位：Hz） 0.15--0.4（0.15）
    double highFreqCeiling;    ///< 高频范围终止值（单位：Hz） 0.15--0.4（0.4）
    double lowFreqFloor;       ///< 低频范围起始值（单位：Hz） 0.04--0.15（0.04）
    double lowFreqCeiling;     ///< 低频范围终止值（单位：Hz） 0.04--0.15（0.15）
    double veryLowFreqFloor;   ///< 极低频范围起始值（单位：Hz） 0.0033--0.04（0.0033）
    double veryLowFreqCeiling; ///< 极低频范围终止值（单位：Hz） 0.0033--0.04（0.04）
} HrvAnalysisConfig;

/**
* @struct	APBConfig
* @brief    房早配置
* @details  房早配置
*/
typedef struct {
    int before; ///< 提前量 before%
} APBConfig;

/**
* @struct	BeatInfo
* @brief    心搏信息
* @details  心搏信息
*/
typedef struct {
    int rWavePos;            ///< R波位置数据
    short rrDuration;        ///< RR间期
    short beatUiType;        ///< 心拍类型界面类型
    short morphoType;        ///< 按照波形形态进行聚类的心拍类型，数量多的排序靠前
    int startIdx;            ///< 心拍起点
    int endIdx;              ///< 心拍终点
} BeatInfo;

typedef struct {
    char dataPath[256];    ///< 算法分析中间数据保存路径, 当前动态心电数据存储路径，不能为空
    char startTime[20];    ///< 采集开始时间
    int days;              ///< 采集天数
    int hw_line;           ///< 设备接线数
    int hw_lead;           ///< 设备输出数据导联数
    int sampleRate;        ///< 采样率
    int lead;              ///< 导联数

    AnalysisSetting analysisSetting;       ///< 分析配置
    LeadConfig leadConfig;                 ///< 导联接线配置
    STReanalysisConfig stReanalysisConfig; ///< ST 分析配置
    HrvAnalysisConfig hrvAnalysisConfig;   ///< 心率变异性分析配置
    APBConfig apbConfig; ///< 房早配置
} AEcgConfig;

typedef struct {
    BeatInfo *beats;           ///< 心搏数据数组指针
    int beatCount;                 ///< 当前心搏数量(已使用)
    int maxBeatCapacity;           ///< 最大心搏容量(缓冲区大小)
}BeatsData;

typedef struct {
    int statusFlag;         ///< 状态位：比如已保存
    int startPosition;      ///< 发生时间（与心搏单位相同：点，该值/采样率可换算成时间）
    int durationTime;       ///< 持续时间（与心搏单位相同：点，该值/采样率可换算成时间）
    float stPointVoltage;   ///< ST 值：mV
    int lead;               ///< 导联：0/1/2/3/4/5/6/7/8/9/10/11
    int stSlope;            ///< 斜率：0（水平）、1（上斜），2（下斜）
    int hrMin;              ///< 最慢心率：单位 bpm
    int hrAvg;              ///< 平均心率：单位 bpm
    int hrMax;              ///< 最快心率：单位 bpm
} StEventInfo;

typedef struct {
    StEventInfo *stEventArray;      ///< ST事件数组指针
    int stEventArrayNum;            ///< 当前ST事件数量(已使用)
    int maxStEventArrayCapacity;    ///< 最大ST事件容量(缓冲区大小)
} StEventData;

typedef struct {
    int stBeatPos;             ///< ST事件心搏位置
    short jAmp;                ///< J点幅值（uV）
    short stAmp;               ///< ST点幅值（uV）
    short slope;               ///< 斜率。0（水平）、1（上斜），2（下斜）
    short tWaveAmp;             ///< T波幅值（uV）
    short secTWaveAmp;          ///< T'波幅值（uV）
} StxInfo;

typedef struct {
    short leadIndex;
    int startIdx;///< 事件起点位置，-1表示无效
    int endIdx;///< 事件终点位置，-1表示无效
} LeadEvent;

typedef struct {
    double dataX;         ///< 频谱图横坐标（频率轴），单位 Hz，数组长度由 `fftNum` 返回。
    double dataY;         ///< 频谱图纵坐标（功率谱密度），单位 ms²/Hz，数组长度由 `fftNum` 返回。
} HrvPsdInfo;

typedef struct {
    double rrInterval;       ///< RR间期
    double respirationRate;  ///< 呼吸频率
} SleepApneaInfo;

/**
* @struct  HRVTimeDomainMeasures
* @brief   心率变异性时域测量参数
* @details  心率变异性时域测量参数，包括：
 * （1）统计参数：nns、mean_nn、sdnn、sdnn_index、sdann、sdsd、nn50_count、pnn50，rmssd
 * （2）几何测量参数：hrv_triangular_index
*/
typedef struct {
    int start_position; ///< 分析的开始位置，单位：索引，调用者赋值
    int length_of_time; ///< 分析的时长，单位：分钟，调用者赋值
    int nns;  ///< NN间期个数
    int mean_nn;  ///< 平均 NN 间期，单位：ms
    int sdnn; ///< NN 间期的标准差, 正常值范围 141±39 ms

    int sdnn_index;   ///< 5mins 段 NN 间期标准差的平均值
    int sdann; ///< 5 mins 段 NN 间期平均值的标准差, 正常值范围 127±35 ms

    int sdsd;  ///< 相邻 NN 间期之差的标准差，单位 ms
    int nn50_count;  ///< 相邻 RR 间期差值 ＞ 50ms 的个数
    float pnn50;  ///< 相邻RR间期差值＞50ms占总体的百分比
    int rmssd; ///< 相邻 NN 间期差的均方根, 正常值范围 27±12 ms

    float hrv_triangular_index;  ///< 三角指数, 正常值范围 37±15
} HrvTimeDomainMeas;

/**
* @struct  HrvFreqDomainMeasures
* @brief   心率变异性频域测量参数
* @details  心率变异性频域测量参数，包括：
 * (1)  统计参数： length_of_time、mean_nn
 * (2) 频域参数：total_power、ulf、vlf、lf、hf、lf_normal、hf_normal、ratio_lf_hf
*/
typedef struct {
    int start_position; ///< 分析的开始位置，单位：索引，需要修改
    int length_of_time; ///< 分析的时长，单位：分钟，调用者赋值，默认5min
    int mean_nn; ///< 分析时间段内 NN 间期的平均值，单位：ms

    float total_power;  ///< 该时间段内 NN 间期变化的总功率, 正常值范围 3466±1018，单位 ms2
    float ulf;  ///< 极低频段（≤ 0.003 Hz）功率，单位 ms2
    float vlf;  ///< 超低频段（0.003-0.04 Hz）功率，单位 ms2
    float lf;    ///< 低频段（0.04~0.15 Hz）功率,  正常值范围 1170±416，单位 ms2
    float hf;  ///< 高频段（0.15~0.4 Hz）功率, 正常值范围 975±203，单位 ms2
    float lf_normal;  ///< 归一化低频段功率： LF/(total power-VLF)*100, 正常值范围 54±4
    float hf_normal;     ///< 归一化高频段功率:HF/(total power-VLF)*100, 正常值范围 29±3
    float ratio_lf_hf;     ///< 正常值范围 1.5-2.0
} HrvFreqDomainMeas;

typedef struct{
    int rPos;        ///< R波峰位置
    short qt;        ///< QT测量值
    short qtc;       ///< QTc测量值
} QTMeasInfo;

/**
* @struct	EventSegment
* @brief    事件段
* @details  事件段
*/
typedef struct {
    int startIdx;///< 事件起点位置，-1表示无效
    int endIdx;///< 事件终点位置，-1表示无效
}SegmentInfo;

typedef struct {
    SegmentInfo *segmentArray;
    int segmentNum;
    int segmentArrayCapacity;
} SegmentEvent;

typedef struct {
    int rWavePosition;          ///< 起搏心搏所在R波位置（单位：样本点）
    int failureType;            ///< 起搏评价类型：0-感知失败，1-输出失败，2-夺获失败
} pacemakerEvaluation;

#ifdef __cplusplus
}
#endif

#endif // AECG_COMMON_H
