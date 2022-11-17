using ControlzEx.Theming;
using Game_Launcher_V2.Scripts;
using AudioSwitcher.AudioApi.CoreAudio;
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
using System.Management;
using Microsoft.Win32;

namespace Game_Launcher_V2.Pages.OptionsWindow
{
    /// <summary>
    /// Interaction logic for PowerControl.xaml
    /// </summary>
    public partial class BasicSettings : Page
    {
        //Get current working directory
        public static string path = Global.path;
        public static CoreAudioDevice defaultPlaybackDevice = new CoreAudioController().DefaultPlaybackDevice;
        public bool isFirstBoot = true;
        public static int bright = 0;
        public static int vol = 0;
        public BasicSettings()
        {
            InitializeComponent();
            _ = Tablet.TabletDevices;

            getVol();
            getBrightness();

            sdVol.Value = vol;
            sdBright.Value = bright;

            tsBootOnStart.IsOn = Settings.Default.bootOnStart;
            tsMini.IsOn = Settings.Default.startMinimised;
            tsGameList.IsOn = Settings.Default.openGameList;

            ThemeManager.Current.ChangeTheme(this, "Dark.Teal");
            isFirstBoot = false;
        }



        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            applySettings();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            applySettings();
        }

        private void Toggle_Toggled(object sender, RoutedEventArgs e)
        {
            updateGUI();
        }
        private void updateGUI()
        {
            if (isFirstBoot == false)
            {
                if (tsBootOnStart.IsOn == true)
                {
                    Settings.Default.bootOnStart = true;


                    var path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                    RegistryKey key = Registry.CurrentUser.OpenSubKey(path, true);
                    key.SetValue("MyApplication", System.Reflection.Assembly.GetExecutingAssembly().Location.ToString());
                }
                else
                {
                    Settings.Default.bootOnStart = false;

                    var path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                    RegistryKey key = Registry.CurrentUser.OpenSubKey(path, true);
                    key.DeleteValue("MyApplication", false);
                }

                if (tsMini.IsOn == true) Settings.Default.startMinimised = true; else Settings.Default.startMinimised = false;

                Settings.Default.openGameList = tsGameList.IsOn;

                Settings.Default.Save();
            }
        }

        private void applySettings()
        {
            if (isFirstBoot == false)
            {
                int bright = Convert.ToInt32(sdBright.Value);
                int volume = Convert.ToInt32(sdVol.Value);
                updateBrightness(bright);
                updateVolume(volume);
            }
        }

        public async void updateBrightness(int newBirghtness)
        {
            await Task.Run(() =>
            {
                var mclass = new ManagementClass("WmiMonitorBrightnessMethods")
                {
                    Scope = new ManagementScope(@"\\.\root\wmi")
                };
                var instances = mclass.GetInstances();
                var args = new object[] { 1, newBirghtness };
                foreach (ManagementObject instance in instances)
                {
                    instance.InvokeMethod("WmiSetBrightness", args);
                }
            });
        }
        public static void getBrightness()
        {
            using var mclass = new ManagementClass("WmiMonitorBrightness")
            {
                Scope = new ManagementScope(@"\\.\root\wmi")
            };
            using var instances = mclass.GetInstances();
            foreach (ManagementObject instance in instances)
            {
                bright = (byte)instance.GetPropertyValue("CurrentBrightness");
            }
        }

        public static void getVol()
        {
            vol = Convert.ToInt32(defaultPlaybackDevice.Volume);
        }

        public async void updateVolume(int newVolume)
        {
            await Task.Run(() =>
            {
                //Set volume of current sound device
                defaultPlaybackDevice.Volume = newVolume;
            });
        }
    }
}
