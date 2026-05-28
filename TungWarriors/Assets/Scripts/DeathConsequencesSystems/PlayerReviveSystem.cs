using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.DeathConsequencesSystems
{
    /// <summary>
    /// Keeps the player alive while the revive decision is pending.
    /// </summary>
    public struct PlayerThinkingFlag : IEnableableComponent, IComponentData { }

    [UpdateInGroup(typeof(DeathConsequencesGroup))]
    public partial struct PlayerReviveSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var shouldClearActiveEnemies = false;

            foreach (var (revivesCount, currentHealth, maxHealth, entity) in
                     SystemAPI.Query<RefRW<RevivePlayerCount>, RefRW<CharacterCurrentHitPoints>, RefRW<CharacterMaxHitPoints>>()
                         .WithAll<DeathEntityFlag>()
                         .WithAll<PlayerTag>()
                         .WithEntityAccess())
            {
                if (revivesCount.ValueRW.Value > 0)
                {
                    revivesCount.ValueRW.Value--;
                    currentHealth.ValueRW.Value = maxHealth.ValueRW.Value;
                    SystemAPI.SetComponentEnabled<DeathEntityFlag>(entity, false);
                    shouldClearActiveEnemies = true;
                }
                else if (!revivesCount.ValueRW.IsAdvUsed)
                {
                    Debug.Log("Start revive");
                    revivesCount.ValueRW.IsAdvUsed = true;
                    GameUIController.Instance.SwitchDeathPanel();
                }
                else if (!SystemAPI.IsComponentEnabled<PlayerThinkingFlag>(entity))
                {
                    Debug.Log("Start Destroy");
                    SystemAPI.SetComponentEnabled<DestroyEntityFlag>(entity, true);
                }
            }

            if (shouldClearActiveEnemies)
                EnemyPoolUtility.ReturnAllActiveEnemiesToPool(state.EntityManager);
        }
    }
}
