using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Physics;
using Unity.Collections;
using Unity.Physics.Systems;
using System;

public partial struct EnemyMoveToPlayerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        var playerPosition = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position.xy;
        var moveToPlayerJob = new EnemyMoveToPlayerJob
        {
            playerPosition = playerPosition
        };
        state.Dependency = moveToPlayerJob.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
[WithAll(typeof(EnemyTag), typeof(EnemyActiveFlag))]
public partial struct EnemyMoveToPlayerJob : IJobEntity
{
    public float2 playerPosition;
    private void Execute(ref CharacterMoveDirection characterMoveDirection, in LocalTransform localTransform)
    {
        var vectorToPlayer = playerPosition - localTransform.Position.xy;
        characterMoveDirection.Value = math.normalize(vectorToPlayer);
    }
}
[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
[UpdateBefore(typeof(AfterPhysicsSystemGroup))]
public partial struct EnemyAttackSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var elapsedTime = SystemAPI.Time.ElapsedTime;
        foreach (var (expirationTimeStamp, cooldownEnadled) in SystemAPI.Query<EnemyCooldownExpirationTimestamp, EnabledRefRW<EnemyCooldownExpirationTimestamp>>())
        {
            if (expirationTimeStamp.value > elapsedTime) continue;
            cooldownEnadled.ValueRW = false;
        }

        var attackJob = new EnemyAttackJob
        {
            PlayerLookup = SystemAPI.GetComponentLookup<PlayerTag>(true),
            AttackDataLookup = SystemAPI.GetComponentLookup<EnemyAttackData>(true),
            EnemyActiveLookup = SystemAPI.GetComponentLookup<EnemyActiveFlag>(true),
            CooldownLookup = SystemAPI.GetComponentLookup<EnemyCooldownExpirationTimestamp>(),
            DamageBufferLookup = SystemAPI.GetBufferLookup<DamageThisFrame>(),
            ElapsedTime = elapsedTime
        };

        var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
        state.Dependency = attackJob.Schedule(simulationSingleton, state.Dependency);
    }
}

[BurstCompile]
public struct EnemyAttackJob : ICollisionEventsJob
{
    [ReadOnly] public ComponentLookup<PlayerTag> PlayerLookup;
    [ReadOnly] public ComponentLookup<EnemyAttackData> AttackDataLookup;
    [ReadOnly] public ComponentLookup<EnemyActiveFlag> EnemyActiveLookup;
    public ComponentLookup<EnemyCooldownExpirationTimestamp> CooldownLookup;
    public BufferLookup<DamageThisFrame> DamageBufferLookup;
    public double ElapsedTime;

    public void Execute(CollisionEvent collisionEvent)
    {
        Entity playerEntity;
        Entity enemyEntity;

        if (PlayerLookup.HasComponent(collisionEvent.EntityA) && AttackDataLookup.HasComponent(collisionEvent.EntityB))
        {
            playerEntity = collisionEvent.EntityA;
            enemyEntity = collisionEvent.EntityB;
        }
        else if (PlayerLookup.HasComponent(collisionEvent.EntityB) && AttackDataLookup.HasComponent(collisionEvent.EntityA))
        {
            playerEntity = collisionEvent.EntityB;
            enemyEntity = collisionEvent.EntityA;
        }
        else
        {
            return;
        }

        if (!EnemyActiveLookup.HasComponent(enemyEntity) || !EnemyActiveLookup.IsComponentEnabled(enemyEntity))
            return;

        if (CooldownLookup.IsComponentEnabled(enemyEntity))
        {
            return;
        }
        var attackData = AttackDataLookup[enemyEntity];
        CooldownLookup[enemyEntity] = new EnemyCooldownExpirationTimestamp { value = ElapsedTime + attackData.CooldownTime };
        CooldownLookup.SetComponentEnabled(enemyEntity, true);

        var playerDamageBuffer = DamageBufferLookup[playerEntity];
        playerDamageBuffer.Add(new DamageThisFrame { Value = attackData.HitPoints });
    }
}
