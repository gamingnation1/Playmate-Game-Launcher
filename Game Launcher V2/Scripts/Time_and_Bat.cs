using Game_Launcher_V2.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Game_Launcher_V2.Scripts
{
    internal class Time_and_Bat
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

        public static double RetrieveSignalStrength()
        {
            try
            {
                int wifiStrengthPercentage = 0;
                long bytesReceived = 0;
                long bytesThreshold = 10000000; // adjust this threshold to suit your needs

                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    // Only consider Wireless network interfaces
                    if (nic.NetworkInterfaceType != NetworkInterfaceType.Wireless80211) continue;

                    IPv4InterfaceStatistics statistics = nic.GetIPv4Statistics();
                    bytesReceived += statistics.BytesReceived;
                }

                // Calculate the Wi-Fi signal strength as a percentage
                if (bytesReceived < bytesThreshold)
                {
                    wifiStrengthPercentage = (int)((bytesReceived / (double)bytesThreshold) * 100);
                }
                else
                {
                    wifiStrengthPercentage = 100;
                }

                return wifiStrengthPercentage;
            } catch
            {
                return 100;
            }
        }

        public static async void GetWifi(Image imgWiFi)
        {
            var bi = new BitmapImage();

            string wifiURL = "";
            double wifi = Global.wifi;

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

            if (imgWiFi.Source != null && imgWiFi.Source is BitmapImage bitmapImage)
            {
                if (bitmapImage.UriSource?.LocalPath == wifiURL)
                {
                    return;
                }
            }

            try
            {
                bi.BeginInit();
                bi.UriSource = new Uri(wifiURL, UriKind.RelativeOrAbsolute);
                bi.DecodePixelWidth = 48;
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();

                bi.Freeze();
                imgWiFi.Source = bi;
            }
            catch (Exception ex)
            {
                // Handle any exceptions here
            }
        }

        static string lastBattery;
        public async static void updateBatTime(TextBlock lblBat, TextBlock lblTime, Image imgBat)
        {
            try
            {
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

                if (imgBat.Source != null && imgBat.Source is BitmapImage bitmapImage)
                {
                    if (bitmapImage.UriSource?.LocalPath == batURL)
                    {
                        return;
                    }
                }

                try
                {
                    bi.BeginInit();
                    bi.UriSource = new Uri(batURL, UriKind.RelativeOrAbsolute);
                    bi.DecodePixelWidth = 48;
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.EndInit();

                    bi.Freeze();
                    imgBat.Source = bi;
                }
                catch
                {

                }
            }
            catch (Exception ex) { path = Settings.Default.Path; }
        }
    }
}
