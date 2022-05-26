using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch.Harmony.GatherManager
{
    public class OnLootSpawnedArgs : Pool.IPooled
    {
        public BaseEntity Entity { get; internal set; }
        public List<ItemContainer> Inventories { get; } = new List<ItemContainer>();

        public void EnterPool()
        {
            
        }

        public void LeavePool()
        {
            Entity = null;
            Inventories.Clear();
        }
    }
}
