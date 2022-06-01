using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;

namespace Facepunch.Harmony.GatherManager
{
    public class GrantBlueprintsTask
    {
        public bool Finished { get; private set; }

        public int EntityIndex { get; private set; }

        public int EntityCount { get; private set; }

        public GrantBlueprintsTask()
        {

        }

        public void Start()
        {
            // Use ServerMgr since we arent oxide & needing to hotload
            // Only refreshes loot, its rare we need to do this anyways
            ServerMgr.Instance.StartCoroutine( Coroutine() );
        }

        private IEnumerator Coroutine()
        {
            var entities = BaseNetworkable.serverEntities.OfType<LootContainer>().ToArray();

            EntityCount = entities.Length;

            yield return null;

            Stopwatch watch = Stopwatch.StartNew();

            foreach( var entity in entities )
            {
                if ( watch.ElapsedMilliseconds > 20 )
                {
                    yield return null;
                    watch.Restart();
                }

                try
                {
                    EntityIndex++;

                    if ( entity == null || entity.IsDestroyed )
                    {
                        continue;
                    }

                    entity.SpawnLoot();
                }
                catch ( Exception ex )
                {
                    UnityEngine.Debug.LogException( ex );
                }
            }

            Finished = true;
        }
    }
}
