using Game_Launcher_V2.Properties;
using Game_Launcher_V2.Scripts;
using Game_Launcher_V2.Scripts.Epic_Games;
using Game_Launcher_V2.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                ProcessStartInfo processStartInfo = new ProcessStartInfo(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase) + "\\Game Launcher V2.exe");

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

                Global.path = Settings.Default.Path;

                _ = Tablet.TabletDevices;

                //set up timer for sensor update
                DispatcherTimer sensor = new DispatcherTimer();
                sensor.Interval = TimeSpan.FromSeconds(0.115);
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

                    if (File.Exists("SavedList.txt")) File.Delete("SavedList.txt");
                    if (File.Exists("SavedListEpic.txt")) File.Delete("SavedListEpic.txt");
                    FindSteamData.getData();
                    FindEpicGamesData.GetData();
                    PagesNavigation.Navigate(new System.Uri("Pages/Home.xaml", UriKind.RelativeOrAbsolute));

                    OptionsWindow win2 = new OptionsWindow();
                    win2.Show();

                    PerformanceOverlay overlay = new PerformanceOverlay();
                    overlay.Show();
                    SelectGameStore gameStore = new SelectGameStore();
                    gameStore.Show();
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
                Time_and_Bat.getBattery();
                Time_and_Bat.getTime();

                Global.wifi = await Task.Run(() => Time_and_Bat.RetrieveSignalString());

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

                if (this.WindowState == WindowState.Minimized) Global.isMainActive = false;

                if (this.Visibility == Visibility && this.WindowState != WindowState.Minimized && this.WindowState != WindowState.Maximized && Global.isMainActive == true) this.WindowState= WindowState.Maximized;
            }
            catch { }
        }

        async void UpdateBatTime_Tick(object sender, EventArgs e)
        {
            try
            {
                Time_and_Bat.getBattery();
                Time_and_Bat.getTime();

                Global.wifi = await Task.Run(() => Time_and_Bat.RetrieveSignalString());
            }
            catch { }
        }
    }
}
