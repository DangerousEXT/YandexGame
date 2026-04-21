using Unity.Entities;

public struct BatWeaponData : IComponentData
{
    public float PlayerDamageCoefficient;
    public float CritChanceCoefficient;
    public float CritDamageCoefficient;
    public float Cooldown;
    public float Range;
    public float ConeAngleDegrees;
    public Entity AttackPrefab;
    public bool HasSpawned;
}

public struct BatWeaponCooldown : IComponentData
{
    public double NextAttackTime;
}
public struct BatOrbitData : IComponentData
{
    public Entity Owner;
    public float Radius;
    public float AngularSpeed;
    public float CurrentAngle;
    public int Damage;
}
