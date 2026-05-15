using Assets.Scripts.DeathConsequencesSystems;
using Unity.Entities;
using Unity.Transforms;

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
                GameUIController.Instance.ShowGameOverUI();

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
