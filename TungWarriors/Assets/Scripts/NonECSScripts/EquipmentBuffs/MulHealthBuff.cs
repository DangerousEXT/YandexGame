using System;
using Unity.Entities;
using UnityEngine;

public class MulHealthBuff : Buff
{
    public override EquipmentType[] Type => new EquipmentType[] { EquipmentType.Accessory, EquipmentType.Armor, EquipmentType.Helmet };

    public override float MinValue => 1;

    public override float MaxValue => 1000;

    public override string Id => "mul_health_buff_desc";

    public override void Apply(Entity playerEntity)
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null || !world.EntityManager.Exists(playerEntity)) return;

        if (world.EntityManager.HasComponent<EquipmentStats>(playerEntity))
        {

            var stats = world.EntityManager.GetComponentData<EquipmentStats>(playerEntity);
            stats.HealthValueMultiplicator = (1 + Value);
            world.EntityManager.SetComponentData(playerEntity, stats);
            Debug.Log($"Applied {Description} to player. MULTIPLICATOR");
            //var stats = world.EntityManager.GetComponentData<EquipmentStats>(playerEntity);
            //stats.Health += stats.Health * Value;
            //world.EntityManager.SetComponentData(playerEntity, stats);
        }
        //if (world.EntityManager.HasComponent<EquipmentStats>(playerEntity))
        //{
        //    var stats = world.EntityManager.GetComponentData<EquipmentStats>(playerEntity);
        //    stats.Health *= Value;
        //    world.EntityManager.SetComponentData(playerEntity, stats);
        //}
        else
        {
            Debug.LogWarning($"PlayerStats not found on entity {playerEntity}");
        }
    }
}
