using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Zoncare.Common;

namespace AnalysisApp.Commons
{
    class BpPulseWaveDataHandler
    {
        private const string winDrivers = @"lib\unmanaged\libnibp_dynamic_d_d.dll";

        [DllImport(winDrivers, CallingConvention = CallingConvention.Cdecl, EntryPoint = "nibp_figure_plot")]
        public static extern int nibpFigurePlot(ref nibpInData data, ref nibpOutExtractWave wave, ref nibpOutFittingCurve curve);

        public static int WaveDataPlot(List<int> dataList, List<int> stageList, int refPR, out double[] dc, out double[] ac, out double[] stepDc, out int[] stepDcLen)
        {
            int result = -1;
            dc = null;
            ac = null;
            stepDc = null;
            stepDcLen = null;

            nibpInData input = new nibpInData();
            int[] arrData = dataList.ToArray();
            int[] arrStage = stageList.ToArray();
            IntPtr dataPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * arrData.Length);
            IntPtr stagePtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * arrStage.Length);
            Marshal.Copy(arrData, 0, dataPtr, arrData.Length);
            Marshal.Copy(arrStage, 0, stagePtr, arrStage.Length);

            IntPtr dcPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(double)) * arrData.Length);
            IntPtr acPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(double)) * arrData.Length);
            IntPtr stepDcPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(double)) * 100);
            IntPtr stepDcLenPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * 100);
            IntPtr fitCurveXPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(double)) * arrData.Length);
            IntPtr fitCurveYPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(double)) * arrData.Length);
            try
            {
                input.preWave = dataPtr;
                input.stage = stagePtr;
                input.len = arrData.Length;
                input.refPR = refPR;

                nibpOutExtractWave outputWave = new nibpOutExtractWave()
                {
                    dc = dcPtr,
                    ac = acPtr,
                    stepDc = stepDcPtr,
                    stepDcLen = stepDcLenPtr
                };
                nibpOutFittingCurve outputCurve = new nibpOutFittingCurve()
                {
                    fitCurveX = fitCurveXPtr,
                    fitCurveY = fitCurveYPtr
                };
                int plot = nibpFigurePlot(ref input, ref outputWave, ref outputCurve);
                if (plot == 0)
                {
                    if (outputWave.len > 0)
                    {
                        dc = new double[outputWave.len];
                        ac = new double[outputWave.len];
                        stepDc = new double[outputWave.stepLen];
                        stepDcLen = new int[outputWave.stepLen];
                        Marshal.Copy(outputWave.dc, dc, 0, dc.Length);
                        Marshal.Copy(outputWave.ac, ac, 0, ac.Length);
                        Marshal.Copy(outputWave.stepDc, stepDc, 0, stepDc.Length);
                        Marshal.Copy(outputWave.stepDcLen, stepDcLen, 0, stepDcLen.Length);
                        result = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "血压数据处理");
            }
            finally
            {
                Marshal.FreeHGlobal(dataPtr);
                Marshal.FreeHGlobal(stagePtr);

                Marshal.FreeHGlobal(dcPtr);
                Marshal.FreeHGlobal(acPtr);
                Marshal.FreeHGlobal(stepDcPtr);
                Marshal.FreeHGlobal(stepDcLenPtr);
                Marshal.FreeHGlobal(fitCurveXPtr);
                Marshal.FreeHGlobal(fitCurveYPtr);
            }
            return result;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct nibpInData
        {
            public IntPtr preWave;       // 采集压力值数组
            public IntPtr stage;         // 状态数组
            public int len;           // 数据长度
            public int refPR;        // 脉率参考值
            public int refHR;        // 心率参考值
            public int cl;           // 测量值置信度
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct nibpOutExtractWave
        {
            public IntPtr dc;           // 原始信号dc值,单位为压力值
            public IntPtr ac;           // 原始信号ac值,单位为幅度
            public int len;             // 信号长度
            public IntPtr stepDc;       //台阶dc值数组，分配空间可设置为100
            public IntPtr stepDcLen;
            public int stepLen;         //台阶dc值个数
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct nibpOutFittingCurve
        {
            public IntPtr fitCurveX;       //拟合曲线X轴，单位为压力值
            public IntPtr fitCurveY;      //拟合曲线Y轴，单位为幅度
            public int len;                // 数据长度
            public int SBP;                //收缩压
            public int MBP;                //平均压
            public int DBP;                //舒张压
        }

        //[StructLayout(LayoutKind.Sequential, Pack = 1)]
        //public struct nibpOutExtractWave
        //{
        //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10000)]
        //    public double[] dc;          // 原始信号dc值,单位为压力值
        //    public IntPtr ac;          // 原始信号ac值,单位为幅度
        //    public int len;          // 信号长度
        //}

        //[StructLayout(LayoutKind.Sequential, Pack = 1)]
        //public struct nibpOutFittingCurve
        //{
        //    public IntPtr fitCurveX;       //拟合曲线X轴，单位为压力值
        //    public IntPtr fitCurveY;      //拟合曲线Y轴，单位为幅度
        //    public int len;                // 数据长度
        //    public int SBP;                //收缩压
        //    public int MBP;                //平均压
        //    public int DBP;                //舒张压
        //}
    }
}
