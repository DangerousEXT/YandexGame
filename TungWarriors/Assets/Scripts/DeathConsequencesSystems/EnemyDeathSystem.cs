using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.DeathConsequencesSystems
{
    [UpdateInGroup(typeof(DeathConsequencesGroup), OrderLast = true)]
    public partial struct EnemyDeathSystem : ISystem
    {
        private struct DeadEnemyInfo
        {
            public Entity Enemy;
            public float3 Position;
        }

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnemySpawnState>();
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var beginEcbSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var beginECB = beginEcbSystem.CreateCommandBuffer(state.WorldUnmanaged);

            var entityManager = state.EntityManager;

            using var deadEnemies = new NativeList<DeadEnemyInfo>(Allocator.Temp);

            foreach (var (_, localToWorld, entity) in
                     SystemAPI.Query<EnemyPoolOwner, LocalToWorld>()
                         .WithAll<EnemyTag, DeathEntityFlag, EnemyActiveFlag>()
                         .WithDisabled<DestroyEntityFlag>()
                         .WithEntityAccess())
            {
                deadEnemies.Add(new DeadEnemyInfo
                {
                    Enemy = entity,
                    Position = localToWorld.Position
                });
            }

            // Process deaths after the query so we can disable entities immediately without mutating the active iterator.
            foreach (var deadEnemy in deadEnemies)
            {
                if (entityManager.HasComponent<GemPrefab>(deadEnemy.Enemy))
                {
                    var gemPrefab = entityManager.GetComponentData<GemPrefab>(deadEnemy.Enemy).Value;
                    var newGem = beginECB.Instantiate(gemPrefab);
                    beginECB.SetComponent(newGem, LocalTransform.FromPosition(deadEnemy.Position));
                }

                EnemyPoolUtility.ReturnEnemyToPool(entityManager, deadEnemy.Enemy);
            }
        }
    }
}
