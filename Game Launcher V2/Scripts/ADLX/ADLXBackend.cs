using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Game_Launcher_V2.Scripts.ADLX
{
    internal class ADLXBackend
    {
        public const string CppFunctionsDLL0 = @"ADLX_PerformanceMetrics.dll";
        public const string CppFunctionsDLL1 = @"ADLX_3DSettings.dll";

        [DllImport(CppFunctionsDLL0, CallingConvention = CallingConvention.Cdecl)] public static extern int GetFPSData();

        [DllImport(CppFunctionsDLL0, CallingConvention = CallingConvention.Cdecl)] public static extern int GetGPUMetrics(int GPU, int Sensor);
        [DllImport(CppFunctionsDLL1, CallingConvention = CallingConvention.Cdecl)] public static extern int SetFPSLimit(int GPU, bool isEnabled, int FPS);
    }
}
