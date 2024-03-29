﻿using ControlzEx.Theming;
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
using static System.Collections.Specialized.BitVector32;
using Windows.Devices.Radios;
using Windows.Networking.Connectivity;
using Windows.Devices.Enumeration;
using Microsoft.Win32.TaskScheduler;
using Task = System.Threading.Tasks.Task;
using Game_Launcher_V2.Scripts.ASUS;
using System.Linq.Expressions;

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

        public static int fanCurve = 0;

        public BasicSettings()
        {
            InitializeComponent();
            _ = Tablet.TabletDevices;

            getWifi();
            getBluetooth();
            getVol();
            getBrightness();

            sdVol.Value = vol;
            sdBright.Value = bright;

            tsMouse.IsOn = Settings.Default.isMouse;
            tsBootOnStart.IsOn = Settings.Default.bootOnStart;
            tsMini.IsOn = Settings.Default.startMinimised;
            tsPerf.IsOn = Settings.Default.isPerfOpen;

            ThemeManager.Current.ChangeTheme(this, "Dark.Teal");
            isFirstBoot = false;

            tsMic.IsOn = Settings.Default.isMuted;
            sdFan.Value = Settings.Default.fanCurve;

            if (Global.isASUS != true)
            {
                if (sdFan.Value == 0) lblFan.Text = "Default";
                if (sdFan.Value == 1) lblFan.Text = "Silent";
                if (sdFan.Value == 2) lblFan.Text = "Balanced";
                if (sdFan.Value == 3) lblFan.Text = "Turbo";
            }
            else
            {
                lblFanName.Text = "Power Profile";
                if (sdFan.Value == 0) lblFan.Text = "Silent";
                if (sdFan.Value == 1) lblFan.Text = "Performance";
                if (sdFan.Value == 2) lblFan.Text = "Turbo";

                sdFan.Maximum = 2;
                sdFan.Minimum = 0;
            }

            //set up timer for key combo system
            checkKeyInput.Interval = TimeSpan.FromSeconds(0.12);
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

        private async void updateGUI()
        {
            try {
                if (isFirstBoot == false)
                {
                    using (TaskService ts = new TaskService())
                    {
                        if (ts.RootFolder.AllTasks.Any(t => t.Name == "Playmate"))
                        {
                            // Remove the task we just created
                            ts.RootFolder.DeleteTask("Playmate");
                        }
                    }

                    if (tsBootOnStart.IsOn == true)
                    {
                        // Get the service on the local machine
                        using (TaskService ts = new TaskService())
                        {
                            if (!ts.RootFolder.AllTasks.Any(t => t.Name == "Playmate"))
                            {
                                // Create a new task definition and assign properties
                                TaskDefinition td = ts.NewTask();
                                td.Principal.RunLevel = TaskRunLevel.Highest;
                                td.RegistrationInfo.Description = "Start Playmate";
                                td.Settings.DisallowStartIfOnBatteries = false;

                                // Create a trigger that will fire the task at this time every other day
                                td.Triggers.Add(new LogonTrigger());

                                string path = System.Reflection.Assembly.GetEntryAssembly().Location;
                                path = path.Replace("Playmate Game Launcher.dll", "Playmate Game Launcher.exe");

                                // Create an action that will launch Notepad whenever the trigger fires
                                td.Actions.Add(path);

                                // Register the task in the root folder
                                ts.RootFolder.RegisterTaskDefinition(@"Playmate", td);
                            }
                        }
                    }

                    SetWifiEnabled();
                    SetBluetoothEnabled();

                    if (tsMic.IsOn == true) SetRecordingDeviceState(true);
                    else SetRecordingDeviceState(false);

                    if (tsMic.IsOn == true) Settings.Default.isMuted = true; else Settings.Default.isMuted = false;
                    if (tsMouse.IsOn == true) Settings.Default.isMouse = true; else Settings.Default.isMouse = false;
                    if (tsMini.IsOn == true) Settings.Default.startMinimised = true; else Settings.Default.startMinimised = false;
                    if (tsBootOnStart.IsOn == true) Settings.Default.bootOnStart = true; else Settings.Default.bootOnStart = false;
                    if (tsPerf.IsOn == true) Settings.Default.isPerfOpen = true; else Settings.Default.isPerfOpen = false;

                    Settings.Default.Save();
                }
            }
            catch { }
        }

        static async void SetRecordingDeviceState(bool mute)
        {
            try
            {
                await Task.Run(() =>
                {
                    using var enumerator = new MMDeviceEnumerator();
                    var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);

                    foreach (var device in devices)
                    {
                        device.AudioEndpointVolume.Mute = mute;
                    }
                });
            } catch { }
        }

        private async void getWifi()
        {
            try
            {
                // Check if Wi-Fi is enabled
                var wifiRadios = await Radio.GetRadiosAsync();
                var wifiRadio = wifiRadios.FirstOrDefault(r => r.Kind == RadioKind.WiFi);
                bool isWifiEnabled = (wifiRadio != null && wifiRadio.State == RadioState.On);

                tsWifi.IsOn = isWifiEnabled;
            }
            catch { }
        }

        private async void getBluetooth()
        {
            try
            {
                // Check if Bluetooth is enabled
                var bluetoothRadios = await Radio.GetRadiosAsync();
                var bluetoothRadio = bluetoothRadios.FirstOrDefault(r => r.Kind == RadioKind.Bluetooth);
                bool isBluetoothEnabled = (bluetoothRadio != null && bluetoothRadio.State == RadioState.On);
                tsBlue.IsOn = isBluetoothEnabled;
            }
            catch { }
        }

        private async void applySettings()
        {
            try
            {
                if (isFirstBoot == false)
                {
                    int bright = Convert.ToInt32(sdBright.Value);
                    int volume = Convert.ToInt32(sdVol.Value);

                    await SetWifiEnabled();
                    await SetBluetoothEnabled();

                    if(Global.isASUS) await Task.Run(() => App.wmi.DeviceSet(ASUSWmi.PerformanceMode, Settings.Default.fanCurve));

                    await Task.Run(() =>
                    {
                        updateBrightness(bright);
                        updateVolume(volume);
                    });
                }
            }
            catch { }
        }

        private async Task SetWifiEnabled()
        {
            try
            {
                // Check if Wi-Fi is enabled
                var wifiRadios = await Radio.GetRadiosAsync();
                var wifiRadio = wifiRadios.FirstOrDefault(r => r.Kind == RadioKind.WiFi);
                await wifiRadio.SetStateAsync(tsWifi.IsOn ? RadioState.On : RadioState.Off);
            }
            catch { }
        }

        private async Task SetBluetoothEnabled()
        {
            try
            {
                // Check if Bluetooth is enabled
                var bluetoothRadios = await Radio.GetRadiosAsync();
                var bluetoothRadio = bluetoothRadios.FirstOrDefault(r => r.Kind == RadioKind.Bluetooth);
                await bluetoothRadio.SetStateAsync(tsBlue.IsOn ? RadioState.On : RadioState.Off);
            }
            catch { }
        }

        public async void updateBrightness(int newBirghtness)
        {
            try
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
            catch { }
        }
        public static void getBrightness()
        {
            try
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
            catch { }
        }

        public static async void getVol()
        {
            try
            {
                // Get the default audio playback device
                MMDevice defaultDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, NAudio.CoreAudioApi.Role.Multimedia);

                // Get the current volume level of the device as an integer between 0 and 100
                vol = (int)Math.Round(defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100.0);
            }
            catch { }
        }

        public async void updateVolume(int newVolume)
        {
            try
            {
                await Task.Run(() =>
                {
                    // Get the default audio playback device
                    MMDevice defaultDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, NAudio.CoreAudioApi.Role.Multimedia);

                    //Set volume of current sound device
                    defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar = (float)newVolume / 100.0f;
                });
            }
            catch { }
        }

        private static Controller controller;

        int optionSelected = 0;
        bool isActive = false;
        bool connected;
        public static Border[] borders;
        bool wasClosed = false;
        bool lastState = true;

        private async void ControllerInput(UserIndex controllerNo)
        {
            try
            {
                if (lastState == false && Global.isAccessMenuOpen == true) wasClosed = true;

                lastState = Global.isAccessMenuOpen;

                if (wasClosed)
                {
                    try
                    {
                        getVol();
                        getBrightness();
                        sdVol.Value = vol;
                        sdBright.Value = bright;

                        getWifi();
                        getBluetooth();
                        wasClosed = false;
                    }
                    catch { }
                }

                if (Global.isASUS != true)
                {
                    if (sdFan.Value == 0) lblFan.Text = "Default";
                    if (sdFan.Value == 1) lblFan.Text = "Silent";
                    if (sdFan.Value == 2) lblFan.Text = "Balanced";
                    if (sdFan.Value == 3) lblFan.Text = "Turbo";
                    Settings.Default.fanCurve = (int)sdFan.Value;
                    Settings.Default.Save();
                }
                else
                {
                    if (sdFan.Value == 0)
                    {
                        lblFan.Text = "Silent";
                        Settings.Default.fanCurve = ASUSWmi.PerformanceSilent;
                    }
                    if (sdFan.Value == 1)
                    {
                        lblFan.Text = "Performance";
                        Settings.Default.fanCurve = ASUSWmi.PerformanceBalanced;
                    }
                    if (sdFan.Value == 2)
                    {
                        lblFan.Text = "Turbo";
                        Settings.Default.fanCurve = ASUSWmi.PerformanceTurbo;
                    }
                    Settings.Default.Save();
                }



                if (Global.AccessMenuSelected == 0 && Global.isAccessMenuOpen == true)
                {
                    borders = new Border[] { Section01, Section02, Section1, Section2, Section201, Section0301, Section03, Section3, Section4, Section51 };

                    try
                    {
                        //Get controller
                        controller = new Controller(controllerNo);

                        if (controllerNo == UserIndex.One) connected = controller.IsConnected;

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

                                        GeneralTransform transform = borders[optionSelected].TransformToAncestor(svMain);
                                        System.Windows.Point topPosition = transform.Transform(new System.Windows.Point(0, 0));
                                        System.Windows.Point bottomPosition = transform.Transform(new System.Windows.Point(0, borders[optionSelected].ActualHeight));

                                        // Check if the border is not fully visible in the current viewport
                                        if (topPosition.Y < svMain.VerticalOffset || bottomPosition.Y > svMain.VerticalOffset + svMain.ViewportHeight)
                                        {
                                            // Scroll to the position of the top of the border
                                            svMain.ScrollToVerticalOffset(topPosition.Y);
                                        }
                                    }

                                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown) && Global.shortCut == false && isActive == false || ty < -18000 && Global.shortCut == false && isActive == false)
                                    {
                                        if (optionSelected < 9) optionSelected++;
                                        else optionSelected = 9;

                                        GeneralTransform transform = borders[optionSelected].TransformToAncestor(svMain);
                                        System.Windows.Point topPosition = transform.Transform(new System.Windows.Point(0, 0));
                                        System.Windows.Point bottomPosition = transform.Transform(new System.Windows.Point(0, borders[optionSelected].ActualHeight));

                                        // Check if the border is not fully visible in the current viewport
                                        if (topPosition.Y < svMain.VerticalOffset || bottomPosition.Y > svMain.VerticalOffset + svMain.ViewportHeight)
                                        {
                                            // Scroll to the position of the top of the border
                                            svMain.ScrollToVerticalOffset(bottomPosition.Y);
                                        }
                                    }

                                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) && Global.shortCut == false && isActive == true || tx < -18000 && Global.shortCut == false && isActive == true)
                                    {
                                        int value = 0;
                                        int maxValue = 0;
                                        int minValue = 0;
                                        Slider selectedSlider = null;

                                        if (borders[optionSelected] == Section1) selectedSlider = sdBright;
                                        if (borders[optionSelected] == Section2) selectedSlider = sdVol;
                                        if (borders[optionSelected] == Section201) selectedSlider = sdFan;

                                        value = (int)selectedSlider.Value;
                                        maxValue = (int)selectedSlider.Maximum;
                                        minValue = (int)selectedSlider.Minimum;

                                        if (selectedSlider == sdFan) value--;
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

                                        if (borders[optionSelected] == Section1) selectedSlider = sdBright;
                                        if (borders[optionSelected] == Section2) selectedSlider = sdVol;
                                        if (borders[optionSelected] == Section201) selectedSlider = sdFan;

                                        value = (int)selectedSlider.Value;
                                        maxValue = (int)selectedSlider.Maximum;
                                        minValue = (int)selectedSlider.Minimum;

                                        if (selectedSlider == sdFan) value++;
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
                                        if (optionSelected == 0 && tsWifi.IsOn == false) tsWifi.IsOn = true;
                                        else if (optionSelected == 0 && tsWifi.IsOn == true) tsWifi.IsOn = false;

                                        if (optionSelected == 1 && tsBlue.IsOn == false) tsBlue.IsOn = true;
                                        else if (optionSelected == 1 && tsBlue.IsOn == true) tsBlue.IsOn = false;

                                        if (borders[optionSelected] == Section1 && isActive == false || borders[optionSelected] == Section2 && isActive == false || borders[optionSelected] == Section201 && isActive == false) isActive = true;
                                        else isActive = false;

                                        if (optionSelected == 5 && tsMic.IsOn == false) tsMic.IsOn = true;
                                        else if (optionSelected == 5 && tsMic.IsOn == true) tsMic.IsOn = false;

                                        if (optionSelected == 6 && tsMouse.IsOn == false) tsMouse.IsOn = true;
                                        else if (optionSelected == 6 && tsMouse.IsOn == true) tsMouse.IsOn = false;

                                        if (optionSelected == 7 && tsBootOnStart.IsOn == false) tsBootOnStart.IsOn = true;
                                        else if (optionSelected == 7 && tsBootOnStart.IsOn == true) tsBootOnStart.IsOn = false;

                                        if (optionSelected == 8 && tsMini.IsOn == false) tsMini.IsOn = true;
                                        else if (optionSelected == 8 && tsMini.IsOn == true) tsMini.IsOn = false;

                                        if (optionSelected == 9 && tsPerf.IsOn == false) tsPerf.IsOn = true;
                                        else if (optionSelected == 9 && tsPerf.IsOn == true) tsPerf.IsOn = false;
                                    }
                                }
                            }
                            updateMenuSelected();
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }


        async void KeyShortCuts_Tick(object sender, EventArgs e)
        {
            ControllerInput(UserIndex.One);
            ControllerInput(UserIndex.Two);
        }

        public void updateMenuSelected()
        {
            var bc = new BrushConverter();
            Section01.Background = new SolidColorBrush(Colors.Transparent);
            Section01.BorderThickness = new Thickness(0, 0.5, 0.5, 0.5);
            Section02.Background = new SolidColorBrush(Colors.Transparent);
            Section02.BorderThickness = new Thickness(0, 0.5, 0.5, 0.5);
            Section1.Background = new SolidColorBrush(Colors.Transparent);
            Section1.BorderThickness = new Thickness(0, 0.5, 0.5, 0.5);
            Section2.Background = new SolidColorBrush(Colors.Transparent);
            Section2.BorderThickness = new Thickness(0, 0.5, 0.5, 0.5);
            Section201.Background = new SolidColorBrush(Colors.Transparent);
            Section201.BorderThickness = new Thickness(0, 0.5, 0.5, 0.5);
            Section03.Background = new SolidColorBrush(Colors.Transparent);
            Section0301.Background = new SolidColorBrush(Colors.Transparent);
            Section3.Background = new SolidColorBrush(Colors.Transparent);
            Section4.Background = new SolidColorBrush(Colors.Transparent);
            Section51.Background = new SolidColorBrush(Colors.Transparent);

            if (connected)
            {
                borders[optionSelected].Background = (Brush)bc.ConvertFrom("#FF393939");

                if (isActive == true && optionSelected >= 2 && optionSelected <= 4)
                {
                    borders[optionSelected].BorderThickness = new Thickness(2.5);
                }
            }
        }
    }
}