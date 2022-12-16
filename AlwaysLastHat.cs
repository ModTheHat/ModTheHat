using static GameController;
using static UIState;
using System;

public class AlwaysLastHat : Mod {

    public String name = "AlwaysLastHat"; 

    public static void Init() {
        // Do nothing because I am lazy.

        GameController.instance.SetMode(GameModeBlueprints.LAST_HAT_STANDING, new UIState());

    }
}