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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Universal_x86_Tuning_Utility.Scripts.Misc;
using Windows.UI.Notifications;

namespace Game_Launcher_V2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ASUSWmi wmi;

        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T GetService<T>()
            where T : class
        {
            return _host.Services.GetService(typeof(T)) as T;
        }

        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)); })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton(wmi);
                services.AddSingleton<XgMobileConnectionService>();

                // Configuration
                services.Configure<AppConfig>(context.Configuration.GetSection(nameof(AppConfig)));
            }).Build();


        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _ = Tablet.TabletDevices;

            if (Settings.Default.openGameList == false)
            {
                StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
            }
            else
            {
                StartupUri = new Uri("Windows/OptionsWindow.xaml", UriKind.Relative);
            }

            _ = Tablet.TabletDevices;

            if (GetSystemInfo.Product.ToLower().Contains("ally") || GetSystemInfo.Product.ToLower().Contains("rog") || GetSystemInfo.Product.ToLower().Contains("tuf"))
            {
                wmi = new ASUSWmi();
                Global.isASUS = true;
                xgMobileConnectionService = GetService<XgMobileConnectionService>();

                if (Settings.Default.isFirstBoot)
                {
                    Settings.Default.fanCurve = wmi.DeviceGet(ASUSWmi.PerformanceMode);
                    Settings.Default.Save();
                }
            }
        }

        private XgMobileConnectionService xgMobileConnectionService;

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
