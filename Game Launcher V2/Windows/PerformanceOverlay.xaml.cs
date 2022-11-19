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
                long t1, t2;
                long dt = 2000;

                lock (sync)
                {
                    t2 = watch.ElapsedMilliseconds;
                    t1 = t2 - dt;

                    foreach (var x in frames.Values)
                    {
                        //get the number of frames
                        int count = x.QueryCount(t1, t2);

                        //calculate FPS
                        FPS = (double)count / dt * 1000.0;
                        Frametime = GetFrameTime(count);
                    }
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
                if (((int)data.ID == EventID_D3D9PresentStart && data.ProviderGuid == D3D9_provider) ||
                ((int)data.ID == EventID_DxgiPresentStart && data.ProviderGuid == DXGI_provider))
                {
                    int pid = data.ProcessID;
                    long t;

                    lock (sync)
                    {
                        t = watch.ElapsedMilliseconds;

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

                        //store frame timestamp in collection
                        frames[pid].Add(t);
                    }
                }
            };

            watch = new Stopwatch();
            watch.Start();

            Thread thETW = new Thread(EtwThreadProc);
            thETW.IsBackground = true;
            thETW.Start();

            OutputThreadProc();

            //set up timer for sensor update
            DispatcherTimer sensor = new DispatcherTimer();
            sensor.Interval = TimeSpan.FromSeconds(1.5);
            sensor.Tick += Update_Tick;
            sensor.Start();


            var bi2 = new BitmapImage();

            string timeURL = "";

            timeURL = path + "//Assets//Icons//time-line.png";

            using (var stream = new FileStream(timeURL, FileMode.Open, FileAccess.Read))
            {
                bi2.BeginInit();
                bi2.DecodePixelWidth = 24;
                bi2.CacheOption = BitmapCacheOption.OnLoad;
                bi2.StreamSource = stream;
                bi2.EndInit();
            }
            bi2.Freeze();
            imgFrame.Source = bi2;
        }

        public static double GetFrameTime(int count)
        {
            double returnValue = 0;

            int listCount = TimestampCollection.timestamps.Count;

            if (listCount > count)
            {
                for (int i = 1; i <= count; i++)
                {
                    returnValue += TimestampCollection.timestamps[listCount - i] - TimestampCollection.timestamps[listCount - (i + 1)];
                }

                returnValue /= count;
            }

            return returnValue;
        }

        //Get battery and time info evry 2 seconds
        void Update_Tick(object sender, EventArgs e)
        {
            OutputThreadProc();

            if (this.WindowState != WindowState.Maximized) this.WindowState = WindowState.Maximized;

            lblFPS.Text =$"{Math.Round(FPS)} FPS  {Math.Round(Frametime, 2)} ms" ;
        }
    }
}
