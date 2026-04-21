using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

public class CritChanceBuff : Buff
{
    public override EquipmentType[] Type => new EquipmentType[] { EquipmentType.Weapon, EquipmentType.Accessory };

    public override float MinValue => -15;

    public override float MaxValue => 15;

    public override string Description => $"CritChance {(Value >= 0 ? "+" : "-")}{Math.Abs(Value)}%";

    public override void Apply(Entity playerEntity)
    {
        PlayerStatModifierUtility.TryAddModifier(playerEntity, PlayerStatType.CritChance, addValue: Value);
    }
}
