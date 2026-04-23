using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(ProcessDamageThisFrameSystem))]
public partial struct BatWeaponAttackSystem : ISystem
{
    private EntityQuery _enemyQuery;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();

        _enemyQuery = state.GetEntityQuery(
            ComponentType.ReadOnly<EnemyTag>(),
            ComponentType.ReadOnly<LocalTransform>(),
            ComponentType.ReadWrite<DamageThisFrame>());
    }

    public void OnUpdate(ref SystemState state)
    {
        state.EntityManager.CompleteDependencyBeforeRW<DamageThisFrame>();

        if (_enemyQuery.IsEmptyIgnoreFilter)
            return;

        var elapsedTime = SystemAPI.Time.ElapsedTime;
        var enemyEntities = _enemyQuery.ToEntityArray(state.WorldUpdateAllocator);
        var enemyTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
        var damageBufferLookup = SystemAPI.GetBufferLookup<DamageThisFrame>();

        foreach (var (weapon, cooldown, transform, lastDirection, resolvedStats) in
                 SystemAPI.Query<BatWeaponData, RefRW<BatWeaponCooldown>, LocalTransform,
                         LastNonZeroMoveDirection, PlayerResolvedStats>()
                     .WithAll<PlayerTag>())
        {
            if (cooldown.ValueRO.NextAttackTime > elapsedTime)
                continue;

            var attackSpeedMultiplier = 1f + math.max(0f, resolvedStats.AttackSpeed);
            cooldown.ValueRW.NextAttackTime = elapsedTime + (weapon.Cooldown / attackSpeedMultiplier);

            var forward = lastDirection.Value;
            if (math.lengthsq(forward) < 0.0001f)
                forward = new float2(1f, 0f);
            else
                forward = math.normalize(forward);

            var rangeSquared = weapon.Range * weapon.Range;
            var cosHalfCone = math.cos(math.radians(weapon.ConeAngleDegrees * 0.5f));
            var totalDamage = CalculateScaledDamage(0, resolvedStats.Damage, resolvedStats.CritChance, resolvedStats.CritDamage,
                weapon.PlayerDamageCoefficient, weapon.CritChanceCoefficient, weapon.CritDamageCoefficient);

            foreach (var enemyEntity in enemyEntities)
            {
                var toEnemy = enemyTransformLookup[enemyEntity].Position.xy - transform.Position.xy;
                var distSq = math.lengthsq(toEnemy);

                if (distSq > rangeSquared)
                    continue;

                if (distSq > 0.0001f)
                {
                    if (math.dot(forward, math.normalize(toEnemy)) < cosHalfCone)
                        continue;
                }

                damageBufferLookup[enemyEntity].Add(new DamageThisFrame
                {
                    Value = totalDamage
                });
            }
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
