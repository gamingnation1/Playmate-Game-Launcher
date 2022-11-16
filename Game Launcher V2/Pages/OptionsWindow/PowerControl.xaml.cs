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

namespace Game_Launcher_V2.Pages.OptionsWindow
{
    /// <summary>
    /// Interaction logic for PowerControl.xaml
    /// </summary>
    public partial class PowerControl : Page
    {
        //Get current working directory
        public static string path = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).LocalPath;


        public PowerControl()
        {
            InitializeComponent();
            //set up timer for sensor update
            DispatcherTimer sensor = new DispatcherTimer();
            sensor.Interval = TimeSpan.FromSeconds(2);
            sensor.Tick += Update_Tick;
            sensor.Start();

            _ = Tablet.TabletDevices;

            lblBat.Text = Time_and_Bat.batPercent;

           BatStats.getBatteryTime(lblBatTime);
           BatStats.updateBatIcon(imgBat);

            ThemeManager.Current.ChangeTheme(this, "Dark.Teal");
        }

        void Update_Tick(object sender, EventArgs e)
        {
            lblBat.Text = Time_and_Bat.batPercent;

            BatStats.getBatteryTime(lblBatTime);
            BatStats.updateBatIcon(imgBat);
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            applySettings();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void Toggle_Toggled(object sender, RoutedEventArgs e)
        {
            if(tsTemp.IsOn == true) Section3.Visibility = Visibility.Visible; else Section3.Visibility = Visibility.Collapsed;
            if (tsPower.IsOn == true) Section5.Visibility = Visibility.Visible; else Section5.Visibility = Visibility.Collapsed;
            if (tsGPU.IsOn == true) Section8.Visibility = Visibility.Visible; else Section8.Visibility = Visibility.Collapsed;
            if (tsCPUCO.IsOn == true) Section10.Visibility = Visibility.Visible; else Section10.Visibility = Visibility.Collapsed;
        }

        private void tsCPUClk_Toggled(object sender, RoutedEventArgs e)
        {
            if (tsCPUClk.IsOn == true) CPUboost.cpuBoost(false); else CPUboost.cpuBoost(true);
        }

        private void applySettings()
        {
            string processRyzenAdj = "";
            string result = "";
            string commandArguments = "";

            int TDP = (int)sdPower.Value;
            int Temp = (int)sdTemp.Value;
            TDP = TDP * 1000;
            int iGFX = (int)sdGFXClock.Value;

            if (tsTemp.IsOn == true) commandArguments = $"--tctl-temp={Temp} --skin-temp-limit={Temp} ";
            if (tsPower.IsOn == true) commandArguments = commandArguments + $"--stapm-limit={TDP} --slow-limit={TDP} --fast-limit={TDP} --vrm-current={TDP * 1.33} --vrmmax-current={TDP * 1.33} ";
            if (tsGPU.IsOn == true) commandArguments = commandArguments + $"--gfx-clk={iGFX} ";

            try
            {
                processRyzenAdj = "\\bin\\AMD\\ryzenadj.exe";
                RunCLI.ApplySettings(processRyzenAdj, commandArguments, true);
            }
            catch (Exception ex) { }
        }
    }
}
