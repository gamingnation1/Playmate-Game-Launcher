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
        public const string CppFunctionsDLL1 = @"Graphics3DSettings.dll";

        [DllImport(CppFunctionsDLL1, CallingConvention = CallingConvention.Cdecl)] public static extern int SetFPSLimit(int GPU, bool isEnabled, int FPS);
    }
}
