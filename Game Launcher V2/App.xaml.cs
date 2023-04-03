using Game_Launcher_V2.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Game_Launcher_V2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
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
        }
    }
}
