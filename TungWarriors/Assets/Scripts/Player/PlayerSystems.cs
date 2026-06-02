using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;

public struct PlayerStatsDirtyTag : IComponentData, IEnableableComponent
{
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateBefore(typeof(ApplyEquipmentBuffsSystem))]
public partial struct InitializePlayerStatsSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (baseStats, resolvedStats, maxHp, moveSpeed, initFlag) in
                 SystemAPI.Query<
                         RefRW<PlayerBaseStats>,
                         RefRW<PlayerResolvedStats>,
                         RefRO<CharacterMaxHitPoints>,
                         RefRO<CharacterMoveSpeed>,
                         EnabledRefRW<InitializePlayerStatsFlag>>()
                     .WithAll<PlayerTag>())
        {
            baseStats.ValueRW.MoveSpeed = moveSpeed.ValueRO.Value;
            baseStats.ValueRW.MaxHitPoints = maxHp.ValueRO.Value;

            resolvedStats.ValueRW.MaxHitPoints = maxHp.ValueRO.Value;

            initFlag.ValueRW = false;
        }
    }
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(InitializePlayerStatsSystem))]
[UpdateBefore(typeof(ApplyPlayerStatOperationsSystem))]
public partial struct ApplyEquipmentBuffsSystem : ISystem
{
    private EntityQuery _playerQuery;

    public void OnCreate(ref SystemState state)
    {
        _playerQuery = state.GetEntityQuery(
            ComponentType.ReadOnly<PlayerTag>(),
            ComponentType.ReadWrite<EquipmentStats>(),
            ComponentType.ReadWrite<PlayerStatsResolvedAfterMainMenu>()
        );

        state.RequireForUpdate(_playerQuery);
    }

    public void OnUpdate(ref SystemState state)
    {
        if (PlayerData.Instance == null)
            return;

        if (_playerQuery.IsEmpty)
            return;

        var playerEntity = _playerQuery.GetSingletonEntity();

        var statsResolvedComponent =
            SystemAPI.GetComponent<PlayerStatsResolvedAfterMainMenu>(playerEntity);

        if (statsResolvedComponent.HasResolved)
            return;

        var metaSnapshot = PlayerData.Instance.GetMetaProgressionSnapshot();

        SystemAPI.SetComponent(playerEntity, new EquipmentStats
        {
            Damage = metaSnapshot.DamageBonus,
            Speed = metaSnapshot.MoveSpeedBonus,
            Health = metaSnapshot.MaxHitPointsBonus
        });

        foreach (var equipment in PlayerData.Instance.EquipmentOnPlayer.Values)
        {
            if (equipment == null)
                continue;

            foreach (var buff in equipment.Buffs)
            {
                buff.Apply(playerEntity);
            }
        }

        statsResolvedComponent.HasResolved = true;
        SystemAPI.SetComponent(playerEntity, statsResolvedComponent);

        MarkPlayerStatsDirty(ref state, playerEntity);
    }

