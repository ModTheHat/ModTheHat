using System.Reflection;
using MonoMod;

namespace ModTheHat
{
    public class Loader
    {

        public static bool loaded = false;
        public static bool is_modding = false;
        public static bool collider_view = false;

        public static void Init()
        {

            if (!loaded)
            {

                // ADD YOUR MODS HERE!

                /*Dictionary<string, string> metadata = new Dictionary<string, string>();

                Notification n = new Notification(NotificationType.Alert, "Ladies and gentleman, we did it", 3f, null, metadata, DateTime.UtcNow);

                StateMB.Singleton.Dispatcher.Run(UIActions.AddNotification(n));*/

                // Start loading the mods

                string modsPath = Path.Combine(Directory.GetCurrentDirectory(), "mods\\");


                /*if (File.Exists(modsPath)) 
                {
                    IEnumerable<String> allMods = Directory.EnumerateFiles(modsPath);

                    foreach (String mod in allMods)
                    {
                        Dictionary<string, string> metadata_mod = new Dictionary<string, string>();

                        Notification n_mod = new Notification(NotificationType.Alert, mod, 3f, null, metadata_mod, DateTime.UtcNow);

                        StateMB.Singleton.Dispatcher.Run(UIActions.AddNotification(n_mod));

                        Assembly modAssembly = Assembly.LoadFrom(mod);
                        Type[] types = modAssembly.GetTypes();
                        foreach (Type type in types)
                        {

                            if (type.GetInterface("IMod") != null)
                            {
                                IMod? mod_i = Activator.CreateInstance(type) as IMod;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
                                mod_i.Load();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                                break;
                            }
                        }
                    }
                } else
                {
                    System.IO.Directory.CreateDirectory(modsPath);

                }*/

                // Mods are loaded.

                loaded = true;

                
                
            }
        }

    }

}