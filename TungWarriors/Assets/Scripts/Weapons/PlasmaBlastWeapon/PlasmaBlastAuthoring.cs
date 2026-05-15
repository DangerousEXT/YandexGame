using UnityEngine;
using Unity.Entities;

public class PlasmaBlastAuthoring : MonoBehaviour
{
    public float MoveSpeed;
    public int AttackDamage;
    public float PlayerDamageCoefficient = 1f;
    public float PlayerMoveSpeedCoefficient = 0.1f;
    public float CritChanceCoefficient = 1f;
    public float CritDamageCoefficient = 1f;

    private class Baker : Baker<PlasmaBlastAuthoring>
    {
        public override void Bake(PlasmaBlastAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlasmaBlastData
            {
                MoveSpeed = authoring.MoveSpeed,
                AttackDamage = authoring.AttackDamage,
                PlayerDamageCoefficient = authoring.PlayerDamageCoefficient,
                PlayerMoveSpeedCoefficient = authoring.PlayerMoveSpeedCoefficient,
                CritChanceCoefficient = authoring.CritChanceCoefficient,
                CritDamageCoefficient = authoring.CritDamageCoefficient
            });
            AddComponent<DestroyEntityFlag>(entity);
            SetComponentEnabled<DestroyEntityFlag>(entity, false);
        }
    }
}