    private void MarkPlayerStatsDirty(ref SystemState state, Entity playerEntity)
    {
        if (!SystemAPI.HasComponent<PlayerStatsDirtyTag>(playerEntity))
            return;

        SystemAPI.SetComponentEnabled<PlayerStatsDirtyTag>(playerEntity, true);
    }
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(ApplyEquipmentBuffsSystem))]
[UpdateBefore(typeof(ResolvePlayerStatsSystem))]
public partial struct ApplyPlayerStatOperationsSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (operations, modifiers, entity) in
                 SystemAPI.Query<
                         DynamicBuffer<PlayerStatOperationElement>,
                         DynamicBuffer<PlayerStatModifier>>()
                     .WithAll<PlayerTag>()
                     .WithEntityAccess())
        {
            if (operations.Length == 0)
                continue;

            for (int i = 0; i < operations.Length; i++)
            {
                var op = operations[i];

                modifiers.Add(new PlayerStatModifier
                {
                    Type = op.Type,
                    AddValue = op.AddValue,
                    MulValue = op.MulValue
                });
            }

            operations.Clear();

            if (SystemAPI.HasComponent<PlayerStatsDirtyTag>(entity))
            {
                SystemAPI.SetComponentEnabled<PlayerStatsDirtyTag>(entity, true);
            }
        }
    }
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(ApplyPlayerStatOperationsSystem))]
public partial struct ResolvePlayerStatsSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (baseStats, equipmentStats, statModifiers, resolvedStats, dirtyFlag, entity) in
                 SystemAPI.Query<
                         RefRO<PlayerBaseStats>,
                         RefRO<EquipmentStats>,
                         DynamicBuffer<PlayerStatModifier>,
                         RefRW<PlayerResolvedStats>,
                         EnabledRefRW<PlayerStatsDirtyTag>>()
                     .WithAll<PlayerTag>()
                     .WithAll<PlayerDamageBonus, CharacterMoveSpeedBonus>()
                     .WithAll<CharacterDefense, CharacterHealthRegen, CharacterMaxHitPoints>()
                     .WithAll<CharacterCurrentHitPoints>()
                     .WithEntityAccess())
        {
            var playerMaxHp =
                SystemAPI.GetComponentRW<CharacterMaxHitPoints>(entity);

            var playerCurrentHp =
                SystemAPI.GetComponentRW<CharacterCurrentHitPoints>(entity);

            var playerDamageBonus =
                SystemAPI.GetComponentRW<PlayerDamageBonus>(entity);

            var playerMoveSpeedBonus =
                SystemAPI.GetComponentRW<CharacterMoveSpeedBonus>(entity);

            var playerDefense =
                SystemAPI.GetComponentRW<CharacterDefense>(entity);

            var playerHealthRegen =
                SystemAPI.GetComponentRW<CharacterHealthRegen>(entity);

            float damageAdd = 0f;
            float moveSpeedAdd = 0f;
            float defenseAdd = 0f;
            float healthRegenAdd = 0f;
            float critChanceAdd = 0f;
            float critDamageAdd = 0f;
            float maxHitPointsAdd = 0f;
            float attackSpeedAdd = 0f;

            float damageMul = 0f;
            float moveSpeedMul = 0f;
            float defenseMul = 0f;
            float healthRegenMul = 0f;
            float critChanceMul = 0f;
            float critDamageMul = 0f;
            float maxHitPointsMul = 0f;
            float attackSpeedMul = 0f;

            for (int i = 0; i < statModifiers.Length; i++)
            {
                var modifier = statModifiers[i];

                switch (modifier.Type)
                {
                    case PlayerStatType.Damage:
                        damageAdd += modifier.AddValue;
                        damageMul += modifier.MulValue;
                        break;

                    case PlayerStatType.MoveSpeedBonus:
                        moveSpeedAdd += modifier.AddValue;
                        moveSpeedMul += modifier.MulValue;
                        break;

                    case PlayerStatType.Defense:
                        defenseAdd += modifier.AddValue;
                        defenseMul += modifier.MulValue;
                        break;

                    case PlayerStatType.HealthRegen:
                        healthRegenAdd += modifier.AddValue;
                        healthRegenMul += modifier.MulValue;
                        break;

                    case PlayerStatType.CritChance:
                        critChanceAdd += modifier.AddValue;
                        critChanceMul += modifier.MulValue;
                        break;

                    case PlayerStatType.CritDamage:
                        critDamageAdd += modifier.AddValue;
                        critDamageMul += modifier.MulValue;
                        break;

                    case PlayerStatType.MaxHitPoints:
                        maxHitPointsAdd += modifier.AddValue;
                        maxHitPointsMul += modifier.MulValue;
                        break;

                    case PlayerStatType.AttackSpeed:
                        attackSpeedAdd += modifier.AddValue;
                        attackSpeedMul += modifier.MulValue;
                        break;
                }
            }

            float finalMaxHp =
                (baseStats.ValueRO.MaxHitPoints + equipmentStats.ValueRO.Health + maxHitPointsAdd) *
                (equipmentStats.ValueRO.HealthPercentageMultiplicator + 1f) *
                (equipmentStats.ValueRO.HealthValueMultiplicator + 1f) *
                (1f + math.max(0f, maxHitPointsMul));

            var shouldInitializeCurrentHpToMax =
                SystemAPI.HasComponent<InitializePlayerCurrentHitPointsToMaxFlag>(entity) &&
                SystemAPI.IsComponentEnabled<InitializePlayerCurrentHitPointsToMaxFlag>(entity);

            playerMaxHp.ValueRW.Value = finalMaxHp;
            playerCurrentHp.ValueRW.Value =
                shouldInitializeCurrentHpToMax
                    ? finalMaxHp
                    : math.min(playerCurrentHp.ValueRO.Value, finalMaxHp);

            if (shouldInitializeCurrentHpToMax)
                SystemAPI.SetComponentEnabled<InitializePlayerCurrentHitPointsToMaxFlag>(entity, false);

            resolvedStats.ValueRW.Damage =
                (baseStats.ValueRO.Damage + equipmentStats.ValueRO.Damage + damageAdd) *
                (equipmentStats.ValueRO.DamageValueMultiplicator + 1f) *
                (equipmentStats.ValueRO.DamagePercentageMultiplicator + 1f) *
                (1f + math.max(0f, damageMul));

            resolvedStats.ValueRW.MoveSpeedBonus =
                (equipmentStats.ValueRO.Speed + moveSpeedAdd) *
                (1f + math.max(0f, moveSpeedMul));

            resolvedStats.ValueRW.Defense =
                (int)math.max(0f, defenseAdd * (1f + math.max(0f, defenseMul)));

            resolvedStats.ValueRW.HealthRegen =
                math.max(0f, healthRegenAdd * (1f + math.max(0f, healthRegenMul)));

            resolvedStats.ValueRW.CritChance =
                math.max(0f, critChanceAdd * (1f + math.max(0f, critChanceMul)));

            resolvedStats.ValueRW.CritDamage =
                math.max(0f, critDamageAdd * (1f + math.max(0f, critDamageMul)));

            resolvedStats.ValueRW.AttackSpeed =
                math.max(0f, attackSpeedAdd + attackSpeedMul);

            resolvedStats.ValueRW.MaxHitPoints = finalMaxHp;

            playerDamageBonus.ValueRW.Value =
                (int)math.max(0f, damageAdd);

            playerMoveSpeedBonus.ValueRW.Value =
                resolvedStats.ValueRO.MoveSpeedBonus;

            playerDefense.ValueRW.Value =
                resolvedStats.ValueRO.Defense;

            playerHealthRegen.ValueRW.ValuePerSecond =
                resolvedStats.ValueRO.HealthRegen;

            dirtyFlag.ValueRW = false;
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

    protected override void OnDestroy()
    {
        _inputActions.Disable();
        _inputActions.Dispose();
    }

    protected override void OnUpdate()
    {
        var currentInput =
            (float2)_inputActions.Player.Move.ReadValue<Vector2>();

        foreach (var direction in
                 SystemAPI.Query<RefRW<CharacterMoveDirection>>()
                     .WithAll<PlayerTag>())
        {
            direction.ValueRW.Value = currentInput;
        }

        if (math.lengthsq(currentInput) <= 0.0001f)
            return;

        var normalized = math.normalize(currentInput);

        foreach (var lastDir in
                 SystemAPI.Query<RefRW<LastNonZeroMoveDirection>>()
                     .WithAll<PlayerTag>())
        {
            lastDir.ValueRW.Value = normalized;
        }
    }
}

