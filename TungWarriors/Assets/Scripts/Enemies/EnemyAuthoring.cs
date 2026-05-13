using UnityEngine;
using Unity.Entities;

[RequireComponent(typeof(CharacterAuthoring))]
public partial class EnemyAuthoring : MonoBehaviour
{
    public int attackDamage;
    public float attackCooldownTime;
    public GameObject GemPrefab;
   private class Baker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring enemyAuthoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<EnemyTag>(entity);
            AddComponent<EnemyActiveFlag>(entity);
            SetComponentEnabled<EnemyActiveFlag>(entity, true);
            AddComponent(entity, new EnemyAttackData() { CooldownTime = enemyAuthoring.attackCooldownTime, HitPoints = enemyAuthoring.attackDamage });
            AddComponent<EnemyCooldownExpirationTimestamp>(entity);
            SetComponentEnabled<EnemyCooldownExpirationTimestamp>(entity, false);
            AddComponent(entity, new GemPrefab
            {
                Value = GetEntity(enemyAuthoring.GemPrefab, TransformUsageFlags.Dynamic)
            });
        }   
   }
}
