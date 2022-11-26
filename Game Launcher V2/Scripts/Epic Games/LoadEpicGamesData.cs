using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static Game_Launcher_V2.Pages.Home;

namespace Game_Launcher_V2.Scripts.Epic_Games
{
    class LoadEpicGamesData
    {
        public static void loadEpicGames(ListBox lbGames)
        {
            try
            {
                List<SteamGame> games = new List<SteamGame>();

                int i = 0;

                games.Add(new SteamGame()
                {
                    ID = 0,
                    gameName = "Open EGS",
                    steamID = "0",
                    imagePath = path + $"\\GameAssets\\EGS\\icon.png",
                    bgImagePath = path + $"\\GameAssets\\EGS\\background.jpg",
                    musicPath = path + $"\\GameAssets\\Default\\audio.mp3",
                    message = ""
                });

                var lines = File.ReadAllLines(path + "\\SavedListEpic.txt");
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
                        steamID = gameList[3],
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
            catch(Exception ex) { MessageBox.Show(ex.ToString()); }       
        }
    }
}
