using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Zoncare.Utilities;

namespace Zoncare.Common
{
    public class ECG
    {
        private const int SMP_STATU_L = (0x0001);
        private const int SMP_STATU_F = (0x0002);
        private const int SMP_STATU_C1 = (0x0004);
        private const int SMP_STATU_C2 = (0x0008);
        private const int SMP_STATU_C3 = (0x0010);
        private const int SMP_STATU_C4 = (0x0020);
        private const int SMP_STATU_C5 = (0x0040);
        private const int SMP_STATU_C6 = (0x0080);
        private const int SMP_STATU_C7 = (0x0100);
        private const int SMP_STATU_C8 = (0x0200);
        private const int SMP_STATU_C9 = (0x0400);
        private const int SMP_STATU_C3r = (0x0800);
        private const int SMP_STATU_C4r = (0x1000);
        private const int SMP_STATU_C5r = (0x2000);

        public static bool CHECK_STATU_L_OFF(int statu) { return (statu & SMP_STATU_L) != 0; }
        public static bool CHECK_STATU_F_OFF(int statu) { return (statu & SMP_STATU_F) != 0; }
        public static bool CHECK_STATU_R_OFF(int statu) { return CHECK_STATU_L_OFF(statu) && CHECK_STATU_F_OFF(statu); }
        public static bool CHECK_STATU_N_OFF(int statu) { return CHECK_STATU_R_OFF(statu); }
        public static bool CHECK_STATU_C_OFF(int statu) { return (CHECK_STATU_N_OFF(statu) || CHECK_STATU_R_OFF(statu) || CHECK_STATU_L_OFF(statu) || CHECK_STATU_F_OFF(statu)); }

        public static bool CHECK_STATU_I_OFF(int statu) { return CHECK_STATU_N_OFF(statu) || CHECK_STATU_R_OFF(statu) || CHECK_STATU_L_OFF(statu); }
        public static bool CHECK_STATU_II_OFF(int statu) { return CHECK_STATU_N_OFF(statu) || CHECK_STATU_R_OFF(statu) || CHECK_STATU_F_OFF(statu); }
        public static bool CHECK_STATU_III_OFF(int statu) { return CHECK_STATU_N_OFF(statu) || CHECK_STATU_R_OFF(statu) || CHECK_STATU_L_OFF(statu) || CHECK_STATU_F_OFF(statu); }
        public static bool CHECK_STATU_AVR_OFF(int statu) { return (CHECK_STATU_N_OFF(statu) || CHECK_STATU_R_OFF(statu) || CHECK_STATU_L_OFF(statu) || CHECK_STATU_F_OFF(statu)); }
        public static bool CHECK_STATU_AVL_OFF(int statu) { return (CHECK_STATU_N_OFF(statu) || CHECK_STATU_R_OFF(statu) || CHECK_STATU_L_OFF(statu) || CHECK_STATU_F_OFF(statu)); }
        public static bool CHECK_STATU_AVF_OFF(int statu) { return (CHECK_STATU_N_OFF(statu) || CHECK_STATU_R_OFF(statu) || CHECK_STATU_L_OFF(statu) || CHECK_STATU_F_OFF(statu)); }

        public static bool CHECK_STATU_C1_OFF(int statu) { return ((statu & SMP_STATU_C1) != 0 || CHECK_STATU_C_OFF(statu)); }
        public static bool CHECK_STATU_C2_OFF(int statu) { return ((statu & SMP_STATU_C2) != 0 || CHECK_STATU_C_OFF(statu)); }
        public static bool CHECK_STATU_C3_OFF(int statu) { return ((statu & SMP_STATU_C3) != 0 || CHECK_STATU_C_OFF(statu)); }
        public static bool CHECK_STATU_C4_OFF(int statu) { return ((statu & SMP_STATU_C4) != 0 || CHECK_STATU_C_OFF(statu)); }
        public static bool CHECK_STATU_C5_OFF(int statu) { return ((statu & SMP_STATU_C5) != 0 || CHECK_STATU_C_OFF(statu)); }
        public static bool CHECK_STATU_C6_OFF(int statu) { return ((statu & SMP_STATU_C6) != 0 || CHECK_STATU_C_OFF(statu)); }
        public static bool CHECK_STATU_C7_OFF(int statu) { return ((statu & SMP_STATU_C7) != 0 || CHECK_STATU_C_OFF(statu)); }
        public static bool CHECK_STATU_C8_OFF(int statu) { return ((statu & SMP_STATU_C8) != 0 || CHECK_STATU_C_OFF(statu)); }
        public static bool CHECK_STATU_C9_OFF(int statu) { return ((statu & SMP_STATU_C9) != 0 || CHECK_STATU_C_OFF(statu)); }
        public static bool CHECK_STATU_C3r_OFF(int statu) { return ((statu & SMP_STATU_C3r) != 0 || CHECK_STATU_C_OFF(statu)); }
        public static bool CHECK_STATU_C4r_OFF(int statu) { return ((statu & SMP_STATU_C4r) != 0 || CHECK_STATU_C_OFF(statu)); }
        public static bool CHECK_STATU_C5r_OFF(int statu) { return ((statu & SMP_STATU_C5r) != 0 || CHECK_STATU_C_OFF(statu)); }

