using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Harmony.GatherManager
{
    public class GetCraftDurationArgs : Pool.IPooled
    {
        public ItemBlueprint Blueprint { get; internal set; }

        public float CraftDurationScale { get; set; } = 1f;

        public void EnterPool()
        {
            
        }

        public void LeavePool()
        {
            Blueprint = null;
            CraftDurationScale = 1;
        }
    }
}
