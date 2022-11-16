using Game_Launcher_V2.Scripts;
using SharpDX.XInput;
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

            this.WindowState = System.Windows.WindowState.Maximized;
            this.Hide();

            //set up timer for sensor update
            DispatcherTimer sensor = new DispatcherTimer();
            sensor.Interval = TimeSpan.FromSeconds(2);
            sensor.Tick += Update_Tick;
            sensor.Start();

            //set up timer for key combo system
            DispatcherTimer checkKeyInput = new DispatcherTimer();
            checkKeyInput.Interval = TimeSpan.FromSeconds(0.115);
            checkKeyInput.Tick += KeyShortCuts_Tick;
            checkKeyInput.Start();

            _ = Tablet.TabletDevices;
            setUpGUI();

            PagesNavigation.Navigate(new System.Uri("Pages/OptionsWindow/PowerControl.xaml", UriKind.RelativeOrAbsolute));
        }

        private static Controller controller;

        public static bool hidden = true;
        void KeyShortCuts_Tick(object sender, EventArgs e)
        {
            //Get controller
            controller = new Controller(UserIndex.One);

            bool connected = controller.IsConnected;

            if (connected)
            {
                //get controller state
                var state = controller.GetState();

                //detect if keyboard or controller combo is being activated
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                {
                    //if hidden show window
                    if (hidden == false)
                    {
                        hidden = true;
                        this.Hide();
                    }
                    //else hide window
                    else
                    {
                        hidden = false;
                        this.Show();
                    }
                }

                //if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                //{
                //    SendKeys.SendWait("%F");
                //}
            }
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
