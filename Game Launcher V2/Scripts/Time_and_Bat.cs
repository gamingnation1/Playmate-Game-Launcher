using Game_Launcher_V2.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Game_Launcher_V2.Scripts
{
    internal class Time_and_Bat_Options
    {
        public static string batPercent = "";
        public static int batPercentInt = 0;
        public static string time = "";
        public static UInt16 statuscode = 0;
        private static string path = Global.path;

        //Pull battery sensor info from Windows
        public async static Task getBattery()
        {
            int batteryLife = 0;
            try
            {
                ManagementClass wmi = new ManagementClass("Win32_Battery");
                ManagementObjectCollection allBatteries = wmi.GetInstances();

                double batteryLevel = 0;

                //Get battery level from each system battery detected
                foreach (var battery in allBatteries)
                {
                    batteryLevel = Convert.ToDouble(battery["EstimatedChargeRemaining"]);
                    statuscode = (UInt16)battery["BatteryStatus"];
                }
                //Set battery level as an int
                batteryLife = (int)batteryLevel;
                batPercentInt = batteryLife;

                //Update battery level string
                batPercent = batteryLife.ToString() + "%";
            }
            catch (Exception ex)
            {

            }
        }

        public async static Task getTime()
        {
            //Get current time
            DateTime currentTime = DateTime.Now;
            time = currentTime.ToString("HH:mm");
        }

        public static double lastWiFi = 0;
        public static double RetrieveSignalString()
        {
            try
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "netsh.exe";
                p.StartInfo.Arguments = "wlan show interfaces";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.Start();

                string s = p.StandardOutput.ReadToEnd();
                string s2 = s.Substring(s.IndexOf("Signal"));
                s2 = s2.Substring(s2.IndexOf(":"));
                s2 = s2.Substring(2, s2.IndexOf("\n")).Trim();
                s2 = s2.Replace("%", "");
                double singal = Convert.ToDouble(s2);
                p.WaitForExit();
                lastWiFi = singal;
                return singal;
            }
            catch (Exception ex)
            {
                return lastWiFi;
            }
        }

        static string lastWifi;
        public static async void getWifi(Image imgWiFi)
        {
            var bi = new BitmapImage();

            string wifiURL = "";
            double wifi = await Task.Run(() => Time_and_Bat.RetrieveSignalString());

            if (wifi > 75)
            {
                wifiURL = path + "//Assets//Icons//signal-wifi-fill.png";
            }
            if (wifi < 75 && wifi > 45)
            {
                wifiURL = path + "//Assets//Icons//signal-wifi-2-fill.png";
            }
            if (wifi < 45)
            {
                wifiURL = path + "//Assets//Icons//signal-wifi-1-fill.png";
            }

            if (wifiURL != lastWifi)
            {
                using (var stream = new FileStream(wifiURL, FileMode.Open, FileAccess.Read))
                {
                    bi.BeginInit();
                    bi.DecodePixelWidth = 48;
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.StreamSource = stream;
                    bi.EndInit();
                }

                bi.Freeze();
                imgWiFi.Source = bi;

                lastWifi = wifiURL;
            }
        }

        static string lastBattery;
        public async static void updateBatTime(TextBlock lblBat, TextBlock lblTime, Image imgBat)
        {
            try
            {
                await Task.Run(() => getBattery());
                await Task.Run(() => getTime());

                //Update battery and time text blocks
                lblBat.Text = Time_and_Bat.batPercent;
                lblTime.Text = Time_and_Bat.time;

                var bi = new BitmapImage();
                var bi2 = new BitmapImage();

                string batURL = "";


                //Update battery icon based on battery level
                if (Convert.ToInt32(batPercentInt) > 50)
                {
                    batURL = path + "//Assets//Icons//battery-fill.png";
                }
                if (Convert.ToInt32(batPercentInt) < 45)
                {
                    batURL = path + "//Assets//Icons//battery-low-line.png";
                }

                if (statuscode == 2 || statuscode == 6 || statuscode == 7 || statuscode == 8)
                {
                    batURL = path + "//Assets//Icons//battery-charge-line.png";
                }

                if (batURL != lastBattery)
                {
                    using (var stream = new FileStream(batURL, FileMode.Open, FileAccess.Read))
                    {
                        bi.BeginInit();
                        bi.DecodePixelWidth = 48;
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.StreamSource = stream;
                        bi.EndInit();
                    }
                    bi.Freeze();

                    imgBat.Source = bi;
                    lastBattery = batURL;
                }
            }
            catch(Exception ex) { path = Settings.Default.Path; }
        }
    }
}
