using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Facepunch.Harmony.GatherManager
{
    [HarmonyPatch( typeof( ServerMgr ), "UpdateServerInformation" )]
    internal class ServerMgr_UpdateServerInformation
    {
        private static string _realDescription;

        private static PropertyInfo property_GameTags = AccessTools.TypeByName( "Steamworks.SteamServer" ).GetProperty( "GameTags", BindingFlags.Public | BindingFlags.Static );

        private static string GetGameTags()
        {
            return property_GameTags.GetValue( null ) as string;
        }

        public static void SetGameTags( string value )
        {
            property_GameTags.SetValue( null, value );
        }

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
                if ( _realDescription != null )
                {
                    ConVar.Server.description = _realDescription;
                }

                // Set server as modded
                string tags = GetGameTags();
                if ( !tags.Contains(",modded") )
                {
                    SetGameTags( tags + ",modded" );
                }
            }
            catch ( Exception ex )
            {
                Debug.LogException( ex );
            }
        }
    }
}
