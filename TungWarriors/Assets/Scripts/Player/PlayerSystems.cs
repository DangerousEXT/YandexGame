using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using Unity.Collections;
using System.Linq;

public partial struct InitializePlayerStatsSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (baseStats, resolvedStats, maxHp, moveSpeed, initFlag) in
                 SystemAPI.Query<RefRW<PlayerBaseStats>, RefRW<PlayerResolvedStats>, RefRW<CharacterMaxHitPoints>, CharacterMoveSpeed, EnabledRefRW<InitializePlayerStatsFlag>>()
                     .WithAll<PlayerTag>())
        {
            baseStats.ValueRW.MoveSpeed = moveSpeed.Value;
            baseStats.ValueRW.MaxHitPoints = maxHp.ValueRO.Value;
            resolvedStats.ValueRW.MaxHitPoints = maxHp.ValueRO.Value;
            initFlag.ValueRW = false;
        }
    }
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct ResolvePlayerStatsSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (baseStats, equipmentStats, statModifiers, resolvedStats, entity) in
                 SystemAPI.Query<PlayerBaseStats, EquipmentStats, DynamicBuffer<PlayerStatModifier>, RefRW<PlayerResolvedStats>>()
                     .WithAll<PlayerTag, PlayerDamageBonus, CharacterMoveSpeedBonus>()
                     .WithAll<CharacterDefense, CharacterHealthRegen, CharacterMaxHitPoints>()
                     .WithAll<CharacterCurrentHitPoints>()
                     .WithEntityAccess())
        {
            var damageBonus = SystemAPI.GetComponentRW<PlayerDamageBonus>(entity);
            var speedBonus = SystemAPI.GetComponentRW<CharacterMoveSpeedBonus>(entity);
            var defense = SystemAPI.GetComponentRW<CharacterDefense>(entity);
            var regen = SystemAPI.GetComponentRW<CharacterHealthRegen>(entity);
            var maxHp = SystemAPI.GetComponentRW<CharacterMaxHitPoints>(entity);
            var currentHp = SystemAPI.GetComponentRW<CharacterCurrentHitPoints>(entity);

            var resolvedDamage = damageBonus.ValueRO.Value + equipmentStats.Damage;
            var resolvedMoveSpeed = speedBonus.ValueRO.Value + equipmentStats.Speed;
            var resolvedDefense = defense.ValueRO.Value;
            var resolvedHealthRegen = regen.ValueRO.ValuePerSecond;
            var resolvedCritChance = equipmentStats.CritChance;
            var resolvedCritDamage = equipmentStats.CritDamage;
            var resolvedMaxHp = (baseStats.MaxHitPoints + equipmentStats.Health) * equipmentStats.HealthValueMultiplicator * equipmentStats.HealthValueMultiplicator;

            foreach (var modifier in statModifiers)
            {
                var mul = 1f + modifier.MulValue;
                switch (modifier.Type)
                {
                    case PlayerStatType.Damage:
                        resolvedDamage = (resolvedDamage + modifier.AddValue) * mul;
                        break;
                    case PlayerStatType.MoveSpeedBonus:
                        resolvedMoveSpeed = (resolvedMoveSpeed + modifier.AddValue) * mul;
                        break;
                    case PlayerStatType.Defense:
                        resolvedDefense = (int)((resolvedDefense + modifier.AddValue) * mul);
                        break;
                    case PlayerStatType.HealthRegen:
                        resolvedHealthRegen = (resolvedHealthRegen + modifier.AddValue) * mul;
                        break;
                    case PlayerStatType.CritChance:
                        resolvedCritChance = (resolvedCritChance + modifier.AddValue) * mul;
                        break;
                    case PlayerStatType.CritDamage:
                        resolvedCritDamage = (resolvedCritDamage + modifier.AddValue) * mul;
                        break;
                    case PlayerStatType.MaxHitPoints:
                        resolvedMaxHp = (resolvedMaxHp + modifier.AddValue) * mul;
                        break;
                }
            }

            var targetMaxHp = math.max(1, (int)math.round(resolvedMaxHp));
            var maxHpDiff = targetMaxHp - maxHp.ValueRO.Value;

            resolvedStats.ValueRW = new PlayerResolvedStats
            {
                Damage = resolvedDamage,
                MoveSpeedBonus = resolvedMoveSpeed,
                Defense = (int)math.round(resolvedDefense),
                HealthRegen = resolvedHealthRegen,
                CritChance = resolvedCritChance,
                CritDamage = resolvedCritDamage,
                MaxHitPoints = targetMaxHp
            };

            maxHp.ValueRW.Value = targetMaxHp;
            if (maxHpDiff > 0)
                currentHp.ValueRW.Value += maxHpDiff;
            currentHp.ValueRW.Value = math.min(currentHp.ValueRO.Value, targetMaxHp);
        }
    }
}

public partial class PlayerInputSystem : SystemBase
{
    private SurvivorsInput _inputActions;

