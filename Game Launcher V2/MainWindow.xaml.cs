using AATUV3.Scripts;
using Game_Launcher_V2.Properties;
using Game_Launcher_V2.Scripts;
using Game_Launcher_V2.Scripts.ADLX;
using Game_Launcher_V2.Scripts.ASUS;
using Game_Launcher_V2.Scripts.Epic_Games;
using Game_Launcher_V2.Scripts.OptionsWindow.PowerControl;
using Game_Launcher_V2.Windows;
using LibreHardwareMonitor.Hardware;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
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
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Windows.Devices.Sensors;
using WindowsInput;
using static Game_Launcher_V2.Scripts.SystemDeviceControl;
using Application = System.Windows.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using SplashScreen = Game_Launcher_V2.Windows.SplashScreen;

namespace Game_Launcher_V2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Function that check's if current user is in Aministrator role
        /// </summary>
        /// <returns></returns>
        public static bool IsRunningAsAdministrator()
        {
            // Get current Windows user
            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();

            // Get current Windows user principal
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(windowsIdentity);

            // Return TRUE if user is in role "Administrator"
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static Frame navFrame;

        public static string FanConfig = "";

        public MainWindow()
        {
            if (!IsRunningAsAdministrator())
            {
                // Setting up start info of the new process of the same application
                ProcessStartInfo processStartInfo = new ProcessStartInfo(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase) + "\\Playmate Game Launcher.exe");

                // Using operating shell and setting the ProcessStartInfo.Verb to “runas” will let it run as admin
                processStartInfo.UseShellExecute = true;
                processStartInfo.Verb = "runas";

                // Start the application as new process
                Process.Start(processStartInfo);

                // Shut down the current (old) process
                Application.Current.Shutdown();
            }
            else
            {
                InitializeComponent();

                GetSensor.openSensor();

                //Detect if an AYA Neo is being used

                ManagementObjectSearcher baseboardSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");
                foreach (ManagementObject queryObj in baseboardSearcher.Get())
                {
                    Global.mbo = queryObj["Manufacturer"].ToString().ToLower();
                }

                navFrame = PagesNavigation;

                Settings.Default.CPUName = System.Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");

                SplashScreen splash = new SplashScreen();
                splash.Show();

                if (Settings.Default.isFirstBoot == true || Settings.Default.Path == "" || Settings.Default.Path == null)
                {
                    Settings.Default.Path = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).LocalPath;
                    Settings.Default.isFirstBoot = false;
                    Settings.Default.Save();
                }

                if (Settings.Default.startMinimised == true) this.WindowState = WindowState.Minimized;

                try
                {
                    Global.path = Settings.Default.Path;
                }
                catch { Global.path = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).LocalPath; }


                _ = Tablet.TabletDevices;

                //set up timer for sensor update
                DispatcherTimer sensor = new DispatcherTimer();
                sensor.Interval = TimeSpan.FromSeconds(0.12);
                sensor.Tick += Update_Tick;
                sensor.Start();

                //set up timer for sensor update
                DispatcherTimer battime = new DispatcherTimer();
                battime.Interval = TimeSpan.FromSeconds(2);
                battime.Tick += UpdateBatTime_Tick;
                battime.Start();

                try
                {
                    RegistryKey myKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\CI\\Config", true);
                    if (myKey != null)
                    {
                        myKey.SetValue("VulnerableDriverBlocklistEnable", "0", RegistryValueKind.String);
                        myKey.Close();
                    }

                    Global.path = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).LocalPath;

                    //if (File.Exists(Global.path + "\\SavedList.txt")) File.Delete(Global.path + "\\SavedList.txt");
                    //if (File.Exists(Global.path + "\\SavedListEpic.txt")) File.Delete(Global.path + "\\SavedListEpic.txt");
                    //FindSteamData.getData();
                    //FindEpicGamesData.GetData();

                    getData();
                    PagesNavigation.Navigate(new System.Uri("Pages/Home.xaml", UriKind.RelativeOrAbsolute));

                    OptionsWindow win2 = new OptionsWindow();
                    win2.Show();

                    PerformanceOverlay perfOverlay = new PerformanceOverlay();
                    perfOverlay.Show();

                    //SelectGameStore gameStore = new SelectGameStore();
                    //gameStore.Show();

                    if (Settings.Default.startMinimised == false)
                    {
                        this.Activate();
                        this.Focus();
                    }

                    _mouseSimulator = new InputSimulator().Mouse;

                    DispatcherTimer _timer = new DispatcherTimer();
                    _timer.Tick += Controller_Tick;
                    _timer.Interval = TimeSpan.FromMilliseconds(1000 / RefreshRate);
                    _timer.Start();

                    BasicExeBackend.Garbage_Collect();

                    // Hide the EPP attributes
                    CPUboost.HideAttribute("SUB_PROCESSOR", "PERFEPP");
                    CPUboost.HideAttribute("SUB_PROCESSOR", "PERFEPP1");

                    // Hide the PROCFREQMAX and PROCFREQMAX1 attributes
                    CPUboost.HideAttribute("SUB_PROCESSOR", "PROCFREQMAX");
                    CPUboost.HideAttribute("SUB_PROCESSOR", "PROCFREQMAX1");

                    CPUboost.SetPowerValue("scheme_current", "sub_processor", "PERFAUTONOMOUS", 1, true);
                    CPUboost.SetPowerValue("scheme_current", "sub_processor", "PERFAUTONOMOUS", 1, false);


                    string fanConfig = "";
                    fanConfig = $"{GetSystemInfo.Manufacturer.ToUpper()}_{GetSystemInfo.Product.ToUpper()}.json";
                    string path = Global.path;
                    path = path + "\\Fan Configs\\" + fanConfig;

                    FanConfig = path;

                    Fan_Control.UpdateAddresses();
                }
                catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            }
        }


        private void Window_Activated(object sender, EventArgs e)
        {
            Global.isMainActive = true;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Global.isMainActive = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (File.Exists(FanConfig)) if (Fan_Control.fanControlEnabled) Fan_Control.disableFanControl();

            Application.Current.Shutdown();
        }

        int lastGameStore = 0;

        async void Update_Tick(object sender, EventArgs e)
        {
            try
            {
                if (Global.GameStore != lastGameStore)
                {
                    PagesNavigation.Refresh();
                }

                if (Global.menuSelectWasOpen)
                {
                    this.Activate();

                    Global.menuSelectWasOpen = false;
                }

                lastGameStore = Global.GameStore;

                if (Global.desktop >= 1)
                {
                    this.WindowState = WindowState.Minimized;
                    Global.desktop = 0;
                }

                if (Global.reload >= 1)
                {
                    PagesNavigation.Refresh();
                    Global.reload = 0;
                }

                if (this.WindowState == WindowState.Minimized) Global.isMainActive = false;

                if (this.Visibility == Visibility && this.WindowState != WindowState.Minimized && this.WindowState != WindowState.Maximized && Global.isMainActive == true) this.WindowState = WindowState.Maximized;
            }
            catch { }
        }

        private Controller _controller;
        private IMouseSimulator _mouseSimulator;

        private const int RefreshRate = 61;
        private const int MovementDivider = 2_000;
        private const int ScrollDivider = 10_000;

        private bool _wasLCDown;
        private bool _wasRCDown;

        private void RightButton(State state)
        {
            var isBDown = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B);
            if (isBDown && !_wasRCDown) _mouseSimulator.RightButtonDown();
            if (!isBDown && _wasRCDown) _mouseSimulator.RightButtonUp();
            _wasRCDown = isBDown;
        }

        private void LeftButton(State state)
        {
            var isADown = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A);
            if (isADown && !_wasLCDown) _mouseSimulator.LeftButtonDown();
            if (!isADown && _wasLCDown) _mouseSimulator.LeftButtonUp();
            _wasLCDown = isADown;
        }

        private void Scroll(State state)
        {
            if (state.Gamepad.RightThumbX > 8000 || state.Gamepad.RightThumbX < -8000 || state.Gamepad.RightThumbY > 8000 || state.Gamepad.RightThumbY < -8000)
            {
                var x = state.Gamepad.RightThumbX / ScrollDivider;
                var y = state.Gamepad.RightThumbY / ScrollDivider;
                _mouseSimulator.HorizontalScroll(x);
                _mouseSimulator.VerticalScroll(y);
            }

        }

        private void Movement(State state)
        {
            if (state.Gamepad.LeftThumbX > 8000 || state.Gamepad.LeftThumbX < -8000 || state.Gamepad.LeftThumbY > 8000 || state.Gamepad.LeftThumbY < -8000)
            {
                var x = state.Gamepad.LeftThumbX / MovementDivider;
                var y = state.Gamepad.LeftThumbY / MovementDivider;
                _mouseSimulator.MoveMouseBy(x, -y);
            }
        }

        async void Controller_Tick(object sender, EventArgs e)
        {
            _controller = new Controller(UserIndex.One);
            if (_controller.IsConnected && Global.isAccessMenuActive == false && Global.isMainActive == false && Settings.Default.isMouse == true)
            {
                _controller.GetState(out var state);
                Movement(state);
                Scroll(state);
                LeftButton(state);
                RightButton(state);
            }

            _controller = new Controller(UserIndex.Two);
            if (_controller.IsConnected && Global.isAccessMenuActive == false && Global.isMainActive == false && Settings.Default.isMouse == true)
            {
                _controller.GetState(out var state);
                Movement(state);
                Scroll(state);
                LeftButton(state);
                RightButton(state);
            }
        }

        public async void getData()
        {
            Global.wifi = await Task.Run(() => Time_and_Bat.RetrieveSignalStrengthAsync());
            await Task.Run(() =>
            {
                Time_and_Bat.getBattery();
                Time_and_Bat.getTime();
                GetSensor.ReadSensors();
            });
        }

        private static int Interpolate(int[] yValues, int[] xValues, int x)
        {
            int i = Array.FindIndex(xValues, t => t >= x);

            if (i == -1) // temperature is lower than the first input point
            {
                return yValues[0];
            }
            else if (i == 0) // temperature is equal to or higher than the first input point
            {
                return yValues[0];
            }
            else if (i == xValues.Length) // temperature is higher than the last input point
            {
                return yValues[xValues.Length - 1];
            }
            else // interpolate between two closest input points
            {
                return Interpolate(yValues[i - 1], xValues[i - 1], yValues[i], xValues[i], x);
            }
        }

        private static int Interpolate(int y1, int x1, int y2, int x2, int x)
        {
            return (y1 * (x2 - x) + y2 * (x - x1)) / (x2 - x1);
        }

        int lastiGPU = 0;
        int ryzenAdj = -1;
        int GC = -1;
        int ACProfile = 0;
        int lastACProfile = -1;
        async void UpdateBatTime_Tick(object sender, EventArgs e)
        {
            try
            {
                getData();

                await Task.Run(() =>
                {
                    string processRyzenAdj = "";
                string commandArguments = Settings.Default.RyzenAdj;

                // Set the desired frequencies in MHz
                uint freqMax = (uint)Settings.Default.CpuClk;
                uint freqMax1 = (uint)Settings.Default.CpuClk;
                uint EPP = (uint)Settings.Default.EPP;
                ryzenAdj++;

                
                    if (ryzenAdj >= 1)
                    {
                        if (commandArguments != null || commandArguments != "")
                        {
                            if (Settings.Default.isiGFX == true)
                            {
                                if (!Settings.Default.CPUName.ToLower().Contains("intel"))
                                {
                                    try
                                    {
                                        if (lastiGPU != Settings.Default.iGFXClk)
                                        {
                                            commandArguments = commandArguments + $" --gfx-clk={(int)Settings.Default.iGFXClk}";
                                            lastiGPU = Settings.Default.iGFXClk;
                                        }

                                    }
                                    catch { }
                                }

                                processRyzenAdj = "\\bin\\AMD\\ryzenadj.exe";
                                RunCLI.ApplySettings(processRyzenAdj, commandArguments, true);
                            }
                        }

                        if (Settings.Default.isCPUClk == true)
                        {
                            // Set the AC and DC values for PROCFREQMAX
                            CPUboost.SetPowerValue("scheme_current", "sub_processor", "PROCFREQMAX", freqMax, true);
                            CPUboost.SetPowerValue("scheme_current", "sub_processor", "PROCFREQMAX", freqMax, false);

                            // Set the AC and DC values for PROCFREQMAX1
                            CPUboost.SetPowerValue("scheme_current", "sub_processor", "PROCFREQMAX1", freqMax1, true);
                            CPUboost.SetPowerValue("scheme_current", "sub_processor", "PROCFREQMAX1", freqMax1, false);

                            // Activate the current power scheme
                            CPUboost.SetActiveScheme("scheme_current");
                        }
                        else
                        {

                            // Set the AC and DC values for PROCFREQMAX
                            CPUboost.SetPowerValue("scheme_current", "sub_processor", "PROCFREQMAX", 0, true);
                            CPUboost.SetPowerValue("scheme_current", "sub_processor", "PROCFREQMAX", 0, false);

                            // Set the AC and DC values for PROCFREQMAX1
                            CPUboost.SetPowerValue("scheme_current", "sub_processor", "PROCFREQMAX1", 0, true);
                            CPUboost.SetPowerValue("scheme_current", "sub_processor", "PROCFREQMAX1", 0, false);

                            // Activate the current power scheme
                            CPUboost.SetActiveScheme("scheme_current");
                        }

                        if (Settings.Default.isEPP == true)
                        {

                            // Set the AC and DC values for PROCFREQMAX
                            CPUboost.SetPowerValue("scheme_current", "sub_processor", "PERFEPP", EPP, true);
                            CPUboost.SetPowerValue("scheme_current", "sub_processor", "PERFEPP", EPP, false);
                            CPUboost.SetPowerValue("scheme_current", "sub_processor", "PERFEPP1", EPP, true);
                            CPUboost.SetPowerValue("scheme_current", "sub_processor", "PERFEPP1", EPP, false);

                            // Activate the current power scheme
                            CPUboost.SetActiveScheme("scheme_current");
                        }
                        else
                        {

                            // Set the AC and DC values for EPP
                            CPUboost.SetPowerValue("scheme_current", "sub_processor", "PERFEPP ", 50, true);
                            CPUboost.SetPowerValue("scheme_current", "sub_processor", "PERFEPP ", 50, false);
                            CPUboost.SetPowerValue("scheme_current", "sub_processor", "PERFEPP1", 50, true);
                            CPUboost.SetPowerValue("scheme_current", "sub_processor", "PERFEPP1", 50, false);

                            // Activate the current power scheme
                            CPUboost.SetActiveScheme("scheme_current");
                        }

                        ryzenAdj = 0;
                    }
                });

                if (GC >= 12)
                {
                    BasicExeBackend.Garbage_Collect();
                    GC = 0;
                }
                GC++;

                try
                {
                    if (Global.isASUS == false)
                    {
                        await Task.Run(() =>
                        {
                            if (File.Exists(FanConfig))
                            {
                                int[] temps = { 25, 35, 45, 55, 65, 75, 85, 95 };
                                int[] speeds = { 0, 5, 15, 25, 40, 55, 70, 100 };

                                if (Settings.Default.fanCurve == 1)
                                {
                                    int[] silent = { 0, 5, 15, 18, 30, 45, 55, 65 };
                                    speeds = silent;
                                }
                                if (Settings.Default.fanCurve == 2)
                                {
                                    int[] bal = { 0, 5, 15, 25, 40, 50, 65, 85 };
                                    speeds = bal;
                                }
                                if (Settings.Default.fanCurve == 3)
                                {
                                    int[] turbo = { 0, 18, 28, 35, 60, 70, 85, 100 };
                                    speeds = turbo;
                                }


                                if (Settings.Default.fanCurve != 0 && Fan_Control.fanControlEnabled)
                                {
                                    int cpuTemperature = GetCpuTemperature();

                                    var fanSpeed = Interpolate(speeds, temps, cpuTemperature);

                                    Fan_Control.setFanSpeed(fanSpeed);
                                }

                                if (Settings.Default.fanCurve == 0 && Fan_Control.fanControlEnabled) Fan_Control.disableFanControl();
                                else if (Settings.Default.fanCurve != 0 && !Fan_Control.fanControlEnabled) Fan_Control.enableFanControl();
                            }
                        });
                    }
                    else
                    {
                        ACProfile = Settings.Default.fanCurve;
                        if (ACProfile != lastACProfile)
                        {
                            await Task.Run(() => App.wmi.DeviceSet(ASUSWmi.PerformanceMode,
                    ACProfile == 0 ? ASUSWmi.PerformanceSilent :
                    ACProfile == 1 ? ASUSWmi.PerformanceBalanced :
                    ACProfile == 2 ? ASUSWmi.PerformanceTurbo : ASUSWmi.PerformanceBalanced));

                            lastACProfile = ACProfile;
                        }

                        
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private int GetCpuTemperature()
        {
            try
            {
                Computer computer = new Computer
                {
                    IsCpuEnabled = true,
                };
                computer.Open();
                var cpu = computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
                cpu.Update();
                var temperature = cpu.Sensors.FirstOrDefault(s => s.SensorType == LibreHardwareMonitor.Hardware.SensorType.Temperature);
                if (temperature != null)
                {
                    return (int)temperature.Value;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                // Log exception
                return 0;
            }
        }
    }
}
