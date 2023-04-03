using AATUV3.Scripts;
using Game_Launcher_V2.Properties;
using Game_Launcher_V2.Scripts;
using Game_Launcher_V2.Scripts.ADLX;
using Game_Launcher_V2.Scripts.Epic_Games;
using Game_Launcher_V2.Windows;
using Microsoft.Win32;
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
                } catch { Global.path = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).LocalPath; }
                

                _ = Tablet.TabletDevices;

                //set up timer for sensor update
                DispatcherTimer sensor = new DispatcherTimer();
                sensor.Interval = TimeSpan.FromSeconds(0.16);
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

                    if(Settings.Default.startMinimised == false)
                    {
                        this.Activate();
                        this.Focus();
                    }

                    BasicExeBackend.Garbage_Collect();
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

                if(Global.desktop >= 1)
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

                if (this.Visibility == Visibility && this.WindowState != WindowState.Minimized && this.WindowState != WindowState.Maximized && Global.isMainActive == true) this.WindowState= WindowState.Maximized;
            }
            catch { }
        }

        public async void getData()
        {
            Global.wifi = await Task.Run(() => Time_and_Bat.RetrieveSignalStrengthAsync());
            Time_and_Bat.getBattery();
            Time_and_Bat.getTime();
            GetSensor.ReadSensors();
        }
        int lastiGPU = 0;
        int ryzen = -1;
        int GC = -1;
        async void UpdateBatTime_Tick(object sender, EventArgs e)
        {
            try
            {
                getData();
                ryzen++;
                if(Settings.Default.RyzenAdj != null || Settings.Default.RyzenAdj != "")
                {
                    string processRyzenAdj = "";
                    string commandArguments = Settings.Default.RyzenAdj;
                    if(ryzen >= 2)
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

                        ryzen = 0;
                    }

                    if(GC >= 5)
                    {
                        BasicExeBackend.Garbage_Collect();
                        GC = 0;
                    }
                    GC++;
                }
            }
            catch { }
        }
    }
}
