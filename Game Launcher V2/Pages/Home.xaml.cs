using Game_Launcher_V2.Properties;
using Game_Launcher_V2.Scripts;
using Game_Launcher_V2.Scripts.Epic_Games;
using Game_Launcher_V2.Windows;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace Game_Launcher_V2.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        //Get current working directory
        public static string path = Global.path;

        //Variables for mediaplayer
        private static MediaPlayer mediaPlayer = new MediaPlayer();
        public static string MediaPath = "";

        //Save if window was out of focus
        private bool wasNotFocused = false;

        public class SteamGame
        {
            public int ID { get; set; }
            public string steamID { get; set; }
            public string gameName { get; set; }
            public string imagePath { get; set; }
            public string bgImagePath { get; set; }
            public string musicPath { get; set; }
            public string message { get; set; }

        }

        int currentGameStore;
        bool thisWorking = true;

        public Home()
        {
            InitializeComponent();
            _ = Tablet.TabletDevices;

            if (Global.GameStore == 0)
            {
                LoadSteamGames.loadSteamGames(lbGames);
            }
            if (Global.GameStore == 1)
            {
                LoadEpicGamesData.loadEpicGames(lbGames);
            }

            thisWorking = true;

            currentGameStore = Global.GameStore;

            setUpTimers();

            setUpGUI();

            Global.isOpen = true;
        }

        public DispatcherTimer sensor = new DispatcherTimer();
        public DispatcherTimer nameUpdate = new DispatcherTimer();
        public DispatcherTimer checkKeyInput = new DispatcherTimer();
        private void setUpTimers()
        {
            //set up timer for sensor update
            
            sensor.Interval = TimeSpan.FromSeconds(2);
            sensor.Tick += Update_Tick;
            sensor.Start();

            //set up timer for key combo system
            
            checkKeyInput.Interval = TimeSpan.FromSeconds(0.12);
            checkKeyInput.Tick += KeyShortCuts_Tick;
            checkKeyInput.Start();
        }

        private void SetImageSource(string imageUrl, Image image)
        {
            var imageSource = new BitmapImage();
            using (var stream = new FileStream(imageUrl, FileMode.Open, FileAccess.Read))
            {
                imageSource.BeginInit();
                imageSource.DecodePixelWidth = 48;
                imageSource.CacheOption = BitmapCacheOption.OnLoad;
                imageSource.StreamSource = stream;
                imageSource.EndInit();
            }
            imageSource.Freeze();

            image.Source = imageSource;
        }

        private void setUpGUI()
        {
            try
            {


                var scrollViewer = Global.GetDescendantByType(lbGames, typeof(ScrollViewer)) as ScrollViewer;
                scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;

                SetImageSource(System.IO.Path.Combine(path, "Assets", "Icons", "time-line.png"), imgTime);
                SetImageSource(System.IO.Path.Combine(path, "Assets", "Icons", "Xbox", "A.png"), imgA);
                SetImageSource(System.IO.Path.Combine(path, "Assets", "Icons", "Xbox", "D-Pad Left.png"), imgDPadLeft);
                SetImageSource(System.IO.Path.Combine(path, "Assets", "Icons", "Xbox", "D-Pad Right.png"), imgDPadRight);

                string wifiURL = "";
                double wifi = Global.wifi;

                if (wifi > 75)
                {
                    wifiURL = path + "//Assets//Icons//signal-wifi-fill.png";
                }
                if (wifi < 75 && wifi > 45)
                {
                    wifiURL = path + "//Assets//Icons//signal-wifi-2-fill.png";
                }
                if (wifi < 45)
                {
                    wifiURL = path + "//Assets//Icons//signal-wifi-1-fill.png";
                }

                imgWiFi.Source = new BitmapImage(new Uri(wifiURL));

                lastWifi = wifiURL;


                //Update battery and time text blocks
                lblBat.Text = Time_and_Bat.batPercent;
                lblTime.Text = Time_and_Bat.time;

                int batPercentInt = Time_and_Bat.batPercentInt;
                UInt16 statuscode = Time_and_Bat.statuscode;

                string batURL = "";


                //Update battery icon based on battery level
                if (Convert.ToInt32(batPercentInt) > 50)
                {
                    batURL = path + "//Assets//Icons//battery-fill.png";
                }
                if (Convert.ToInt32(batPercentInt) < 45)
                {
                    batURL = path + "//Assets//Icons//battery-low-line.png";
                }

                if (statuscode == 2 || statuscode == 6 || statuscode == 7 || statuscode == 8)
                {
                    batURL = path + "//Assets//Icons//battery-charge-line.png";
                }

                imgBat.Source = new BitmapImage(new Uri(batURL));
                lastBattery = batURL;
            }
            catch { }
            

        }

        //Get battery and time info evry 2 seconds
        void Update_Tick(object sender, EventArgs e)
        {
            try
            {
                GC.Collect();

                Time_and_Bat.updateBatTime(lblBat, lblTime, imgBat);
                Time_and_Bat.GetWifi(imgWiFi);
            }
            catch
            {

            }
        }


        private void lbGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (thisWorking)
            {
                SteamGame model = lbGames.SelectedItem as SteamGame;
                LoadSteamGames.changeSteamGame(lbGames, lblGameName, btnControl);
                updateBGImage(model.bgImagePath);
                playAudio(model.musicPath);
                lastAudio = model.musicPath;
                lastBG = model.bgImagePath;
            }
        }

        string lastBG = "";
        public async Task updateBGImage(string url)
        {
            try
            {
                if (url != lastBG && thisWorking)
                {
                    await StartAnimationBGFadeOut();
                    //Save new image and load it
                    var bi = new BitmapImage();

                    using (var stream = new FileStream(url, FileMode.Open, FileAccess.Read))
                    {
                        bi.BeginInit();
                        bi.DecodePixelWidth = 3072;
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.StreamSource = stream;
                        bi.EndInit();
                    }

                    bi.Freeze();
                    //Set BG image
                    await Task.Delay(145);
                    GameBG.Source = bi;
                    await Task.Delay(145);
                    //Start fade in animation
                    await StartAnimationBGFadeIn();
                }
            }
            catch { }
        }

        DoubleAnimation fadeOut = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = new Duration(TimeSpan.FromSeconds(0.50)),
        };

        //Fade in animation
        DoubleAnimation fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromSeconds(0.60)),
        };

        //background fade out animation
        private async Task StartAnimationBGFadeOut()
        {
            GameBG.BeginAnimation(OpacityProperty, fadeOut);
            await Task.Delay(950);
        }

        //background fade in animation
        private async Task StartAnimationBGFadeIn()
        {
            GameBG.BeginAnimation(OpacityProperty, fadeIn);
            await Task.Delay(950);
        }

        string lastAudio;
        //Update media player with current game BG music 
        private async Task playAudio(string audioPath)
        {
            try
            {
                if (lastAudio != audioPath && thisWorking)
                {
                    if (audioPath == null || audioPath == "N/A")
                    {
                        //Stop current music 
                        mediaPlayer.Stop();
                    }
                    else
                    {
                        await Task.Delay(350);
                        //Stop current music 
                        mediaPlayer.Stop();
                        //Open new game music
                        mediaPlayer.Open(new Uri(audioPath));
                        MediaPath = audioPath;
                        //Make sure music repeats on end
                        mediaPlayer.MediaEnded += new EventHandler(Media_Ended);
                        //Set volume to 90%
                        mediaPlayer.Volume = 0.9;
                        //Play music
                        mediaPlayer.Play();
                    }
                }
            }
            catch { }
        }

        private void Media_Ended(object sender, EventArgs e)
        {
            //Restart music play back
            mediaPlayer.Open(new Uri(MediaPath));
            mediaPlayer.Play();
        }

        private static Controller controller = new Controller(UserIndex.One);

        void KeyShortCuts_Tick(object sender, EventArgs e)
        {
            ControllerInput();
        }

        private void ControllerInput()
        {
            try
            {
                bool isActive = Global.isMainActive;

                //If window is not focused stop music
                if (isActive != true)
                {
                    mediaPlayer.Pause();
                    wasNotFocused = true;
                }

                //If window is now focused resume music
                else if (isActive == true && wasNotFocused == true)
                {
                    if (lastAudio != "N/A") mediaPlayer.Play();
                    wasNotFocused = false;
                }

                //Get controller
                bool connected = controller.IsConnected;

                var scrollViewer = Global.GetDescendantByType(lbGames, typeof(ScrollViewer)) as ScrollViewer;

                if (connected && bottomBar.Visibility == Visibility.Hidden) bottomBar.Visibility = Visibility.Visible;
                if (!connected && bottomBar.Visibility == Visibility.Visible) bottomBar.Visibility = Visibility.Hidden;

                if (connected && isActive)
                {
                    //get controller state
                    var state = controller.GetState();

                    //detect if keyboard or controller combo is being activated
                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) && !Global.isAccessMenuOpen)
                    {
                        //Increase selected item by 1
                        int current = lbGames.SelectedIndex;

                        if (current < lbGames.Items.Count) current++;

                        lbGames.SelectedIndex = current;
                        lbGames.ScrollIntoView(lbGames.SelectedItem);
                    }

                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) && !Global.isAccessMenuOpen)
                    {
                        //Decrease selected item by 1
                        int current = lbGames.SelectedIndex;

                        if (current > 0) current--;

                        if (current == 0) scrollViewer.ScrollToHorizontalOffset(0);

                        lbGames.SelectedIndex = current;
                        lbGames.ScrollIntoView(lbGames.SelectedItem);
                    }

                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A) && !Global.isAccessMenuOpen)
                    {
                        loadApp();
                    }
                }
            }
            catch { }
        }


        public void loadApp()
        {
            LoadSteamGames.openSteamGame(lbGames);
        }

        private void lbGames_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            ScrollViewer scrollviewer = Global.FindVisualChildren<ScrollViewer>(listBox).FirstOrDefault();
            if (e.Delta > 0)
                scrollviewer.LineLeft();
            else
                scrollviewer.LineRight();
            e.Handled = true;
        }

        private void btnControl_Click(object sender, RoutedEventArgs e)
        {
            loadApp();
        }

        static string lastWifi;

        static string lastBattery;
    }
}
