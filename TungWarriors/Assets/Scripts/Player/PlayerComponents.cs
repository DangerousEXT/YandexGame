using Unity.Entities;

public struct EquipmentStats : IComponentData
{
    public float Damage;
    public float Speed;
    public float Health;
    public float CritChance;
    public float CritDamage;
    public float HealthValueMultiplicator;
    public float HealthPercentageMultiplicator;
}

public struct PlayerBaseStats : IComponentData
{
    public float MoveSpeed;
    public float MaxHitPoints;
}

public struct PlayerResolvedStats : IComponentData
{
    public float Damage;
    public float MoveSpeedBonus;
    public int Defense;
    public float HealthRegen;
    public float CritChance;
    public float CritDamage;
    public float MaxHitPoints;
}

public struct InitializePlayerStatsFlag : IComponentData, IEnableableComponent { }

public enum PlayerStatType : byte
{
    Damage = 0,
    MoveSpeedBonus = 1,
    Defense = 2,
    HealthRegen = 3,
    CritChance = 4,
    CritDamage = 5,
    MaxHitPoints = 6
}

public struct PlayerStatModifier : IBufferElementData
{
    public PlayerStatType Type;
    public float AddValue;
    public float MulValue;
}

public struct RevivePlayerCount : IComponentData
{
    public int Value;
    public bool IsAdvUsed;
}

public struct PlayerTag : IComponentData { }
