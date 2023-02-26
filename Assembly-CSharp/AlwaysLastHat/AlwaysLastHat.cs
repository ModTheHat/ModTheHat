using ModTheHat;

namespace AlwaysLastHat
{
    public class AlwaysLastHat
    {

        public void Load()
        {
            // LandingPageView_onShow, but only once.

            //ModTheHat.API.do_Something();


            On.VersionView.Render += (orig, self, state) =>
            {
                Dictionary<string, string> metadata = new Dictionary<string, string>();

                Notification n = new Notification(NotificationType.Alert, "Always last hat standing!", 3f, null, metadata, DateTime.UtcNow);

                StateMB.Singleton.Dispatcher.Run(UIActions.AddNotification(n));
            };

        }

    }
}