

namespace ModTheHat
{
    public class Loader
    {

        public static bool loaded = false;

        public static void Init()
        {

            if (!loaded)
            {

                // ADD YOUR MODS HERE!

                Dictionary<string, string> metadata = new Dictionary<string, string>();

                Notification n = new Notification(NotificationType.Alert, "Ladies and gentleman, we did it", 3f, null, metadata, DateTime.UtcNow);

                StateMB.Singleton.Dispatcher.Run(UIActions.AddNotification(n));

                // Start loading the mods
    
                

                // Mods are loaded.

                loaded = true;

            }
        }

    }
}