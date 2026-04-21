using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public struct PlasmaBlastData : IComponentData
{
    public float MoveSpeed;
    public int AttackDamage;
    public float PlayerDamageCoefficient;
    public float PlayerMoveSpeedCoefficient;
    public float CritChanceCoefficient;
    public float CritDamageCoefficient;
}

public struct PlasmaBlastWeaponData : IComponentData
{
    public Entity AttackPrefab;
    public float CooldownTime;
    public float3 DetectionSize;
    public CollisionFilter CollisionFilter;
}

public struct PlasmaBlastWeaponCooldown : IComponentData
{
    public double Value;
}
