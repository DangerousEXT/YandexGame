using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

public class MulDamageBuff : Buff
{
    public override float MinValue => 0;

    public override float MaxValue => (float)1.5;

    public override EquipmentType[] Type => new EquipmentType[] { EquipmentType.Weapon, EquipmentType.Accessory };

    public override string Id => "mul_damage_buff_desc";

    public override void Apply(Entity playerEntity)
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null || !world.EntityManager.Exists(playerEntity)) return;
        if (world.EntityManager.HasComponent<EquipmentStats>(playerEntity))
        {

            var stats = world.EntityManager.GetComponentData<EquipmentStats>(playerEntity);
            stats.DamageValueMultiplicator = (1 + Value);
            world.EntityManager.SetComponentData(playerEntity, stats);
            Debug.Log($"Applied {Description} to player. MULTIPLICATOR DAMAGE");
        }
        else
        {
            Debug.LogWarning($"PlayerStats not found on entity {playerEntity}");
        }
    }
}