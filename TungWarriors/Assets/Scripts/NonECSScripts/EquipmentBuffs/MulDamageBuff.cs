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

    public override string Description => $"Damage *{Math.Abs(Value * 100)}";

    public override void Apply(Entity playerEntity)
    {
        PlayerStatModifierUtility.TryAddModifier(
            playerEntity,
            PlayerStatType.Damage,
            mulValue: PlayerStatModifierUtility.MultiplierToMulDelta(Value));
    }
}
