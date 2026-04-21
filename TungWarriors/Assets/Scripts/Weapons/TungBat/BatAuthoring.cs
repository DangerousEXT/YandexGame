using Unity.Entities;
using UnityEngine;

public class BatWeaponAuthoring : MonoBehaviour
{
    public float PlayerDamageCoefficient = 1f;
    public float CritChanceCoefficient = 1f;
    public float CritDamageCoefficient = 1f;
    public float Cooldown;
    public float Range;
    public float ConeAngleDegrees;
    public GameObject attackPrefab;
    public bool HasSpawned;
    private class Baker : Baker<BatWeaponAuthoring>
    {
        public override void Bake(BatWeaponAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new BatWeaponData
            {
                PlayerDamageCoefficient = authoring.PlayerDamageCoefficient,
                CritChanceCoefficient = authoring.CritChanceCoefficient,
                CritDamageCoefficient = authoring.CritDamageCoefficient,
                Cooldown = authoring.Cooldown,
                Range = authoring.Range,
                ConeAngleDegrees = authoring.ConeAngleDegrees,
                AttackPrefab = GetEntity(authoring.attackPrefab, TransformUsageFlags.Dynamic),
                HasSpawned = false
            });

            AddComponent(entity, new BatWeaponCooldown
            {
                NextAttackTime = 0d
            });
            
        }
    }
}
