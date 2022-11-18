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
using Color = System.Windows.Media.Color;
using Brush = System.Windows.Media.Brush;

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
        public DispatcherTimer checkKeyInput = new DispatcherTimer();
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

            //set up timer for key combo system
            checkKeyInput.Interval = TimeSpan.FromSeconds(0.117);
            checkKeyInput.Tick += KeyShortCuts_Tick;
            checkKeyInput.Start();
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
                if (tsBootOnStart.IsOn == true) Settings.Default.bootOnStart = true; else Settings.Default.bootOnStart = false;

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

        private static Controller controller;

        int optionSelected = 0;
        bool isActive = false;
        bool connected;
        public static Border[] borders;

        async void KeyShortCuts_Tick(object sender, EventArgs e)
        {
            borders = new Border[] { Section1, Section2, Section3, Section4 };

            if (Global.AccessMenuSelected == 0)
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

                            }

                            if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown) && Global.shortCut == false && isActive == false)
                            {
                                if (optionSelected < 3) optionSelected++;
                                else optionSelected = 3;
                            }

                            if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) && Global.shortCut == false && isActive == true)
                            {
                                int value = 0;
                                int maxValue = 0;
                                int minValue = 0;
                                Slider selectedSlider = null;

                                if (optionSelected == 0) selectedSlider = sdBright;

                                if (optionSelected == 1) selectedSlider = sdVol;

                                value = (int)selectedSlider.Value;
                                maxValue = (int)selectedSlider.Maximum;
                                minValue = (int)selectedSlider.Minimum;

                                value--;
                                value--;

                                if (value > maxValue) value = maxValue;
                                if (value < minValue) value = minValue;
                                selectedSlider.Value = value;
                            }

                            if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) && Global.shortCut == false && isActive == true)
                            {
                                int value = 0;
                                int maxValue = 0;
                                int minValue = 0;
                                Slider selectedSlider = null;

                                if (optionSelected == 0) selectedSlider = sdBright;

                                if (optionSelected == 1) selectedSlider = sdVol;

                                value = (int)selectedSlider.Value;
                                maxValue = (int)selectedSlider.Maximum;
                                minValue = (int)selectedSlider.Minimum;

                                value++;
                                value++;

                                if (value > maxValue) value = maxValue;
                                if (value < minValue) value = minValue;
                                selectedSlider.Value = value;
                            }


                            if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A) && Global.shortCut == false)
                            {
                                if (optionSelected < 2 && isActive == false) isActive = true;
                                else isActive = false;

                                if (optionSelected == 2 && tsBootOnStart.IsOn == false) tsBootOnStart.IsOn = true;
                                else if (optionSelected == 2 && tsBootOnStart.IsOn == true) tsBootOnStart.IsOn = false;

                                if (optionSelected == 3 && tsMini.IsOn == false) tsMini.IsOn = true;
                                else if (optionSelected == 3 && tsMini.IsOn == true) tsMini.IsOn = false;
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
            }
         }

        public void updateMenuSelected()
        {
            var bc = new BrushConverter();
            Section1.Background = new SolidColorBrush(Colors.Transparent);
            Section1.BorderThickness = new Thickness(0, 0.5, 0.5, 0.5);
            Section2.Background = new SolidColorBrush(Colors.Transparent);
            Section2.BorderThickness = new Thickness(0, 0.5, 0.5, 0.5);
            Section3.Background = new SolidColorBrush(Colors.Transparent);
            Section4.Background = new SolidColorBrush(Colors.Transparent);

            if (connected)
            {
                borders[optionSelected].Background = (Brush)bc.ConvertFrom("#F2252525");

                if (isActive == true && optionSelected < 2)
                {
                    borders[optionSelected].BorderThickness = new Thickness(2.5);
                }
            }
        }
    }
}
