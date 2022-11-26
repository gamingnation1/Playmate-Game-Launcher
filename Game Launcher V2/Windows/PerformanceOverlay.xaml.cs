using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;
using Microsoft.Diagnostics.Tracing.Session;
using System.Windows.Threading;
using Game_Launcher_V2.Scripts;
using ControlzEx.Standard;
using System.IO;
using LibreHardwareMonitor.Hardware;
using System.Windows.Forms;
using Game_Launcher_V2.Properties;
using Microsoft.Diagnostics.Tracing.StackSources;

namespace Game_Launcher_V2.Windows
{
    /// <summary>
    /// Interaction logic for PerformanceOverlay.xaml
    /// </summary>
    public partial class PerformanceOverlay : Window
    {
        //helper class to store frame timestamps
        public class TimestampCollection
        {
            const int MAXNUM = 1000;

            public string Name { get; set; }

            public static List<long> timestamps = new List<long>(MAXNUM + 1);
            object sync = new object();

            //add value to the collection
            public void Add(long timestamp)
            {
                lock (sync)
                {
                    timestamps.Add(timestamp);
                    if (timestamps.Count > MAXNUM) timestamps.RemoveAt(0);
                }
            }

            //get the number of timestamps withing interval
            public int QueryCount(long from, long to)
            {
                int c = 0;

                lock (sync)
                {
                    foreach (var ts in timestamps)
                    {
                        if (ts >= from && ts <= to) c++;
                    }
                }
                return c;
            }

            public static double GetFrameTime(int count)
            {
                double returnValue = 0;

                int listCount = timestamps.Count;

                if (listCount > count)
                {
                    for (int i = 1; i <= count; i++)
                    {
                        returnValue += timestamps[listCount - i] - timestamps[listCount - (i + 1)];
                    }

                    returnValue /= count;
                }

                return returnValue;
            }
        }

        public const int EventID_D3D9PresentStart = 1;
        public const int EventID_DxgiPresentStart = 42;

        //ETW provider codes
        public static readonly Guid DXGI_provider = Guid.Parse("{CA11C036-0102-4A2D-A6AD-F03CFED5D3C9}");
        public static readonly Guid D3D9_provider = Guid.Parse("{783ACA0A-790E-4D7F-8451-AA850511C6B9}");

        static TraceEventSession m_EtwSession;
        static Dictionary<int, TimestampCollection> frames = new Dictionary<int, TimestampCollection>();
        static Stopwatch watch = null;
        static object sync = new object();

        static void EtwThreadProc()
        {
            //start tracing
            m_EtwSession.Source.Process();
        }

        static void OutputThreadProc()
        {
            try
            {
                while (true)
                {
                    long t1, t2;
                    long dt = 2000;

                    lock (sync)
                    {
                        t2 = watch.ElapsedMilliseconds;
                        t1 = t2 - dt;

                        foreach (var x in frames.Values)
                        {
                            //Console.Write(x.Name + ": ");
                            proName = x.Name;
                            //get the number of frames
                            int count = x.QueryCount(t1, t2);

                            //calculate FPS
                            //Console.WriteLine("{0} FPS", (double)count / dt * 1000.0);
                            Frametime = TimestampCollection.GetFrameTime(count);
                        }
                    }
                    Thread.Sleep(1000);
                }
            }
            catch { }

        }