public partial struct PlayerAttackSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        double elapsedTime = SystemAPI.Time.ElapsedTime;

        var entityCommandBufferSystem =
            SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();

        var ecb =
            entityCommandBufferSystem.CreateCommandBuffer(state.WorldUnmanaged);

        var physicsWorldSingleton =
            SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        foreach (var (expirationTimestamp, attackData, transform, entity) in
                 SystemAPI.Query<
                         RefRW<PlasmaBlastWeaponCooldown>,
                         RefRO<PlasmaBlastWeaponData>,
                         RefRO<LocalTransform>>()
                     .WithEntityAccess())
        {
            if (expirationTimestamp.ValueRO.Value > elapsedTime)
                continue;

            var spawnPosition = transform.ValueRO.Position;

            var minDetectPosition = spawnPosition - attackData.ValueRO.DetectionSize;
            var maxDetectPosition = spawnPosition + attackData.ValueRO.DetectionSize;

            var aabbInput = new OverlapAabbInput
            {
                Aabb = new Aabb
                {
                    Min = minDetectPosition,
                    Max = maxDetectPosition
                },
                Filter = attackData.ValueRO.CollisionFilter
            };

            var overlapHits = new NativeList<int>(state.WorldUpdateAllocator);

            if (!physicsWorldSingleton.OverlapAabb(aabbInput, ref overlapHits))
                continue;

            float maxDistanceSquared = float.MaxValue;
            float3 closestEnemyPosition = float3.zero;

            for (int i = 0; i < overlapHits.Length; i++)
            {
                int overlapHit = overlapHits[i];

                var currentEnemyPosition =
                    physicsWorldSingleton.Bodies[overlapHit].WorldFromBody.pos;

                float distanceToPlayerSquared =
                    math.distancesq(spawnPosition.xy, currentEnemyPosition.xy);

                if (distanceToPlayerSquared >= maxDistanceSquared)
                    continue;

                maxDistanceSquared = distanceToPlayerSquared;
                closestEnemyPosition = currentEnemyPosition;
            }

            var vectorToClosestEnemy = closestEnemyPosition - spawnPosition;
            var angleToClosestEnemy =
                math.atan2(vectorToClosestEnemy.y, vectorToClosestEnemy.x);

            var spawnOrientation =
                quaternion.Euler(0f, 0f, angleToClosestEnemy);

            var newAttack =
                ecb.Instantiate(attackData.ValueRO.AttackPrefab);

            AudioController.Instance.PlayShoot();

            ecb.SetComponent(
                newAttack,
                LocalTransform.FromPositionRotation(spawnPosition, spawnOrientation)
            );

            var projectileData =
                SystemAPI.GetComponent<PlasmaBlastData>(attackData.ValueRO.AttackPrefab);

            projectileData.Owner = entity;

            float attackSpeedMultiplier = 1f;

            if (SystemAPI.HasComponent<PlayerResolvedStats>(entity))
            {
                var stats = SystemAPI.GetComponent<PlayerResolvedStats>(entity);

                projectileData.MoveSpeed +=
                    stats.MoveSpeedBonus * projectileData.PlayerMoveSpeedCoefficient;

                attackSpeedMultiplier +=
                    math.max(0f, stats.AttackSpeed);
            }

            ecb.SetComponent(newAttack, projectileData);

            expirationTimestamp.ValueRW.Value =
                elapsedTime + attackData.ValueRO.CooldownTime / attackSpeedMultiplier;
        }
    }
}