    protected override void OnCreate()
    {
        _inputActions = new SurvivorsInput();
        _inputActions.Enable();
    }
    protected override void OnUpdate()
    {
        var currentInput = (float2)_inputActions.Player.Move.ReadValue<Vector2>();
        foreach (var direction in SystemAPI.Query<RefRW<CharacterMoveDirection>>().WithAll<PlayerTag>())
        {
            direction.ValueRW.Value = currentInput;
        }
        if (math.lengthsq(currentInput) > 0.0001f)
        {
            var normalized = math.normalize(currentInput);
            foreach (var lastDir in SystemAPI.Query<RefRW<LastNonZeroMoveDirection>>().WithAll<PlayerTag>())
                lastDir.ValueRW.Value = normalized;
        }
    }
}

public partial struct PlayerAttackSystem : ISystem
{
    public void OnCreate(ref SystemState systemState)
    {
        systemState.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
    }
    public void OnUpdate(ref SystemState systemState)
    {
        var elapsedTime = SystemAPI.Time.ElapsedTime;
        var entityCommandBufferSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = entityCommandBufferSystem.CreateCommandBuffer(systemState.WorldUnmanaged);
        var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        foreach (var (expirationTimestamp, attackData, transform, entity) in SystemAPI.Query<RefRW<PlasmaBlastWeaponCooldown>, PlasmaBlastWeaponData, LocalTransform>()
            .WithEntityAccess())
        {
            if (expirationTimestamp.ValueRO.Value > elapsedTime) continue;
            var spawnPosition = transform.Position;
            var minDetectPosition = spawnPosition - attackData.DetectionSize;
            var maxDetectPosition = spawnPosition + attackData.DetectionSize;
            var aabbinput = new OverlapAabbInput
            {
                Aabb = new Aabb
                {
                    Min = minDetectPosition,
                    Max = maxDetectPosition
                },
                Filter = attackData.CollisionFilter
            };

            var overlapHits = new NativeList<int>(systemState.WorldUpdateAllocator);
            if (!physicsWorldSingleton.OverlapAabb(aabbinput, ref overlapHits)) continue;

            var maxDistanceSquared = float.MaxValue;
            var closestEnemyPosition = float3.zero;
            foreach (var overlapHit in overlapHits)
            {
                var currentEnemyPosition = physicsWorldSingleton.Bodies[overlapHit].WorldFromBody.pos;
                var distanceToPlayerSquared = math.distancesq(spawnPosition.xy, currentEnemyPosition.xy);
                if (distanceToPlayerSquared < maxDistanceSquared)
                {
                    maxDistanceSquared = distanceToPlayerSquared;
                    closestEnemyPosition = currentEnemyPosition;
                }
            }

            var vectorToClosestEnemy = closestEnemyPosition - spawnPosition;
            var angleToClosestEnemy = math.atan2(vectorToClosestEnemy.y, vectorToClosestEnemy.x);
            var spawnOrientation = quaternion.Euler(0f, 0f, angleToClosestEnemy);
            var newAttack = ecb.Instantiate(attackData.AttackPrefab);
            ecb.SetComponent(newAttack, LocalTransform.FromPositionRotation(spawnPosition, spawnOrientation));

            if (SystemAPI.HasComponent<PlayerResolvedStats>(entity))
            {
                var stats = SystemAPI.GetComponent<PlayerResolvedStats>(entity);
                var projectileData = SystemAPI.GetComponent<PlasmaBlastData>(attackData.AttackPrefab);
                projectileData.AttackDamage = CalculateScaledDamage(projectileData.AttackDamage, stats.Damage, stats.CritChance, stats.CritDamage,
                    projectileData.PlayerDamageCoefficient, projectileData.CritChanceCoefficient, projectileData.CritDamageCoefficient);
                projectileData.MoveSpeed += stats.MoveSpeedBonus * projectileData.PlayerMoveSpeedCoefficient;
                ecb.SetComponent(newAttack, projectileData);
            }
            expirationTimestamp.ValueRW.Value = elapsedTime + attackData.CooldownTime;
        }
    }

    private static int CalculateScaledDamage(int baseDamage, float playerDamage, float critChance, float critDamage, float damageCoef, float critChanceCoef, float critDamageCoef)
    {
        var damageWithStats = baseDamage + playerDamage * damageCoef;
        var normalizedCritChance = math.max(0f, critChance * critChanceCoef) / 100f;
        var critMultiplier = 1f + normalizedCritChance * (math.max(0f, critDamage * critDamageCoef) / 100f);
        return math.max(1, (int)math.round(damageWithStats * critMultiplier));
    }
}

[UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
public partial struct ApplyEquipmentBuffsSystem : ISystem
{
    public void OnUpdate(ref SystemState systemState)
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null) return;

        var playerQuery = world.EntityManager.CreateEntityQuery(typeof(PlayerTag));
        if (playerQuery.IsEmpty) return;

        var playerEntity = playerQuery.GetSingletonEntity();

        foreach (var equipment in PlayerData.Instance.EquipmentOnPlayer.Values)
        {
            foreach (var buff in equipment.Buffs)
            {
                buff.Apply(playerEntity);
            }
        }
        Debug.Log("Buffs Are Applying");
        systemState.Enabled = false;
    }
}
