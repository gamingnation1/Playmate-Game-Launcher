using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Game_Launcher_V2.Scripts
{
    class RunCLI
    {
        public static string RunCommand(string arguments, bool readOutput, string processName = "cmd.exe")
        {
            //Runs CLI, if readOutput is true then returns output
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.UseShellExecute = false;
                if (readOutput) { startInfo.RedirectStandardOutput = true; } else { startInfo.RedirectStandardOutput = false; }
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = processName;
                startInfo.Arguments = "/c " + arguments;
                startInfo.Verb = "runas";
                process.StartInfo = startInfo;
                startInfo.CreateNoWindow = true;
                process.Start();
                if (readOutput)
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    return output;

                }
                else
                {
                    process.WaitForExit();
                    return "COMPLETE";
                }


            }
            catch (Exception ex)
            {
                return "Error running CLI: " + ex.Message + " " + arguments;
            }
        }

        public static async void ApplySettings(string program, string input, bool isHidden)
        {
            try
            {
                await Task.Run(() =>
                {
                    //Get current path and join it with program executable
                    string path = Global.path + program;
                    //Create new process
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    //Hide program if required
                    if (isHidden == true)
                    {
                        startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    }
                    //Pass on path and arguments
                    startInfo.FileName = path;
                    startInfo.Arguments = input;
                    process.StartInfo = startInfo;
                    //Start program
                    process.Start();

                    Thread.Sleep(10000);
                    process.Close();
                    return;
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
        }
    }
}
