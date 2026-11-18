using System;
using System.Linq;
using Zoncare.Common;

namespace AnalysisApp.Commons
{
    class OverlayWaveAdjust
    {
        HolterConfig _holterConfig;
        public OverlayWaveAdjust(HolterConfig holterConf)
        {
            _holterConfig = holterConf;
        }

        /// <summary>
        /// 自适应基线校正 + 形态差异增强
        /// 特点：
        /// 1. 根据波形陡峭程度自动调整滑动窗口大小（陡峭波形用小窗口保留细节）
        /// 2. 增强不同形态波形之间的差异（突出聚类特征）
        /// 3. 可配置的基线校正强度
        /// </summary>
        public void adaptiveBaselineCorrectionWithEnhancement(
            short[] srcData,
            short[] sharedBuffer,
            int windowSize = 200,
            float baselineStrength = 0.85f,     // 基线校正强度，0.7-0.9 保留部分原始基线差异
            float enhanceFactor = 1.2f)         // 形态差异增强因子，>1 放大差异
        {
            DateTime tStart = DateTime.Now;

            // 根据采样率计算基础窗口大小（毫秒转点数）
            int baseWindowSize = (int)(200F * _holterConfig.SampleRate / 1000);

            int len = srcData.Length;
            float[] corrected = new float[len];

            // ========== 第一步：分析波形特征 ==========
            float maxSlope = CalculateMaxSlope(srcData);
            float waveformEnergy = CalculateWaveformEnergy(srcData);
            float peakToPeak = GetPeakToPeak(srcData);

            // 动态调整窗口大小
            // 陡峭波形（高斜率）=> 小窗口保留细节
            // 平坦波形 => 大窗口平滑
            int adaptiveWindowSize = baseWindowSize;
            if (maxSlope > 1500)        // 非常陡峭（如室早、R波高尖）
                adaptiveWindowSize = baseWindowSize / 3;
            else if (maxSlope > 800)    // 中等陡峭
                adaptiveWindowSize = baseWindowSize / 2;
            else if (maxSlope < 200)    // 平坦波形
                adaptiveWindowSize = baseWindowSize * 2;

            // 限制窗口范围，避免过大或过小
            adaptiveWindowSize = Math.Max(5, Math.Min(adaptiveWindowSize, len / 4));

            // ========== 第二步：滑动窗口基线估计 ==========
            float[] baseline = new float[len];
            float windowSum = 0;

            for (int i = 0; i < len; i++)
            {
                int start = Math.Max(0, i - adaptiveWindowSize);
                int end = Math.Min(len - 1, i + adaptiveWindowSize);

                if (i == 0)
                {
                    // 初始化窗口
                    for (int j = start; j <= end; j++)
                        windowSum += srcData[j];
                }
                else
                {
                    // 滑动更新
                    if (start > 0) windowSum -= srcData[start - 1];
                    if (end < len - 1) windowSum += srcData[end + 1];
                }

                baseline[i] = windowSum / (end - start + 1);
            }

            // ========== 第三步：基线校正（带强度控制）==========
            // 保留部分原始基线差异，避免不同形态波形过度趋同
            float[] tempCorrected = new float[len];
            for (int i = 0; i < len; i++)
            {
                tempCorrected[i] = srcData[i] - (baseline[i] * baselineStrength);
            }

            // ========== 第四步：形态差异增强 ==========
            // 放大不同心搏之间的特征差异，使聚类更明显
            // 方法：对波形的高频成分进行增益，保留形态特征

            // 4.1 计算波形的局部特征（斜率、曲率）
            float[] slope = new float[len];
            float[] curvature = new float[len];

            for (int i = 1; i < len - 1; i++)
            {
                slope[i] = tempCorrected[i + 1] - tempCorrected[i - 1];
                curvature[i] = tempCorrected[i + 1] - 2 * tempCorrected[i] + tempCorrected[i - 1];
            }

            // 4.2 根据波形能量和峰峰值动态调整增强系数
            float dynamicEnhanceFactor = enhanceFactor;
            if (waveformEnergy > 500000)    // 高能量波形（如室早）
                dynamicEnhanceFactor = enhanceFactor * 1.3f;
            else if (waveformEnergy < 100000) // 低能量波形
                dynamicEnhanceFactor = enhanceFactor * 0.8f;

            // 4.3 形态增强：突出峰值和谷值
            float[] enhanced = new float[len];
            float globalMean = tempCorrected.Average();

            for (int i = 0; i < len; i++)
            {
                // 基础信号
                enhanced[i] = tempCorrected[i];

                // 增强局部极值点（放大波峰波谷差异）
                bool isPeak = (i > 0 && i < len - 1 &&
                               tempCorrected[i] > tempCorrected[i - 1] &&
                               tempCorrected[i] > tempCorrected[i + 1]);
                bool isValley = (i > 0 && i < len - 1 &&
                                 tempCorrected[i] < tempCorrected[i - 1] &&
                                 tempCorrected[i] < tempCorrected[i + 1]);

                if (isPeak)
                {
                    // 峰值增强：向上拉高
                    float peakHeight = tempCorrected[i] - globalMean;
                    enhanced[i] += peakHeight * (dynamicEnhanceFactor - 1) * 0.5f;
                }
                else if (isValley)
                {
                    // 谷值增强：向下压低
                    float valleyDepth = globalMean - tempCorrected[i];
                    enhanced[i] -= valleyDepth * (dynamicEnhanceFactor - 1) * 0.5f;
                }

                // 高斜率区域增强（波形陡峭部分）
                if (Math.Abs(slope[i]) > 500)
                {
                    enhanced[i] += slope[i] * 0.05f * (dynamicEnhanceFactor - 1);
                }
            }

            // ========== 第五步：非线性映射增强聚类效果 ==========
            // 使用 Sigmoid -like 变换，放大偏离均值的信号
            float[] finalCorrected = new float[len];
            float signalStd = ComputeStdDev(enhanced);
            float threshold = signalStd * 0.5f;

            for (int i = 0; i < len; i++)
            {
                float deviation = enhanced[i] - globalMean;

                // 对较大偏差进行非线性放大
                if (Math.Abs(deviation) > threshold)
                {
                    float gain = 1.0f + (Math.Abs(deviation) / signalStd) * 0.3f;
                    gain = Math.Min(gain, 1.5f); // 限制最大增益
                    finalCorrected[i] = globalMean + deviation * gain;
                }
                else
                {
                    finalCorrected[i] = enhanced[i];
                }
            }

            // ========== 第六步：边界平滑处理（避免增强带来的锯齿）==========
            finalCorrected = SmoothEdges(finalCorrected, 3);

            // ========== 第七步：写入共享缓冲区 ==========
            for (int i = 0; i < len; i++)
            {
                int value = (int)Math.Round(finalCorrected[i]);
                value = Math.Max(short.MinValue, Math.Min(short.MaxValue, value));
                sharedBuffer[i] = (short)value;
            }

            var elapsed = (DateTime.Now - tStart).TotalMilliseconds;
            // Console.WriteLine($"Adaptive baseline correction time: {elapsed:F2} ms, window={adaptiveWindowSize}, slope={maxSlope:F0}, energy={waveformEnergy:F0}, enhance={dynamicEnhanceFactor:F2}");
        }

