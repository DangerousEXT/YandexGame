using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using ReadOnlyAttribute = Unity.Collections.ReadOnlyAttribute;
public partial struct BatAttackSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (batWeaponData, playerTransform, playerStats, playerEntity) in
                 SystemAPI.Query<RefRW<BatWeaponData>, RefRO<LocalTransform>, RefRO<PlayerResolvedStats>>()
                     .WithAll<PlayerTag>()
                     .WithEntityAccess())
        {
            if (batWeaponData.ValueRO.HasSpawned)
                continue;

            if (batWeaponData.ValueRO.AttackPrefab == Entity.Null)
            {
                Debug.LogError($"BatAttackSystem: AttackPrefab == Entity.Null on player {playerEntity}");
                continue;
            }

            Entity batEntity = ecb.Instantiate(batWeaponData.ValueRO.AttackPrefab);

            float radius = batWeaponData.ValueRO.Range > 0f ? batWeaponData.ValueRO.Range : 1.5f;
            float startAngle = 0f;

            float3 offset = new float3(
                math.cos(startAngle) * radius,
                math.sin(startAngle) * radius,
                0f
            );

            float3 batPosition = playerTransform.ValueRO.Position + offset;

            ecb.SetComponent(batEntity, LocalTransform.FromPositionRotationScale(
                batPosition,
                quaternion.identity,
                1f
            ));

            ecb.AddComponent(batEntity, new BatOrbitData
            {
                Owner = playerEntity,
                Radius = radius,
                AngularSpeed = math.radians(180f),
                CurrentAngle = startAngle,
                Damage = CalculateScaledDamage(
                    playerStats.ValueRO.Damage,
                    playerStats.ValueRO.CritChance,
                    playerStats.ValueRO.CritDamage,
                    batWeaponData.ValueRO.PlayerDamageCoefficient,
                    batWeaponData.ValueRO.CritChanceCoefficient,
                    batWeaponData.ValueRO.CritDamageCoefficient)
            });

            batWeaponData.ValueRW.HasSpawned = true;
            Debug.Log("Bat spawned");
        }
    }

    private static int CalculateScaledDamage(float playerDamage, float critChance, float critDamage, float damageCoef, float critChanceCoef, float critDamageCoef)
    {
        var damageWithStats = playerDamage * damageCoef;
        var normalizedCritChance = math.max(0f, critChance * critChanceCoef) / 100f;
        var critMultiplier = 1f + normalizedCritChance * (math.max(0f, critDamage * critDamageCoef) / 100f);
        return math.max(1, (int)math.round(damageWithStats * critMultiplier));
    }
}

// =========================
// SYSTEM 2: ROTATE BAT
// =========================

public partial struct BatOrbitSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);

        foreach (var (batTransform, orbitData) in
                 SystemAPI.Query<RefRW<LocalTransform>, RefRW<BatOrbitData>>())
        {
            Entity owner = orbitData.ValueRO.Owner;

            if (owner == Entity.Null)
                continue;

            if (!transformLookup.HasComponent(owner))
                continue;

            var ownerTransform = transformLookup[owner];

            orbitData.ValueRW.CurrentAngle += orbitData.ValueRO.AngularSpeed * deltaTime;

            float angle = orbitData.ValueRO.CurrentAngle;
            float radius = orbitData.ValueRO.Radius;

            float3 offset = new float3(
                math.cos(angle) * radius,
                math.sin(angle) * radius,
                0f
            );

            batTransform.ValueRW.Position = ownerTransform.Position + offset;
        }
    }
}


[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
[UpdateBefore(typeof(AfterPhysicsSystemGroup))]
public partial struct BatDamageSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var job = new BatDamageJob
        {
            BatLookup = SystemAPI.GetComponentLookup<BatOrbitData>(true),
            EnemyLookup = SystemAPI.GetComponentLookup<EnemyTag>(true),
            DamageBufferLookup = SystemAPI.GetBufferLookup<DamageThisFrame>()
        };

        var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
        state.Dependency = job.Schedule(simulationSingleton, state.Dependency);
    }
}

[BurstCompile]
public struct BatDamageJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<BatOrbitData> BatLookup;
    [ReadOnly] public ComponentLookup<EnemyTag> EnemyLookup;
    public BufferLookup<DamageThisFrame> DamageBufferLookup;

    public void Execute(TriggerEvent triggerEvent)
    {
        Entity batEntity;
        Entity enemyEntity;

        if (BatLookup.HasComponent(triggerEvent.EntityA) && EnemyLookup.HasComponent(triggerEvent.EntityB))
        {
            batEntity = triggerEvent.EntityA;
            enemyEntity = triggerEvent.EntityB;
        }
        else if (BatLookup.HasComponent(triggerEvent.EntityB) && EnemyLookup.HasComponent(triggerEvent.EntityA))
        {
            batEntity = triggerEvent.EntityB;
            enemyEntity = triggerEvent.EntityA;
        }
        else
        {
            return;
        }

        if (!DamageBufferLookup.HasBuffer(enemyEntity))
            return;

        int damage = BatLookup[batEntity].Damage;
        var damageBuffer = DamageBufferLookup[enemyEntity];
        damageBuffer.Add(new DamageThisFrame { Value = damage });
    }
}
