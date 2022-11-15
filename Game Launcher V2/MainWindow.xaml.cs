using Game_Launcher_V2.Scripts;
using Game_Launcher_V2.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Game_Launcher_V2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            try
            {
                if (File.Exists("SavedList.txt")) File.Delete("SavedList.txt");
                InitializeComponent();
                FindSteamData.getData();
                PagesNavigation.Navigate(new System.Uri("Pages/Home.xaml", UriKind.RelativeOrAbsolute));

                OptionsWindow win2 = new OptionsWindow();
                win2.Show();
            }
            catch(Exception ex) { MessageBox.Show(ex.ToString()); }
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
    }
}
