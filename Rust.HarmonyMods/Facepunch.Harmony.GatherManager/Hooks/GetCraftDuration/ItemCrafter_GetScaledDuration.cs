using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Facepunch.Harmony.Weaver;
using Harmony;
using UnityEngine;

namespace Facepunch.Harmony.GatherManager.Hooks.OnLootSpawned
{
    [HarmonyPatch( typeof( ItemCrafter ), "GetScaledDuration" )]
    internal class ItemCrafter_GetScaledDuration
    {
        [HarmonyPostfix]
        public static void Postfix( ItemBlueprint __0, float __1, ref float __result )
        {
            try
            {
                var args = Pool.Get<GetCraftDurationArgs>();
                args.Blueprint = __0;

                // In modloader this will call broadcast 
                GatherManagerMod.Instance.GetCraftDuration( args );

                if ( args.CraftDurationScale != 1f )
                {
                    __result *= args.CraftDurationScale;
                }

                Debug.Log( $"New craft duration {__result}" );

                Pool.Free( ref args );
            }
            catch ( Exception ex )
            {
                Debug.LogException( ex );
            }
        }
    }
}
