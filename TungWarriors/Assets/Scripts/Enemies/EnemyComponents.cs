using Unity.Entities;

public struct EnemyTag : IComponentData { }

public struct EnemyActiveFlag : IComponentData, IEnableableComponent { }

public struct EnemyAttackData : IComponentData
{
    public int HitPoints;
    public float CooldownTime;
}

public struct EnemyCooldownExpirationTimestamp : IComponentData, IEnableableComponent
{
    public double value;
}

public struct GemPrefab : IComponentData
{
    public Entity Value;
}
