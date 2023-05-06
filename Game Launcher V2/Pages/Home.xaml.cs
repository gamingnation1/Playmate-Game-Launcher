using Game_Launcher_V2.Properties;
using Game_Launcher_V2.Scripts;
using Game_Launcher_V2.Scripts.Epic_Games;
using Game_Launcher_V2.Windows;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Management;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Windows.Devices.Sensors;
using Windows.Gaming.Input;
using Windows.Gaming.Preview.GamesEnumeration;
using Cursors = System.Windows.Input.Cursors;
using ListBox = System.Windows.Controls.ListBox;
using MessageBox = System.Windows.MessageBox;

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

        string mbo = "";
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
        int lastSelection = 0;

        public Home()
        {
            InitializeComponent();
            _ = Tablet.TabletDevices;

            lblAll.FontWeight = FontWeights.Normal;
            lblAll.Foreground = new SolidColorBrush(Colors.Gray);
            lblSteam.FontWeight = FontWeights.Normal;
            lblSteam.Foreground = new SolidColorBrush(Colors.Gray);
            lblEpic.FontWeight = FontWeights.Normal;
            lblEpic.Foreground = new SolidColorBrush(Colors.Gray);
            lblBattle.FontWeight = FontWeights.Normal;
            lblBattle.Foreground = new SolidColorBrush(Colors.Gray);
            lblgog.FontWeight = FontWeights.Normal;
            lblgog.Foreground = new SolidColorBrush(Colors.Gray);
            lblea.FontWeight = FontWeights.Normal;
            lblea.Foreground = new SolidColorBrush(Colors.Gray);
            lblUbi.FontWeight = FontWeights.Normal;
            lblUbi.Foreground = new SolidColorBrush(Colors.Gray);
            lblRock.FontWeight = FontWeights.Normal;
            lblRock.Foreground = new SolidColorBrush(Colors.Gray);

            loadGames();

            thisWorking = true;

            currentGameStore = Global.GameStore;

            setUpTimers();

            setUpGUI();

            Global.isOpen = true;

            if (Global.mbo.Contains("aya")) { 
                SetImageSource(System.IO.Path.Combine(path, "Assets", "Icons", "aya-logo.png"), imgSettings);
                imgSettings.Visibility = Visibility.Visible;
                lblSettings.Visibility = Visibility.Visible;
            }
            else
            {
                imgSettings.Visibility = Visibility.Collapsed;
                lblSettings.Visibility = Visibility.Collapsed;
                imgDPadLeft.Margin = new Thickness(0, 0, 4, 0);
            }

            var scrollviewer = Global.GetDescendantByType(lbGames, typeof(ScrollViewer)) as ScrollViewer;

            if (scrollviewer != null)
            {
                if (scrollviewer.HorizontalOffset <= 50)
                {
                    lbGames.Margin = new Thickness(5, -15, 0, 0);
                }
                else if (scrollviewer.HorizontalOffset >= scrollviewer.ScrollableWidth - 125)
                {
                    lbGames.Margin = new Thickness(0, -15, 10, 0);
                }
                else
                {
                    lbGames.Margin = new Thickness(0, -15, 0, 0);
                }
            }
        }

        public void loadGames()
        {
            if (Global.GameStore == 0)
            {
                LoadGames.LoadAllGameData(lbGames);
                lblAll.FontWeight = FontWeights.DemiBold;
                lblAll.Foreground = new SolidColorBrush(Colors.White);
            }
            if (Global.GameStore == 1)
            {
                LoadGames.LoadGameData("Steam", lbGames);
                lblSteam.FontWeight = FontWeights.DemiBold;
                lblSteam.Foreground = new SolidColorBrush(Colors.White);
            }
            if (Global.GameStore == 2)
            {
                LoadGames.LoadGameData("Epic Games", lbGames);
                lblEpic.FontWeight = FontWeights.DemiBold;
                lblEpic.Foreground = new SolidColorBrush(Colors.White);
            }
            //if (Global.GameStore == 3)
            //{
            //    LoadGames.LoadGameData("Battle.net", lbGames);
            //    lblBattle.FontWeight = FontWeights.DemiBold;
            //    lblBattle.Foreground = new SolidColorBrush(Colors.White);
            //}
            if (Global.GameStore == 3)
            {
                LoadGames.LoadGameData("GOG Galaxy", lbGames);
                lblgog.FontWeight = FontWeights.DemiBold;
                lblgog.Foreground = new SolidColorBrush(Colors.White);
            }
            //if (Global.GameStore == 4)
            //{
            //    LoadGames.LoadGameData("Origin", lbGames);
            //    lblea.FontWeight = FontWeights.DemiBold;
            //    lblea.Foreground = new SolidColorBrush(Colors.White);
            //}
            if (Global.GameStore == 4)
            {
                LoadGames.LoadGameData("Ubisoft Connect", lbGames);
                lblUbi.FontWeight = FontWeights.DemiBold;
                lblUbi.Foreground = new SolidColorBrush(Colors.White);
            }
            //if (Global.GameStore == 6)
            //{
            //    LoadGames.LoadGameData("Rockstar Games", lbGames);
            //    lblRock.FontWeight = FontWeights.DemiBold;
            //    lblRock.Foreground = new SolidColorBrush(Colors.White);
            //}
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
            int pixelWidth = 64;

            if (image == imgSettings) pixelWidth = 96;

            using (var stream = new FileStream(imageUrl, FileMode.Open, FileAccess.Read))
            {
                imageSource.BeginInit();
                imageSource.DecodePixelWidth = pixelWidth;
                imageSource.CacheOption = BitmapCacheOption.OnLoad;
                imageSource.StreamSource = stream;
                imageSource.EndInit();
            }
            imageSource.Freeze();

            image.Source = imageSource;
        }

        private void ScrollViewerCanvas_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        private async void setUpGUI()
        {
            try
            { 
                var scrollViewer = Global.GetDescendantByType(lbGames, typeof(ScrollViewer)) as ScrollViewer;
                scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;

                SetImageSource(System.IO.Path.Combine(path, "Assets", "Icons", "time-line.png"), imgTime);
                SetImageSource(System.IO.Path.Combine(path, "Assets", "Icons", "Xbox", "A.png"), imgA);
                SetImageSource(System.IO.Path.Combine(path, "Assets", "Icons", "Xbox", "X.png"), imgX);
                SetImageSource(System.IO.Path.Combine(path, "Assets", "Icons", "Xbox", "B.png"), imgB);
                SetImageSource(System.IO.Path.Combine(path, "Assets", "Icons", "Xbox", "D-Pad Left.png"), imgDPadLeft);
                SetImageSource(System.IO.Path.Combine(path, "Assets", "Icons", "Xbox", "D-Pad Right.png"), imgDPadRight);
                SetImageSource(System.IO.Path.Combine(path, "Assets", "Icons", "play-mini-fill.png"), imgPlay);
                SetImageSource(System.IO.Path.Combine(path, "Assets", "Icons", "Xbox", "Left Bumper.png"), imgLB);
                SetImageSource(System.IO.Path.Combine(path, "Assets", "Icons", "Xbox", "Right Bumper.png"), imgRB);
                SetImageSource(System.IO.Path.Combine(path, "Assets", "Icons", "settings-4-line.png"), imgSettingsBtn);

                Animate._imageBlurState.Add(GameBG, false);
                Animate._dockPanelOpacityState.Add(mainBody, true);
                Animate._dockPanelOpacityState.Add(gameLaunch, false);

                string wifiURL = "";
                double wifi = Global.wifi;

                if (wifi >= 4)
                {
                    wifiURL = path + "//Assets//Icons//signal-wifi-fill.png";
                }
                else if (wifi >= 2)
                {
                    wifiURL = path + "//Assets//Icons//signal-wifi-2-fill.png";
                }
                else if (wifi < 2)
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
                if (Convert.ToInt32(batPercentInt) < 50)
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
                Time_and_Bat.updateBatTime(lblBat, lblTime, imgBat);
                Time_and_Bat.GetWifi(imgWiFi);
            }
            catch
            {

            }
        }

        bool held = false;
        bool waiting = false;
        private async void lbGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!hasLaunched)
                {
                    if (thisWorking)
                    {
                        lastSelection = lbGames.SelectedIndex;
                        SteamGame model = lbGames.SelectedItem as SteamGame;
                        LoadSteamGames.changeSteamGame(lbGames, lblGameName, lblControl);

                        var controller = new Controller(UserIndex.One);
                        var controller2 = new Controller(UserIndex.Two);

                        tbLaunchGameName.Text = model.gameName;
                        tbGameLaunchGameMessage.Text = model.message;

                        var bitmapImage = new BitmapImage(new Uri(model.imagePath));

                        // Create an ImageBrush with a uniform-to-fill mode and set its ImageSource to the loaded image
                        var brush = new ImageBrush()
                        {
                            Stretch = Stretch.UniformToFill,
                            ImageSource = bitmapImage
                        };

                        // Set the existing Border's Background to the ImageBrush
                        tbGameLaunchGameImg.Background = brush;

                        lbGames.UpdateLayout();
                        lbGames.ScrollIntoView(lbGames.SelectedItem);
                        FrameworkElement container = (FrameworkElement)lbGames.ItemContainerGenerator.ContainerFromItem(lbGames.SelectedItem);
                        container?.BringIntoView();

                        mediaPlayer.Pause();
                        if(GameBG.Opacity == 1) await StartAnimationBGFadeOut();

                        if (controller2.IsConnected)
                        {
                            SharpDX.XInput.Gamepad gamepad = controller2.GetState().Gamepad;
                            float tx = gamepad.LeftThumbX;
                            // Check if the D-Pad left button is being held down
                            if (controller.GetState().Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) || controller.GetState().Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) || tx > 18000 || tx < -18000)
                            {
                                held = true;
                            }
                            else
                            {
                                held = false;
                            }
                        }
                        else if (controller.IsConnected)
                        {
                            SharpDX.XInput.Gamepad gamepad = controller.GetState().Gamepad;
                            float tx = gamepad.LeftThumbX;
                            // Check if the D-Pad left button is being held down
                            if (controller.GetState().Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) || controller.GetState().Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) || tx > 18000 || tx < -18000)
                            {
                                held = true;
                            }
                            else
                            {
                                held = false;
                            }
                        }

                        if (waiting) return;

                        if (held)
                        {
                            waiting = true;

                            while (held)
                            {
                                await Task.Delay(10);

                                if (controller2.IsConnected)
                                {
                                    SharpDX.XInput.Gamepad gamepad = controller2.GetState().Gamepad;
                                    float tx = gamepad.LeftThumbX;

                                    // Check if the D-Pad left button is being held down
                                    if (controller.GetState().Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) || controller.GetState().Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) || tx > 18000 || tx < -18000)
                                    {
                                        held = true;
                                    }
                                    else
                                    {
                                        held = false;
                                    }
                                }
                                else if (controller.IsConnected)
                                {
                                    SharpDX.XInput.Gamepad gamepad = controller.GetState().Gamepad;
                                    float tx = gamepad.LeftThumbX;

                                    // Check if the D-Pad left button is being held down
                                    if (controller.GetState().Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) || controller.GetState().Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) || tx > 18000 || tx < -18000)
                                    {
                                        held = true;
                                    }
                                    else
                                    {
                                        held = false;
                                    }
                                }
                            }

                            model = lbGames.SelectedItem as SteamGame;
                            waiting = false;
                            updateBGImage(model.bgImagePath);
                            playAudio(model.musicPath);
                            lastAudio = model.musicPath;
                            lastBG = model.bgImagePath;
                        }
                        else
                        {
                            model = lbGames.SelectedItem as SteamGame;
                            waiting = false;
                            updateBGImage(model.bgImagePath);
                            playAudio(model.musicPath);
                            lastAudio = model.musicPath;
                            lastBG = model.bgImagePath;
                        }
                    }


                    ListBox listBox = sender as ListBox;
                    ScrollViewer scrollviewer = Global.FindVisualChildren<ScrollViewer>(listBox).FirstOrDefault();
                    if (scrollviewer != null)
                    {
                        if (scrollviewer.HorizontalOffset <= 50)
                        {
                            lbGames.Margin = new Thickness(5, -15, 0, 0);
                        }
                        else if (scrollviewer.HorizontalOffset >= scrollviewer.ScrollableWidth - 125)
                        {
                            lbGames.Margin = new Thickness(0, -15, 10, 0);
                        }
                        else
                        {
                            lbGames.Margin = new Thickness(0, -15, 0, 0);
                        }
                    }
                }
                else
                {
                    lbGames.SelectedIndex = lastSelection;
                    lbGames.ScrollIntoView(lbGames.SelectedItem);
                }
            } catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        string lastBG = "";
        public async Task updateBGImage(string url)
        {
            try
            {
                if (url != lastBG && thisWorking)
                {
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
            Duration = new Duration(TimeSpan.FromSeconds(0.3)),
        };

        //Fade in animation
        DoubleAnimation fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromSeconds(0.4)),
        };

        //background fade out animation
        private async Task StartAnimationBGFadeOut()
        {
            // Get the current opacity value of GameBG
            double currentOpacity = GameBG.Opacity;

            // Set the frame rate of the fadeOut animation to 60 frames per second
            Timeline.SetDesiredFrameRate(fadeOut, 60);

            // Set the From property of the fadeOut animation to the current opacity value
            fadeOut.From = currentOpacity;

            // Begin the animation with the DoubleAnimation instance
            GameBG.BeginAnimation(OpacityProperty, fadeOut, HandoffBehavior.Compose);
            await Task.Delay(750);
        }

        //background fade in animation
        private async Task StartAnimationBGFadeIn()
        {
            // Get the current opacity value of GameBG
            double currentOpacity = GameBG.Opacity;

            // Set the frame rate of the fadeOut animation to 60 frames per second
            Timeline.SetDesiredFrameRate(fadeOut, 60);

            // Set the From property of the fadeOut animation to the current opacity value
            fadeIn.From = currentOpacity;

            // Begin the animation with the DoubleAnimation instance
            GameBG.BeginAnimation(OpacityProperty, fadeIn, HandoffBehavior.Compose);
            await Task.Delay(750);
        }

        string lastAudio;
        //Update media player with current game BG music 
        private async Task playAudio(string audioPath)
        {
            try
            {
                if (thisWorking)
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
                        mediaPlayer.Volume = 0.75;
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

        Vector2 position = new Vector2(0, 0);

        async void KeyShortCuts_Tick(object sender, EventArgs e)
        {
            ControllerInput(UserIndex.One);
            ControllerInput(UserIndex.Two);
        }


        private static Controller controller;
        bool hasLaunched = false;
        private async void ControllerInput(UserIndex controllerNo)
        {
            try
            {
                controller = new Controller(controllerNo);

                bool isActive = Global.isMainActive;

                //If window is not focused stop music
                if (isActive != true)
                {
                    mediaPlayer.Pause();
                    wasNotFocused = true;

                    if (mainBody.Opacity != 1 && hasLaunched == true)
                    {
                        hasLaunched = false;

                        Animate.AnimateDockPanelOpacity(gameLaunch);
                        lbGames.Visibility = Visibility.Visible;
                        while (gameLaunch.Opacity != 0)
                        {
                            await Task.Delay(10);
                        }

                        gameLaunch.Visibility = Visibility.Collapsed;

                        Animate.AnimateBlur(GameBG);
                        Animate.AnimateDockPanelOpacity(mainBody);
                    }
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

                if (controllerNo == UserIndex.One)
                {
                    if (connected && bottomBar.Visibility == Visibility.Hidden) bottomBar.Visibility = Visibility.Visible;
                    if (!connected && bottomBar.Visibility == Visibility.Visible) bottomBar.Visibility = Visibility.Hidden;

                    if(this.Cursor != Cursors.None && connected) this.Cursor = Cursors.None;
                    if (this.Cursor == Cursors.None && !connected) this.Cursor = Cursors.Arrow;
                }

                if (connected && isActive)
                {

                    //get controller state
                    var state = controller.GetState();

                    //detect if keyboard or controller combo is being activated
                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight) && !Global.isAccessMenuOpen && !hasLaunched)
                    {
                        //Increase selected item by 1
                        int current = lbGames.SelectedIndex;

                        if (current < lbGames.Items.Count) current++;

                        lbGames.SelectedIndex = current;
                        lbGames.ScrollIntoView(lbGames.SelectedItem);
                        if (current == lbGames.Items.Count - 1) scrollViewer.ScrollToHorizontalOffset(scrollViewer.ScrollableWidth);
                    }

                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) && !Global.isAccessMenuOpen && !hasLaunched)
                    {
                        //Decrease selected item by 1
                        int current = lbGames.SelectedIndex;

                        if (current > 0) current--;

                        if (current == 0) scrollViewer.ScrollToHorizontalOffset(0);

                        lbGames.SelectedIndex = current;
                        lbGames.ScrollIntoView(lbGames.SelectedItem);
                        if (current == 0) scrollViewer.ScrollToHorizontalOffset(0);

                    }

                    //if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Y) && !Global.isAccessMenuOpen)
                    //{
                    //    if (mainBody.Opacity == 1)
                    //    {
                    //        hasLaunched = true;

                    //        Animate.AnimateBlur(GameBG);
                    //        Animate.AnimateDockPanelOpacity(mainBody);

                    //        while (mainBody.Opacity != 0)
                    //        {
                    //            await Task.Delay(10);
                    //        }
                    //        lbGames.Visibility = Visibility.Collapsed;
                    //        gameLaunch.Visibility = Visibility.Visible;

                    //        Animate.AnimateDockPanelOpacity(gameLaunch);
                    //    }
                    //}

                    SharpDX.XInput.Gamepad gamepad = controller.GetState().Gamepad;
                    float tx = gamepad.LeftThumbX;


                    if (tx < -18000 && !Global.isAccessMenuOpen && !Global.isAccessMenuOpen && !hasLaunched)
                    {
                        //Decrease selected item by 1
                        int current = lbGames.SelectedIndex;

                        if (current > 0) current--;

                        if (current == 0) scrollViewer.ScrollToHorizontalOffset(0);

                        lbGames.SelectedIndex = current;
                        lbGames.ScrollIntoView(lbGames.SelectedItem);
                        if (current == 0) scrollViewer.ScrollToHorizontalOffset(0);
                    }

                    
                    if (tx > 18000 && !Global.isAccessMenuOpen && !Global.isAccessMenuOpen && !hasLaunched)
                    {
                        //Increase selected item by 1
                        int current = lbGames.SelectedIndex;

                        if (current < lbGames.Items.Count) current++;

                        lbGames.SelectedIndex = current;
                        lbGames.ScrollIntoView(lbGames.SelectedItem);
                        if (current == lbGames.Items.Count - 1) scrollViewer.ScrollToHorizontalOffset(scrollViewer.ScrollableWidth);
                    }

                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A) && !Global.isAccessMenuOpen && !hasLaunched)
                    {
                        loadApp();
                    }

                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B) && !Global.isAccessMenuOpen && !hasLaunched)
                    {
                        Global.desktop = 1;
                    }
                    else if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B) && hasLaunched)
                    {
                        if (mainBody.Opacity != 1 && hasLaunched == true)
                        {
                            hasLaunched = false;

                            Animate.AnimateDockPanelOpacity(gameLaunch);
                            lbGames.Visibility = Visibility.Visible;
                            while (gameLaunch.Opacity != 0)
                            {
                                await Task.Delay(10);
                            }

                            gameLaunch.Visibility = Visibility.Collapsed;

                            Animate.AnimateBlur(GameBG);
                            Animate.AnimateDockPanelOpacity(mainBody);
                        }
                    }

                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.X) && !Global.isAccessMenuOpen && !hasLaunched)
                    {
                        Global.reload = 1;
                    }

                    bool combo = false;

                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder) && !Global.isAccessMenuOpen && !hasLaunched)
                    {
                        combo = true;
                        return;
                    }

                    int min = 0;
                    int max = 4;

                    int gameStore = Global.GameStore;
                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder) && !Global.isAccessMenuOpen && !hasLaunched && !combo)
                    {
                        gameStore--;

                        if (gameStore < min) gameStore = max;

                        Global.GameStore = gameStore;

                        checkKeyInput.Stop();
                    }

                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder) && !Global.isAccessMenuOpen && !hasLaunched && !combo)
                    {
                        gameStore++;

                        if (gameStore > max) gameStore = min;

                        Global.GameStore = gameStore;

                        checkKeyInput.Stop();
                    }
                }
            }
            catch { }
        }


        public async void loadApp()
        {
            if (mainBody.Opacity == 1)
            {
                hasLaunched = true;

                Animate.AnimateBlur(GameBG);
                Animate.AnimateDockPanelOpacity(mainBody);

                while (mainBody.Opacity != 0)
                {
                    await Task.Delay(10);
                }

                gameLaunch.Visibility = Visibility.Visible;

                Animate.AnimateDockPanelOpacity(gameLaunch);
            }

            LoadSteamGames.openSteamGame(lbGames);
        }

        private void lbGames_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!hasLaunched)
            {
                ListBox listBox = sender as ListBox;
                ScrollViewer scrollviewer = Global.FindVisualChildren<ScrollViewer>(listBox).FirstOrDefault();
                if (e.Delta > 0)
                    scrollviewer.LineLeft();
                else
                    scrollviewer.LineRight();
                e.Handled = true;

                if (scrollviewer != null)
                {
                    if (scrollviewer.HorizontalOffset <= 50)
                    {
                        lbGames.Margin = new Thickness(5, -15, 0, 0);
                    }
                    else if (scrollviewer.HorizontalOffset >= scrollviewer.ScrollableWidth - 125)
                    {
                        lbGames.Margin = new Thickness(0, -15, 10, 0);
                    }
                    else
                    {
                        lbGames.Margin = new Thickness(0, -15, 0, 0);
                    }
                }
            }
        }

        private void btnControl_Click(object sender, RoutedEventArgs e)
        {
            if (!hasLaunched)
            {
                loadApp();
            }
        }

        static string lastWifi;

        static string lastBattery;

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (!hasLaunched)
            {
                Global.settings = 1;
            }
        }

        private void lbGames_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!hasLaunched)
            {
                ScrollViewer scrollviewer = Global.FindVisualChildren<ScrollViewer>(lbGames).FirstOrDefault();
                if (scrollviewer != null)
                {
                    if (scrollviewer.HorizontalOffset <= 50)
                    {
                        lbGames.Margin = new Thickness(5, -15, 0, 0);
                    }
                    else if (scrollviewer.HorizontalOffset >= scrollviewer.ScrollableWidth - 125)
                    {
                        lbGames.Margin = new Thickness(0, -15, 10, 0);
                    }
                    else
                    {
                        lbGames.Margin = new Thickness(0, -15, 0, 0);
                    }
                }
            }
        }
    }

    public class DpiDecorator : Decorator
    {
        public DpiDecorator()
        {
            this.Loaded += (s, e) =>
            {
                System.Windows.Media.Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
                ScaleTransform dpiTransform = new ScaleTransform(1 / m.M11, 1 / m.M22);
                if (dpiTransform.CanFreeze)
                    dpiTransform.Freeze();
                this.LayoutTransform = dpiTransform;
            };
        }
    }
}