        private const int SMP_STATU_PACE = (0x4000);
        private const int SMP_STATU_OUT_OF_RANGE = (0x8000);

        public static bool CHECK_STATU_PACE(int statu) { return (SMP_STATU_PACE == (statu & 0xc000)); }
        public static bool CHECK_STATU_OUT_OF_RANGE(int statu) { return (SMP_STATU_OUT_OF_RANGE == (statu & 0xc000)); }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int fnCallBackFunc(ref string printInfo); //打印输出信息

        /// <summary>
        /// 获取导联线接线方式图
        /// </summary>
        /// <param name="leadMode"></param>
        /// <param name="leadSys"></param>
        /// <returns></returns>
        public static string GetLeadHintImage(int leadMode, Tissues leadSys)
        {
            string leadHintFile = string.Empty;

            switch (leadMode)
            {
                case 0: //6导
                    leadHintFile = "images\\leadhint\\lead6_0.png";
                    break;
                case 1: //12导
                    switch (leadSys)
                    {
                        case Tissues.STANDARD: leadHintFile = "images\\leadhint\\lead12_0.png"; break;
                        case Tissues.PARIES_POSTERIOR: leadHintFile = "images\\leadhint\\lead12_1.png"; break;
                        case Tissues.RIGHT_THORAX: leadHintFile = "images\\leadhint\\lead12_2.png"; break;
                        case Tissues.LEFT_THORAX: leadHintFile = "images\\leadhint\\lead12_3.png"; break;
                        case Tissues.BETWEEN_RIB: leadHintFile = "images\\leadhint\\lead12_4.png"; break;
                        case Tissues.NEXT_RIB: leadHintFile = "images\\leadhint\\lead12_5.png"; break;
                        case Tissues.FRANK: leadHintFile = "images\\leadhint\\lead12_6.png"; break;
                        case Tissues.CABRERA: leadHintFile = "images\\leadhint\\lead12_7.png"; break;
                        default: break;
                    }
                    break;
                case 2: //15导
                    leadHintFile = "images\\leadhint\\lead15_0.png";
                    break;
                case 3: //18导
                    leadHintFile = "images\\leadhint\\lead18_0.png";
                    break;
                case 4: //9导
                    leadHintFile = "images\\leadhint\\lead9_0.png";
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(leadHintFile))
            {
                leadHintFile = PubFuns.GetAbsolutePath(leadHintFile);
            }

            return leadHintFile;
        }

        /// <summary>
        /// 计算标准差
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double CalculateStdDev(IEnumerable<int> values)
        {
            double result = 0.0;
            if (values.Count<int>() > 0)
            {
                double avg = values.Average();
                double num = values.Sum((int x) => ((double)x - avg) * ((double)x - avg));
                if (num != 0.0)
                {
                    result = Math.Sqrt(num / (double)(values.Count<int>()));
                }
            }
            return Math.Round(result, 3, MidpointRounding.AwayFromZero);
        }
    }

    public class ECG_bluetooth
    {
        private const int SMP_STATU_L = (0x0001);
        private const int SMP_STATU_F = (0x0002);
        private const int SMP_STATU_C1 = (0x0004);
        private const int SMP_STATU_C2 = (0x0008);
        private const int SMP_STATU_C3 = (0x0010);
        private const int SMP_STATU_C4 = (0x0020);
        private const int SMP_STATU_C5 = (0x0040);
        private const int SMP_STATU_C6 = (0x0080);
        private const int SMP_STATU_C7 = (0x0100);
        private const int SMP_STATU_C8 = (0x0200);
        private const int SMP_STATU_C9 = (0x0400);
        private const int SMP_STATU_C3r = (0x0800);
        private const int SMP_STATU_C4r = (0x1000);
        private const int SMP_STATU_C5r = (0x2000);
        private const int SMP_STATU_N = (0x1000); //(0x10000); (蓝牙数据处理时已舍弃状态数据的第二字节)
        private const int SMP_STATU_R = (0x2000); //(0x20000);

