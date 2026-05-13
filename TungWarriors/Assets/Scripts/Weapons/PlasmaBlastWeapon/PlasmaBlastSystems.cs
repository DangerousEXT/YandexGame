using Unity.Entities;
using Unity.Transforms;
using Unity.Physics;
using Unity.Mathematics;
using ReadOnlyAttribute = Unity.Collections.ReadOnlyAttribute;
using Unity.Physics.Systems;
using UnityEngine;

public partial struct MovePlasmaBlastSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        foreach (var (transform, plasmaBlastData) in SystemAPI.Query<RefRW<LocalTransform>, PlasmaBlastData>())
        {
            transform.ValueRW.Position += transform.ValueRO.Right() * plasmaBlastData.MoveSpeed * deltaTime;
        }
    }
}

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
[UpdateBefore(typeof(AfterPhysicsSystemGroup))]
public partial struct PlasmaBlastAttackSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var attackJob = new PlasmaBlastAttackJob
        {
            PlasmaBlastDataLookup = SystemAPI.GetComponentLookup<PlasmaBlastData>(true),
            EnemyLookup = SystemAPI.GetComponentLookup<EnemyTag>(true),
            EnemyActiveLookup = SystemAPI.GetComponentLookup<EnemyActiveFlag>(true),
            EnemyAttackDataLookup = SystemAPI.GetComponentLookup<EnemyAttackData>(true),
            DamageBufferLookup = SystemAPI.GetBufferLookup<DamageThisFrame>(),
            DestroyEntityFlagLookup = SystemAPI.GetComponentLookup<DestroyEntityFlag>(),
            PlayerStatsLookup = SystemAPI.GetComponentLookup<PlayerResolvedStats>(true)
        };

        var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
        state.Dependency = attackJob.Schedule(simulationSingleton, state.Dependency);
    }
}


public struct PlasmaBlastAttackJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<PlasmaBlastData> PlasmaBlastDataLookup;
    [ReadOnly] public ComponentLookup<EnemyTag> EnemyLookup;
    [ReadOnly] public ComponentLookup<EnemyActiveFlag> EnemyActiveLookup;
    [ReadOnly] public ComponentLookup<EnemyAttackData> EnemyAttackDataLookup;
    [ReadOnly] public ComponentLookup<PlayerResolvedStats> PlayerStatsLookup;
    public BufferLookup<DamageThisFrame> DamageBufferLookup;
    public ComponentLookup<DestroyEntityFlag> DestroyEntityFlagLookup;
    public void Execute(TriggerEvent triggerEvent)
    {
        Entity plasmaBlastEntity;
        Entity enemyEntity;

        if (PlasmaBlastDataLookup.HasComponent(triggerEvent.EntityA) && EnemyLookup.HasComponent(triggerEvent.EntityB))
        {
            plasmaBlastEntity = triggerEvent.EntityA;
            enemyEntity = triggerEvent.EntityB;
        }
        else if (PlasmaBlastDataLookup.HasComponent(triggerEvent.EntityB)
                 && EnemyLookup.HasComponent(triggerEvent.EntityA))
        {
            plasmaBlastEntity = triggerEvent.EntityB;
            enemyEntity = triggerEvent.EntityA;
        }
        else
        {
            return;
        }

        if (!EnemyActiveLookup.HasComponent(enemyEntity) || !EnemyActiveLookup.IsComponentEnabled(enemyEntity))
            return;

        var plasmaBlastData = PlasmaBlastDataLookup[plasmaBlastEntity];
        var attackDamage = plasmaBlastData.AttackDamage;
        var ownerEntity = plasmaBlastData.Owner;

        if (ownerEntity != Entity.Null)
        {
            if (PlayerStatsLookup.HasComponent(ownerEntity))
            {
                var ownerStats = PlayerStatsLookup[ownerEntity];
                attackDamage = CalculateScaledDamage(
                    plasmaBlastData.AttackDamage,
                    ownerStats.Damage,
                    ownerStats.CritChance,
                    ownerStats.CritDamage,
                    plasmaBlastData.PlayerDamageCoefficient,
                    plasmaBlastData.CritChanceCoefficient,
                    plasmaBlastData.CritDamageCoefficient);
            }
            else if (EnemyAttackDataLookup.HasComponent(ownerEntity))
            {
                attackDamage = math.max(1, EnemyAttackDataLookup[ownerEntity].HitPoints);
            }
        }

        var enemyDamageBuffer = DamageBufferLookup[enemyEntity];
        enemyDamageBuffer.Add(new DamageThisFrame { Value = attackDamage });
        Debug.Log($"Plasma Blast hit enemy for {attackDamage} damage!");
        DestroyEntityFlagLookup.SetComponentEnabled(plasmaBlastEntity, true);
    }

    private static int CalculateScaledDamage(int baseDamage, float ownerDamage, float critChance, float critDamage, float damageCoef, float critChanceCoef, float critDamageCoef)
    {
        var damageWithStats = baseDamage + ownerDamage * damageCoef;
        var normalizedCritChance = math.max(0f, critChance * critChanceCoef) / 100f;
        var critMultiplier = 1f + normalizedCritChance * (math.max(0f, critDamage * critDamageCoef) / 100f);
        return math.max(1, (int)math.round(damageWithStats * critMultiplier));
    }
}
