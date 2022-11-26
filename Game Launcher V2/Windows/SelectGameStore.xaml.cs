using Game_Launcher_V2.Pages;
using Game_Launcher_V2.Scripts;
using SharpDX.XInput;
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
using System.Windows.Threading;
using static Game_Launcher_V2.Pages.Home;

namespace Game_Launcher_V2.Windows
{
    /// <summary>
    /// Interaction logic for SelectGameStore.xaml
    /// </summary>
    public partial class SelectGameStore : Window
    {
        class Game
        {
            public string gameStoreName { get; set; }
            public string imagePath { get; set; }
        }

        public SelectGameStore()
        {
            InitializeComponent();

            List<Game> gameStores = new List<Game>();

            gameStores.Add(new Game()
            {
                gameStoreName = "Steam",
                imagePath = "https://upload.wikimedia.org/wikipedia/commons/thumb/8/83/Steam_icon_logo.svg/512px-Steam_icon_logo.svg.png",
            });

            gameStores.Add(new Game()
            {
                gameStoreName = "Epic Games Store",
                imagePath = "https://cdn2.unrealengine.com/Unreal+Engine%2Feg-logo-filled-1255x1272-0eb9d144a0f981d1cbaaa1eb957de7a3207b31bb.png",
            });

            lbGameStores.ItemsSource = gameStores;

            //set up timer for key combo system
            DispatcherTimer checkKeyInput = new DispatcherTimer();
            checkKeyInput.Interval = TimeSpan.FromSeconds(0.117);
            checkKeyInput.Tick += KeyShortCuts_Tick;
            checkKeyInput.Start();
        }

        private void lbGameStores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Game model = lbGameStores.SelectedItem as Game;

            if (model.gameStoreName == "Steam")
            {
                Global.GameStore = 0;
            }
            if (model.gameStoreName == "Epic Games Store")
            {
                Global.GameStore = 1;
            }
        }

        private static Controller controller;

        public static bool hidden = true;
        async void KeyShortCuts_Tick(object sender, EventArgs e)
        {
            try
            {
                //Get controller
                controller = new Controller(UserIndex.One);

                bool connected = controller.IsConnected;
                if (MainDock.Visibility == Visibility.Hidden) MainDock.Visibility = Visibility.Visible;

                if (connected && Global.isMainActive || connected && hidden == false)
                {
                    //get controller state
                    var state = controller.GetState();

                    if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Start))
                    {

                        //if hidden show window
                        if (hidden == false)
                        {
                            hidden = true;
                            Global.menuSelectWasOpen = true;
                            Global.menuselectOpen = false;
                            this.Hide();
                        }
                        //else hide window
                        else
                        {
                            hidden = false;
                            Global.menuselectOpen = true;
                            this.Show();
                            this.Activate();
                        }
                    }

                    //if (state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown) && state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder))
                    //{
                    //    SendKeys.SendWait("%F");
                    //}

                    Global.shortCut = false;
                }
            }
            catch { }
        }
    }
}
