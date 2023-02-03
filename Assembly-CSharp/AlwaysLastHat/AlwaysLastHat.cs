using ModTheHat;

namespace AlwaysLastHat
{
    public class AlwaysLastHat
    {

        public void Init()
        {
            // LandingPageView_onShow, but only once.

            ModTheHat.API.do_Something();

            On.LandingPageView.OnShow += (orig, self) =>
            {
                Dictionary<string, string> metadata = new Dictionary<string, string>();

                Notification n = new Notification(NotificationType.Alert, "Always last hat standing!", 3f, null, metadata, DateTime.UtcNow);

                StateMB.Singleton.Dispatcher.Run(UIActions.AddNotification(n));
            };

        }

    }
}