﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Facepunch.Harmony.GatherManager
{
    // We will treat this as if it is a mod
    public class GatherManagerMod
    {
        public static readonly GatherManagerMod Instance = new GatherManagerMod();

        private float _globalScale = 1f;

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

        public bool OnCommand( CommandContext context )
        {
            if ( !context.IsAdmin() )
            {
                context.AddReply( "Not admin" );
                return false;
            }

            var split = context.RawCommand.Split( ' ' );

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
                    context.SetReply( $"{split[1]} is not a valid amount" );
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
