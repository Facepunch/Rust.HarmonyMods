using Harmony;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Facepunch.Harmony.GatherManager
{
    [HarmonyPatch( typeof( ServerMgr ), "UpdateServerInformation" )]
    internal class ServerMgr_UpdateServerInformation
    {
        private static string _realDescription;

        [HarmonyPrefix]
        static void Prefix()
        {
            try
            {
                // Append gather rate to server description dynamically since its reasonable to assume finely grained config will change later
                _realDescription = ConVar.Server.description;
                ConVar.Server.description += "\n" + GatherManagerMod.Instance.GetGatherDescription();
            }
            catch (Exception ex )
            {
                Debug.LogException( ex );
            }
        }

        [HarmonyPostfix]
        static void Postfix() 
        {
            try
            {
                // Set server as modded
                if ( !SteamServer.GameTags.Contains(",modded") )
                {
                    SteamServer.GameTags += ",modded";
                }

                if ( _realDescription != null )
                {
                    ConVar.Server.description = _realDescription;
                }
            }
            catch ( Exception ex )
            {
                Debug.LogException( ex );
            }
        }
    }
}
