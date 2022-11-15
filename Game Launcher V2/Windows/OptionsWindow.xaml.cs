using Game_Launcher_V2.Scripts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Game_Launcher_V2.Windows
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        //Get current working directory
        public static string path = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).LocalPath;

        public OptionsWindow()
        {
            InitializeComponent();

            //set up timer for sensor update
            DispatcherTimer sensor = new DispatcherTimer();
            sensor.Interval = TimeSpan.FromSeconds(2);
            sensor.Tick += Update_Tick;
            sensor.Start();

            _ = Tablet.TabletDevices;
            setUpGUI();

            PagesNavigation.Navigate(new System.Uri("Pages/OptionsWindow/PowerControl.xaml", UriKind.RelativeOrAbsolute));
        }

        private void setUpGUI()
        {
            var bi2 = new BitmapImage();

            string timeURL = "";

            timeURL = path + "//Assets//Icons//time-line.png";

            using (var stream = new FileStream(timeURL, FileMode.Open, FileAccess.Read))
            {
                bi2.BeginInit();
                bi2.DecodePixelWidth = 48;
                bi2.CacheOption = BitmapCacheOption.OnLoad;
                bi2.StreamSource = stream;
                bi2.EndInit();
            }
            bi2.Freeze();

            imgTime.Source = bi2;

            Time_and_Bat_Options.getBattery();
            Time_and_Bat_Options.getTime();
            Time_and_Bat_Options.getWifi(imgWiFi);
            Time_and_Bat_Options.updateBatTime(lblBat, lblTime, imgBat);
        }

        //Get battery and time info evry 2 seconds
        void Update_Tick(object sender, EventArgs e)
        {
            Time_and_Bat_Options.getWifi(imgWiFi);
            Time_and_Bat_Options.updateBatTime(lblBat, lblTime, imgBat);
        }
    }
}
