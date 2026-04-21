using System;
using Unity.Entities;
using UnityEngine;

public class MulHealthBuff : Buff
{
    public override EquipmentType[] Type => new EquipmentType[] { EquipmentType.Accessory, EquipmentType.Armor };

    public override float MinValue => 0;

    public override float MaxValue => 2;

    public override string Description => $"Health *{Math.Abs(Value * 100)}";

    public override void Apply(Entity playerEntity)
    {
        PlayerStatModifierUtility.TryAddModifier(
            playerEntity,
            PlayerStatType.MaxHitPoints,
            mulValue: PlayerStatModifierUtility.MultiplierToMulDelta(Value));
    }
}