        public static double FPS;
        public static double Frametime;
        public static string path = Global.path;
        public PerformanceOverlay()
        {
            InitializeComponent();

            //create ETW session and register providers
            m_EtwSession = new TraceEventSession("mysess");
            m_EtwSession.StopOnDispose = true;
            m_EtwSession.EnableProvider("Microsoft-Windows-D3D9");
            m_EtwSession.EnableProvider("Microsoft-Windows-DXGI");


            //handle event
            m_EtwSession.Source.AllEvents += data =>
            {
                //filter out frame presentation events
                if ((int)data.ID == EventID_DxgiPresentStart && data.ProviderGuid == DXGI_provider)
                {
                    int pid = data.ProcessID;
                    long t;

                    t = watch.ElapsedMilliseconds;
                    try
                    {
                        var check = Process.GetProcessById(pid);
                        if (!check.ProcessName.ToLower().Contains("steamweb") && !check.ProcessName.ToLower().Contains("discord") && !check.ProcessName.ToLower().Contains("msedge") && !check.ProcessName.ToLower().Contains("devenv") && !check.ProcessName.ToLower().Contains("chrome") && !check.ProcessName.ToLower().Contains("wsaclient") && !check.ProcessName.ToLower().Contains("epicgamesl") && !check.ProcessName.ToLower().Contains("eadesktop") && !check.ProcessName.ToLower().Contains("battle.net") && !check.ProcessName.ToLower().Contains("ayaspace") && !check.ProcessName.ToLower().Contains("galaxyclient") && check.ProcessName.ToLower() != "dwm" && !check.ProcessName.ToLower().Contains("socialclub") && !check.ProcessName.ToLower().Contains("amdrs") && !check.ProcessName.ToLower().Contains("amdow") && !check.ProcessName.ToLower().Contains("atieclxx") && !check.ProcessName.ToLower().Contains("radeonsoftware") && !check.ProcessName.ToLower().Contains("spotify") && !check.ProcessName.ToLower().Contains("disneyplus") && !check.ProcessName.ToLower().Contains("microsoft.media") && !check.ProcessName.ToLower().Contains("epicwebh"))
                        {
                            //if process is not yet in Dictionary, add it
                            if (!frames.ContainsKey(pid))
                            {
                                frames[pid] = new TimestampCollection();
                                string name = "";
                                var proc = Process.GetProcessById(pid);

                                if (proc != null)
                                {
                                    using (proc)
                                    {
                                        name = proc.ProcessName;
                                    }
                                }
                                else name = pid.ToString();

                                frames[pid].Name = name;
                            }
                            frames[pid].Add((long)data.TimeStampRelativeMSec);
                        }
                    } catch { }
                }
            };

            watch = new Stopwatch();
            watch.Start();

            Thread thETW = new Thread(EtwThreadProc);
            thETW.IsBackground = true;
            thETW.Start();

            Thread thOutput = new Thread(OutputThreadProc);
            thOutput.IsBackground = true;
            thOutput.Start();


            //set up timer for sensor update
            DispatcherTimer sensor = new DispatcherTimer();
            sensor.Interval = TimeSpan.FromSeconds(2);
            sensor.Tick += Update_Tick;
            sensor.Start();

            DispatcherTimer frame = new DispatcherTimer();
            frame.Interval = TimeSpan.FromSeconds(1);
            frame.Tick += FPS_Tick;
            frame.Start();

            imgCPU.Source = new BitmapImage(new Uri(path + "\\Assets\\Icons\\cpu-line.png"));
            imgGPU.Source = new BitmapImage(new Uri(path + "\\Assets\\Icons\\cpu-fill.png"));
            imgRAM.Source = new BitmapImage(new Uri(path + "\\Assets\\Icons\\database-2-line.png"));
            imgFrame.Source = new BitmapImage(new Uri(path + "\\Assets\\Icons\\time-line.png"));

            getBatteryTime();
            updateBatIcon();

            updateInfo();

            _ = Tablet.TabletDevices;
        }

        public static string proName;
        public static int CPUTemp, CPULoad, CPUClock, CPUPower;
        public static int GPUTemp, GPULoad, GPUClock;
        public static int RAMLoad, RAMData, RAMClock;
        public static float batRate;
        async void Update_Tick(object sender, EventArgs e)
        {
            updateInfo();
        }

        public async void updateInfo()
        {
            try
            {
                if(Global.isMainActive || Global.menuselectOpen) this.Hide();
                else this.Show();

                if (Settings.Default.CPUName.ToLower().Contains("intel")) await Task.Run(() => { CPUTemp = (int)GetSensor.getCPUInfo(SensorType.Temperature, "Package"); });
                else await Task.Run(() => { CPUTemp = (int)GetSensor.getCPUInfo(SensorType.Temperature, "Core"); });

                await Task.Run(() => { CPUClock = (int)GetSensor.getCPUInfo(SensorType.Clock, "Core #1"); });
                await Task.Run(() => { CPULoad = (int)GetSensor.getCPUInfo(SensorType.Load, "Total"); });
                await Task.Run(() => { CPUPower = (int)GetSensor.getCPUInfo(SensorType.Power, "Package"); });

                await Task.Run(() => { GPUTemp = (int)GetSensor.getAMDGPU(SensorType.Temperature, "GPU Core"); });
                await Task.Run(() => { GPULoad = (int)GetSensor.getAMDGPU(SensorType.Load, "GPU Core"); });
                await Task.Run(() => { GPUClock = (int)GetSensor.getAMDGPU(SensorType.Clock, "GPU Core"); });

                await Task.Run(() => { RAMLoad = (int)GetSensor.getRAMInfo(SensorType.Load, "Virtual"); });
                await Task.Run(() => { RAMData = (int)(GetSensor.getRAMInfo(SensorType.Data, "Memory Used") * 1000); });
                await Task.Run(() => { RAMClock = (int)GetSensor.getAMDGPU(SensorType.Clock, "Memory"); });

                lblCPU.Text = $"{CPUTemp}°C  {CPULoad}%  {CPUClock} MHz  {CPUPower}W";
                lblGPU.Text = $"{GPUTemp}°C  {GPULoad}%  {GPUClock} MHz";

                if (Settings.Default.CPUName.ToLower().Contains("intel"))
                {
                    lblRAM.Text = $"{RAMLoad}%  {RAMData} MB";
                    spGPU.Visibility = Visibility.Collapsed;
                }
                else lblRAM.Text = $"{RAMLoad}%  {RAMData} MB  {RAMClock} MHz";
            }
            catch { }
        }

