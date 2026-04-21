using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

public class AddSpeedBuff : Buff
{
    public override EquipmentType[] Type => new EquipmentType[] { EquipmentType.Weapon, EquipmentType.Accessory, EquipmentType.Armor };

    public override float MinValue => -1;

    public override float MaxValue => 1;

    public override string Description => $"Speed {(Value >= 0 ? "+" : "-")}{Math.Abs(Value)}";

    public override void Apply(Entity playerEntity)
    {
        PlayerStatModifierUtility.TryAddModifier(playerEntity, PlayerStatType.MoveSpeedBonus, addValue: Value);
    }
}
