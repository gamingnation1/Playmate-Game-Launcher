﻿using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace Game_Launcher_V2.Scripts.Epic_Games
{
    class FindEpicGamesData
    {
        private static string pathToEpicGamesData = "C:\\ProgramData\\Epic\\EpicGamesLauncher\\Data\\Manifests\\";
        public static void GetData()
        {
            if (Directory.Exists(pathToEpicGamesData))
            {
                var gameDataFiles = Directory.GetFiles(pathToEpicGamesData, "*.item");
                SaveData(gameDataFiles);
            }
        }

        public static void SaveData(string[] gameDataFiles)
        {
            int i = 0;
            string[] games = new string[gameDataFiles.Count()];

            foreach (string file in gameDataFiles)
            {
                var lines = File.ReadAllLines(gameDataFiles[i]);
                string gameName = "";
                string gameEXE = "";
                string gamePath = "";
                string ID = "";

                foreach(string line in lines)
                {
                    if (line.Contains("\"LaunchExecutable\":"))
                    {
                        gameEXE = line.Replace("\",", "");
                    }

                    if (line.Contains("\"DisplayName\":"))
                    {
                        gameName = line.Replace("\",", "");
                    }

                    if (line.Contains("\"InstallLocation\":"))
                    {
                        gamePath = line.Replace("\",", "");
                    }

                    if (line.Contains("\"MainGameAppName\":"))
                    {
                        ID = line.Replace("\",", "");
                    }
                }

                gameEXE.Replace("/", "\\");

                gameEXE = gameEXE.Remove(0, 22);
                gamePath = gamePath.Remove(0, 21);
                gameName = gameName.Remove(0, 17);
                ID = ID.Remove(0, 21);

                games[i] = $"{gameName}~{gamePath}~{gameEXE}~{ID}";
                i++;
            }

            File.WriteAllLines(Global.path + "\\SavedListEpic.txt", games);
        }
    }
}
