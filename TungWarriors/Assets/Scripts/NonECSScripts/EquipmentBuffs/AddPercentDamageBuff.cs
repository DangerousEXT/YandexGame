using System;
using Unity.Entities;
using UnityEngine;

public class AddPercentDamageBuff : Buff
{
    public override EquipmentType[] Type => new EquipmentType[] { EquipmentType.Accessory, EquipmentType.Armor };

    public override float MinValue => -1;

    public override float MaxValue => 1;

    public override string Description => $"Damage {(Value >= 0 ? "+" : "-")}{Math.Abs(Value * 100)}%";

    public override void Apply(Entity playerEntity)
    {
        PlayerStatModifierUtility.TryAddModifier(playerEntity, PlayerStatType.Damage, mulValue: Value);
    }
}
