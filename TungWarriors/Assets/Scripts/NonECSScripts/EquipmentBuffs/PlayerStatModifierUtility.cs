using Unity.Entities;
using UnityEngine;

public static class PlayerStatModifierUtility
{
    public static bool TryAddModifier(Entity playerEntity, PlayerStatType statType, float addValue = 0f, float mulValue = 0f)
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null || !world.EntityManager.Exists(playerEntity))
            return false;

        var entityManager = world.EntityManager;
        if (!entityManager.HasBuffer<PlayerStatModifier>(playerEntity))
        {
            Debug.LogWarning($"PlayerStatModifier buffer not found on entity {playerEntity}");
            return false;
        }

        var modifiers = entityManager.GetBuffer<PlayerStatModifier>(playerEntity);
        modifiers.Add(new PlayerStatModifier
        {
            Type = statType,
            AddValue = addValue,
            MulValue = mulValue
        });
        return true;
    }

    public static float MultiplierToMulDelta(float multiplier) => multiplier - 1f;
}
