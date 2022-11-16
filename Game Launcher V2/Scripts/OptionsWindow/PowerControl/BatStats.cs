using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Game_Launcher_V2.Scripts.OptionsWindow.PowerControl
{
    class BatStats
    {
        static string path = Global.path;
        static string lastBat = "";
        public static void updateBatIcon(Image imgBat)
        {
            var bi = new BitmapImage();

            string batURL = "";

            //Update battery icon based on battery level

            //Update battery icon based on battery level
            if (Convert.ToInt32(Time_and_Bat.batPercentInt) > 50)
            {
                batURL = path + "//Assets//Icons//battery-fill-vert.png";
            }
            if (Convert.ToInt32(Time_and_Bat.batPercentInt) < 45)
            {
                batURL = path + "//Assets//Icons//battery-low-line-vert.png";
            }

            if (Time_and_Bat.statuscode == 2 || Time_and_Bat.statuscode == 6 || Time_and_Bat.statuscode == 7 || Time_and_Bat.statuscode == 8)
            {
                batURL = path + "//Assets//Icons//battery-charge-line-vert.png";
            }

            if (batURL != lastBat)
            {
                using (var stream = new FileStream(batURL, FileMode.Open, FileAccess.Read))
                {
                    bi.BeginInit();
                    bi.DecodePixelWidth = 256;
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.StreamSource = stream;
                    bi.EndInit();
                }
                bi.Freeze();

                imgBat.Source = bi;
                lastBat = batURL;
            }
        }

        public static TimeSpan time;
        public static float batTime;


        public static void getBatteryTime(TextBlock lblBatTime)
        {
            PowerStatus pwr = System.Windows.Forms.SystemInformation.PowerStatus;
            //Get battery life

            batTime = (float)pwr.BatteryLifeRemaining;

            bool isCharging = false;

            if (Time_and_Bat.statuscode == 2 || Time_and_Bat.statuscode == 6 || Time_and_Bat.statuscode == 7 || Time_and_Bat.statuscode == 8)
            {
                batTime = 0;
                isCharging = true;
            }
            time = TimeSpan.FromSeconds(batTime);

            lblBatTime.Text = $"{time:%h} Hours {time:%m} Minutes Remaining";

            if (lblBatTime.Text == "0 Hours 0 Minutes Remaining" && isCharging == true) lblBatTime.Text = "Battery Charging";
            if (lblBatTime.Text == "0 Hours 0 Minutes Remaining" && isCharging == false) lblBatTime.Text = "Calculating";
        }
    }
}