        double lastFPS = 0;
        double lastFrametime = 0;
        async void FPS_Tick(object sender, EventArgs e)
        {
            try
            {
                FPS = 1000 / Frametime;

                getBatteryTime();
                updateBatIcon();

                if (!double.IsFinite(Frametime) && !double.IsFinite(FPS)) FPS = lastFPS;
                if (!double.IsFinite(Frametime) && !double.IsFinite(FPS)) Frametime = lastFrametime;

                Process[] processes = Process.GetProcessesByName(proName);

                if (processes.Length > 0)
                {
                    if (double.IsFinite(Frametime) && double.IsFinite(FPS)) lblFPS.Text = $"{Math.Round(FPS)} FPS  {Math.Round(Frametime, 2)} ms  {proName}";
                }
                else
                {
                    lblFPS.Text = "";
                }

                if (GPULoad < 15 && !Settings.Default.CPUName.ToLower().Contains("intel") && processes.Length <= 0) spFrameData.Visibility = Visibility.Collapsed;
                else spFrameData.Visibility = Visibility.Visible;

                if (lblFPS.Text == "") spFrameData.Visibility = Visibility.Collapsed;

                if (double.IsFinite(Frametime) && double.IsFinite(FPS)) lastFPS = FPS;
                if (double.IsFinite(Frametime) && double.IsFinite(FPS)) lastFrametime = Frametime;
            }
            catch { }
        }

        static string lastBat = "";
        public void updateBatIcon()
        {
            try
            {
                string batURL = "";

                //Update battery icon based on battery level

                //Update battery icon based on battery level
                if (Convert.ToInt32(Time_and_Bat.batPercentInt) > 50)
                {
                    batURL = path + "//Assets//Icons//battery-fill.png";
                }
                if (Convert.ToInt32(Time_and_Bat.batPercentInt) < 45)
                {
                    batURL = path + "//Assets//Icons//battery-low-line.png";
                }

                if (Time_and_Bat.statuscode == 2 || Time_and_Bat.statuscode == 6 || Time_and_Bat.statuscode == 7 || Time_and_Bat.statuscode == 8)
                {
                    batURL = path + "//Assets//Icons//battery-charge-line.png";
                }
                imgBat.Source = new BitmapImage(new Uri(batURL));
                lastBat = batURL;
            } catch { }
        }

        public static TimeSpan time;
        public static float batTime;


        public void getBatteryTime()
        {
            try
            {
                PowerStatus pwr = System.Windows.Forms.SystemInformation.PowerStatus;
                //Get battery life

                batTime = (float)pwr.BatteryLifeRemaining;

                bool isCharging = false;

                if (Time_and_Bat.statuscode == 2 || Time_and_Bat.statuscode == 6 || Time_and_Bat.statuscode == 7 || Time_and_Bat.statuscode == 8)
                {
                    batTime = 0;
                    isCharging = true;
                }
                time = TimeSpan.FromSeconds(batTime);

                lblBat.Text = $"{Time_and_Bat.batPercentInt}%  {time:%h} Hours {time:%m} Minutes";

                if (lblBat.Text.Contains("0 Hours 0 Minutes") && isCharging == true) lblBat.Text = $"{Time_and_Bat.batPercentInt}%";
                if (lblBat.Text.Contains("0 Hours 0 Minutes") && isCharging == false) lblBat.Text = $"{Time_and_Bat.batPercentInt}%  Calculating";

                spBat.Visibility = Visibility.Visible;
            } catch { }
        }
    }
}
