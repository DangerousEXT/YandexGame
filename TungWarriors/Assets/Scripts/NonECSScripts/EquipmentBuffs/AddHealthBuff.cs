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

    public override string Description => $"Health {(Value >= 0 ? "+" : "-")}{Math.Abs(Value)}";

    public override void Apply(Entity playerEntity)
    {
        PlayerStatModifierUtility.TryAddModifier(playerEntity, PlayerStatType.MaxHitPoints, addValue: Value);
    }
}
