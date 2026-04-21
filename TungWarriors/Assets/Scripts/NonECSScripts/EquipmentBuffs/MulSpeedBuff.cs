using System;
using Unity.Entities;
using UnityEngine;

public class MulSpeedBuff : Buff
{
    public override EquipmentType[] Type => new EquipmentType[] { EquipmentType.Accessory, EquipmentType.Armor };

    public override float MinValue => 0;

    public override float MaxValue => 2;

    public override string Description => $"Speed *{Math.Abs(Value * 100)}";

    public override void Apply(Entity playerEntity)
    {
        PlayerStatModifierUtility.TryAddModifier(
            playerEntity,
            PlayerStatType.MoveSpeedBonus,
            mulValue: PlayerStatModifierUtility.MultiplierToMulDelta(Value));
    }
}
