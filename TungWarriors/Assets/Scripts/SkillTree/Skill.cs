using Unity.Entities;
using UnityEngine;

public abstract class Skill : MonoBehaviour
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract int MaxLevel { get; }
    public abstract int CurrentLevel { get; protected set; }
    public abstract int CostToLevel { get; }
    public abstract float CostProgression { get; }
    public abstract SkillType Type { get; }
    public abstract PassiveSkillType? PassiveType { get; }
    public abstract UpgradeableStat? UpgradeableStat { get; }

    public abstract void ApplyEffect(Entity playerEntity);
}


public enum SkillType
{
    Active,
    Passive
}

public enum UpgradeableStat
{
    Base,
    Total
}

public enum PassiveSkillType
{
    DamageUp,
    DefenseUp,
    HealthUp,
    SpeedUp
}