        // ==================== 辅助函数 ====================

        /// <summary>
        /// 计算波形最大斜率（用于判断波形陡峭程度）
        /// </summary>
        private float CalculateMaxSlope(short[] data)
        {
            if (data.Length < 2) return 0;

            float maxSlope = 0;
            for (int i = 1; i < data.Length; i++)
            {
                float slope = Math.Abs(data[i] - data[i - 1]);
                if (slope > maxSlope)
                    maxSlope = slope;
            }
            return maxSlope;
        }

        /// <summary>
        /// 计算波形能量（平方和）
        /// </summary>
        private float CalculateWaveformEnergy(short[] data)
        {
            double energy = 0;
            for (int i = 0; i < data.Length; i++)
            {
                energy += data[i] * data[i];
            }
            return (float)energy;
        }

        /// <summary>
        /// 计算峰峰值
        /// </summary>
        private float GetPeakToPeak(short[] data)
        {
            short max = data.Max();
            short min = data.Min();
            return max - min;
        }

        /// <summary>
        /// 计算标准差
        /// </summary>
        private float ComputeStdDev(float[] data)
        {
            if (data.Length == 0) return 0;

            float mean = data.Average();
            double sumSq = 0;
            for (int i = 0; i < data.Length; i++)
            {
                float diff = data[i] - mean;
                sumSq += diff * diff;
            }
            return (float)Math.Sqrt(sumSq / data.Length);
        }

        /// <summary>
        /// 边缘平滑（简单移动平均）
        /// </summary>
        private float[] SmoothEdges(float[] data, int windowSize)
        {
            if (data.Length < windowSize * 2) return data;

            float[] smoothed = new float[data.Length];
            Array.Copy(data, smoothed, data.Length);

            // 只平滑开头和结尾部分，保留中间细节
            for (int i = 0; i < windowSize; i++)
            {
                int leftStart = i;
                int leftEnd = Math.Min(i + windowSize, data.Length);
                if (leftEnd > leftStart)
                {
                    smoothed[i] = data.Skip(leftStart).Take(leftEnd - leftStart).Average();
                }

                int rightStart = Math.Max(0, data.Length - windowSize + i);
                int rightEnd = data.Length - i;
                if (rightEnd > rightStart)
                {
                    smoothed[data.Length - 1 - i] = data.Skip(rightStart).Take(rightEnd - rightStart).Average();
                }
            }

            return smoothed;
        }

        /// <summary>
        /// 可选的波形聚类标记函数（用于叠加图显示时区分不同形态）
        /// </summary>
        public enum BeatMorphologyType
        {
            Normal,      // 正常波形
            PVC,         // 室性早搏
            Aberrant,    // 差异性传导
            Paced,       // 起搏波形
            Artifact     // 干扰波形
        }

        /// <summary>
        /// 基于处理后的波形特征自动分类（用于后续叠加图分簇显示）
        /// </summary>
        public BeatMorphologyType ClassifyBeatMorphology(short[] processedBeat)
        {
            float maxSlope = CalculateMaxSlope(processedBeat);
            float energy = CalculateWaveformEnergy(processedBeat);
            float peakToPeak = GetPeakToPeak(processedBeat);

            // 寻找R波宽度（半高宽）
            short maxValue = processedBeat.Max();
            int maxIndex = Array.IndexOf(processedBeat, maxValue);
            int halfMax = maxValue / 2;
            int leftWidth = 0, rightWidth = 0;
            for (int i = maxIndex; i >= 0 && processedBeat[i] > halfMax; i--) leftWidth++;
            for (int i = maxIndex; i < processedBeat.Length && processedBeat[i] > halfMax; i++) rightWidth++;
            int qrsWidth = leftWidth + rightWidth;

            // 分类逻辑
            if (maxSlope > 1200 && qrsWidth < 10 && energy > 400000)
                return BeatMorphologyType.PVC;
            else if (maxSlope > 800 && qrsWidth > 15)
                return BeatMorphologyType.Aberrant;
            else if (peakToPeak < 300)
                return BeatMorphologyType.Artifact;
            else
                return BeatMorphologyType.Normal;
        }

    }
}
