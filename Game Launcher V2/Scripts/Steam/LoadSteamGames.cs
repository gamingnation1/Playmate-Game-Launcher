using AATUV3.Scripts;
using GameLib.Core;
using Microsoft.Diagnostics.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using static Game_Launcher_V2.Pages.Home;
using Path = System.IO.Path;

namespace Game_Launcher_V2.Scripts
{
    class LoadSteamGames
    {
        public static void loadSteamGames(ListBox lbGames)
        {
            lbGames.Items.Clear();

            List<SteamGame> games = new List<SteamGame>();

            int i = 0;

            games.Add(new SteamGame()
            {
                ID = 0,
                gameName = "Open Steam",
                steamID = "0",
                imagePath = path + $"\\GameAssets\\Steam\\icon.png",
                bgImagePath = path + $"\\GameAssets\\Steam\\background.jpg",
                musicPath = path + $"\\GameAssets\\Default\\audio.mp3",
                message = ""
            });

            var lines = File.ReadAllLines(path + "\\SavedList.txt");
            lines = lines.Distinct().ToArray();
            Array.Sort(lines);
            string path2 = AppDomain.CurrentDomain.BaseDirectory;
            path2 = path2 + "\\GameAssets\\";
            do
            {
                string[] gameList = lines[i].Split('~');
                string icon = "";
                string background = "";
                string message = "";
                string music = "";
                string gameName = gameList[0];

                var charsToRemove = new string[] { "@", ",", ".", ";", "'", "/", "\\", "|", "?", "*", ">", "<", ":" };
                foreach (var c in charsToRemove)
                {
                    gameName = gameName.Replace(c, string.Empty);
                }

                if (!Directory.Exists(path2 + gameName))
                {
                    Directory.CreateDirectory(path2 + gameName);
                }

                if (File.Exists(path + $"\\GameAssets\\{gameName}\\icon.jpg"))
                {
                    icon = path + $"\\GameAssets\\{gameName}\\icon.jpg";
                }
                else if (File.Exists(path + $"\\GameAssets\\{gameName}\\icon.jpeg"))
                {
                    icon = path + $"\\GameAssets\\{gameName}\\icon.jpeg";
                }
                else if (File.Exists(path + $"\\GameAssets\\{gameName}\\icon.png"))
                {
                    icon = path + $"\\GameAssets\\{gameName}\\icon.png";
                }
                else
                {
                    icon = path + $"\\GameAssets\\Default\\icon.png";
                }

                if (File.Exists(path + $"\\GameAssets\\{gameName}\\background.jpg"))
                {
                    background = path + $"\\GameAssets\\{gameName}\\background.jpg";
                }
                else if (File.Exists(path + $"\\GameAssets\\{gameName}\\background.jpeg"))
                {
                    background = path + $"\\GameAssets\\{gameName}\\background.jpeg";
                }
                else if (File.Exists(path + $"\\GameAssets\\{gameName}\\background.png"))
                {
                    background = path + $"\\GameAssets\\{gameName}\\background.png";
                }
                else
                {
                    background = path + $"\\GameAssets\\Default\\background.jpg";
                }

                if (File.Exists(path + $"\\GameAssets\\{gameName}\\audio.m4a"))
                {
                    music = path + $"\\GameAssets\\{gameName}\\audio.m4a";
                }
                else if (File.Exists(path + $"\\GameAssets\\{gameName}\\audio.mp3"))
                {
                    music = path + $"\\GameAssets\\{gameName}\\audio.mp3";
                }
                else
                {
                    music = path + $"\\GameAssets\\Default\\audio.mp3";
                }

                if (icon == "" || icon == path + $"\\GameAssets\\Default\\icon.png" || icon == null) message = gameList[0];

                games.Add(new SteamGame()
                {
                    ID = i,
                    gameName = gameList[0],
                    steamID = gameList[1],
                    imagePath = icon,
                    bgImagePath = background,
                    musicPath = music,
                    message = message
                });

                i++;
            } while (i < lines.Count());

            //games.Add(new SteamGame()
            //{
            //    ID = 0,
            //    gameName = "All Software",
            //    steamID = 0,
            //    imagePath = path + $"\\GameAssets\\Library\\icon.png",
            //    bgImagePath = path + $"\\GameAssets\\Library\\background.jpg",
            //    musicPath = "N/A",
            //    message = ""
            //});

            lbGames.ItemsSource = games;

            lbGames.SelectedIndex = 0;

        }

        public static void openSteamGame(ListBox lbGames)
        {
            SteamGame model = lbGames.SelectedItem as SteamGame;
            string steamID = model.steamID;
            string gameName = model.gameName;

            if(steamID == "0")
            {
                if (Global.GameStore == 1)
                {
                    steamID = LoadGames.getLauncherPath("Steam");
                }
                if (Global.GameStore == 2)
                {
                    steamID = LoadGames.getLauncherPath("Epic Games");
                }
                //if (Global.GameStore == 2)
                //{
                //    steamID = LoadGames.getLauncherPath("Battle.net");
                //}
                if (Global.GameStore == 4)
                {
                    steamID = LoadGames.getLauncherPath("GOG Galaxy");
                }
                if (Global.GameStore == 5)
                {
                    steamID = LoadGames.getLauncherPath("Origin");
                }
                if (Global.GameStore == 6)
                {
                    steamID = LoadGames.getLauncherPath("Ubisoft Connect");
                }
                //if (Global.GameStore == 6)
                //{
                //    steamID = LoadGames.getLauncherPath("Rockstar Games");
                //}
            }

            if(steamID != null && steamID != "")
            {

                if (steamID.Contains(".exe"))
                {
                    MessageBox.Show(steamID);
                    Process.Start("cmd.exe", "/C " + steamID);
                }
                else
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        UseShellExecute = true,
                        FileName = steamID
                    });
                }
            }
        }

        public static void changeSteamGame(ListBox lbGames, TextBlock lblGameName, Label lblControl) 
        {
            SteamGame model = lbGames.SelectedItem as SteamGame;
            if (model.gameName.Contains("Open Steam")) lblGameName.Text = model.gameName.Replace("Open ", "");
            else if (model.gameName.Contains("Open Epic Games")) lblGameName.Text = "Open Epic Games Store";
            else lblGameName.Text = model.gameName;


            if (model.gameName.Contains("Open Epic Games") || model.gameName.Contains("Open Steam") || model.gameName.Contains("All Software") || model.gameName.Contains("No Games Found")) lblControl.Content = model.gameName;
            else lblControl.Content = "Play Game";
        }
    }
}
