using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Facepunch.Harmony.Weaver;
using Harmony;
using UnityEngine;

namespace Facepunch.Harmony.GatherManager.Hooks
{
    [HarmonyPatch( typeof( HumanNPC ), "CreateCorpse" )]
    internal class HumanNPC_CreateCorpse : BaseTranspileHook
    {
        //Copy paste into each transpile hook
        static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase originalMethod )
        {
            return DoTranspile( MethodBase.GetCurrentMethod().DeclaringType, instructions, generator, originalMethod );
        }

        public override bool WeaveHook( CodeInstruction instruction )
        {
            if ( SearchStoreLocal( SearchDirection.After, typeof( NPCPlayerCorpse ), out var corpseLocal ) == null )
            {
                Debug.LogError( $"Couldn't find local for {GetType().Name}" );
                return false;
            }

            MoveToEnd();

            LoadThis();

            LoadLocal( corpseLocal.LocalIndex );

            CallHookMethod( GetType() );

            return true;
        }

        public static bool Hook( HumanNPC entity, NPCPlayerCorpse corpse )
        {
            try
            {
                var args = Pool.Get<OnLootSpawnedArgs>();
                args.Entity = corpse;
                args.Inventories.AddRange( corpse.containers );

                // In modloader this will call broadcast
                GatherManagerMod.Instance.OnLootSpawned( args );

                Pool.Free( ref args );
            }
            catch ( Exception ex )
            {
                Debug.LogException( ex );
            }

            return true;
        }
    }
}
