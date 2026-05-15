using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

public class AddHealthBuff : Buff
{
    public override EquipmentType[] Type => new EquipmentType[] { EquipmentType.Accessory, EquipmentType.Armor};

    public override float MinValue => -1000;

    public override float MaxValue => 1000;

    public override string Id => "add_health_buff_desc";

    public override void Apply(Entity playerEntity)
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null || !world.EntityManager.Exists(playerEntity)) return;

        if (world.EntityManager.HasComponent<EquipmentStats>(playerEntity))
        {
            var stats = world.EntityManager.GetComponentData<EquipmentStats>(playerEntity);
            stats.Health += Value;
            world.EntityManager.SetComponentData(playerEntity, stats);
        }
        else
        {
            Debug.LogWarning($"PlayerStats not found on entity {playerEntity}");
        }
    }
}
