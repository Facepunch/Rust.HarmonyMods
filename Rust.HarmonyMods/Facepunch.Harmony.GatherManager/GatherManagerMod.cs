using Facepunch.Extend;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Facepunch.Harmony.GatherManager
{
    // We will treat this as if it is a mod
    public class GatherManagerMod
    {
        public static readonly GatherManagerMod Instance = new GatherManagerMod();

        private float _globalScale = 1f;
        private float _craftScale = 1f;
        private bool _unlockAllBps = false;

        private void Log(string message )
        {
            Debug.Log( message );
        }

        private float GetItemScale( string itemName )
        {
            return _globalScale;
        }

        private void SetGatherScale( float newAmount )
        {
            _globalScale = newAmount;

            OnLootScaleChanged();
        }

        private void SetCraftSpeed( float newAmount )
        {
            _craftScale = newAmount;
        }

        private void OnLootScaleChanged()
        {
            // Coroutine to iterate all loot containers and respawn loot
            // Wouldn't need this if we keep track of the first time a player opens a loot container & spawn loot then (is that more complicated?)
            new RepopulateLootTask().Start();
        }

        public void OnGatherItem( OnGatherItemArgs args )
        {
            float scale = GetItemScale( args.GivenItem.info.shortname );

            args.GivenItem.amount = Mathf.FloorToInt( args.GivenItem.amount * scale );
        }

        public void OnLootSpawned( OnLootSpawnedArgs args )
        {
            foreach( var inventory in args.Inventories )
            {
                foreach ( var item in inventory.itemList )
                {
                    float scale = GetItemScale( item.info.shortname );

                    item.amount = Mathf.FloorToInt( item.amount * scale );
                }
            }
        }

        public void OnPlayerConnected( OnPlayerConnectedArgs args )
        {
            if ( _unlockAllBps )
            {
                args.Player.blueprints.UnlockAll();
            }
        }

        public void GetCraftDuration( GetCraftDurationArgs args )
        {
            args.CraftDurationScale = _craftScale;
        }

        public bool OnCommand( CommandContext context )
        {
            if ( !context.IsAdmin() )
            {
                return false;
            }

            // If this gets added to modloader we will have a better command helper than string[]
            var split = context.RawCommand.SplitQuotesStrings();

            var command = split[ 0 ];

            if ( command == "gather.scale" )
            {
                if ( split.Length < 2 )
                {
                    context.AddReply( $"gather.scale: {_globalScale}" );
                    return true;
                }
                
                if ( float.TryParse( split[1], out var amount ) == false )
                {
                    context.AddReply( $"{split[1]} is not a valid amount" );
                    return true;
                }

                amount = Mathf.Clamp( amount, 1, 1000 );

                SetGatherScale( amount );
                context.AddReply( $"Gather.scale: {amount}" );

                return true;
            }
            else if ( command == "gather" )
            {
                OnGatherIngameCommand( context );
                return true;
            }
            else if ( command == "blueprints.grantall" )
            {
                if ( split.Length < 2 )
                {
                    context.AddReply( $"blueprints.grantall: {_unlockAllBps}" );
                    return true;
                }

                bool value = bool.Parse( split[ 1 ] );

                _unlockAllBps = value;
            }
            else if ( command == "craft.scale" )
            {
                if ( split.Length < 2 )
                {
                    context.AddReply( $"craft.scale: {_craftScale}" );
                    return true;
                }

                if ( float.TryParse( split[ 1 ], out var amount ) == false )
                {
                    context.AddReply( $"{split[ 1 ]} is not a valid amount" );
                    return true;
                }

                if ( amount > 1 )
                {
                    context.AddReply( "Value too high! Use decimals: '0.5' = 1/2 craft time" );
                    return true;
                }

                amount = Mathf.Clamp( amount, 0.01f, 1f );

                SetCraftSpeed( amount );
                context.AddReply( $"craft.scale: {amount}" );
            }
            else
            {
                // Unhandled command
                return false;
            }

            context.AddReply( "Ended up at end of gather command?" );
            return true;
        }

        private void OnGatherIngameCommand( CommandContext context )
        {
            context.AddReply( GetGatherDescription() );
        }

        public string GetGatherDescription()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine( $"Gather Rate: {System.Math.Round(_globalScale, 1 )}x" );

            return builder.ToString();
        }
    }
}
