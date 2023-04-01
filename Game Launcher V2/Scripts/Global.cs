﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using Game_Launcher_V2.Windows;

namespace Game_Launcher_V2.Scripts
{
    class Global
    {

        //Steam = 0
        //EGS = 1
        //EA = 2
        //Battle.net = 3
        public static int GameStore = 0;
        public static bool isMainActive = true;
        public static bool isAccessMenuActive = false;
        public static bool isOpen = false;
        public static bool isAccessMenuOpen = false;
        public static bool shortCut = false;

        public static bool menuSelectWasOpen = false;
        public static bool menuselectOpen = false;

        public static double wifi = 0;

        public static int AccessMenuSelected = 0;

        public static string RyzenAdj = "";

        public static string mbo = "";

        public static int desktop = 0;

        public static int settings = 0;

        //Get current working directory
        public static string path;
        public static Visual GetDescendantByType(Visual element, Type type)
        {
            if (element == null)
            {
                return null;
            }
            if (element.GetType() == type)
            {
                return element;
            }
            Visual foundElement = null;
            if (element is FrameworkElement)
            {
                (element as FrameworkElement).ApplyTemplate();
            }
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType(visual, type);
                if (foundElement != null)
                {
                    break;
                }
            }
            return foundElement;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
