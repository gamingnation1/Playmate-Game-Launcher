using Game_Launcher_V2.Scripts;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Launcher_V2.Services
{
    internal class ToastNotifications
    {
        public static void PromptXgMobileActivate()
        {
            new ToastContentBuilder()
                  .AddArgument("XgMobileOpen")
                  .AddText("XG Mobile detected!")
                  .AddAppLogoOverride(new Uri("file:///" + Global.path + "\\Assets\\applicationIcon.png"))
                  .AddInlineImage(new Uri("file:///" + Global.path + "\\Images\\XGMobile\\XGMobile-1.png"))
                  .AddButton(new ToastButton().SetContent("Activate")
                  .AddArgument("XgMobileActivate"))
                  .Show(toast => {
                      toast.Tag = "AcXgMobile";
                  });

        }

        public static bool IsActivateXgMobileToastButtonClicked(ToastNotificationActivatedEventArgsCompat toastArgs)
        {
            return toastArgs.Argument.Split(";", StringSplitOptions.RemoveEmptyEntries).Any((value) => "XgMobileActivate".Equals(value));
        }

        public static bool IsOpenXgMobileToastClicked(ToastNotificationActivatedEventArgsCompat toastArgs)
        {
            return toastArgs.Argument.Split(";", StringSplitOptions.RemoveEmptyEntries).Any((value) => "XgMobileOpen".Equals(value));
        }

        public static void HideXgMobileActivateToasts()
        {
            ToastNotificationManagerCompat.History.Remove("AcXgMobile");
        }
    }
}
