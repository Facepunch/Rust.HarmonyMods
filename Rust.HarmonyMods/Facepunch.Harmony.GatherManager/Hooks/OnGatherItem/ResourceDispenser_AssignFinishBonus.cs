using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Facepunch.Harmony.Weaver;
using Harmony;
using UnityEngine;

namespace Facepunch.Harmony.GatherManager
{
    // Seperate hook for finish bonus because reasons?
    [HarmonyPatch( typeof( ResourceDispenser ), "AssignFinishBonus" )]
    internal class ResourceDispenser_AssignFinishBonus : BaseTranspileHook
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

            CallHookMethod( GetType() );

            return true;
        }

        public static bool Hook( BaseEntity entity, Item givenItem, ResourceDispenser dispenser )
        {
            try
            {
                var player = entity as BasePlayer;
                var resourceEntity = dispenser.GetComponent<BaseEntity>();

                var args = Pool.Get<OnGatherItemArgs>();
                args.Entity = resourceEntity;
                args.Player = player;
                args.GivenItem = givenItem;
                args.IsFinishingBonus = true;

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
