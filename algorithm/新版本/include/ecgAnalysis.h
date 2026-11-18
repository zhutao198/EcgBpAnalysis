/**
* @file			ecgAnalysis.h
* @brief		动态心电分析接口
* @details		动态心电分析接口
* @author		袁艳
* @date			2026-01-01 13:40:28
* @version		1.0.0.0
* @copyright    Copyright (c) 2026-2028  武汉中旗生物医疗电子有限公司
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
*		<td>2026-01-04</td>
*		<td>1.0.1.2</td>
*		<td>zq1940</td>
*		<td>1.库版本信息
*		    2.文件转换
*		    3.预分析
*		    4.开始分析
*		    5.修改指定位置的心搏类型
*		    6.批量修改同一类的心搏类型
*		    7.删除指定位置的心搏类型
*		    8.将房颤信息写入到心搏文件中
*		    9.将新增房颤信息（房扑(M)或房颤(K)）写入到心搏文件中
*		    10.删除心搏文件中指定的房颤片段
*		    11.ST 事件重分析
*		    12.新增一条 ST 段事件项
*		    13.删除指定位置的 ST 事件项
*		    14.起搏分析，输出pacingEventFile文件
*		    15.心率变异性分析
*		    16.报告函数计算
*		</td>
*	</tr>
* 	<tr>
*		<td>2022-07-26</td>
*		<td>1.0.1.3</td>
*		<td>zq1940</td>
*		<td>1.文件加载
*		    2.起搏删除
*		</td>
*	</tr>
* </table>
*/

#ifndef AECGANALYSIS_H
#define AECGANALYSIS_H

