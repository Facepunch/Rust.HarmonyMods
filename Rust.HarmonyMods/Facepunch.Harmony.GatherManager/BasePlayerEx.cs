using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Harmony.GatherManager
{
    internal static class BasePlayerEx
    {

        private static MethodInfo method_UnlockAll = typeof( PlayerBlueprints ).GetMethod( "UnlockAll", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );

        public static void UnlockAll( this PlayerBlueprints bps )
        {
            method_UnlockAll.Invoke( bps, null );
        }
    }
}
