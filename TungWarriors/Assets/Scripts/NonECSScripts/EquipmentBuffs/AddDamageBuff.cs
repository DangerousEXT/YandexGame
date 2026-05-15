using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

public class AddDamageBuff : Buff
{
    public override float MinValue => -50;

    public override float MaxValue => 50;

    public override EquipmentType[] Type => new EquipmentType[] { EquipmentType.Weapon, EquipmentType.Accessory };

    public override string Id => "add_damage_buff_desc";

    public override void Apply(Entity playerEntity)
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null || !world.EntityManager.Exists(playerEntity)) return;

        if (world.EntityManager.HasComponent<EquipmentStats>(playerEntity))
        {
            var stats = world.EntityManager.GetComponentData<EquipmentStats>(playerEntity);
            stats.Damage += Value;
            world.EntityManager.SetComponentData(playerEntity, stats);
        }
        else
        {
            Debug.LogWarning($"PlayerStats not found on entity {playerEntity}");
        }
    }
}
