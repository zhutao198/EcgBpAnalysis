using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Zoncare.Common;

namespace EcgAnalyseStart
{
    public class DataHandler
    {
        private const string winDrivers = @"testWave.dll";

        [DllImport(winDrivers, CallingConvention = CallingConvention.StdCall, EntryPoint = "initDll")]
        public static extern void InitDll(); //初始化C++获取数据的dll

        [DllImport(winDrivers, CallingConvention = CallingConvention.StdCall, EntryPoint = "getOneLeadSize")]
        public static extern long GetOneLeadSize(); //获取某一导联数据存储空间大小

        [DllImport(winDrivers, CallingConvention = CallingConvention.StdCall, EntryPoint = "readOneLeadData")]
        public static extern int ReadOneLeadData(int lead, IntPtr waveDataMap, ref int total); //获取某一导联数据

        [DllImport(winDrivers, CallingConvention = CallingConvention.StdCall, EntryPoint = "readOneLeadData")]
        public static extern int ReadOneLeadData(int lead, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] short[] waveData, ref int total); //获取某一导联数据


        [DllImport(winDrivers, CallingConvention = CallingConvention.StdCall, EntryPoint = "readWaveData")]
        public static extern int ReadWaveData(int index, int size, IntPtr waveDataMap, ref int total); //获取所有8导数据

        [DllImport(winDrivers, CallingConvention = CallingConvention.StdCall, EntryPoint = "readWaveData")]
        public static extern int ReadWaveData(int index, int size, byte[] waveDataMap, ref int total);

        [DllImport(winDrivers, CallingConvention = CallingConvention.StdCall, EntryPoint = "readWaveData")]
        public static extern int ReadWaveData(int index, int size, ref WaveDataMap[] waveDataMap, ref int total);


        [DllImport(winDrivers, CallingConvention = CallingConvention.StdCall, EntryPoint = "uninitDll")]
        public static extern void UninitDll(); //释放C++ dll使用的资源


