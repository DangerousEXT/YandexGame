using System;
using Unity.Entities;
using UnityEngine;

public class AddPercentHealthBuff : Buff
{
    public override EquipmentType[] Type => new EquipmentType[] { EquipmentType.Accessory, EquipmentType.Armor };

    public override float MinValue => -1;

    public override float MaxValue => 1;

    public override string Description => $"Health {(Value >= 0 ? "+" : "-")}{Math.Abs(Value * 100)}%";

    public override void Apply(Entity playerEntity)
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null || !world.EntityManager.Exists(playerEntity)) return;

        if (world.EntityManager.HasComponent<EquipmentStats>(playerEntity))
        {

                var stats = world.EntityManager.GetComponentData<EquipmentStats>(playerEntity);
                stats.HealthPercentageMultiplicator = (1 + Value/100);
                world.EntityManager.SetComponentData(playerEntity, stats);
                Debug.Log($"Applied {Description} to player. New MaxHitPoints: {stats.Health * stats.HealthPercentageMultiplicator} PERCENTAGE");
            //var stats = world.EntityManager.GetComponentData<EquipmentStats>(playerEntity);
            //stats.Health += stats.Health * Value;
            //world.EntityManager.SetComponentData(playerEntity, stats);
        }
        else
        {
            Debug.LogWarning($"PlayerStats not found on entity {playerEntity}");
        }
    }
}
