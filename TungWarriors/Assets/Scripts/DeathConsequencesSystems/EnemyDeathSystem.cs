using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Assets.Scripts.DeathConsequencesSystems
{
    [UpdateInGroup(typeof(DeathConsequencesGroup), OrderLast = true)]
    public partial struct EnemyDeathSystem : ISystem
    {
        private struct DeadEnemyInfo
        {
            public Entity Enemy;
            public Entity Spawner;
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

            var spawnStateLookup = SystemAPI.GetComponentLookup<EnemySpawnState>();
            var poolLookup = SystemAPI.GetBufferLookup<EnemyPoolElement>();
            var maxHitPointsLookup = SystemAPI.GetComponentLookup<CharacterMaxHitPoints>(true);
            var currentHitPointsLookup = SystemAPI.GetComponentLookup<CharacterCurrentHitPoints>();
            var moveDirectionLookup = SystemAPI.GetComponentLookup<CharacterMoveDirection>();
            var physicsVelocityLookup = SystemAPI.GetComponentLookup<PhysicsVelocity>();
            var cooldownLookup = SystemAPI.GetComponentLookup<EnemyCooldownExpirationTimestamp>();
            var damageBufferLookup = SystemAPI.GetBufferLookup<DamageThisFrame>();
            var entityManager = state.EntityManager;

            using var deadEnemies = new NativeList<DeadEnemyInfo>(Allocator.Temp);

            foreach (var (poolOwner, localToWorld, entity) in
                     SystemAPI.Query<EnemyPoolOwner, LocalToWorld>()
                         .WithAll<EnemyTag, DeathEntityFlag, EnemyActiveFlag>()
                         .WithDisabled<DestroyEntityFlag>()
                         .WithEntityAccess())
            {
                deadEnemies.Add(new DeadEnemyInfo
                {
                    Enemy = entity,
                    Spawner = poolOwner.Spawner,
                    Position = localToWorld.Position
                });
            }

            // Process deaths after the query so we can disable entities immediately without mutating the active iterator.
            foreach (var deadEnemy in deadEnemies)
            {
                entityManager.SetComponentEnabled<EnemyActiveFlag>(deadEnemy.Enemy, false);

                if (spawnStateLookup.HasComponent(deadEnemy.Spawner))
                {
                    var spawnState = spawnStateLookup[deadEnemy.Spawner];
                    if (spawnState.CurrentSpawnedEnemies > 0)
                    {
                        spawnState.CurrentSpawnedEnemies--;
                        spawnStateLookup[deadEnemy.Spawner] = spawnState;
                    }
                }

                if (entityManager.HasComponent<GemPrefab>(deadEnemy.Enemy))
                {
                    var gemPrefab = entityManager.GetComponentData<GemPrefab>(deadEnemy.Enemy).Value;
                    var newGem = beginECB.Instantiate(gemPrefab);
                    beginECB.SetComponent(newGem, LocalTransform.FromPosition(deadEnemy.Position));
                }

                if (poolLookup.HasBuffer(deadEnemy.Spawner))
                {
                    poolLookup[deadEnemy.Spawner].Add(new EnemyPoolElement
                    {
                        Value = deadEnemy.Enemy
                    });
                }

                if (maxHitPointsLookup.HasComponent(deadEnemy.Enemy) && currentHitPointsLookup.HasComponent(deadEnemy.Enemy))
                {
                    currentHitPointsLookup[deadEnemy.Enemy] = new CharacterCurrentHitPoints
                    {
                        Value = maxHitPointsLookup[deadEnemy.Enemy].Value
                    };
                }

                if (moveDirectionLookup.HasComponent(deadEnemy.Enemy))
                {
                    moveDirectionLookup[deadEnemy.Enemy] = new CharacterMoveDirection
                    {
                        Value = float2.zero
                    };
                }

                if (physicsVelocityLookup.HasComponent(deadEnemy.Enemy))
                    physicsVelocityLookup[deadEnemy.Enemy] = default;

                if (damageBufferLookup.HasBuffer(deadEnemy.Enemy))
                    damageBufferLookup[deadEnemy.Enemy].Clear();

                if (cooldownLookup.HasComponent(deadEnemy.Enemy))
                {
                    cooldownLookup[deadEnemy.Enemy] = new EnemyCooldownExpirationTimestamp
                    {
                        value = 0d
                    };
                    cooldownLookup.SetComponentEnabled(deadEnemy.Enemy, false);
                }

                entityManager.SetComponentEnabled<DeathEntityFlag>(deadEnemy.Enemy, false);
                entityManager.SetComponentEnabled<DestroyEntityFlag>(deadEnemy.Enemy, false);
                entityManager.SetEnabled(deadEnemy.Enemy, false);
            }
        }
    }
}
