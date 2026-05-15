using System;
using Assets.Scripts.DeathConsequencesSystems;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

internal static class EnemyDifficultyUtility
{
    public const int InitialSpawnCap = 10;
    public const int SecondMinuteSpawnCap = 20;
    public const int ThirdMinuteSpawnCap = 50;
    public const int MaxSpawnCap = 100;

    public static int GetSpawnCapForMinute(int minute)
    {
        if (minute <= 0)
            return InitialSpawnCap;
        if (minute == 1)
            return SecondMinuteSpawnCap;
        if (minute == 2)
            return ThirdMinuteSpawnCap;
        return MaxSpawnCap;
    }

    public static float GetScaledMaxHitPoints(float baseHitPoints, int minute)
    {
        var scale = System.Math.Pow(2d, math.max(0, minute));
        var scaledValue = baseHitPoints * scale;
        return scaledValue >= float.MaxValue ? float.MaxValue : (float)scaledValue;
    }

    public static int GetScaledDamage(int baseDamage, int minute)
    {
        var scale = System.Math.Pow(2d, math.max(0, minute));
        var scaledValue = baseDamage * scale;
        return scaledValue >= int.MaxValue ? int.MaxValue : (int)System.Math.Round(scaledValue);
    }
}

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
                 SystemAPI.Query<RefRW<EnemySpawnPoolState>, RefRW<EnemySpawnState>, EnemySpawnData>()
                     .WithEntityAccess())
        {
            if (poolState.ValueRO.IsInitialized)
                continue;

            spawnState.ValueRW.MaxSpawnedEnemies = EnemyDifficultyUtility.InitialSpawnCap;
            spawnState.ValueRW.CurrentSpawnedEnemies = math.min(
                spawnState.ValueRO.CurrentSpawnedEnemies,
                spawnState.ValueRO.MaxSpawnedEnemies);

            // Prewarm the whole progression cap once so gameplay never instantiates enemies mid-run.
            for (var i = 0; i < EnemyDifficultyUtility.MaxSpawnCap; i++)
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
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(EnemySpawnSystem))]
public partial struct EnemyDifficultySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MatchTimerState>();
        state.RequireForUpdate<EnemySpawnState>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var timerState = SystemAPI.GetSingletonRW<MatchTimerState>();
        timerState.ValueRW.ElapsedSeconds += SystemAPI.Time.DeltaTime;

        var currentMinute = (int)math.floor(timerState.ValueRO.ElapsedSeconds / 60f);
        var targetSpawnCap = EnemyDifficultyUtility.GetSpawnCapForMinute(currentMinute);

        foreach (var spawnState in SystemAPI.Query<RefRW<EnemySpawnState>>())
            spawnState.ValueRW.MaxSpawnedEnemies = targetSpawnCap;

        if (timerState.ValueRO.AppliedDifficultyMinute == currentMinute)
            return;

        timerState.ValueRW.AppliedDifficultyMinute = currentMinute;

        foreach (var (baseStats, maxHitPoints, currentHitPoints, attackData) in
                 SystemAPI.Query<EnemyBaseStats, RefRW<CharacterMaxHitPoints>, RefRW<CharacterCurrentHitPoints>, RefRW<EnemyAttackData>>()
                     .WithAll<EnemyActiveFlag>())
        {
            var oldMax = math.max(1f, maxHitPoints.ValueRO.Value);
            var healthFraction = math.clamp(currentHitPoints.ValueRO.Value / oldMax, 0f, 1f);
            var newMax = EnemyDifficultyUtility.GetScaledMaxHitPoints(baseStats.MaxHitPoints, currentMinute);

            maxHitPoints.ValueRW.Value = newMax;
            currentHitPoints.ValueRW.Value = newMax * healthFraction;
            attackData.ValueRW.HitPoints = EnemyDifficultyUtility.GetScaledDamage(baseStats.AttackDamage, currentMinute);
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
        state.RequireForUpdate<MatchTimerState>();
        state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var ecbSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

        var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        var playerPosition = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position;
        var difficultyMinute = math.max(0, SystemAPI.GetSingleton<MatchTimerState>().AppliedDifficultyMinute);
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

            // Runtime spawn only reconfigures and enables a pooled entity with the current difficulty.
            ResetPooledEnemy(ref ecb, pooledEnemy, spawnPoint, entityManager, difficultyMinute);
        }
    }

    private static void ResetPooledEnemy(ref EntityCommandBuffer ecb, Entity enemyEntity, float3 spawnPoint, EntityManager entityManager, int difficultyMinute)
    {
        if (entityManager.HasComponent<EnemyBaseStats>(enemyEntity))
        {
            var baseStats = entityManager.GetComponentData<EnemyBaseStats>(enemyEntity);
            var scaledMaxHitPoints = EnemyDifficultyUtility.GetScaledMaxHitPoints(baseStats.MaxHitPoints, difficultyMinute);
            var scaledDamage = EnemyDifficultyUtility.GetScaledDamage(baseStats.AttackDamage, difficultyMinute);

            if (entityManager.HasComponent<CharacterMaxHitPoints>(enemyEntity))
            {
                ecb.SetComponent(enemyEntity, new CharacterMaxHitPoints
                {
                    Value = scaledMaxHitPoints
                });
            }

            if (entityManager.HasComponent<CharacterCurrentHitPoints>(enemyEntity))
            {
                ecb.SetComponent(enemyEntity, new CharacterCurrentHitPoints
                {
                    Value = scaledMaxHitPoints
                });
            }

            if (entityManager.HasComponent<EnemyAttackData>(enemyEntity))
            {
                var attackData = entityManager.GetComponentData<EnemyAttackData>(enemyEntity);
                attackData.HitPoints = scaledDamage;
                ecb.SetComponent(enemyEntity, attackData);
            }
        }
        else if (entityManager.HasComponent<CharacterMaxHitPoints>(enemyEntity))
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
