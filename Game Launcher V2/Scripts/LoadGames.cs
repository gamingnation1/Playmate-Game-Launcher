using GameLib.Plugin.Steam;
using GameLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static Game_Launcher_V2.Pages.Home;
using GameLib.Core;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
using System.Drawing;
using System.Net.NetworkInformation;

namespace Game_Launcher_V2.Scripts
{
    internal class LoadGames
    {
        private static LauncherManager launcherManager = new LauncherManager(new LauncherOptions() { QueryOnlineData = true });

        public static List<string> gameURLs = new List<string>(), gamePaths = new List<string>();
        public static async void LoadAllGameData(ListBox lbGames)
        {
            List<SteamGame> games = new List<SteamGame>();

            int i = 0;

            launcherManager.Refresh();

            IEnumerable<IGame> allGames = Enumerable.Empty<IGame>();

            allGames = launcherManager.GetAllGames().OrderBy(g => g.Name);

            foreach (var game in allGames)
            {
                string LauncherName = launcherManager.GetLaunchers().First(l => l.Id == game.LauncherId).Name;
                if (LauncherName != "Battle.net" && LauncherName != "Origin" && LauncherName != "Rockstar Games")
                {
                    if (!game.Name.Contains("Steamworks") && !game.Name.Contains("SteamVR") && !game.Name.Contains("Google Earth") && !game.Name.Contains("Wallpaper Engine") && !game.Name.Contains("tModLoader") && game.Name != "3DMark")
                    {

                        string path2 = AppDomain.CurrentDomain.BaseDirectory;
                        path2 = path2 + "\\GameAssets\\";

                        string icon = "";
                        string background = "";
                        string message = "";
                        string music = "";
                        string gameName = game.Name;

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
                            if (IsInternetAvailable())
                            {
                                icon = await GetImages.GetGridImageUrl(gameName);
                            }
                            else icon = path + $"\\GameAssets\\Default\\icon.png";
                        }

                        if (File.Exists(path + $"\\GameAssets\\{gameName}\\background.mp4"))
                        {
                            background = path + $"\\GameAssets\\{gameName}\\background.mp4";
                        }
                        else if (File.Exists(path + $"\\GameAssets\\{gameName}\\background.jpg"))
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
                            if (IsInternetAvailable())
                            {
                                 background = await GetImages.GetHeroImageUrl(gameName);
                            }
                            else if (File.Exists(path + $"\\GameAssets\\Default\\background.mp4"))
                            {
                                background = path + $"\\GameAssets\\Default\\background.mp4";
                            }
                            else icon = path + $"\\GameAssets\\Default\\background.png";
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

                        if (icon == "" || icon == path + $"\\GameAssets\\Default\\icon.png" || icon == null) message = gameName;

                        games.Add(new SteamGame()
                        {
                            ID = i,
                            gameName = gameName,
                            steamID = game.LaunchString,
                            imagePath = icon,
                            bgImagePath = background,
                            musicPath = music,
                            message = message
                        });

                        i++;
                    }
                }
            }

            if(i < 1)
            {
                games.Add(new SteamGame()
                {
                    ID = 0,
                    gameName = "No Games Found",
                    steamID = "0",
                    imagePath = path + $"\\GameAssets\\Default\\icon.png",
                    bgImagePath = path + $"\\GameAssets\\Default\\background.jpg",
                musicPath = path + $"\\GameAssets\\Default\\audio.mp3",
                    message = "No Games Found"
                });
            }

            lbGames.ItemsSource = games;
            lbGames.SelectedIndex = 0;
        }

        public async static void LoadGameData(string selectLauncher, ListBox lbGames)
        {
            List<SteamGame> games = new List<SteamGame>();

            int i = 0;

            games.Add(new SteamGame()
            {
                ID = 0,
                gameName = "Open " + selectLauncher,
                steamID = "0",
                imagePath = path + $"\\GameAssets\\{selectLauncher}\\icon.png",
                bgImagePath = path + $"\\GameAssets\\Default\\background.mp4",
                musicPath = path + $"\\GameAssets\\Default\\audio.mp3",
                message = ""
            });
            launcherManager.Refresh();
            foreach (var launcher in launcherManager.GetLaunchers())
            {
                if (launcher.Name == selectLauncher)
                {
                    foreach (var game in launcher.Games.OrderBy(g => g.Name))
                    {
                        if (!game.Name.Contains("Steamworks") && game.Name != "3DMark")
                        {

                            string path2 = AppDomain.CurrentDomain.BaseDirectory;
                            path2 = path2 + "\\GameAssets\\";

                            string icon = "";
                            string background = "";
                            string message = "";
                            string music = "";
                            string gameName = game.Name;

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

                            if (File.Exists(path + $"\\GameAssets\\{gameName}\\background.mp4"))
                            {
                                background = path + $"\\GameAssets\\{gameName}\\background.mp4";
                            }
                            else if (File.Exists(path + $"\\GameAssets\\{gameName}\\background.jpg"))
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
                                if (IsInternetAvailable())
                                {
                                    background = await GetImages.GetHeroImageUrl(gameName);
                                }
                                else if (File.Exists(path + $"\\GameAssets\\Default\\background.mp4"))
                                {
                                    background = path + $"\\GameAssets\\Default\\background.mp4";
                                }
                                else icon = path + $"\\GameAssets\\Default\\background.png";
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

                            if (icon == "" || icon == path + $"\\GameAssets\\Default\\icon.png" || icon == null) message = gameName;

                            games.Add(new SteamGame()
                            {
                                ID = i,
                                gameName = gameName,
                                steamID = game.LaunchString,
                                imagePath = icon,
                                bgImagePath = background,
                                musicPath = music,
                                message = message
                            });

                            i++;
                        }
                    }
                }
            }

            lbGames.ItemsSource = games;
            lbGames.SelectedIndex = 0;
        }


        public static string getLauncherPath(string selectLauncher)
        {
            string path = "";

            launcherManager.Refresh();
            foreach (var launcher in launcherManager.GetLaunchers())
            {
                if (launcher.Name == selectLauncher)
                {
                    return launcher.Executable;
                }
            }
            return path;
        }

        public static bool IsInternetAvailable()
        {
            try
            {
                using (var ping = new Ping())
                {
                    var result = ping.Send("8.8.8.8", 2000); // ping Google DNS server
                    return result.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