        /// <summary>
        /// 获取所有8导联的数据(二进制形式)
        /// </summary>
        /// <param name="startIndex">数据点开始位置</param>
        /// <param name="pointCount">数据点个数</param>
        /// <returns></returns>
        public static List<byte[]> GetWaveBytes(int startIndex, int pointCount)
        {
            int readSize = pointCount * 2 * 8;
            IntPtr pt = Marshal.AllocHGlobal(readSize);
            List<byte[]> allLeadData = new List<byte[]>();

            try
            {
                int totalSize = 0;
                if (0 == ReadWaveData(startIndex, pointCount, pt, ref totalSize))
                {
                    for (int i = 0; i < 8; i++)
                    {
                        IntPtr ptr = (IntPtr)((uint)pt + i * pointCount * 2);

                        byte[] data = new byte[pointCount * 2]; //数组长度 = 每一个导联的数据长度(字节)


                        allLeadData.Add(data);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "错误信息记录");
            }
            finally
            {
                Marshal.FreeHGlobal(pt);  //释放内存
            }

            return allLeadData;
        }


        /// <summary>
        /// 获取所有8导联的数据
        /// </summary>
        /// <param name="startIndex">数据点开始位置</param>
        /// <param name="pointCount">数据点个数</param>
        /// <returns></returns>
        public static List<short[]> GetWaveData(int startIndex, int pointCount)
        {
            int readSize = pointCount * 2 * 8;
            IntPtr pt = Marshal.AllocHGlobal(readSize);
            List<short[]> allLeadData = new List<short[]>();

            try
            {
                int totalSize = 0;
                if (0 == ReadWaveData(startIndex, pointCount, pt, ref totalSize))
                {
                    for (int i = 0; i < 8; i++)
                    {
                        IntPtr ptr = (IntPtr)((uint)pt + i * pointCount * 2);

                        byte[] data = new byte[pointCount * 2]; //数组长度 = 每一个导联的数据长度(字节)
                        Marshal.Copy(ptr, data, 0, data.Length);//读出来
                        short[] oneLeadData = new short[pointCount];
                        for (int j = 0; j < pointCount; j++)
                        {
                            //oneLeadData[j] = (short)Marshal.PtrToStructure(ptr, typeof(short));
                            //ptr = (IntPtr)((uint)ptr + 2);
                            oneLeadData[j] = BitConverter.ToInt16(data, j * 2);
                        }

                        allLeadData.Add(oneLeadData);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex, "错误信息记录");
            }
            finally
            {
                Marshal.FreeHGlobal(pt);  //释放内存
            }

            return allLeadData;
        }

        public static short[] GetOneLeadWaveData()
        {
            short[] oneLeadData = null;
            long leadSize = GetOneLeadSize();

            //可以直接获取简单类型数组
            //int total = 0;
            //short[] arrData = new short[leadSize/2];
            //int result = ReadOneLeadData(1, arrData, ref total);

            IntPtr pt = Marshal.AllocHGlobal((int)leadSize);

            try
            {
                int totalSize = 0;
                if (0 == ReadOneLeadData(1, pt, ref totalSize))
                {

                    byte[] data = new byte[leadSize];
                    Marshal.Copy(pt, data, 0, data.Length);//读出来

                    int pointCount = (int)leadSize / 2;
                    oneLeadData = new short[pointCount];
                    for (int j = 0; j < pointCount; j++)
                    {
                        //oneLeadData[j] = (short)Marshal.PtrToStructure(pt, typeof(short));
                        //pt = (IntPtr)((uint)pt + 2);
                        oneLeadData[j] = BitConverter.ToInt16(data, j * 2);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                Marshal.FreeHGlobal(pt);  //释放内存
            }

            return oneLeadData;
        }



        public static void TestGetOneWaveData()
        {
            long leadSize = GetOneLeadSize();

            IntPtr pt = Marshal.AllocHGlobal((int)leadSize);
            //IntPtr[] ptArray = new IntPtr[1];
            //ptArray[0] = Marshal.AllocHGlobal((int)leadSize);
            //IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)) * 1);
            //Marshal.Copy(ptArray, 0, pt, 1);

            try
            {
                int totalSize = 0;
                int result = ReadOneLeadData(1, pt, ref totalSize);

                WaveDataMap waveData = (WaveDataMap)Marshal.PtrToStructure(pt, typeof(WaveDataMap));
            }
            catch (Exception ex)
            {
            }
            finally
            {
                //Marshal.FreeHGlobal(pBuff);  //释放内存
                // Marshal.FreeHGlobal(ptArray[0]);  //释放内存
                Marshal.FreeHGlobal(pt);  //释放内存
            }
        }

        public static void TestGetWaveData()
        {
            //WaveDataMap dataMap = new WaveDataMap();
            //for (int i = 0; i < DataMaps.Length; i++)
            //{
            //    DataMaps[i].DataValues = new short[500];
            //}

            int count = 8; //8导联数据
            int size = Marshal.SizeOf(typeof(WaveDataMap));
            IntPtr pt = Marshal.AllocHGlobal(size * count);

            //IntPtr[] ptArray = new IntPtr[1];
            //ptArray[0] = Marshal.AllocHGlobal(size * count);
            //IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)) * 1);
            //Marshal.Copy(ptArray, 0, pt, 1);

            try
            {
                int totalSize = 0;
                int result = ReadWaveData(0, 100, pt, ref totalSize);

                WaveDataMap[] waveMaps = new WaveDataMap[count];
                for (int i = 0; i < count; i++)
                {
                    IntPtr ptr = (IntPtr)((UInt32)pt + i * size);
                    waveMaps[i] = (WaveDataMap)Marshal.PtrToStructure(ptr, typeof(WaveDataMap));
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                //Marshal.FreeHGlobal(ptArray[0]);  //释放内存
                Marshal.FreeHGlobal(pt);  //释放内存
            }
        }

        public static void TestGetWaveData2(int index, int count)
        {
            //WaveDataMap dataMap = new WaveDataMap();
            int size = Marshal.SizeOf(typeof(WaveDataMap));
            byte[] bytes = new byte[size * count];


            int totalSize = 0;
            Logger.CreateLog($"开辟的内存缓冲区字节长度:{bytes.Length}", "测试读取波形数据");
            int result = ReadWaveData(1, 10, bytes, ref totalSize);

            Logger.CreateLog($"成功读取字节数据长度:{totalSize}", "测试读取波形数据");
            WaveDataMap dataMap = (WaveDataMap)bytesToStruct(bytes, typeof(WaveDataMap));
        }

        public static void TestGetWaveData3()
        {
            WaveDataMap[] dataMaps = new WaveDataMap[128];

            try
            {
                int totalSize = 0;
                //ReadWaveData(1, 10, ref waveDataMap, ref totalSize);
                int result = ReadWaveData(1, 10, ref dataMaps, ref totalSize);

                foreach (WaveDataMap dataMap in dataMaps)
                {

                }
            }
            catch (Exception ex)
            {
            }
        }

        private static object bytesToStruct(byte[] bytes, Type type)
        {
            int size = Marshal.SizeOf(type);//Get size of the struct or class.            
            if (bytes.Length < size)
            {
                return null;
            }
            IntPtr structPtr = Marshal.AllocHGlobal(size);//Allocate memory space of the struct or class.   
            Marshal.Copy(bytes, 0, structPtr, size);//Copy byte array to the memory space.  
            object obj = Marshal.PtrToStructure(structPtr, type);//Convert memory space to destination struct or class.           
            Marshal.FreeHGlobal(structPtr);//Release memory space.      
            return obj;
        }


        public static void TestGetWaveData22(int index, int count)
        {
            int readSize = count * 2 * 8;
            IntPtr pt = Marshal.AllocHGlobal(readSize);
            List<short[]> allLeadData = new List<short[]>();

            try
            {
                int totalSize = 0;
                int result = ReadWaveData(index, count, pt, ref totalSize);

                for (int i = 0; i < 8; i++)
                {
                    IntPtr ptr = (IntPtr)((uint)pt + i * count * 2);
                    short[] oneLeadData = new short[count];
                    for (int j = 0; j < count; j++)
                    {
                        oneLeadData[j] = (short)Marshal.PtrToStructure(ptr, typeof(short));
                        ptr = (IntPtr)((uint)ptr + 2);
                    }

                    allLeadData.Add(oneLeadData);
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                //Marshal.FreeHGlobal(ptArray[0]);  //释放内存
                Marshal.FreeHGlobal(pt);  //释放内存
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WaveDataMap
        {
            //[MarshalAs(UnmanagedType.LPStr, SizeConst = 50)]
            //public int key;

            //ArraySubType = UnmanagedType.I2,
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public short[] value;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WaveDataMap22
        {
            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
            public short[] value;
        }
    }

    public class PulseWaveDataHandler
    {
        private const string winDrivers = @"libnibp_dynamic_d_d.dll";

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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct nibpInData
        {
            public IntPtr preWave;       // 采集压力值数组
            public IntPtr stage;         // 状态数组
            public int len;           // 数据长度
            public int refPR;        // 脉率参考值
            public int refHR;        // 心率参考值
            public int cl;           // 测量值置信度
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct nibpOutExtractWave
        {
            public IntPtr dc;           // 原始信号dc值,单位为压力值
            public IntPtr ac;           // 原始信号ac值,单位为幅度
            public int len;             // 信号长度
            public IntPtr stepDc;       //台阶dc值数组，分配空间可设置为100
            public IntPtr stepDcLen;
            public int stepLen;         //台阶dc值个数
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
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
