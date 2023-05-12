using Game_Launcher_V2.Models;
using Game_Launcher_V2.Properties;
using Game_Launcher_V2.Scripts;
using Game_Launcher_V2.Scripts.ASUS;
using Game_Launcher_V2.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Navigation;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Windows.UI.Notifications;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Game_Launcher_V2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ASUSWmi wmi;     

        public static bool IsRunningAsAdministrator()
        {
            // Get current Windows user
            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();

            // Get current Windows user principal
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(windowsIdentity);

            // Return TRUE if user is in role "Administrator"
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _ = Tablet.TabletDevices;

            if (IsRunningAsAdministrator())
            {
                try
                {
                    if (GetSystemInfo.ProductName.ToLower().Contains("rog") || GetSystemInfo.ProductName.ToLower().Contains("tuf") || GetSystemInfo.ProductName.ToLower().Contains("ally") || GetSystemInfo.ProductName.ToLower().Contains("flow"))
                    {
                        wmi = new ASUSWmi();
                        Global.isASUS = true;
                        xgMobileConnectionService = new XgMobileConnectionService(wmi);

                        if (Settings.Default.isFirstBoot == true)
                        {
                            if (wmi.DeviceGet(ASUSWmi.PerformanceMode) == 0) Settings.Default.fanCurve = ASUSWmi.PerformanceBalanced;
                            if (wmi.DeviceGet(ASUSWmi.PerformanceMode) == 1) Settings.Default.fanCurve = ASUSWmi.PerformanceTurbo;
                            if (wmi.DeviceGet(ASUSWmi.PerformanceMode) == 2) Settings.Default.fanCurve = ASUSWmi.PerformanceSilent;
                            Settings.Default.Save();
                        }
                    }

                    RenderOptions.ProcessRenderMode = RenderMode.Default;
                } catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            if (Settings.Default.openGameList == false)
            {
                StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
            }
            else
            {
                StartupUri = new Uri("Windows/OptionsWindow.xaml", UriKind.Relative);
            }
        }

        public static XgMobileConnectionService xgMobileConnectionService;

        private void SetUpXgMobileDetection()
        {
            //xgMobileConnectionService.XgMobileStatus += (_, e) =>
            //{
            //    if (e.DetectedChanged)
            //    {
            //        ShowDetectedToast(e.Detected);
            //    }
            //    if (e.Connected)
            //    {
            //        ToastNotifications.HideXgMobileActivateToasts();
            //    }
            //};
            //ToastNotificationManagerCompat.OnActivated += toastArgs =>
            //{
            //    if (ToastNotifications.IsActivateXgMobileToastButtonClicked(toastArgs))
            //    {
            //        HandleXgMobileToast(true);
            //    }
            //    else if (ToastNotifications.IsOpenXgMobileToastClicked(toastArgs))
            //    {
            //        HandleXgMobileToast(false);
            //    }
            //};
        }

        private void ShowDetectedToast(bool detected)
        {
            //if (detected)
            //{
            //    ToastNotifications.PromptXgMobileActivate();
            //}
            //else
            //{
            //    ToastNotifications.HideXgMobileActivateToasts();
            //}
        }

        private void HandleXgMobileToast(bool activate)
        {
            //Dispatcher.Invoke(() =>
            //{
            //    new XG_Mobile_Prompt(xgMobileConnectionService, activate).Show();
            //});
        }
    }
}
