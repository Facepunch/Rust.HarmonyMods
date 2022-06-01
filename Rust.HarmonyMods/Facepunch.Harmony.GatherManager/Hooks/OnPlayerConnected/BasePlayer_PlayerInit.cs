using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Facepunch.Harmony.Weaver;
using Harmony;
using UnityEngine;

namespace Facepunch.Harmony.GatherManager.Hooks.OnLootSpawned
{
    [HarmonyPatch( typeof( BasePlayer ), "PlayerInit" )]
    internal class BasePlayer_PlayerInit
    {
        [HarmonyPostfix]
        public static void Postfix( BasePlayer __instance )
        {
            try
            {
                var args = Pool.Get<OnPlayerConnectedArgs>();
                args.Player = __instance;

                // In modloader this will call broadcast 
                GatherManagerMod.Instance.OnPlayerConnected( args );

                Pool.Free( ref args );
            }
            catch ( Exception ex )
            {
                Debug.LogException( ex );
            }
        }
    }
}
