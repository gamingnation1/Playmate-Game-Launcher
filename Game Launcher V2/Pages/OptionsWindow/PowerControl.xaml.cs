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
using Brush = System.Windows.Media.Brush;

namespace Game_Launcher_V2.Pages.OptionsWindow
{
    /// <summary>
    /// Interaction logic for PowerControl.xaml
    /// </summary>
    public partial class PowerControl : Page
    {
        //Get current working directory
        public static string path = Global.path;
        public DispatcherTimer checkKeyInput = new DispatcherTimer();
        public DispatcherTimer sensor = new DispatcherTimer();
        public PowerControl()
        {
            InitializeComponent();
            _ = Tablet.TabletDevices;

            sdPower.Value = Settings.Default.PowerLimit;
            sdTemp.Value = Settings.Default.TempLimit;
            sdGFXClock.Value = Settings.Default.iGFXClk;
            sdCOOffset.Value = Settings.Default.COCPU;


            if(Settings.Default.isTemp == true) tsTemp.IsOn = true;
            if (Settings.Default.isPower == true) tsPower.IsOn = true;
            if (Settings.Default.isBoost == true) tsCPUClk.IsOn = true;
            if (Settings.Default.isiGFX == true) tsGPU.IsOn = true;

            if (Settings.Default.CPUName.ToLower().Contains("intel"))
            {
                Section2.Visibility = Visibility.Collapsed;
                Section7.Visibility = Visibility.Collapsed;
                tsTemp.IsOn = false;
                tsGPU.IsOn = false;
            }

            //set up timer for sensor update
            sensor.Interval = TimeSpan.FromSeconds(2);
            sensor.Tick += Update_Tick;
            sensor.Start();

            lblBat.Text = Time_and_Bat.batPercent;

            getBatteryTime();
            updateBatIcon();

            ThemeManager.Current.ChangeTheme(this, "Dark.Teal");

            //set up timer for key combo system
            checkKeyInput.Interval = TimeSpan.FromSeconds(0.117);
            checkKeyInput.Tick += KeyShortCuts_Tick;
            checkKeyInput.Start();

            lastBorder = Section2;

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

                    Settings.Default.TempLimit = (int)sdTemp.Value;
                    Settings.Default.PowerLimit = (int)sdPower.Value;
                    Settings.Default.iGFXClk = (int)sdGFXClock.Value;
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

        private static Controller controller;

        int optionSelected = 0;
        bool isActive = false;
        bool connected;
        public static Border[] borders;
        public static Border lastBorder;
        bool goingDown = true;

        async void KeyShortCuts_Tick(object sender, EventArgs e)
        {

            if (Settings.Default.CPUName.ToLower().Contains("intel"))
            {
                borders = new Border[] { Section4, Section5, Section6};
            }
            else
            {
                borders = new Border[] { Section2, Section3, Section4, Section5, Section6, Section7, Section8 };
            }

            if(Global.AccessMenuSelected == 1)
            {
                try
                {
                    //Get controller
                    controller = new Controller(UserIndex.One);

                    connected = controller.IsConnected;

                    if (connected)
                    {
                        //get controller state
                        var state = controller.GetState();

                        //detect if keyboard or controller combo is being activated
                        if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                        {
                            Global.shortCut = true;
                        }

                        if (Global.isAccessMenuOpen == true && Global.shortCut == false)
                        {
                            if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp) && Global.shortCut == false && isActive == false)
                            {
                                if (optionSelected > 0) optionSelected--;
                                else optionSelected = 0;
                                goingDown = false;
                            }

                            if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown) && Global.shortCut == false && isActive == false)
                            {
                                if (optionSelected < 6) optionSelected++;
                                else optionSelected = 6;
                                goingDown = true;
                            }

                            if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) && Global.shortCut == false && isActive == true)
                            {
                                int value = 0;
                                int maxValue = 0;
                                int minValue = 0;
                                Slider selectedSlider = null;

                                if (borders[optionSelected] == Section3) selectedSlider = sdTemp;
                                if (borders[optionSelected] == Section5) selectedSlider = sdPower;
                                if (borders[optionSelected] == Section8) selectedSlider = sdGFXClock;

                                value = (int)selectedSlider.Value;
                                maxValue = (int)selectedSlider.Maximum;
                                minValue = (int)selectedSlider.Minimum;

                                if (borders[optionSelected] == Section8) value = value - 50;
                                else
                                {
                                    value--;
                                    value--;
                                }

                                if (value > maxValue) value = maxValue;
                                if (value < minValue) value = minValue;
                                selectedSlider.Value = value;

                                applySettings();
                            }

