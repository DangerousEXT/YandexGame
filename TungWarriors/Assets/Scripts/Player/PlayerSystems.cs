using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using Unity.Collections;
using System.Linq;
using Unity.VisualScripting;

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
[UpdateBefore(typeof(ResolvePlayerStatsSystem))]
public partial struct ApplyPlayerStatOperationsSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (operations, modifiers) in
                 SystemAPI.Query<DynamicBuffer<PlayerStatOperationElement>, DynamicBuffer<PlayerStatModifier>>()
                     .WithAll<PlayerTag>())
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
            var playerMaxHP = SystemAPI.GetComponentRW<CharacterMaxHitPoints>(entity);
            var playerCurrentHP = SystemAPI.GetComponentRW<CharacterCurrentHitPoints>(entity);
            var playerDamageBonus = SystemAPI.GetComponentRW<PlayerDamageBonus>(entity);
            var playerMoveSpeedBonus = SystemAPI.GetComponentRW<CharacterMoveSpeedBonus>(entity);
            var playerDefense = SystemAPI.GetComponentRW<CharacterDefense>(entity);
            var playerHealthRegen = SystemAPI.GetComponentRW<CharacterHealthRegen>(entity);

            float damageAdd = 0f;
            float moveSpeedAdd = 0f;
            float defenseAdd = 0f;
            float healthRegenAdd = 0f;
            float critChanceAdd = 0f;
            float critDamageAdd = 0f;
            float maxHitPointsAdd = 0f;

            float damageMul = 0f;
            float moveSpeedMul = 0f;
            float defenseMul = 0f;
            float healthRegenMul = 0f;
            float critChanceMul = 0f;
            float critDamageMul = 0f;
            float maxHitPointsMul = 0f;
            float attackSpeedAdd = 0f;
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
            
            playerMaxHP.ValueRW.Value =
                (baseStats.MaxHitPoints + equipmentStats.Health + maxHitPointsAdd) *
                (equipmentStats.HealthPercentageMultiplicator + 1f) *
                (equipmentStats.HealthValueMultiplicator + 1f) *
                (1f + math.max(0f, maxHitPointsMul));
            playerCurrentHP.ValueRW.Value = math.min(playerCurrentHP.ValueRO.Value, playerMaxHP.ValueRO.Value);

            resolvedStats.ValueRW.Damage =
                (baseStats.Damage + equipmentStats.Damage + damageAdd) *
                (equipmentStats.DamageValueMultiplicator + 1f) *
                (equipmentStats.DamagePercentageMultiplicator + 1f) *
                (1f + math.max(0f, damageMul));
            resolvedStats.ValueRW.MoveSpeedBonus = (equipmentStats.Speed + moveSpeedAdd) * (1f + math.max(0f, moveSpeedMul));
            resolvedStats.ValueRW.Defense = (int)math.max(0f, defenseAdd * (1f + math.max(0f, defenseMul)));
            resolvedStats.ValueRW.HealthRegen = math.max(0f, healthRegenAdd * (1f + math.max(0f, healthRegenMul)));
            resolvedStats.ValueRW.CritChance = math.max(0f, critChanceAdd * (1f + math.max(0f, critChanceMul)));
            resolvedStats.ValueRW.CritDamage = math.max(0f, critDamageAdd * (1f + math.max(0f, critDamageMul)));
            resolvedStats.ValueRW.AttackSpeed = math.max(0f, attackSpeedAdd + attackSpeedMul);
            resolvedStats.ValueRW.MaxHitPoints = playerMaxHP.ValueRO.Value;

            playerDamageBonus.ValueRW.Value = (int)math.max(0f, damageAdd);
            playerMoveSpeedBonus.ValueRW.Value = resolvedStats.ValueRO.MoveSpeedBonus;
            playerDefense.ValueRW.Value = resolvedStats.ValueRO.Defense;
            playerHealthRegen.ValueRW.ValuePerSecond = resolvedStats.ValueRO.HealthRegen;


            //Debug.Log($"base stat: {baseStats.MaxHitPoints}, equip flat: {equipmentStats.Health}, equip %: {equipmentStats.HealthPercentageMultiplicator}, equip value mul: {equipmentStats.HealthValueMultiplicator}, final max HP: {playerMaxHP.ValueRO.Value}");

            Debug.Log(entity.Index);
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

                var projectileData = SystemAPI.GetComponent<PlasmaBlastData>(attackData.AttackPrefab);
                projectileData.Owner = entity;

                if (SystemAPI.HasComponent<PlayerResolvedStats>(entity))
                {
                    var stats = SystemAPI.GetComponent<PlayerResolvedStats>(entity);
                    projectileData.MoveSpeed += stats.MoveSpeedBonus * projectileData.PlayerMoveSpeedCoefficient;
                }

                ecb.SetComponent(newAttack, projectileData);
                var attackSpeedMultiplier = 1f;
                if (SystemAPI.HasComponent<PlayerResolvedStats>(entity))
                {
                    var stats = SystemAPI.GetComponent<PlayerResolvedStats>(entity);
                    attackSpeedMultiplier += math.max(0f, stats.AttackSpeed);
                }

                expirationTimestamp.ValueRW.Value = elapsedTime + (attackData.CooldownTime / attackSpeedMultiplier);
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
            var statsResolvedComponent = SystemAPI.GetComponent<PlayerStatsResolvedAfterMainMenu>(playerEntity);
            if (statsResolvedComponent.HasResolved == false)
            {
                foreach (var equipment in PlayerData.Instance.EquipmentOnPlayer.Values)
                {
                    foreach (var buff in equipment.Buffs)
                    {
                        Debug.Log(buff.Value.ToString() + "BLYAT ITS HERE");
                        buff.Apply(playerEntity);
                    }
                }
                Debug.Log("Buffs Are Applying");
                statsResolvedComponent.HasResolved = true;
                SystemAPI.SetComponent(playerEntity, statsResolvedComponent);
            }
            else
            {
                //елсе нахуй так-то тут не нужен, и без него работает =))))))) Че то я здесь хотел умное высрать
            }
        }
    }
}
