using Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModTheHat
{
    public class Menu : KothInputListener
    {
        protected override void OnInput(InputTick inputTick)
        {
            if (!inputTick.HasMovement && !inputTick.HasInput || this.State.screenId == ScreenId.Game)
                return;
            // no ingame

            if (Input.GetKeyDown(KeyCode.Period)) {
                
                Dictionary<string, string> metadata = new Dictionary<string, string>();

                Notification n = new Notification(NotificationType.Alert, "", 3f, null, metadata, DateTime.UtcNow);

                StateMB.Singleton.Dispatcher.Run(UIActions.AddNotification(n));
            }
        }
    }
}
