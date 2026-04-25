using Unity.Entities;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class DamageUpSkill : Skill
{
/*stats*/    public override string Name => "Банка брейн-колы";

    public override string Description => "Увеличивает вашу силу брейн-жижей!";

    public override int MaxLevel => 5;

    public override int CurrentLevel { get => throw new System.NotImplementedException(); protected set => throw new System.NotImplementedException(); }

    public override int CostToLevel => 100;

    public override float CostProgression => 1.2f;

    public override SkillType Type => SkillType.Passive;

    public override PassiveSkillType? PassiveType => PassiveSkillType.DamageUp;

    public override UpgradeableStat? UpgradeableStat => global::UpgradeableStat.Base;

    public float DamageIncreaseStartValue = 1f;

    public float DamageIncreaseCoefficient = 1f;
    public override void ApplyEffect(Entity playerEntity)
    {
        SkillsStats.getInstance().SetBaseDamage(DamageIncreaseStartValue + DamageIncreaseCoefficient * (CurrentLevel - 1));
    }
}
