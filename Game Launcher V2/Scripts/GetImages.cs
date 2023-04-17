using Newtonsoft.Json.Linq;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using craftersmine.SteamGridDBNet;
using F23.StringSimilarity;
using System.Drawing;
using System.IO;
using System.Security.Policy;
using System.Windows.Shapes;

namespace Game_Launcher_V2.Scripts
{
    class GetImages
    {
        public static async Task<string> GetGridImageUrl(string gameName)
        {
            var client = new SteamGridDb("2b1dd6701a770112a9bef38ee1652f3d");
            SteamGridDbGame[]? games = await client.SearchForGamesAsync(gameName);

            // Instantiate the string distance algorithm
            var levenshtein = new Levenshtein();

            // Loop through the search results
            foreach (var result in games)
            {
                // Calculate the Levenshtein distance between the searched game name and the found game name
                double distance = levenshtein.Distance(gameName.ToLower(), result.Name.ToLower());

                // If the distance is less than or equal to 3, consider the names similar
                if (distance <= 3)
                {
                    // Get the grids for the game
                    SteamGridDbGrid[]? grids = await client.GetGridsByGameIdAsync(result.Id);

                    int i = 0;

                    // Loop through the grids and return the first 1024x1024 grid found
                    foreach (var grid in grids)
                    {
                        if (grid.Width == 1024 && grid.Height == 1024)
                        {
                            string filePath =  Global.path + $"\\GameAssets\\{gameName}\\icon.jpeg";
                            await DownloadImage(grid.FullImageUrl, filePath);
                            i++;
                            return filePath;
                        }  
                    }

                    if(i == 0)
                    {
                        foreach (var grid in grids)
                        {
                            if (grid.Width == 512 && grid.Height == 512)
                            {
                                string filePath = Global.path + $"\\GameAssets\\{gameName}\\icon.jpeg";
                                await DownloadImage(grid.FullImageUrl, filePath);
                                return filePath;
                            }
                        }
                    }
                }
            }

            return Global.path + $"\\GameAssets\\Default\\icon.png";
        }

        public static async Task<string> GetHeroImageUrl(string gameName)
        {
            var client = new SteamGridDb("2b1dd6701a770112a9bef38ee1652f3d");
            SteamGridDbGame[]? games = await client.SearchForGamesAsync(gameName);

            // Instantiate the string distance algorithm
            var levenshtein = new Levenshtein();

            // Loop through the search results
            foreach (var result in games)
            {
                // Calculate the Levenshtein distance between the searched game name and the found game name
                double distance = levenshtein.Distance(gameName.ToLower(), result.Name.ToLower());

                // If the distance is less than or equal to 3, consider the names similar
                if (distance <= 3)
                {
                    // Get the heros for the game
                    SteamGridDbHero[]? heros = await client.GetHeroesByGameIdAsync(result.Id);
                    int i = 0;
                    // Loop through the heros and return the first heros found
                    foreach (var hero in heros)
                    {
                        if (hero.Width >= 1500)
                        {
                            string filePath = Global.path + $"\\GameAssets\\{gameName}\\background.jpeg";
                            await DownloadImage(hero.FullImageUrl, filePath);
                            i++;
                            return filePath;
                        }
                    }

                    if(i == 0)
                    {
                        foreach (var hero in heros)
                        {
                            if (hero.Width >= 1920)
                            {
                                string filePath = Global.path + $"\\GameAssets\\{gameName}\\background.jpeg";
                                await DownloadImage(hero.FullImageUrl, filePath);
                                i++;
                                return filePath;
                            }
                        }
                    }
                }
            }

            return Global.path + $"\\GameAssets\\Default\\background.png";
        }

        public static async Task DownloadImage(string url, string filePath)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }
            }
        }
    }
}