#ifdef __cplusplus
extern "C"
{
#endif

#include "aecg_common.h"

typedef void (*ReportProgress)(AEcgFuncType type, int percent);   ///< 上报进度

/**
 * @par 返回码契约
 *   所有声明为 int 返回值的 aecg* / ecg* 导出接口统一约定：
 *   - 0           成功（对应 ecg_errors.h 中的 ERR_SUCCESS）
 *   - < -1000     错误：见 ecg_errors.h
 *     - -1001~-1060   通用错误（入参 / 文件 / 内存 / 状态 / 非错误状态码）
 *     - -100001~-100051  aecg 本库专用错误码
 *   - -1051~-1053 非错误语义状态码（AECG_CMP_DIFFER / AECG_NOT_FOUND / AECG_TRUNCATED），
 *                 调用方应优先按语义判定，不应一律视为失败
 *   调用方判定请使用 `if (ret != ERR_SUCCESS)` 或 `if (ret < 0)`，避免硬编码具体负值。
 */

/**
* @name			ecgAnalysisVersion
* @brief    	库版本信息
* @details  	库版本信息
*
* @return		库版本信息
*
* @author		zq1940
* @date			2026-01-04 13:46:53
*
*/
const char *aecgVersion();

/**
* @name			aecgInit
* @brief    	库初始化
* @details  	库资源申请，日志功能初始化。库不再使用时，须调用 aecgDeinit释放资源
*
* @param[in]    callBack    进度上报回调函数
* @return
* @retval		非NULL指针       aecg实例
* @retval		NULL            失败
*
* @author		zq1940
* @date			2026-01-04 15:08
*
*/
void *aecgInit(const ReportProgress callBack);
/**
* @name			aecgDeinit
* @brief    	库资源释放
* @details  	库资源释放，调用此函数前须调用newDemoLib
* @param[in]    handleP    句柄
* @return
* @retval		0       成功
* @retval		!0      失败
*
* @author		zq1940
* @date			2026-01-04 15:08
*
*/
int aecgDeinit(void ** handleP);

/**
* @name			aecgSetConfig
* @brief    	重新配置动态心电
* @details      切换病例或者配置文件holter.info变更时调用
* @param[in]    handle        句柄
* @param[in]    pathName      病例路径
* @return
* @retval		0       成功
* @retval		!0      失败
*
* @author		zq1940
* @date			2026-01-04 15:08
*
*/
int aecgSetConfig(void *handle, char *pathName);

/**
* @name			aecgLoadData
* @brief    	文件加载（动态心电报告诊断完后，重新查看时调用）
* @details  	文件加载（动态心电报告诊断完后，重新查看时调用,如果ecg_revised.dat与ecg_raw.dat文件不存在，则生成）
*
* @param[in]    handle    句柄
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2026-01-04 16:38:36
*
*/
int aecgLoadData(void * handle);

/**
* @name		    aecgReanalyze
* @brief    	开始分析
* @details  	输出beats.dat,pacing_event.dat,holter_revised.dat,ST 测量文件 stx.dat 等文件
*
* @param[in]    handle    句柄
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2026-01-04 14:58:45
*
*/
int aecgReanalyze(void * handle);

/**
* @name			aecgDetectAF
* @brief    	房颤检测
* @details  	无
*
* @param[in]    handle    句柄
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2026-01-04 15:28:56
*
*/
int aecgDetectAF(void * handle);

/**
* @name			aecgAnalyzeST
* @brief    	ST事件重分析
* @details  	根据用户修改后的 ST 事件参数值, 重新生成一份 ST 事件分析文件
*
* @param[in]     handle     句柄
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2026-01-04 15:40:57
*
*/
int aecgAnalyzeST(void * handle);

/**
* @name			aecgCalculateHRV
* @brief    	心率变异性分析
* @details  	心率变异性分析
*
* @param[in]     handle     句柄
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2026-01-04 08:43:35
*
*/
int aecgCalculateHRV(void * handle);

/**
* @name			  aecgReanalyzeSegment
* @brief    	区间重分析(查找R波和心搏类型)
* @details  	对区间片段进行R波查找，再进行心搏类型判断，将心拍信息写入beats_tempt.dat文件
*
* @param[in/out]    handle              句柄
* @param[in]        startPos            重分析片段起点
* @param[in]        endPos              重分析片段终点
* @param[in]        leadIndex           重分析片段所在导联编号
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2026-01-04 10:20:35
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgReanalyzeSegment(void *handle, const int startPos, const int endPos, const int leadIndex);

/**
* @name			aecgReanalyzePACSegment
* @brief
* @details  	区间分析房早
*
* @param[in/out]    handle              句柄
* @param[in]    sample[startPos,endPos]   分析起点、终点的数组(外调传进来的非房颤片段)
* @param[in]    sampleNum              数组个数
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2023-04-17 09:29:57
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgReanalyzePACSegment(void *handle, int leadIndex, SegmentInfo *sample, int sampleNum);

/**
* @name			aecgAnalyzeTWA
* @brief    	T波电交替的幅值
* @details  	默认计算给定信号起点后的1min数据的TWA
*
* @param[in]    handle            句柄
* @param[in]    samplePos     心电信号的起点位置
* @param[out]	  twaInfo       T波电交替参数
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2023-12-07 10:03:48
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgAnalyzeTWA(void * handle, int samplePos, TWaveAlternansInfo *twaInfo);

/**
* @name			aecgAnalyzeQTC
* @brief    	计算主导联的QT和QTc值
* @details  	计算主导联的QT和QTc值
*
* @param[in]    handle            句柄
* @param[in]    samplePos     心电信号的起点位置
* @param[out]	  twaInfo       T波电交替参数
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2023-12-07 10:03:48
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgAnalyzeQTC(void *handle);

/**
* @name			aecgReclassifySimilarBeatsInSubTemplate
* @brief    	函数简单概述
* @details  	仅针对指定的导联及 QRS 心搏，将其所在子模板下与该 QRS 形态相似的 QRS 波，归类为指定的模板类型，归类后默认的子模板为单独一类
* @param[in/out]    handle            句柄
* @param[in]    sample               指定的模板心拍的位置
* @param[in]    beatType             心搏类型
* @param[in]    leadIndex            导联序号
* @param[in]    morphoType           心搏形态类型
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2023-04-17 09:31:25
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgReclassifySimilarBeatsInSubTemplate(void * handle, int sample, const short beatType, const int leadIndex, short morphoType);

/**
* @name			aecgReclusterSubTemplatesByLead
* @brief    	函数简单概述
* @details  	仅针对指定的导联，依据指定的心搏类型模板，重新划分子模板
*
* @param[in/out]    handle            句柄
* @param[in]    leadIndex            导联序号
* @param[in]    beatType             新的心搏类型
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2023-04-27 10:17:32
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgReclusterSubTemplatesByLead(void *handle, int leadIndex, const short beatType);

/**
* @name			aecgReclusterBeatsByLead
* @brief    	按指定导联对心拍数据进行重聚类
* @details  	针对指定的导联，对指定位置的心拍数据重新聚类，聚类子类型结果输出到 beatSubType 数组
*
* @param[in/out]    handle            句柄
* @param[in]    leadIndex            导联序号，从0开始计数
* @param[in]    beatPos              待重聚类的心拍R波位置数组
* @param[in]    beatPosNum           待重聚类的心拍个数
* @param[out]   beatSubType          输出心拍子类型数组，空间由调用者申请和释放，个数与 beatPosNum 一致
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2026-11-17 10:00:00
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgReclusterBeatsByLead(void *handle, const int leadIndex, const int *beatPos, const int beatPosNum, short *beatSubType);

/**
* @name			aecgMergeTemplates
* @brief    	函数简单概述
* @details  	合并子模板
*
* @param[in/out]    handle            句柄
* @param[in]    fromShape            待合并的心拍类型数组
* @param[in]    fromShapeNum         待合并的心拍类型数组的个数
* @param[in]    toShape              新的心搏子类型
* @param[in]    beatType             新的心搏类型
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2023-04-17 09:33:02
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgMergeTemplates(void * handle, const int *fromShape, int fromShapeNum, short toShape, const short beatType);

/**
* @name			ecgModifySpecifiedBeats
* @brief    	修改指定位置的心搏类型
* @details  	根据用户指定的心搏位置，修改心搏文件中的心搏类型字段，修改为 beatTypeNew 对应的 ASCII 码. 并将修改结果写入
*                 原心搏文件 beats.dat.dat (主导联心搏文件)
*
* @param[in]     handle             句柄
* @param[in]     beatPos            待修改的心搏位置数组
* @param[in]     beatPosNum         待修改的心搏的个数
* @param[in]     beatType           修改后的心搏类型
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2026-01-04 15:14:02
*
*/
int aecgModifyBeats(void *handle, const int *beatPos, const int beatPosNum, const short beatType);

/**
* @name			aecgDeleteBeats
* @brief    	删除指定位置的心搏类型(等价于将心搏类型修改为 'D')
* @details  	直接修改 beats.dat
*
* @param[in]     handle             句柄
* @param[in]     beatPos            待修改的心搏位置数组
* @param[in]     beatPosNum         待修改的心搏的个数
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2026-01-04 15:26:42
*
*/
int aecgDeleteBeats(void * handle, const int *beatPos, int beatPosNum);

/**
* @name			ecgAnalysisAddBeat
* @brief    	添加波
* @details  	在指定时间点精确插入心搏(绝对位置)。这里分两种情况，如果新增心拍在原波中，修改原波波形，如果不在，则保存在新的文件中
*
* @param[in/out]    handle               句柄
* @param[in]        beatPos              插入位置数组
* @param[in]        beatPosNum           插入位置数组的个数
* @param[in]        beatType             心搏类型
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2023-04-14 10:02:11
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgInsertBeat(void *handle, int *beatPos, int beatPosNum, const short beatType);

/**
* @name			aecgAddAFEvent
* @brief    	将新增房颤信息（房扑(M)或房颤(K)）写入到心搏文件中
* @details  	无
*
* @param[in]     handle                句柄
* @param[in]     beatPosStart          新增的房颤片段开始位置
* @param[in]     beatPosEnd            新增的房颤片段结束位置
* @param[in]     ecgBeatType           新增的是房扑(M:77)或房颤(K:75)
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2026-01-04 15:34:40
*
*/
int aecgAddAFEvent(void *handle, int beatPosStart, int beatPosEnd, const short ecgBeatType);

/**
* @name			aecgDeleteAFEvents
* @brief    	函数简单概述
* @details  	批量删除房扑房颤(将区间内的房扑、房颤心拍类型修改为N)
*
* @param[in/out]    handle                句柄
* @param[in]        sample                起止点数组
* @param[in]        sampleNum             起止点数组个数
* @param[in]        ecgBeatType           删除的是房扑(M:77)或房颤(K:75)

* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2023-07-06 10:43:51
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgDeleteAFEvents(void *handle, SegmentInfo *sample, const int sampleNum, const short ecgBeatType);

/**
* @name			aecgAddSTEvent
* @brief    	新增一条 ST 段事件项
* @details  	根据输入的参数, 向 ST 事件文件中新增一行
*
* @param[in]     handle             句柄
* @param[in]     statusFlag         状态标准信息码
* @param[in]     startPos           新增的 ST 事件开始位置
* @param[in]     duration           新增的 ST 事件持续时间
* @param[in]     stVoltage          新增的 ST 事件的 ST 值的电压
* @param[in]     lead               新增的 ST 事件所在导联编号
* @param[in]     stSlope            新增的 ST 事件的斜率
* @param[in]     hrMin              新增的 ST 事件段的最小心率
* @param[in]     hrAvg              新增的 ST 事件段的平均心率
* @param[in]     hrMax              新增的 ST 事件段的最大心率
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2026-01-04 15:46:58
*
*/
int aecgAddSTEvent(void * handle, const StEventInfo stEvent);

/**
* @name			aecgDeleteSTEvents
* @brief    	删除指定位置的 ST 事件项
* @details  	根据输入的 ST 事件开始位置,删除对应的 ST 事件项
*
* @param[in]     handle              句柄
* @param[in]     leadEvent           待删除的 ST 事件项
* @param[in]     num                 待删除的 ST 事件项个数
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2026-01-04 15:52:14
*
*/
int aecgDeleteSTEvents(void *handle, LeadEvent *leadEvent, const int num);

/**
* @name			aecgGetQRSInfo
* @brief    	查询波型qrs起、止点信息
* @details  	查询波型qrs起、止点信息
*
* @param[in]    handle              句柄
* @param[in]    rPos                R波位置
* @param[out] 	startPos            QRS波形起点位置信息
* @param[out] 	endPos              QRS波形终点位置信息
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2022-08-22 17:44:29
*
*/
int aecgGetQRSInfo(void * handle, int samplePos, int *startPos, int *endPos);

/**
* @name			  aecgGetLeadQuality
* @brief    	获取导联的信号质量评分（分数越高代表质量越好）
* @details  	获取导联的信号质量评分
*
* @param[in]    handle               句柄
* @param[in]    leadIndex            导联数组
* @param[in]    leadIndexNum         导联数组个数
* @param[out]   score                质量评分（长度同导联数组个数）
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2023-07-06 10:43:39
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgGetLeadQuality(void *handle, const int *leadIndex, int leadIndexNum, int *score);

/**
* @name			aecgMarkSegmentUnanalyzable
* @brief    标记/设置为非分析区间
* @details  a) 增加：在不可分析段文件 invalid_segment.dat 中增加对应行
            b) 修改 beats.dat 文件对应位置为 D
            c) 修改 beat_temp.dat 文件对应位置为 D
*
* @param[in/out]    handle              句柄
* @param[in]        startPos            片段起点
* @param[in]        endPos              片段终点
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1970
* @date			2024-4-28 16:52:48
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgMarkSegmentUnanalyzable(void *handle, const int startPos, const int endPos);

/**
* @name			aecgMarkSegmentAnalyzable
* @brief    恢复/标记/设置为可分析区间
* @details  对该区间段重新进行 R 波识别和心搏分类，结果写入到beat_temp.dat
            删除：不可分析段文件 invalid_segment.dat 中对应行
*
* @param[in/out]    handle              句柄
* @param[in]        startPos            片段起点
* @param[in]        endPos              片段终点
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1970
* @date			2024-4-28 16:52:48
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgMarkSegmentAnalyzable(void *handle, const int startPos, const int endPos);


// 获取数据接口
/**
* @name			aecgGetBeatsInfo
* @brief    获取心拍信息（R波位置、起点、终点、RR间期、心拍类型、子类型）
* @details  
*
* @param[in/out]     handle              句柄
* @param[out]        beatInfo            心拍信息
* @param[out]        beatsNum            心拍个数
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1970
* @date			2024-4-28 16:52:48
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgGetBeatsInfo(void *handle, BeatInfo **beatInfo, int *beatsNum);

/**
* @name			aecgGetStxInfo
* @brief    获取单导联ST测量值
* @details  
*
* @param[in/out]     handle              句柄
* @param[in]         leadIndex           导联序号
* @param[out]        stxInfo             单导联ST测量信息数组地址
* @param[out]        stxInfo             单导ST测量信息个数
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1970
* @date			2024-4-28 16:52:48
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgGetStxInfo(void *handle, const int leadIndex, StxInfo **stxInfo, int *stxNum);

/**
* @name			aecgGetStEventInfo
* @brief        获取指定导联的ST测量值
* @details  
* @param[in/out]     handle              句柄
* @param[out]        stEvent             ST事件信息
* @param[out]        stEventNum          ST事件个数
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1970
* @date			2024-4-28 16:52:48
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgGetStEventInfo(void *handle, StEventInfo **stEvent, int *stEventNum);

/**
* @name			aecgGetSegmentUnanalyzable
* @brief        获取不可分析片段（分析导联都不可分析的片段）
* @details  
* @param[in/out]     handle              句柄
* @param[out]        segment             ST事件信息
* @param[out]        segmentNum          ST事件个数
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1970
* @date			2024-4-28 16:52:48
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgGetSegmentUnanalyzable(void *handle, SegmentInfo **segment, int *segmentNum);

/**
* @name			aecgGetHrvPsd
* @brief        获取HRV的功率谱数据
* @details      每小时一组数据，常规26组，每组262145个
* @param[in]     handle              句柄
* @param[out]        dataX               X轴数据数组
* @param[out]        dataY               Y轴数据数组
* @param[in]         index               功率谱分组索引
* @param[out]        psdNum              功率谱数据数组个数
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1970
* @date			2024-4-28 16:52:48
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgGetHrvPsd(void *handle, double **dataX, double **dataY, const int index, int *psdNum);

/**
* @name			aecgGetHrvTimeDomain
* @brief        获取HRV的时域数据
* @details      mehtod:0 获取5分钟的HRV的频域数据
*               mehtod:1 获取1小时的HRV的频域数据
* @param[in/out]     handle                 句柄
* @param[in]         method                 0:获取5分钟的HRV的频域数据  1:获取1小时的HRV的频域数据
* @param[out]        timeDomain             频域数据数组
* @param[out]        timeDomainNum          频域数据数组个数
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1970
* @date			2024-4-28 16:52:48
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgGetHrvTimeDomain(void *handle, const short method, HrvTimeDomainMeas **timeDomain, int *timeDomainNum);

/**
* @name			aecgGetHrvFreqDomain
* @brief        获取HRV的频域数据
* @details      
* @param[in/out]     handle                 句柄
* @param[out]        freqDomain             频域数据数组
* @param[out]        freqDomainNum          频域数据数组个数
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1970
* @date			2024-4-28 16:52:48
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgGetHrvFreqDomain(void *handle, HrvFreqDomainMeas **freqDomain, int *freqDomainNum);


/**
* @name			aecgGetQtcInfo
* @brief        获取QTc计算结果数据（只计算主导联的QTc数据）
* @details      
* @param[in/out]     handle                 句柄
* @param[out]        qtMeas             QTc计算结果数组
* @param[out]        qtMeasNum          QTc计算结果数组个数
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1970
* @date			2024-4-28 16:52:48
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgGetQtcInfo(void *handle, QTMeasInfo **qtMeas, int *qtMeasNum);

/**
* @name			aecgGetQtcInfo
* @brief        获取房颤/房扑片段数据
* @details      
* @param[in/out]     handle                 句柄
* @param[out]        afEvent                房颤片段数组
* @param[out]        afEventNum             房颤片段数组个数
* @param[in]         ecgBeatType           删除的是房扑(M:77)或房颤(K:75)
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1970
* @date			2026年1月16日17:21:41
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgGetAfEvent(void *handle, SegmentInfo **afEvent, int *afEventNum, const int ecgBeatType);

/**
* @name			aecgGetSegmentsWave
* @brief        获取指定导联多个片段的波形数据
* @details      
* @param[in/out]     handle                 句柄
* @param[in]         leadIndex              导联数
* @param[in]         segments               片段信息（起点索引，终点索引数组）
* @param[in]         segmentsNum            片段信息个数
* @param[out]        wave                   波形信息数组
* @param[out]        status                 波形信息对应的状态
* @param[inout]      waveNum                波形信息数组个数(输入为内存个数，输出为实际个数)
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1970
* @date			2026年1月16日17:21:41
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgGetSegmentsWave(void *handle, const int leadIndex, SegmentInfo *segments, const int segmentsNum, short *wave, int *status, int *waveNum);

/**
* @name			aecgGetNextQrsInfo
* @brief        获取主导联指定采样点后的第一个波形qrs信息
* @details      
* @param[in/out]     handle                 句柄
* @param[in]         leadIndex              导联数
* @param[in]         segments               片段信息（起点索引，终点索引数组）
* @param[in]         segmentsNum            片段信息个数
* @param[out]        wave                   波形信息数组
* @param[inout]      waveNum                波形信息数组个数(输入为内存个数，输出为实际个数)
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1970
* @date			2026年1月16日17:21:41
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgGetNextQrsInfo(void *handle, const int point, AEcgWaveInfo *waveInfo);

/**
* @name			aecgGetPacemakerEvaluationInfo
* @brief        获取起搏功能评价结果
* @details      
* @param[in/out]         handle                 句柄
* @param[in/out]         paceEvaluationInfo              起搏功能评价结果数组
* @param[in/out]         paceEvaluationNum               起搏功能评价结果数组个数
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1970
* @date			2026年1月16日17:21:41
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgGetPacemakerEvaluationInfo(void *handle, pacemakerEvaluation **paceEvaluationInfo, int *paceEvaluationNum);

/**
* @name			aecgGetPaceSimilarity
* @brief        获取心拍相识度评分
* @details      
* @param[in]         handle                 句柄
* @param[in]         leadIndex              导联序号
* @param[in]         pos                    心拍R波峰位置
* @param[in]         posNum                 心拍个数
* @param[in/out]     similarity             相识度评分数组
* @return
* @retval        0          成功
* @retval        !0         失败
* @author		zq1970
* @date			2026年1月16日17:21:41
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgGetPaceSimilarity(void *handle, const int leadIndex, int *pos, const int posNum, double *similarity);

/**
* @name			aecgSaveAnalyzeData
* @brief        保存分析结果数据（退出病例时调用）
* @details
* @param[in/out]     handle                 句柄
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1970
* @date			2024-4-28 16:52:48
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgSaveAnalyzeData(void *handle);


/**
* @name			setEcgSublibDebugFlag
* @brief        设置子库调试输入输出标志（内部调试用）
* @details      bug定位时，调用aecg_example.exe + 子库名称，可以保存对应子库的输入输出数据，方便查看
*
* @param[in/out]    handle              句柄
* @param[in]        flag                对应子库的编号
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1970
* @date			2024-4-28 16:52:48
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int setEcgSublibDebugFlag(void *handle, const AEcgDebugFlag flag);

/**
* @name			aecgInsertBeatBefore
* @brief    	向前插入心搏：在参考心搏之前插入心搏
* @details  	向前插入心搏：在参考心搏之前插入心搏 (相对位置)
*
* @param[in/out]    handle               句柄
* @param[in]        beatPos              插入位置数组
* @param[in]        beatPosNum           插入位置数组的个数
* @param[in]        leadIndex            导联序号
* @param[in]        beatType             心搏类型
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2023-04-14 10:02:11
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgInsertBeatBefore(void *handle, int *beatPos, const int beatPosNum, const int leadIndex, const short beatType);

/**
* @name			aecgInsertBeatAfter
* @brief    	向后插入心搏：在参考心搏之后插入心搏
* @details  	向后插入心搏：在参考心搏之后插入心搏 (相对位置)
*
* @param[in/out]    handle               句柄
* @param[in]        beatPos              插入位置数组
* @param[in]        beatPosNum           插入位置数组的个数
* @param[in]        leadIndex            导联序号
* @param[in]        beatType             心搏类型
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2023-04-14 10:02:11
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgInsertBeatAfter(void *handle, int *beatPos, const int beatPosNum, const int leadIndex, const short beatType);

/**
* @name			aecgCorrectLead
* @brief    	校正导联
* @details  	校正导联
*
* @param[in/out]    handle                   句柄
* @param[in]        lFReverseFlag            肢体导联反接标志 0:未反接 1:反接
* @param[in]        indexArray               胸导联反接导联编号 （1~6）
* @return
* @retval        0          成功
* @retval        !0         失败
*
* @author		zq1940
* @date			2023-04-14 10:02:11
*
* @par 示例:
* @code
*	函数使用实例
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgCorrectLead(void *handle, int lFReverseFlag, int *indexArray);

/**
* @name			  ecgAnalyzeSleepApnea
* @brief    	分析睡眠呼吸暂停
* @details  	根据R间期插值得到4Hz心率序列并提取呼吸信号（EDR），检测呼吸暂停事件并计算呼吸暂停指数AHI。
*               事件与AHI保存在句柄内部，同时按4Hz逐点输出RR间期与呼吸信号。
*
* @param[in]    handle               句柄
* @param[out]   sleepApneaInfo       结果输出缓冲区（4Hz逐点；容量不足时只写入前*infoNum个点，容量为0时可为NULL）
* @param[in,out] infoNum             入参为缓冲区容量；出参为实际结果点数（4Hz）
*
* @return
* @retval        0          成功
* @retval        AECG_TRUNCATED  成功但输出被截断（*infoNum出参为所需容量，可据此重新调用）
* @retval        !0         失败
*
* @author		zq1940
* @date			2026年5月22日14:56:24
*
* @par 示例:
* @code
* @endcode
* @see      类、结构体、枚举等（超链接）
*/
int aecgAnalyzeSleepApnea(void *handle, SleepApneaInfo **sleepApneaInfo, int *infoNum);
#ifdef __cplusplus


}
#endif

#endif // PRIOR_ANALYSIS_H