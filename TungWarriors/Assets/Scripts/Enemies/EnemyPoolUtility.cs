using Assets.Scripts.DeathConsequencesSystems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public static class EnemyPoolUtility
{
    public static void ReturnAllActiveEnemiesToPool(EntityManager entityManager)
    {
        var enemyQuery = entityManager.CreateEntityQuery(
            ComponentType.ReadOnly<EnemyTag>(),
            ComponentType.ReadOnly<EnemyActiveFlag>());

        using var activeEnemies = enemyQuery.ToEntityArray(Allocator.Temp);
        foreach (var enemyEntity in activeEnemies)
            ReturnEnemyToPool(entityManager, enemyEntity);

        enemyQuery.Dispose();
    }

    public static bool ReturnEnemyToPool(EntityManager entityManager, Entity enemyEntity)
    {
        if (!entityManager.Exists(enemyEntity) || !entityManager.HasComponent<EnemyTag>(enemyEntity))
            return false;

        if (entityManager.HasComponent<EnemyActiveFlag>(enemyEntity))
            entityManager.SetComponentEnabled<EnemyActiveFlag>(enemyEntity, false);

        if (entityManager.HasComponent<EnemyPoolOwner>(enemyEntity))
        {
            var spawnerEntity = entityManager.GetComponentData<EnemyPoolOwner>(enemyEntity).Spawner;
            if (entityManager.Exists(spawnerEntity))
            {
                if (entityManager.HasComponent<EnemySpawnState>(spawnerEntity))
                {
                    var spawnState = entityManager.GetComponentData<EnemySpawnState>(spawnerEntity);
                    if (spawnState.CurrentSpawnedEnemies > 0)
                    {
                        spawnState.CurrentSpawnedEnemies--;
                        entityManager.SetComponentData(spawnerEntity, spawnState);
                    }
                }

                if (entityManager.HasBuffer<EnemyPoolElement>(spawnerEntity))
                {
                    entityManager.GetBuffer<EnemyPoolElement>(spawnerEntity).Add(new EnemyPoolElement
                    {
                        Value = enemyEntity
                    });
                }
            }
        }

        if (entityManager.HasComponent<CharacterMaxHitPoints>(enemyEntity) &&
            entityManager.HasComponent<CharacterCurrentHitPoints>(enemyEntity))
        {
            var maxHitPoints = entityManager.GetComponentData<CharacterMaxHitPoints>(enemyEntity);
            entityManager.SetComponentData(enemyEntity, new CharacterCurrentHitPoints
            {
                Value = maxHitPoints.Value
            });
        }

        if (entityManager.HasComponent<CharacterMoveDirection>(enemyEntity))
        {
            entityManager.SetComponentData(enemyEntity, new CharacterMoveDirection
            {
                Value = float2.zero
            });
        }

        if (entityManager.HasComponent<PhysicsVelocity>(enemyEntity))
            entityManager.SetComponentData(enemyEntity, default(PhysicsVelocity));

        if (entityManager.HasBuffer<DamageThisFrame>(enemyEntity))
            entityManager.GetBuffer<DamageThisFrame>(enemyEntity).Clear();

        if (entityManager.HasComponent<EnemyCooldownExpirationTimestamp>(enemyEntity))
        {
            entityManager.SetComponentData(enemyEntity, new EnemyCooldownExpirationTimestamp
            {
                value = 0d
            });
            entityManager.SetComponentEnabled<EnemyCooldownExpirationTimestamp>(enemyEntity, false);
        }

        if (entityManager.HasComponent<DeathEntityFlag>(enemyEntity))
            entityManager.SetComponentEnabled<DeathEntityFlag>(enemyEntity, false);

        if (entityManager.HasComponent<DestroyEntityFlag>(enemyEntity))
            entityManager.SetComponentEnabled<DestroyEntityFlag>(enemyEntity, false);

        entityManager.SetEnabled(enemyEntity, false);
        return true;
    }
}
