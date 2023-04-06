using Game_Launcher_V2.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Launcher_V2.Scripts.OptionsWindow.PowerControl
{
    class CPUboost
    {
        public static async void cpuBoost(bool isEnabled)
        {
            int MaxCPUPerf = 100;
            int boostMode = 1;

            if (isEnabled == false)
            {
                //CPUPerf
                MaxCPUPerf = 99;
                boostMode = 0;
                Settings.Default.isBoost = true;
            }
            else
            {
                MaxCPUPerf = 100;
                boostMode = 1;
                Settings.Default.isBoost = false;
            }

            Settings.Default.Save();

            int i = 0;

            await Task.Run(() =>
            {
                do
                {
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    startInfo.CreateNoWindow= true;

                    if(i == 1)
                    {
                        startInfo.FileName = "cmd.exe";
                        startInfo.Arguments = "/c powercfg -setacvalueindex scheme_current sub_processor PERFBOOSTMODE " + boostMode;
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(300);
                    }
                    
                    if(i == 2)
                    {
                        startInfo.FileName = "cmd.exe";
                        startInfo.Arguments = "/c powercfg -setacvalueindex scheme_current sub_processor PROCTHROTTLEMAX " + MaxCPUPerf;
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(300);
                    }
                    
                    if (i == 3)
                    {
                        startInfo.FileName = "cmd.exe";
                        startInfo.Arguments = "/c powercfg -setdcvalueindex scheme_current sub_processor PERFBOOSTMODE " + boostMode;
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(300);
                    }
                    
                    if (i == 4)
                    {
                        startInfo.FileName = "cmd.exe";
                        startInfo.Arguments = "/c powercfg -setdcvalueindex scheme_current sub_processor PROCTHROTTLEMAX " + MaxCPUPerf;
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(300);
                    }
                    
                    if(i == 5)
                    {
                        startInfo.FileName = "cmd.exe";
                        startInfo.Arguments = "/c powercfg /setactive scheme_current";
                        process.StartInfo = startInfo;
                        process.Start();
                        System.Threading.Thread.Sleep(300);
                    }

                    i++;
                } while (i < 6);
            });
        }

        public async static void HideAttribute(string subGroup, string attribute)
        {
            await Task.Run(() =>
            {
                // Execute the "powercfg -attributes" command to hide the attribute
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powercfg",
                        Arguments = $"-attributes {subGroup} {attribute} -ATTRIB_HIDE",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                process.Start();
                process.WaitForExit();
            });
        }

        public async static void SetPowerValue(string scheme, string subGroup, string powerSetting, uint value, bool isAC)
        {
            await Task.Run(() =>
            {
                // Execute the "powercfg /setacvalueindex" or "powercfg /setdcvalueindex" command to set the power value
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powercfg",
                        Arguments = $"/set{(isAC ? "ac" : "dc")}valueindex {scheme} {subGroup} {powerSetting} {value}",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                process.Start();
                process.WaitForExit();
            });
        }

        public async static void SetActiveScheme(string scheme)
        {
            await Task.Run(() =>
            {
                // Execute the "powercfg /setactive" command to activate the power scheme
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powercfg",
                        Arguments = $"/setactive {scheme}",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                process.Start();
                process.WaitForExit();
            });
        }
    }
}
