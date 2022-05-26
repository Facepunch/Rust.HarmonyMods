using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Facepunch;
using Facepunch.Harmony.GatherManager;
using Facepunch.Harmony.Weaver;
using Harmony;
using UnityEngine;

namespace Facepunch.Harmony.GatherManager
{

    [HarmonyPatch( typeof( ResourceDispenser ), "GiveResourceFromItem" )]
    internal class ResourceDispenser_GiveResourceFromItem : BaseTranspileHook
    {
        //Copy paste into each transpile hook
        static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase originalMethod )
        {
            return DoTranspile( MethodBase.GetCurrentMethod().DeclaringType, instructions, generator, originalMethod );
        }

        public override bool WeaveHook( CodeInstruction instruction )
        {
            if ( instruction.CheckMethod( "GiveItem", typeof( BaseEntity ) ) == false )
            {
                return false;
            }

            MoveBeforeMethod();

            if ( SearchStoreLocal( SearchDirection.Before, typeof( Item ), out var itemLocal ) == null )
            {
                Debug.LogError( "Couldn't find Item local for OnGatherItem" );
                return false;
            }

            // Player
            LoadParameter( 0 );

            // Item
            LoadLocal( itemLocal.LocalIndex );

            // ResourceDispenser
            LoadThis();

            // ItemAmount
            LoadParameter( 1 );

            CallHookMethod( GetType() );

            return true;
        }

        public static bool Hook( BaseEntity entity, Item givenItem, ResourceDispenser dispenser, ItemAmount amount )
        {
            try
            {
                var player = entity as BasePlayer;
                var resourceEntity = dispenser.GetComponent<BaseEntity>();

                var args = Pool.Get<OnGatherItemArgs>();
                args.Entity = resourceEntity;
                args.Player = player;
                args.GivenItem = givenItem;

                // In modloader this will call broadcast
                GatherManagerMod.Instance.OnGatherItem( args );

                bool result = !args.Cancel;

                Pool.Free( ref args );

                return result;
            }
            catch ( Exception ex )
            {
                Debug.LogException( ex );
                return true;
            }
        }
    }
}
