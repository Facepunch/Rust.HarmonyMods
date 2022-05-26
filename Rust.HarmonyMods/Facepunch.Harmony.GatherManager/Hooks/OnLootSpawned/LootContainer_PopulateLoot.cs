using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Facepunch.Harmony.Weaver;
using Harmony;
using UnityEngine;

namespace Facepunch.Harmony.GatherManager.Hooks.OnLootSpawned
{
    [HarmonyPatch( typeof( LootContainer ), "PopulateLoot" )]
    internal class LootContainer_PopulateLoot
    {
        [HarmonyPostfix]
        public static void Postfix( LootContainer __instance )
        {
            try
            {
                var args = Pool.Get<OnLootSpawnedArgs>();
                args.Entity = __instance;

                // In modloader this will call broadcast 
                GatherManagerMod.Instance.OnLootSpawned( args );

                Pool.Free( ref args );
            }
            catch ( Exception ex )
            {
                Debug.LogException( ex );
            }
        }
    }
}
