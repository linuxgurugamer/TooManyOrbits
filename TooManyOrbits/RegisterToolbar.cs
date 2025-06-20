﻿using KSP.Localization;
using KSP.UI.Screens;
using KSP_Log;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using ToolbarControl_NS;
using UnityEngine;

namespace TooManyOrbits.UI
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        static public Log Log;
        void Start()
        {
            ToolbarControl.RegisterMod(TooManyOrbitsCoreModule.MODID, TooManyOrbitsCoreModule.MODNAME);

            Log = new KSP_Log.Log(Localizer.Format("TooManyOrbits")
#if DEBUG
                , KSP_Log.Log.LEVEL.INFO
#endif
                );
        }
    }
}