        public static bool NLeadStatus;
        public static bool CHECK_STATU_L_OFF(int statu) { return (statu & SMP_STATU_L) != 0; }
        public static bool CHECK_STATU_F_OFF(int statu) { return (statu & SMP_STATU_F) != 0; }
        public static bool CHECK_STATU_R_OFF(int statu) { return (statu & SMP_STATU_R) != 0; }
        public static bool CHECK_STATU_N_OFF() { return NLeadStatus; }
        public static bool CHECK_STATU_N_OFF(int statu)
        {
            return ((statu & SMP_STATU_N) != 0);
        }
        public static bool CHECK_STATU_C_OFF(int statu) { return (CHECK_STATU_N_OFF() || CHECK_STATU_R_OFF(statu) || CHECK_STATU_L_OFF(statu) || CHECK_STATU_F_OFF(statu)); }

        public static bool CHECK_STATU_I_OFF(int statu) { return CHECK_STATU_N_OFF() || CHECK_STATU_R_OFF(statu) || CHECK_STATU_L_OFF(statu); }
        public static bool CHECK_STATU_II_OFF(int statu) { return CHECK_STATU_N_OFF() || CHECK_STATU_R_OFF(statu) || CHECK_STATU_F_OFF(statu); }
        public static bool CHECK_STATU_III_OFF(int statu) { return CHECK_STATU_N_OFF() || CHECK_STATU_R_OFF(statu) || CHECK_STATU_L_OFF(statu) || CHECK_STATU_F_OFF(statu); }
        public static bool CHECK_STATU_AVR_OFF(int statu) { return (CHECK_STATU_N_OFF() || CHECK_STATU_R_OFF(statu) || CHECK_STATU_L_OFF(statu) || CHECK_STATU_F_OFF(statu)); }
        public static bool CHECK_STATU_AVL_OFF(int statu) { return (CHECK_STATU_N_OFF() || CHECK_STATU_R_OFF(statu) || CHECK_STATU_L_OFF(statu) || CHECK_STATU_F_OFF(statu)); }
        public static bool CHECK_STATU_AVF_OFF(int statu) { return (CHECK_STATU_N_OFF() || CHECK_STATU_R_OFF(statu) || CHECK_STATU_L_OFF(statu) || CHECK_STATU_F_OFF(statu)); }

        public static bool CHECK_STATU_C1_OFF(int statu) { return ((statu & SMP_STATU_C1) != 0 || CHECK_STATU_C_OFF(statu)); }
        public static bool CHECK_STATU_C2_OFF(int statu) { return ((statu & SMP_STATU_C2) != 0 || CHECK_STATU_C_OFF(statu)); }
        public static bool CHECK_STATU_C3_OFF(int statu) { return ((statu & SMP_STATU_C3) != 0 || CHECK_STATU_C_OFF(statu)); }
        public static bool CHECK_STATU_C4_OFF(int statu) { return ((statu & SMP_STATU_C4) != 0 || CHECK_STATU_C_OFF(statu)); }
        public static bool CHECK_STATU_C5_OFF(int statu) { return ((statu & SMP_STATU_C5) != 0 || CHECK_STATU_C_OFF(statu)); }
        public static bool CHECK_STATU_C6_OFF(int statu) { return ((statu & SMP_STATU_C6) != 0 || CHECK_STATU_C_OFF(statu)); }
        public static bool CHECK_STATU_C7_OFF(int statu) { return ((statu & SMP_STATU_C7) != 0 || CHECK_STATU_C_OFF(statu)); }
        public static bool CHECK_STATU_C8_OFF(int statu) { return ((statu & SMP_STATU_C8) != 0 || CHECK_STATU_C_OFF(statu)); }
        public static bool CHECK_STATU_C9_OFF(int statu) { return ((statu & SMP_STATU_C9) != 0 || CHECK_STATU_C_OFF(statu)); }
        public static bool CHECK_STATU_C3r_OFF(int statu) { return ((statu & SMP_STATU_C3r) != 0 || CHECK_STATU_C_OFF(statu)); }
        public static bool CHECK_STATU_C4r_OFF(int statu) { return ((statu & SMP_STATU_C4r) != 0 || CHECK_STATU_C_OFF(statu)); }
        public static bool CHECK_STATU_C5r_OFF(int statu) { return ((statu & SMP_STATU_C5r) != 0 || CHECK_STATU_C_OFF(statu)); }

        private const int SMP_STATU_PACE = (0x40000);
        public static bool CHECK_STATU_PACE(int statu) { return (statu & SMP_STATU_PACE) != 0; }
    }

}
