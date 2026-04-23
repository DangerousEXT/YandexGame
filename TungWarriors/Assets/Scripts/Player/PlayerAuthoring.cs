using Assets.Scripts.DeathConsequencesSystems;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Physics;

public class PlayerAuthoring : MonoBehaviour
{
    public GameObject AttackPrefab;
    public float CooldownTime;
    public float DetectionSize;
    public GameObject WorldUiPrefab;
    public float BaseDamage;
    public float BaseMoveSpeed;
    public float BaseHealth;
    private class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<PlayerTag>(entity);
            AddComponent<InitializeCameraTargetTag>(entity);
            AddComponent<CameraTarget>(entity);

            var EnemyLayer = LayerMask.NameToLayer("Enemy");
            var EnemyLayerMask = (uint)math.pow(2, EnemyLayer);

            var attackCollisionFilter = new CollisionFilter
            {
                BelongsTo = uint.MaxValue,
                CollidesWith = EnemyLayerMask
            };

            AddComponent(entity, new RevivePlayerCount()
            {
                Value = 1,
                IsAdvUsed = false
            });

            AddComponent<PlayerThinkingFlag>(entity);
            SetComponentEnabled<PlayerThinkingFlag>(entity, false);

            AddComponent(entity, new PlasmaBlastWeaponData()
            {
                AttackPrefab = GetEntity(authoring.AttackPrefab, TransformUsageFlags.Dynamic),
                CooldownTime = authoring.CooldownTime,
                DetectionSize = new float3(authoring.DetectionSize),
                CollisionFilter = attackCollisionFilter
            });

            AddComponent<PlasmaBlastWeaponCooldown>(entity);
            AddComponent(entity, new GemsCollectedCount { Value = 0 });
            AddComponent<UpdateGemUIFlag>(entity);
            AddComponent(entity, new PlayerWorldUIPrefab()
            {
                Value = authoring.WorldUiPrefab
            });
            AddComponent(entity, new EquipmentStats());
            AddComponent(entity, new PlayerResolvedStats());
            AddBuffer<PlayerStatModifier>(entity);
            AddBuffer<PlayerStatOperationElement>(entity);
            AddComponent<InitializePlayerStatsFlag>(entity);
            SetComponentEnabled<InitializePlayerStatsFlag>(entity, true);
            AddComponent(entity, new PlayerStatsResolvedAfterMainMenu { HasResolved = false });
            AddComponent(entity, new PlayerBaseStats { 
                Damage = authoring.BaseDamage, 
                MoveSpeed = authoring.BaseMoveSpeed, 
                MaxHitPoints = authoring.BaseHealth 
            });

            Debug.Log("Player Creates");
        }
    }
}
