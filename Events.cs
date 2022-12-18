using System;
using System.Collections.Generic;
using VioletUI;
using static ModTheHat.ModTheHat;

namespace ModTheHat
{

    class Events
    {
        
        public static void LandingPageView() {
            
            ModTheHat.Init(); // Although we call it here, functions here get triggered only ONCE.

        }

    }

}