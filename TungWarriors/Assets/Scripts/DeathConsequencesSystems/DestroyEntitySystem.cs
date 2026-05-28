using Assets.Scripts.DeathConsequencesSystems;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public struct DestroyEntityFlag : IEnableableComponent, IComponentData { }

[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
[UpdateAfter(typeof(DeathConsequencesGroup))]
[UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
public partial struct DestroyEntitySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var endEcbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var endECB = endEcbSystem.CreateCommandBuffer(state.WorldUnmanaged);
        var beginEcbSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var beginECB = beginEcbSystem.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (_, entity) in SystemAPI.Query<DestroyEntityFlag>().WithEntityAccess())
        {
            if (SystemAPI.HasComponent<PlayerTag>(entity))
            {
                // Register run result (updates local best and submits to leaderboard if authorized)
                try
                {
                    // Update local best from current match timer before submitting
                    if (PlayerData.Instance != null && SystemAPI.HasSingleton<MatchTimerState>())
                    {
                        var timerState = SystemAPI.GetSingleton<MatchTimerState>();
                        int ms = Mathf.FloorToInt(timerState.ElapsedSeconds * 1000f);
                        var updated = PlayerData.Instance.TrySetBestSurvivalTimeMilliseconds(ms);
                        Debug.Log($"Player run time: {ms} ms. Best updated: {updated}");
                    }

                    SurvivalLeaderboardService.RegisterCurrentRunResult();
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogWarning($"Failed to register survival run result: {e.Message}");
                }

                GameUIController.Instance.ShowGameOverUI();
            }

            if (SystemAPI.HasComponent<GemPrefab>(entity))
            {
                var gemPrefab = SystemAPI.GetComponent<GemPrefab>(entity).Value;
                var newGem = beginECB.Instantiate(gemPrefab);
                var gemSpawnPosition = SystemAPI.GetComponent<LocalToWorld>(entity).Position;
                beginECB.SetComponent(newGem, LocalTransform.FromPosition(gemSpawnPosition));
            }

            endECB.DestroyEntity(entity);
        }
    }
}
