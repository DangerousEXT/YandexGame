using System;
using Unity.Entities;
using UnityEngine;

public class AddPercentSpeedBuff : Buff
{
    public override EquipmentType[] Type => new EquipmentType[] { EquipmentType.Accessory, EquipmentType.Armor };

    public override float MinValue => -1;

    public override float MaxValue => 1;

    public override string Description => $"Speed {(Value >= 0 ? "+" : "-")}{Math.Abs(Value * 100)}%";

    public override void Apply(Entity playerEntity)
    {
        PlayerStatModifierUtility.TryAddModifier(playerEntity, PlayerStatType.MoveSpeedBonus, mulValue: Value);
    }
}
