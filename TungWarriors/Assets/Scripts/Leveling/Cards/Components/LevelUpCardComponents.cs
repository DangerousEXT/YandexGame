using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public struct LevelUpCardMeta : IComponentData
{
    public FixedString64Bytes Title;
    public FixedString128Bytes Description;
    public UnityObjectRef<Sprite> Icon;
}

public struct LevelUpCardPoolTag : IComponentData { }

public struct InitialLevelUpCardElement : IBufferElementData
{
    public Entity Value;
}

public struct LevelUpCardUpgradeTrack : IComponentData
{
    public FixedString64Bytes UpgradeId;
    public int UpgradeLevel;
    public int MaxLevel;
    public int OfferWeight;
}

public struct LevelUpCardRequirement : IComponentData
{
    public FixedString64Bytes UpgradeId;
    public int RequiredLevel;
}

public struct CardStatModifierEffectElement : IBufferElementData
{
    public PlayerStatType Type;
    public float AddValue;
    public float MulValue;
}
