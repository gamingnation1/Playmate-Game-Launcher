﻿using Game_Launcher_V2.Scripts;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Game_Launcher_V2.Windows
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        //Get current working directory
        public static string path = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).LocalPath;
        public static string mbo = "";
        

        public OptionsWindow()
        {
            InitializeComponent();

            this.WindowState = System.Windows.WindowState.Maximized;
            this.Hide();

            //set up timer for sensor update
            DispatcherTimer sensor = new DispatcherTimer();
            sensor.Interval = TimeSpan.FromSeconds(2);
            sensor.Tick += Update_Tick;
            sensor.Start();

            //set up timer for key combo system
            DispatcherTimer checkKeyInput = new DispatcherTimer();
            checkKeyInput.Interval = TimeSpan.FromSeconds(0.117);
            checkKeyInput.Tick += KeyShortCuts_Tick;
            checkKeyInput.Start();

            _ = Tablet.TabletDevices;
            setUpGUI();

            PagesNavigation.Navigate(new System.Uri("Pages/OptionsWindow/BasicSettings.xaml", UriKind.RelativeOrAbsolute));

            //Detect if an AYA Neo is being used
            ManagementObjectSearcher baseboardSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");
            foreach (ManagementObject queryObj in baseboardSearcher.Get())
            {
                mbo = queryObj["Manufacturer"].ToString();
                mbo = mbo.ToLower();
            }

            Global.shortCut = true;
        }

        private static Controller controller;

        public static bool hidden = true;

        int menuSelected = 0;
        async void KeyShortCuts_Tick(object sender, EventArgs e)
        {
            try
            {
                //Get controller
                controller = new Controller(UserIndex.One);

                bool connected = controller.IsConnected;
                if (MainDock.Visibility == Visibility.Hidden) MainDock.Visibility = Visibility.Visible;

                if (connected)
                {
                    //get controller state
                    var state = controller.GetState();

                    //detect if keyboard or controller combo is being activated
                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                    {
                        Global.shortCut = true;
                    }


                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                    {
                        Global.shortCut = true;

                        //if hidden show window
                        if (hidden == false)
                        {
                            hidden = true;
                            Global.isAccessMenuOpen = false;
                            this.Hide();
                        }
                        //else hide window
                        else
                        {
                            hidden = false;
                            this.Show();
                            this.Activate();
                            PagesNavigation.NavigationService.Refresh();
                            Global.isAccessMenuOpen = true;
                            this.Activate();
                        }
                    }

                    if(this.Visibility == Visibility.Visible && Global.shortCut == false)
                    {
                        if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder) && Global.shortCut == false)
                        {
                            if (menuSelected > 0) menuSelected--;
                            else menuSelected = 0;
                            changeMenu();
                        }

                        if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder) && Global.shortCut == false)
                        {
                            if (menuSelected < 3) menuSelected++;
                            else menuSelected = 3;
                            changeMenu();
                        }
                    }
                    

                    //if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                    //{
                    //    SendKeys.SendWait("%F");
                    //}

                    Global.shortCut = false;
                }

                int i = 0;
                await Task.Run(() =>
                {
                    foreach (var process in Process.GetProcessesByName("ayaspace")) i++;
                    foreach (var process in Process.GetProcessesByName("AYASpace")) i++;
                });
                if (i < 1 && mbo.Contains("aya"))
                {
                    //detect if keyboard or controller combo is being activated
                    if ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.F12) & KeyStates.Down) > 0 || (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.F12) & KeyStates.Down) > 0 || (Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.LWin) & KeyStates.Down) > 0 || (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.LWin) & KeyStates.Down) > 0 || (Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.RWin) & KeyStates.Down) > 0 || (Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) > 0 && (Keyboard.GetKeyStates(Key.RWin) & KeyStates.Down) > 0)
                    {
                        //if hidden show window
                        if (hidden == false)
                        {
                            hidden = true;
                            Global.isAccessMenuOpen = false;
                            this.Hide();
                        }
                        //else hide window
                        else
                        {
                            hidden = false;
                            this.Show();
                            this.Activate();
                            int openMenu = Global.AccessMenuSelected;
                            Global.AccessMenuSelected = 9999;
                            Global.isAccessMenuOpen = true;
                            this.Activate();
                            this.Focus();
                            Global.AccessMenuSelected = openMenu;
                            PagesNavigation.NavigationService.Refresh();
                        }

                    }
                }
            }
            catch { }
        }

        private void changeMenu()
        {
            try
            {
                PagesNavigation.Content = null;
                PagesNavigation.NavigationService.RemoveBackEntry();

                if (menuSelected == 0) rdBasic.IsChecked = true;
                if (menuSelected == 1) rdPower.IsChecked = true;
                if (menuSelected == 2) rdDisplay.IsChecked = true;
                if (menuSelected == 3) rdMagpie.IsChecked = true;

                if (rdBasic.IsChecked == true) PagesNavigation.Navigate(new System.Uri("Pages/OptionsWindow/BasicSettings.xaml", UriKind.RelativeOrAbsolute));
                if (rdPower.IsChecked == true) PagesNavigation.Navigate(new System.Uri("Pages/OptionsWindow/PowerControl.xaml", UriKind.RelativeOrAbsolute));
                if (rdDisplay.IsChecked == true) PagesNavigation.Navigate(new System.Uri("Pages/OptionsWindow/ComingSoon.xaml", UriKind.RelativeOrAbsolute));
                if (rdMagpie.IsChecked == true) PagesNavigation.Navigate(new System.Uri("Pages/OptionsWindow/ComingSoon.xaml", UriKind.RelativeOrAbsolute));

                Global.AccessMenuSelected = menuSelected;
            }
            catch { }
        }

        private void setUpGUI()
        {
            var bi2 = new BitmapImage();

            string timeURL = "";

            timeURL = path + "//Assets//Icons//time-line.png";

            using (var stream = new FileStream(timeURL, FileMode.Open, FileAccess.Read))
            {
                bi2.BeginInit();
                bi2.DecodePixelWidth = 48;
                bi2.CacheOption = BitmapCacheOption.OnLoad;
                bi2.StreamSource = stream;
                bi2.EndInit();
            }
            bi2.Freeze();

            imgTime.Source = bi2;

            Time_and_Bat_Options.getBattery();
            Time_and_Bat_Options.getTime();
            Time_and_Bat_Options.getWifi(imgWiFi);
            Time_and_Bat_Options.updateBatTime(lblBat, lblTime, imgBat);
        }

        //Get battery and time info evry 2 seconds
        void Update_Tick(object sender, EventArgs e)
        {
            try
            {
                Time_and_Bat_Options.getWifi(imgWiFi);
                Time_and_Bat_Options.updateBatTime(lblBat, lblTime, imgBat);

                string processRyzenAdj = "\\bin\\AMD\\ryzenadj.exe";
                if (Global.RyzenAdj != "" || Global.RyzenAdj != null) RunCLI.ApplySettings(processRyzenAdj, Global.RyzenAdj, true);
            }
            catch { }
        }

        private void rd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PagesNavigation.Content = null;
                PagesNavigation.NavigationService.RemoveBackEntry();

                if (rdBasic.IsChecked == true) menuSelected = 0;
                if (rdPower.IsChecked == true) menuSelected = 1;
                if (rdDisplay.IsChecked == true) menuSelected = 2;
                if (rdMagpie.IsChecked == true) menuSelected = 3;
                Global.AccessMenuSelected = menuSelected;

                if (rdBasic.IsChecked == true) PagesNavigation.Navigate(new System.Uri("Pages/OptionsWindow/BasicSettings.xaml", UriKind.RelativeOrAbsolute));
                if (rdPower.IsChecked == true) PagesNavigation.Navigate(new System.Uri("Pages/OptionsWindow/PowerControl.xaml", UriKind.RelativeOrAbsolute));
                if (rdDisplay.IsChecked == true) PagesNavigation.Navigate(new System.Uri("Pages/OptionsWindow/ComingSoon.xaml", UriKind.RelativeOrAbsolute));
                if (rdMagpie.IsChecked == true) PagesNavigation.Navigate(new System.Uri("Pages/OptionsWindow/ComingSoon.xaml", UriKind.RelativeOrAbsolute));
            }
            catch { }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            Global.isAccessMenuActive = true;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Global.isAccessMenuActive = false;
        }
    }
}
