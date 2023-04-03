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
using System.Management;
using Microsoft.Win32;
using Color = System.Windows.Media.Color;
using Brush = System.Windows.Media.Brush;
using Application = System.Windows.Application;
using Windows.Media.Audio;
using Windows.Media.Render;
using System.Data;
using NAudio.CoreAudioApi;

namespace Game_Launcher_V2.Pages.OptionsWindow
{
    /// <summary>
    /// Interaction logic for PowerControl.xaml
    /// </summary>
    public partial class BasicSettings : Page
    {
        //Get current working directory
        public static string path = Global.path;
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
            tsPerf.IsOn = Settings.Default.isPerfOpen;

            ThemeManager.Current.ChangeTheme(this, "Dark.Teal");
            isFirstBoot = false;

            //set up timer for key combo system
            checkKeyInput.Interval = TimeSpan.FromSeconds(0.14);
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

        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
               ? Application.Current.Windows.OfType<T>().Any()
               : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
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
                if (tsPerf.IsOn == true) Settings.Default.isPerfOpen = true; else Settings.Default.isPerfOpen = false;


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

        public static async void getVol()
        {
            // Get the default audio playback device
            MMDevice defaultDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, NAudio.CoreAudioApi.Role.Multimedia);

            // Get the current volume level of the device as an integer between 0 and 100
            vol = (int)Math.Round(defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100.0);
        }

        public async void updateVolume(int newVolume)
        {
            await Task.Run(() =>
            {
                // Get the default audio playback device
                MMDevice defaultDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, NAudio.CoreAudioApi.Role.Multimedia);

                //Set volume of current sound device
                defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar = (float)newVolume / 100.0f;
            });
        }

        private static Controller controller;

        int optionSelected = 0;
        bool isActive = false;
        bool connected;
        public static Border[] borders;
        bool wasClosed = false;
        bool lastState = true;
        async void KeyShortCuts_Tick(object sender, EventArgs e)
        {
            borders = new Border[] { Section1, Section2, Section3, Section4, Section51 };
            
            if (lastState == false && Global.isAccessMenuOpen == true) wasClosed = true;

            lastState = Global.isAccessMenuOpen;

            if (wasClosed) { getVol(); getBrightness(); sdVol.Value = vol; sdBright.Value = bright; }

            if (Global.AccessMenuSelected == 0 && Global.isAccessMenuOpen == true)
            {
                try
                {
                    //Get controller
                    controller = new Controller(UserIndex.One);

                    connected = controller.IsConnected;

                    if (connected)
                    {
                        SharpDX.XInput.Gamepad gamepad = controller.GetState().Gamepad;
                        float tx = gamepad.LeftThumbX;
                        float ty = gamepad.LeftThumbY;

                        //get controller state
                        var state = controller.GetState();

                        //detect if keyboard or controller combo is being activated
                        if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                        {
                            Global.shortCut = true;
                        }

                        if (Global.isAccessMenuOpen == true && Global.shortCut == false)
                        {
                            if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp) && Global.shortCut == false && isActive == false || ty > 18000 && Global.shortCut == false && isActive == false)
                            {
                                if (optionSelected > 0) optionSelected--;
                                else optionSelected = 0;

                            }

                            if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown) && Global.shortCut == false && isActive == false || ty < -18000 && Global.shortCut == false && isActive == false)
                            {
                                if (optionSelected < 4) optionSelected++;
                                else optionSelected = 4;
                            }

                            if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) && Global.shortCut == false && isActive == true || tx < -18000 && Global.shortCut == false && isActive == true)
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

                            if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) && Global.shortCut == false && isActive == true || tx > 18000 && Global.shortCut == false && isActive == true)
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

                                if (optionSelected == 4 && tsPerf.IsOn == false) tsPerf.IsOn = true;
                                else if (optionSelected == 4 && tsPerf.IsOn == true) tsPerf.IsOn = false;
                            }
                        }
                    }
                    updateMenuSelected();
                }
                catch { }
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
            Section51.Background = new SolidColorBrush(Colors.Transparent);

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
