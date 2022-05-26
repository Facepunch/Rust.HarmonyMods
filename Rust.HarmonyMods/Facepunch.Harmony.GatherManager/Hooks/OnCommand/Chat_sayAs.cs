using System;
using System.Reflection;
using System.Reflection.Emit;
using Facepunch;
using Facepunch.Harmony.GatherManager;
using Facepunch.Harmony.Weaver;
using Harmony;
using UnityEngine;

namespace Facepunch.Harmony.GatherManager
{
    [HarmonyPatch( typeof( ConVar.Chat ), "sayAs" )]
    internal class Chat_sayAs
    {
        [HarmonyPrefix]
        static bool Prefix( ConVar.Chat.ChatChannel targetChannel, ulong userId, string username, string message, BasePlayer player = null )
        {
            try
            {
                return Hook( player, message );
            }
            catch (Exception ex )
            {
                Debug.LogException( ex );
                return true;
            }
        }

        public static bool Hook( BasePlayer player, string message )
        {
            if ( message.StartsWith( "/" ) || message.StartsWith( "\\" ) )
            {
                message = message.TrimStart( '/', '\\' );

                var args = Pool.Get<CommandContext>();
                args.PlayerConnection = player.Connection;
                args.PlayerModel = player;
                args.RawCommand = message;

                // In modloader this will call broadcast
                GatherManagerMod.Instance.OnCommand( args );

                foreach( var reply in args.Replies )
                {
                    player.ChatMessage( reply );
                }

                return false;
            }

            return true;
        }
    }
}