                            if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) && Global.shortCut == false && isActive == true)
                            {
                                int value = 0;
                                int maxValue = 0;
                                int minValue = 0;
                                Slider selectedSlider = null;

                                if (borders[optionSelected] == Section3) selectedSlider = sdTemp;
                                if (borders[optionSelected] == Section5) selectedSlider = sdPower;
                                if (borders[optionSelected] == Section8) selectedSlider = sdGFXClock;

                                value = (int)selectedSlider.Value;
                                maxValue = (int)selectedSlider.Maximum;
                                minValue = (int)selectedSlider.Minimum;

                                if (borders[optionSelected] == Section8) value = value + 50;
                                else
                                {
                                    value++;
                                    value++;
                                }


                                if (value > maxValue) value = maxValue;
                                if (value < minValue) value = minValue;
                                selectedSlider.Value = value;
                                applySettings();
                            }


                            if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A) && Global.shortCut == false)
                            {
                                if (borders[optionSelected].Visibility == Visibility.Visible && borders[optionSelected] == Section2 && tsTemp.IsOn == false) tsTemp.IsOn= true;
                                else if (borders[optionSelected].Visibility == Visibility.Visible && borders[optionSelected] == Section2 && tsTemp.IsOn == true) tsTemp.IsOn = false;

                                if (borders[optionSelected].Visibility == Visibility.Visible && borders[optionSelected] == Section4 && tsPower.IsOn == false) tsPower.IsOn = true;
                                else if (borders[optionSelected].Visibility == Visibility.Visible && borders[optionSelected] == Section4 && tsPower.IsOn == true) tsPower.IsOn = false;

                                if (borders[optionSelected] == Section6 && tsCPUClk.IsOn == false) tsCPUClk.IsOn = true;
                                else if (borders[optionSelected].Visibility == Visibility.Visible && borders[optionSelected] == Section6 && tsCPUClk.IsOn == true) tsCPUClk.IsOn = false;

                                if (borders[optionSelected].Visibility == Visibility.Visible && borders[optionSelected] == Section7 && tsGPU.IsOn == false) tsGPU.IsOn = true;
                                else if (borders[optionSelected].Visibility == Visibility.Visible && borders[optionSelected] == Section7 && tsGPU.IsOn == true) tsGPU.IsOn = false;


                                if (borders[optionSelected] == Section3 && isActive == false) isActive = true;
                                else if (borders[optionSelected] == Section3 && isActive == true) isActive = false;

                                if (borders[optionSelected] == Section5 && isActive == false) isActive = true;
                                else if (borders[optionSelected] == Section5 && isActive == true) isActive = false;

                                if (borders[optionSelected] == Section8 && isActive == false) isActive = true;
                                else if (borders[optionSelected] == Section8 && isActive == true) isActive = false;
                            }
                        }
                    }
                    updateMenuSelected();
                }
                catch { }
            }
            else
            {
                checkKeyInput.Stop();
                sensor.Stop();
            }
        }

        public void updateMenuSelected()
        {
            var bc = new BrushConverter();
            Section2.Background = new SolidColorBrush(Colors.Transparent);
            Section3.Background = new SolidColorBrush(Colors.Transparent);
            Section3.BorderThickness = new Thickness(0, 0.5, 0.5, 0.5);
            Section4.Background = new SolidColorBrush(Colors.Transparent);
            Section5.Background = new SolidColorBrush(Colors.Transparent);
            Section5.BorderThickness = new Thickness(0, 0.5, 0.5, 0.5);
            Section6.Background = new SolidColorBrush(Colors.Transparent);
            Section7.Background = new SolidColorBrush(Colors.Transparent);
            Section8.Background = new SolidColorBrush(Colors.Transparent);
            Section8.BorderThickness = new Thickness(0, 0.5, 0.5, 0.5);

            if (connected)
            {
                if (lastBorder.Visibility == Visibility.Collapsed && isActive == true) { isActive = false; optionSelected--; }

                if (goingDown)
                {
                    if (borders[optionSelected].Visibility == Visibility.Collapsed) optionSelected++;
                    if (borders[optionSelected].Visibility == Visibility.Collapsed) optionSelected++;
                }
                else
                {
                    if (borders[optionSelected].Visibility == Visibility.Collapsed) optionSelected--;
                    if (borders[optionSelected].Visibility == Visibility.Collapsed) optionSelected--;
                }

                if (borders[optionSelected] == Section8 && tsGPU.IsOn == false) { optionSelected = 5;}
                if (borders[optionSelected] == Section8 && tsGPU.IsOn == false && lastBorder == Section8) { isActive = false; }

                borders[optionSelected].Background = (Brush)bc.ConvertFrom("#F2252525");
                lastBorder= borders[optionSelected];

                if (isActive == true)
                {
                    borders[optionSelected].BorderThickness = new Thickness(2.5);
                }

                Settings.Default.TempLimit = (int)sdTemp.Value;
                Settings.Default.PowerLimit = (int)sdPower.Value;
                Settings.Default.iGFXClk = (int)sdGFXClock.Value;
                Settings.Default.COCPU = 0;
                Settings.Default.Save();
            }
        }
    }
}
