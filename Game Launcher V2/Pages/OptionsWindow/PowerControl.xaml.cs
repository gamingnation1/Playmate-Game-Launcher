using ControlzEx.Theming;
using Game_Launcher_V2.Scripts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Game_Launcher_V2.Scripts.OptionsWindow.PowerControl;
using SharpDX.XInput;
using Game_Launcher_V2.Properties;
using System.Drawing;
using UXTU.Scripts.Intel;
using Microsoft.VisualBasic;

namespace Game_Launcher_V2.Pages.OptionsWindow
{
    /// <summary>
    /// Interaction logic for PowerControl.xaml
    /// </summary>
    public partial class PowerControl : Page
    {
        //Get current working directory
        public static string path = Global.path;

        public PowerControl()
        {
            InitializeComponent();
            _ = Tablet.TabletDevices;


            if (Settings.Default.CPUName.ToLower().Contains("intel"))
            {
                Section2.Visibility= Visibility.Collapsed;
                Section7.Visibility = Visibility.Collapsed;
            }

            sdPower.Value = Settings.Default.PowerLimit;
            sdTemp.Value = Settings.Default.TempLimit;
            sdGFXClock.Value = Settings.Default.iGFXClk;
            sdCOOffset.Value = Settings.Default.COCPU;


            if(Settings.Default.isTemp == true) tsTemp.IsOn = true;
            if (Settings.Default.isPower == true) tsPower.IsOn = true;
            if (Settings.Default.isBoost == true) tsCPUClk.IsOn = true;
            if (Settings.Default.isiGFX == true) tsGPU.IsOn = true;

            //set up timer for sensor update
            DispatcherTimer sensor = new DispatcherTimer();
            sensor.Interval = TimeSpan.FromSeconds(2);
            sensor.Tick += Update_Tick;
            sensor.Start();

            

            lblBat.Text = Time_and_Bat.batPercent;

            getBatteryTime();
            updateBatIcon();

            ThemeManager.Current.ChangeTheme(this, "Dark.Teal");

            isFirstBoot = false;
        }

        void Update_Tick(object sender, EventArgs e)
        {
            if(this.Visibility == Visibility.Visible)
            {
                lblBat.Text = Time_and_Bat.batPercent;

                getBatteryTime();
                updateBatIcon();
            }
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            applySettings();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void Toggle_Toggled(object sender, RoutedEventArgs e)
        {
            updateGUI();
        }

        bool isFirstBoot = true;
        private void updateGUI()
        {
            if (tsTemp.IsOn == true) Section3.Visibility = Visibility.Visible; else Section3.Visibility = Visibility.Collapsed;
            if (tsPower.IsOn == true) Section5.Visibility = Visibility.Visible; else Section5.Visibility = Visibility.Collapsed;
            if (tsGPU.IsOn == true) Section8.Visibility = Visibility.Visible; else Section8.Visibility = Visibility.Collapsed;
            if (tsCPUCO.IsOn == true) Section10.Visibility = Visibility.Visible; else Section10.Visibility = Visibility.Collapsed;
            
            if(isFirstBoot == false)
            {
                Settings.Default.isTemp = tsTemp.IsOn;
                Settings.Default.isPower = tsPower.IsOn;
                Settings.Default.isBoost = tsCPUClk.IsOn;
                Settings.Default.isiGFX= tsGPU.IsOn;
                Settings.Default.Save();
            }

            applySettings();
        }
        private void tsCPUClk_Toggled(object sender, RoutedEventArgs e)
        {
            if (tsCPUClk.IsOn == true) CPUboost.cpuBoost(false); else CPUboost.cpuBoost(true);
        }

        private async void applySettings()
        {
            try
            {
                if (!Settings.Default.CPUName.ToLower().Contains("intel"))
                {
                    string processRyzenAdj = "";
                    string result = "";
                    string commandArguments = "";

                    int TDP = (int)sdPower.Value;
                    int Temp = (int)sdTemp.Value;
                    TDP = TDP * 1000;
                    int iGFX = (int)sdGFXClock.Value;

                    if (tsTemp.IsOn == true) commandArguments = $"--tctl-temp={Temp} --skin-temp-limit={Temp} ";
                    if (tsPower.IsOn == true) commandArguments = commandArguments + $"--stapm-limit={TDP} --slow-limit={TDP} --fast-limit={TDP} --vrm-current={TDP * 1.33} --vrmmax-current={TDP * 1.33} ";
                    if (tsGPU.IsOn == true) commandArguments = commandArguments + $"--gfx-clk={iGFX} ";

                    Global.RyzenAdj = commandArguments;

                    Settings.Default.TempLimit = Temp;
                    Settings.Default.PowerLimit = (int)sdPower.Value;
                    Settings.Default.iGFXClk = iGFX;
                    Settings.Default.COCPU = 0;
                    Settings.Default.Save();

                    processRyzenAdj = "\\bin\\AMD\\ryzenadj.exe";
                    RunCLI.ApplySettings(processRyzenAdj, commandArguments, true);
                }
                else
                {
                    Settings.Default.PowerLimit = (int)sdPower.Value;
                    Settings.Default.Save();
                    int TDP = (int)sdPower.Value;
                    await Task.Run(() =>
                    {
                        ChangeTDP.changeTDP(TDP, TDP);
                    });
                }          
            }
            catch (Exception ex) { }
        }

        static string lastBat = "";
        public void updateBatIcon()
        {
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
                imgBat.Source = new BitmapImage(new Uri(batURL));
                lastBat = batURL;
        }

        public static TimeSpan time;
        public static float batTime;


        public void getBatteryTime()
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
