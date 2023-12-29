using BepInEx;
using BepInEx.Logging;
using CoDOnCollect.Patches;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CoDOnCollect
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class MinishCODModBase : BaseUnityPlugin
    {
        private const string modGUID = "Minish.CauseOfDeathOnCollect";
        private const string modName = "Cause Of Death On Collect";
        private const string modVersion = "1.0.0.1";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static MinishCODModBase Instance;

        internal ManualLogSource mls;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            Logger.LogInfo($"{modGUID} has loaded!");
            //mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            //mls.LogInfo($"{modGUID} has loaded!");

            //harmony.PatchAll(typeof(MinishCODModBase));
            harmony.PatchAll(typeof(HUDManagerPatch));
        }
    }
}
