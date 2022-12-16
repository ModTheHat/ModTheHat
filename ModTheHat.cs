using System;
using System.Collections.Generic;
using VioletUI;

namespace ModTheHat
{
    
    public class ModTheHat
    {
        public static void Init() {
            
            // ADD YOUR MODS HERE!

            Dictionary<string, string> metadata = new Dictionary<string, string>();

            Notification n = new Notification(NotificationType.Alert, "Ladies and gentleman, we did it" , 3f, null, metadata, DateTime.UtcNow);

            Mods.AlwaysLastHat.Init(); // Always Last Hat Mod

            StateMB.Singleton.Dispatcher.Run(UIActions.AddNotification(n));
        }
    }

}