using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Facepunch.Harmony.GatherManager
{
    [HarmonyPatch( typeof( ConsoleSystem ), "Run" )]
    internal class ConsoleSystem_Run
    {
        [HarmonyPrefix]
        static bool Prefix( ConsoleSystem.Option __0, string __1, object[] __2, ref string __result )
        {
            try
            {
                // Commands from clients & server console will not use object[] args, so we don't gotta hook after BuildCommand()
                // Grabbing ConsoleSystem.Run() as ConsoleSystem.Internal() only processes registered commands & we want to do it simple on the fly
                bool result = Hook( __0, __1, out var reply );

                if ( reply != null )
                {
                    __result = reply;

                    if ( __0.IsServer && __0.PrintOutput && reply.Length > 0 )
                    {
                        DebugEx.Log( reply );
                    }
                }

                return result;
            }
            catch (Exception ex )
            {
                Debug.LogException( ex );
                return true;
            }
        }

        public static bool Hook( ConsoleSystem.Option options, string strCommand, out string reply )
        {
            reply = null;
            var context = Pool.Get<CommandContext>();
            context.PlayerConnection = options.Connection;
            context.IsServerConsole = options.FromRcon || options.IsServer;
            context.RawCommand = strCommand;

            // In modloader this will call broadcast
            bool handled = GatherManagerMod.Instance.OnCommand( context );

            if ( handled == false )
            {
                return true;
            }

            if ( context.Replies.Count > 0 )
            {
                reply = string.Join( "\n", context.Replies );
            }

            Pool.Free( ref context );

            // Don't allow normal commands to go through
            return false;
        }
    }
}
