using Game_Launcher_V2.Scripts;
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
            public int steamID { get; set; }
            public string gameName { get; set; }
            public string imagePath { get; set; }
            public string bgImagePath { get; set; }
            public string musicPath { get; set; }
            public string message { get; set; }

        }

        public Home()
        {
            InitializeComponent();
            _ = Tablet.TabletDevices;

            setUpTimers();

            LoadSteamGames.loadSteamGames(lbGames);
           
            setUpGUI();

            Global.isOpen = true;
        }

        private void setUpTimers()
        {
            //set up timer for sensor update
            DispatcherTimer sensor = new DispatcherTimer();
            sensor.Interval = TimeSpan.FromSeconds(2);
            sensor.Tick += Update_Tick;
            sensor.Start();

            //set up timer for game name label update
            DispatcherTimer nameUpdate = new DispatcherTimer();
            nameUpdate.Interval = TimeSpan.FromSeconds(0.05);
            nameUpdate.Tick += gameName_Tick;
            nameUpdate.Start();

            //set up timer for key combo system
            DispatcherTimer checkKeyInput = new DispatcherTimer();
            checkKeyInput.Interval = TimeSpan.FromSeconds(0.115);
            checkKeyInput.Tick += KeyShortCuts_Tick;
            checkKeyInput.Start();
        }

        private void setUpGUI()
        {
            var scrollViewer = Global.GetDescendantByType(lbGames, typeof(ScrollViewer)) as ScrollViewer;
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;

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

            Time_and_Bat.getBattery();
            Time_and_Bat.getTime();
            Time_and_Bat.getWifi(imgWiFi);
            Time_and_Bat.updateBatTime(lblBat, lblTime, imgBat);
        }

        //Get battery and time info evry 2 seconds
        void Update_Tick(object sender, EventArgs e)
        {
            Time_and_Bat.getWifi(imgWiFi);
            Time_and_Bat.updateBatTime(lblBat, lblTime, imgBat);
        }

        void gameName_Tick(object sender, EventArgs e)
        {
            if (GameNameBar.ActualWidth != lblGameName.ActualWidth)
            {
                //Update game name label 
                double width = lblGameName.ActualWidth;
                GameNameBar.Width = (width + 28);
            }
        }

       
        private void lbGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Global.GameStore == 0)
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
            if (url != lastBG)
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
            if(lastAudio != audioPath)
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

        private void Media_Ended(object sender, EventArgs e)
        {
            //Restart music play back
            mediaPlayer.Open(new Uri(MediaPath));
            mediaPlayer.Play();
        }

        private static Controller controller;
        void KeyShortCuts_Tick(object sender, EventArgs e)
        {
            ControllerInput();
        }

        private void ControllerInput()
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
                if(lastAudio != "N/A") mediaPlayer.Play();
                wasNotFocused = false;
            }

            //Get controller
            controller = new Controller(UserIndex.One);

            bool connected = controller.IsConnected;

            var scrollViewer = Global.GetDescendantByType(lbGames, typeof(ScrollViewer)) as ScrollViewer;

            btnControl.Visibility = Visibility.Visible;

            

            if (connected && isActive == true)
            {
                //get controller state
                var state = controller.GetState();

                //detect if keyboard or controller combo is being activated
                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) && Global.isAccessMenuOpen == false)
                {
                    //Increase selected item by 1
                    int current = lbGames.SelectedIndex;

                    if (current < lbGames.Items.Count) current++;

                    lbGames.SelectedIndex = current;
                    lbGames.ScrollIntoView(lbGames.SelectedItem);
                }

                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) && Global.isAccessMenuOpen == false)
                {
                    //Decrease selected item by 1
                    int current = lbGames.SelectedIndex;

                    if (current > 0) current--;

                    if (current == 0) scrollViewer.ScrollToHorizontalOffset(0);

                    lbGames.SelectedIndex = current;
                    lbGames.ScrollIntoView(lbGames.SelectedItem);
                }

                if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A) && Global.isAccessMenuOpen == false)
                {
                    loadApp();
                }
            }
        }

        public void loadApp()
        {
            if(Global.GameStore == 0)
            {
                LoadSteamGames.openSteamGame(lbGames);
            }
            
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
    }
}
