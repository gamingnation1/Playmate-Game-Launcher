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
using Game_Launcher_V2.Scripts.ADLX;
using ControlzEx.Standard;
using System.Xml;
using System.DirectoryServices.ActiveDirectory;
using System.Diagnostics.Eventing.Reader;

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
            sdFPSLimit.Value = Settings.Default.fpsLimit;
            sdCPUClock.Value = Settings.Default.CpuClk;
            sdEPP.Value = Settings.Default.EPP;

            if (Settings.Default.isTemp == true) tsTemp.IsOn = true;
            if (Settings.Default.isPower == true) tsPower.IsOn = true;
            if (Settings.Default.isBoost == true) tsCPUClk.IsOn = true;
            if (Settings.Default.isiGFX == true) tsGPU.IsOn = true;
            if (Settings.Default.isFPSLimit == true) tsFPS.IsOn = true;
            if (Settings.Default.isCPUClk == true) tsCPU.IsOn = true;
            if (Settings.Default.isEPP == true) tsEPP.IsOn = true;

            if (Settings.Default.CPUName.ToLower().Contains("intel"))
            {
                Section2.Visibility = Visibility.Collapsed;
                Section7.Visibility = Visibility.Collapsed;
                Section9.Visibility = Visibility.Collapsed;
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
            checkKeyInput.Interval = TimeSpan.FromSeconds(0.1);
            checkKeyInput.Tick += KeyShortCuts_Tick;
            checkKeyInput.Start();

            lastBorder = Section2;

            isFirstBoot = false;
        }

        void Update_Tick(object sender, EventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                lblBat.Text = Time_and_Bat.batPercent;

                getBatteryTime();
                updateBatIcon();
            }
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (isFirstBoot == false)
            {
                applySettings();
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isFirstBoot == false)
            {
                applySettings();
            }
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
            if (tsCPU.IsOn == true) Section062.Visibility = Visibility.Visible; else Section062.Visibility = Visibility.Collapsed;
            if (tsGPU.IsOn == true) Section8.Visibility = Visibility.Visible; else Section8.Visibility = Visibility.Collapsed;
            if (tsEPP.IsOn == true) Section082.Visibility = Visibility.Visible; else Section082.Visibility = Visibility.Collapsed;
            if (tsFPS.IsOn == true) Section10.Visibility = Visibility.Visible; else Section10.Visibility = Visibility.Collapsed;

            if (isFirstBoot == false)
            {
                Settings.Default.isTemp = tsTemp.IsOn;
                Settings.Default.isPower = tsPower.IsOn;
                Settings.Default.isBoost = tsCPUClk.IsOn;
                Settings.Default.isCPUClk = tsCPU.IsOn;
                Settings.Default.isiGFX = tsGPU.IsOn;
                Settings.Default.isEPP = tsEPP.IsOn;
                Settings.Default.isFPSLimit = tsFPS.IsOn;
                Settings.Default.TempLimit = (int)sdTemp.Value;
                Settings.Default.PowerLimit = (int)sdPower.Value;
                Settings.Default.iGFXClk = (int)sdGFXClock.Value;
                Settings.Default.fpsLimit = (int)sdFPSLimit.Value;
                Settings.Default.CpuClk = (uint)sdCPUClock.Value;
                Settings.Default.EPP = (uint)sdEPP.Value;
                Settings.Default.Save();
                applySettings();
            }
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

                    if (tsTemp.IsOn == true) commandArguments = $"--tctl-temp={(int)sdTemp.Value} --skin-temp-limit={(int)sdTemp.Value} ";
                    if (tsPower.IsOn == true) commandArguments = commandArguments + $"--stapm-limit={(int)sdPower.Value * 1000} --slow-limit={(int)sdPower.Value * 1000} --fast-limit={(int)sdPower.Value * 1000} --vrm-current={((int)sdPower.Value * 1000) * 2} --vrmmax-current={((int)sdPower.Value * 1000) * 2}";

                    Global.RyzenAdj = commandArguments;

                    if (isFirstBoot == false)
                    {
                        Settings.Default.isTemp = tsTemp.IsOn;
                        Settings.Default.isPower = tsPower.IsOn;
                        Settings.Default.isBoost = tsCPUClk.IsOn;
                        Settings.Default.isCPUClk = tsCPU.IsOn;
                        Settings.Default.isiGFX = tsGPU.IsOn;
                        Settings.Default.isEPP = tsEPP.IsOn;
                        Settings.Default.isFPSLimit = tsFPS.IsOn;
                        Settings.Default.TempLimit = (int)sdTemp.Value;
                        Settings.Default.PowerLimit = (int)sdPower.Value;
                        Settings.Default.iGFXClk = (int)sdGFXClock.Value;
                        Settings.Default.fpsLimit = (int)sdFPSLimit.Value;
                        Settings.Default.CpuClk = (uint)sdCPUClock.Value;
                        Settings.Default.EPP = (uint)sdEPP.Value;
                        Settings.Default.RyzenAdj = commandArguments;
                        Settings.Default.Save();
                    }

                    processRyzenAdj = "\\bin\\AMD\\ryzenadj.exe";
                    RunCLI.ApplySettings(processRyzenAdj, commandArguments, true);

                    bool isEnabled = false;
                    if (tsFPS.IsOn == true) isEnabled = true;
                    int fpsLimit = (int)sdFPSLimit.Value;

                    SetFPS(isEnabled, fpsLimit);
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

        private async void SetFPS(bool isEnabled, int fpsLimit)
        {
            try
            {
                ADLXBackend.SetFPSLimit(0, isEnabled, fpsLimit);
            }
            catch { }
        }
        public void updateBatIcon()
        {
            try
            {
                string batURL = "";

                var bi = new BitmapImage();

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
                    bi.DecodePixelWidth = 128;
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.EndInit();

                    bi.Freeze();
                    imgBat.Source = bi;
                }
                catch
                {

                }
            }
            catch
            {
                path = Global.path;
            }
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

            float dischargeRate = (float)GetSensor.BatteryDischarge;

            if (dischargeRate != 0)
            {
                lblBatDisCharge.Visibility = Visibility.Visible;
                lblBatDisCharge.Text = $"-{dischargeRate.ToString("0.00")}W Charge Rate";
            }
            else lblBatDisCharge.Visibility = Visibility.Collapsed;
        }

        private static Controller controller;

        int optionSelected = 0;
        bool isActive = false;
        bool connected;
        public static Border[] borders;
        public static Border lastBorder;
        bool goingDown = true;

        private void ControllerInput(UserIndex controllerNo)
        {
            if (Global.AccessMenuSelected == 1 && Global.isAccessMenuOpen == true)
            {
                try
                {
                    if (Settings.Default.CPUName.ToLower().Contains("intel"))
                    {
                        borders = new Border[] { Section4, Section5, Section6, Section061, Section062, Section081, Section082 };
                    }
                    else
                    {
                        borders = new Border[] { Section2, Section3, Section4, Section5, Section6, Section061, Section062, Section7, Section8, Section081, Section082, Section9, Section10 };
                    }

                    lblBat.Text = Time_and_Bat.batPercent;

                    getBatteryTime();
                    updateBatIcon();

                    //Get controller
                    controller = new Controller(controllerNo);

                    if(controllerNo == UserIndex.One) connected = controller.IsConnected;

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

                            SharpDX.XInput.Gamepad gamepad = controller.GetState().Gamepad;
                            float tx = gamepad.LeftThumbX;
                            float ty = gamepad.LeftThumbY;

                            if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp) && Global.shortCut == false && isActive == false || ty > 18000 && Global.shortCut == false && isActive == false)
                            {
                                if (optionSelected > 0) optionSelected--;
                                else optionSelected = 0;
                                goingDown = false;
                            }

                            if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown) && Global.shortCut == false && isActive == false || ty < -18000 && Global.shortCut == false && isActive == false)
                            {
                                int max = 0;
                                if (Settings.Default.CPUName.ToLower().Contains("intel"))
                                {
                                    max = 6;
                                    if (tsEPP.IsOn == false) max = 5;
                                }
                                else if (tsFPS.IsOn == false) max = 11;
                                else max = 12;

                                if (optionSelected < max) optionSelected++;
                                else optionSelected = max;
                                goingDown = true;
                            }

                            if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) && Global.shortCut == false && isActive == true || tx < -18000 && Global.shortCut == false && isActive == true)
                            {
                                int value = 0;
                                int maxValue = 0;
                                int minValue = 0;
                                Slider selectedSlider = null;

                                if (borders[optionSelected] == Section3) selectedSlider = sdTemp;
                                if (borders[optionSelected] == Section5) selectedSlider = sdPower;
                                if (borders[optionSelected] == Section062) selectedSlider = sdCPUClock;
                                if (borders[optionSelected] == Section8) selectedSlider = sdGFXClock;
                                if (borders[optionSelected] == Section082) selectedSlider = sdEPP;
                                if (borders[optionSelected] == Section10) selectedSlider = sdFPSLimit;

                                value = (int)selectedSlider.Value;
                                maxValue = (int)selectedSlider.Maximum;
                                minValue = (int)selectedSlider.Minimum;

                                if (borders[optionSelected] == Section8 || borders[optionSelected] == Section062) value = value - 50;
                                else
                                {
                                    value--;
                                    value--;
                                }

                                if (value > maxValue) value = maxValue;
                                if (value < minValue) value = minValue;
                                selectedSlider.Value = value;
                            }

                            if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) && Global.shortCut == false && isActive == true || tx > 18000 && Global.shortCut == false && isActive == true)
                            {
                                int value = 0;
                                int maxValue = 0;
                                int minValue = 0;
                                Slider selectedSlider = null;

                                if (borders[optionSelected] == Section3) selectedSlider = sdTemp;
                                if (borders[optionSelected] == Section5) selectedSlider = sdPower;
                                if (borders[optionSelected] == Section062) selectedSlider = sdCPUClock;
                                if (borders[optionSelected] == Section8) selectedSlider = sdGFXClock;
                                if (borders[optionSelected] == Section082) selectedSlider = sdEPP;
                                if (borders[optionSelected] == Section10) selectedSlider = sdFPSLimit;

                                value = (int)selectedSlider.Value;
                                maxValue = (int)selectedSlider.Maximum;
                                minValue = (int)selectedSlider.Minimum;

                                if (borders[optionSelected] == Section8 || borders[optionSelected] == Section062) value = value + 50;
                                else
                                {
                                    value++;
                                    value++;
                                }


                                if (value > maxValue) value = maxValue;
                                if (value < minValue) value = minValue;
                                selectedSlider.Value = value;
                            }


                            if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A) && Global.shortCut == false)
                            {
                                if (borders[optionSelected].Visibility == Visibility.Visible && borders[optionSelected] == Section2 && tsTemp.IsOn == false) tsTemp.IsOn = true;
                                else if (borders[optionSelected].Visibility == Visibility.Visible && borders[optionSelected] == Section2 && tsTemp.IsOn == true) tsTemp.IsOn = false;

                                if (borders[optionSelected].Visibility == Visibility.Visible && borders[optionSelected] == Section4 && tsPower.IsOn == false) tsPower.IsOn = true;
                                else if (borders[optionSelected].Visibility == Visibility.Visible && borders[optionSelected] == Section4 && tsPower.IsOn == true) tsPower.IsOn = false;

                                if (borders[optionSelected] == Section6 && tsCPUClk.IsOn == false) tsCPUClk.IsOn = true;
                                else if (borders[optionSelected].Visibility == Visibility.Visible && borders[optionSelected] == Section6 && tsCPUClk.IsOn == true) tsCPUClk.IsOn = false;

                                if (borders[optionSelected].Visibility == Visibility.Visible && borders[optionSelected] == Section061 && tsCPU.IsOn == false) tsCPU.IsOn = true;
                                else if (borders[optionSelected].Visibility == Visibility.Visible && borders[optionSelected] == Section061 && tsCPU.IsOn == true) tsCPU.IsOn = false;

                                if (borders[optionSelected].Visibility == Visibility.Visible && borders[optionSelected] == Section7 && tsGPU.IsOn == false) tsGPU.IsOn = true;
                                else if (borders[optionSelected].Visibility == Visibility.Visible && borders[optionSelected] == Section7 && tsGPU.IsOn == true) tsGPU.IsOn = false;

                                if (borders[optionSelected].Visibility == Visibility.Visible && borders[optionSelected] == Section081 && tsEPP.IsOn == false) tsEPP.IsOn = true;
                                else if (borders[optionSelected].Visibility == Visibility.Visible && borders[optionSelected] == Section081 && tsEPP.IsOn == true) tsEPP.IsOn = false;

                                if (borders[optionSelected].Visibility == Visibility.Visible && borders[optionSelected] == Section9 && tsFPS.IsOn == false) tsFPS.IsOn = true;
                                else if (borders[optionSelected].Visibility == Visibility.Visible && borders[optionSelected] == Section9 && tsFPS.IsOn == true) tsFPS.IsOn = false;

                                if (borders[optionSelected] == Section3 && isActive == false) isActive = true;
                                else if (borders[optionSelected] == Section3 && isActive == true) isActive = false;

                                if (borders[optionSelected] == Section5 && isActive == false) isActive = true;
                                else if (borders[optionSelected] == Section5 && isActive == true) isActive = false;

                                if (borders[optionSelected] == Section062 && isActive == false) isActive = true;
                                else if (borders[optionSelected] == Section062 && isActive == true) isActive = false;

                                if (borders[optionSelected] == Section8 && isActive == false) isActive = true;
                                else if (borders[optionSelected] == Section8 && isActive == true) isActive = false;

                                if (borders[optionSelected] == Section082 && isActive == false) isActive = true;
                                else if (borders[optionSelected] == Section082 && isActive == true) isActive = false;

                                if (borders[optionSelected] == Section10 && isActive == false) isActive = true;
                                else if (borders[optionSelected] == Section10 && isActive == true) isActive = false;
                            }
                        }
                    }
                    updateMenuSelected();


                    if (isFirstBoot == false)
                    {
                        Settings.Default.isTemp = tsTemp.IsOn;
                        Settings.Default.isPower = tsPower.IsOn;
                        Settings.Default.isBoost = tsCPUClk.IsOn;
                        Settings.Default.isiGFX = tsGPU.IsOn;
                        Settings.Default.TempLimit = (int)sdTemp.Value;
                        Settings.Default.PowerLimit = (int)sdPower.Value;
                        Settings.Default.iGFXClk = (int)sdGFXClock.Value;
                        Settings.Default.fpsLimit = (int)sdFPSLimit.Value;
                        Settings.Default.Save();
                    }
                }
                catch { }
            }
        }

        void KeyShortCuts_Tick(object sender, EventArgs e)
        {
            ControllerInput(UserIndex.One);
            ControllerInput(UserIndex.Two);
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
            Section061.Background = new SolidColorBrush(Colors.Transparent);
            Section062.Background = new SolidColorBrush(Colors.Transparent);
            Section062.BorderThickness = new Thickness(0, 0.5, 0.5, 0.5);
            Section7.Background = new SolidColorBrush(Colors.Transparent);
            Section8.Background = new SolidColorBrush(Colors.Transparent);
            Section8.BorderThickness = new Thickness(0, 0.5, 0.5, 0.5);
            Section081.Background = new SolidColorBrush(Colors.Transparent);
            Section082.Background = new SolidColorBrush(Colors.Transparent);
            Section082.BorderThickness = new Thickness(0, 0.5, 0.5, 0.5);
            Section9.Background = new SolidColorBrush(Colors.Transparent);
            Section10.Background = new SolidColorBrush(Colors.Transparent);
            Section10.BorderThickness = new Thickness(0, 0.5, 0.5, 0.5);

            if (connected)
            {
                if (lastBorder.Visibility == Visibility.Collapsed && isActive == true) { isActive = false; optionSelected--; }

                if (goingDown)
                {
                    if (borders[optionSelected].Visibility == Visibility.Collapsed) optionSelected++;
                    if (borders[optionSelected].Visibility == Visibility.Collapsed) optionSelected++;

                    if (optionSelected >= 7) mainView.ScrollToBottom();
                    else
                    {
                        GeneralTransform transform = borders[optionSelected].TransformToAncestor(mainView);
                        System.Windows.Point topPosition = transform.Transform(new System.Windows.Point(0, 0));
                        System.Windows.Point bottomPosition = transform.Transform(new System.Windows.Point(0, borders[optionSelected].ActualHeight));

                        // Check if the border is not fully visible in the current viewport
                        if (topPosition.Y < mainView.VerticalOffset || bottomPosition.Y > mainView.VerticalOffset + mainView.ViewportHeight)
                        {
                            // Scroll to the position of the top of the border
                            mainView.ScrollToVerticalOffset(bottomPosition.Y);
                        }
                    }
                }
                else
                {
                    if (borders[optionSelected].Visibility == Visibility.Collapsed) optionSelected--;
                    if (borders[optionSelected].Visibility == Visibility.Collapsed) optionSelected--;

                    if (optionSelected <= 1) mainView.ScrollToTop();
                    else
                    {
                        GeneralTransform transform = borders[optionSelected].TransformToAncestor(mainView);
                        System.Windows.Point topPosition = transform.Transform(new System.Windows.Point(0, 0));
                        System.Windows.Point bottomPosition = transform.Transform(new System.Windows.Point(0, borders[optionSelected].ActualHeight));

                        // Check if the border is not fully visible in the current viewport
                        if (topPosition.Y < mainView.VerticalOffset || bottomPosition.Y > mainView.VerticalOffset + mainView.ViewportHeight)
                        {
                            // Scroll to the position of the top of the border
                            mainView.ScrollToVerticalOffset(topPosition.Y);
                        }
                    }
                }

                if (borders[optionSelected] == Section10 && Section10.Visibility == Visibility.Collapsed) { optionSelected = 7; }
                if (borders[optionSelected] == Section10 && tsFPS.IsOn == false && lastBorder == Section10) { isActive = false; }

                borders[optionSelected].Background = (Brush)bc.ConvertFrom("#FF393939");
                lastBorder = borders[optionSelected];

                if (isActive == true)
                {
                    borders[optionSelected].BorderThickness = new Thickness(2.5);
                }
            }
        }
    }
}
