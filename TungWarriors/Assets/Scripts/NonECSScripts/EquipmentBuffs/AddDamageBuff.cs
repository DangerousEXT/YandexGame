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

    public override string Description => $"Damage {(Value >= 0 ? "+" : "-")}{Math.Abs(Value)}";

    public override void Apply(Entity playerEntity)
    {
        PlayerStatModifierUtility.TryAddModifier(playerEntity, PlayerStatType.Damage, addValue: Value);
    }
}
