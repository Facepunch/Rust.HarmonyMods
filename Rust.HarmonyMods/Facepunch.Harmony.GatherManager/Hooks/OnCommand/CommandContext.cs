using System;
using System.Collections.Generic;
using Facepunch;
using Network;

namespace Facepunch.Harmony.GatherManager
{
    public class CommandContext : Pool.IPooled
    {
        internal List<string> Replies { get; } = new List<string>();

        public BasePlayer PlayerModel { get; internal set; }

        public Connection PlayerConnection { get; internal set; }

        public bool IsServerConsole { get; internal set; }

        public string RawCommand { get; internal set; }

        public CommandAuthLevel GetAuthLevel()
        {
            if ( PlayerConnection != null )
            {
                switch( PlayerConnection.authLevel )
                {
                    case 0: return CommandAuthLevel.User;
                    case 1: return CommandAuthLevel.Moderator;
                    case 2: return CommandAuthLevel.Admin;
                    case 3: return CommandAuthLevel.Developer;
                    default: return CommandAuthLevel.Unknown;
                }
            }
            else if ( IsServerConsole )
            {
                return CommandAuthLevel.ServerConsole;
            }
            else
            {
                return CommandAuthLevel.Unknown;
            }
        }

        public bool IsAdmin()
        {
            return GetAuthLevel() >= CommandAuthLevel.Admin;
        }

        public void AddReply( string message )
        {
            Replies.Add( message );
        }

        public void SetReply( string message )
        {
            Replies.Clear();
            Replies.Add( message );
        }

        public void EnterPool()
        {
            
        }

        public void LeavePool()
        {
            Replies.Clear();
            PlayerModel = null;
            PlayerConnection = null;
            RawCommand = null;
            IsServerConsole = false;
        }
    }

    public enum CommandAuthLevel
    {
        Unknown = 0,
        User = 1,
        Moderator = 2,
        Admin = 3,
        Developer = 4,
        ServerConsole = 5,
    }
}
