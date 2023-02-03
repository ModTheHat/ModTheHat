using System;
using System.Threading;
using Controllers;
using ModTheHat;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LandingPageView : KothInputListener
{

    public extern void orig_OnShow();
    protected override void OnShow()
    {
        GameController.isInMenus = true;
        this.friendPassCtnr.SetActive(Release.Flag.TEAM_FRIEND_PASS);
        SteamAchievements.Award("ACH_OPEN_GAME");

        Loader.Init();
        Events.LandingPageView_onShow(); // Run onShow event.
    }

    protected override void OnInput(InputTick inputTick)
    {
        if (inputTick.IsAnyUIButtonDown)
        {
            this.Dispatcher.Run(UIActions.RegisterParticipant(inputTick.playerId));
            KothNavigator.Go(ScreenId.MainMenu, ScreenTransition.WhiteOverlay, null, true, 0, 0, default(CancellationToken));
        }
    }

    protected override bool IsDirty(UIState state, UIState lastState)
    {
        return false;
    }

    public GameObject friendPassCtnr;
}
