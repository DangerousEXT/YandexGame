using Assets.Scripts.DeathConsequencesSystems;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateBefore(typeof(EndInitializationEntityCommandBufferSystem))]
public partial struct EnemySpawnPoolInitializationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemySpawnData>();
        state.RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (poolState, spawnState, spawnData, spawnerEntity) in
                 SystemAPI.Query<RefRW<EnemySpawnPoolState>, EnemySpawnState, EnemySpawnData>()
                     .WithEntityAccess())
        {
            if (poolState.ValueRO.IsInitialized)
                continue;

            // Prewarm the full pool once so gameplay spawning only reuses disabled entities.
            for (var i = 0; i < spawnState.MaxSpawnedEnemies; i++)
            {
                var pooledEnemy = ecb.Instantiate(spawnData.EnemyPrefab);
                ecb.AddComponent(pooledEnemy, new EnemyPoolOwner
                {
                    Spawner = spawnerEntity
                });
                ecb.AppendToBuffer(spawnerEntity, new EnemyPoolElement
                {
                    Value = pooledEnemy
                });
                ecb.SetEnabled(pooledEnemy, false);
            }

            poolState.ValueRW.IsInitialized = true;
        }
    }
}

[BurstCompile]
public partial struct EnemySpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();
        state.RequireForUpdate<EnemySpawnData>();
        state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var ecbSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

        var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        var playerPosition = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position;
        var entityManager = state.EntityManager;

        foreach (var (spawnState, spawnData, poolState, poolBuffer) in
                 SystemAPI.Query<RefRW<EnemySpawnState>, EnemySpawnData, EnemySpawnPoolState, DynamicBuffer<EnemyPoolElement>>())
        {
            if (!poolState.IsInitialized)
                continue;

            if (spawnState.ValueRO.CurrentSpawnedEnemies >= spawnState.ValueRO.MaxSpawnedEnemies)
                continue;

            spawnState.ValueRW.SpawnTimer -= deltaTime;
            if (spawnState.ValueRO.SpawnTimer > 0f)
                continue;

            if (poolBuffer.Length == 0)
            {
                spawnState.ValueRW.SpawnTimer = 0f;
                continue;
            }

            spawnState.ValueRW.SpawnTimer = spawnData.spawnInterval;

            var poolIndex = poolBuffer.Length - 1;
            var pooledEnemy = poolBuffer[poolIndex].Value;
            poolBuffer.RemoveAt(poolIndex);

            var spawnAngle = spawnState.ValueRW.Random.NextFloat(0f, math.TAU);
            var spawnPoint = new float3
            {
                x = math.sin(spawnAngle),
                y = math.cos(spawnAngle),
                z = 0f
            };
            spawnPoint *= spawnData.spawnDistance;
            spawnPoint += playerPosition;
            spawnState.ValueRW.CurrentSpawnedEnemies++;

            // Runtime spawn now only resets and enables a pooled entity.
            ResetPooledEnemy(ref ecb, pooledEnemy, spawnPoint, entityManager);
        }
    }

    private static void ResetPooledEnemy(ref EntityCommandBuffer ecb, Entity enemyEntity, float3 spawnPoint, EntityManager entityManager)
    {
        if (entityManager.HasComponent<CharacterMaxHitPoints>(enemyEntity))
        {
            var maxHitPoints = entityManager.GetComponentData<CharacterMaxHitPoints>(enemyEntity);
            ecb.SetComponent(enemyEntity, new CharacterCurrentHitPoints
            {
                Value = maxHitPoints.Value
            });
        }

        if (entityManager.HasComponent<CharacterMoveDirection>(enemyEntity))
        {
            ecb.SetComponent(enemyEntity, new CharacterMoveDirection
            {
                Value = float2.zero
            });
        }

        if (entityManager.HasComponent<PhysicsVelocity>(enemyEntity))
            ecb.SetComponent(enemyEntity, default(PhysicsVelocity));

        if (entityManager.HasBuffer<DamageThisFrame>(enemyEntity))
            entityManager.GetBuffer<DamageThisFrame>(enemyEntity).Clear();

        ecb.SetComponentEnabled<EnemyActiveFlag>(enemyEntity, true);

        if (entityManager.HasComponent<EnemyCooldownExpirationTimestamp>(enemyEntity))
        {
            ecb.SetComponent(enemyEntity, new EnemyCooldownExpirationTimestamp
            {
                value = 0d
            });
            ecb.SetComponentEnabled<EnemyCooldownExpirationTimestamp>(enemyEntity, false);
        }

        ecb.SetComponentEnabled<DeathEntityFlag>(enemyEntity, false);
        ecb.SetComponentEnabled<DestroyEntityFlag>(enemyEntity, false);
        ecb.SetComponent(enemyEntity, LocalTransform.FromPosition(spawnPoint));
        ecb.SetEnabled(enemyEntity, true);
    }
}
