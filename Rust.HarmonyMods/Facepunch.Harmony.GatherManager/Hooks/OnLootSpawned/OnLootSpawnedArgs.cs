using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Harmony.GatherManager
{
    public class OnLootSpawnedArgs : Pool.IPooled
    {
        public LootContainer Entity { get; internal set; }

        public void EnterPool()
        {
            
        }

        public void LeavePool()
        {
            Entity = null;
        }
    }
}
