namespace Facepunch.Harmony.GatherManager
{
    public class OnGatherItemArgs : Pool.IPooled
    {
        public BasePlayer Player { get; internal set; }

        public BaseEntity Entity { get; internal set; }

        public Item GivenItem { get; internal set; }

        public bool IsFinishingBonus { get; internal set; }

        public bool Cancel { get; internal set; }

        public void EnterPool()
        {
            
        }

        public void LeavePool()
        {
            Player = null;
            Entity = null;
            GivenItem = null;
            Cancel = false;
        }
    }
}
