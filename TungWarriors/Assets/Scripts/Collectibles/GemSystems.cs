using System.Threading;
using Unity.Entities;
using Unity.Physics;
using ReadOnly = Unity.Collections.ReadOnlyAttribute;

public partial struct CollectGemSystem : ISystem
{
    internal static int s_PendingGold;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GemTag>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var newCollectJob = new CollectGemJob
        {
            GemLookup = SystemAPI.GetComponentLookup<GemTag>(true),
            GemsCollectedLookup = SystemAPI.GetComponentLookup<GemsCollectedCount>(),
            DestroyEntityFlagLookup = SystemAPI.GetComponentLookup<DestroyEntityFlag>(),
            UpdateGemUILookup = SystemAPI.GetComponentLookup<UpdateGemUIFlag>(),
            PlayerExperienceLookup = SystemAPI.GetComponentLookup<PlayerExperience>()
        };

        var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
        state.Dependency = newCollectJob.Schedule(simulationSingleton, state.Dependency);
        state.Dependency.Complete();
        if (s_PendingGold != 0 && PlayerData.Instance != null)
        {
            AudioController.Instance.PlayExperiencePickup();
            PlayerData.Instance.Gold += s_PendingGold;
            s_PendingGold = 0;
        }
    }
}


public struct CollectGemJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<GemTag> GemLookup;
    public ComponentLookup<GemsCollectedCount> GemsCollectedLookup;
    public ComponentLookup<DestroyEntityFlag> DestroyEntityFlagLookup;
    public ComponentLookup<UpdateGemUIFlag> UpdateGemUILookup;
    public ComponentLookup<PlayerExperience> PlayerExperienceLookup;
    public void Execute(TriggerEvent triggerEvent)
    {
        Entity gemEntity;
        Entity playerEntity;

        if (GemLookup.HasComponent(triggerEvent.EntityA) && GemsCollectedLookup.HasComponent(triggerEvent.EntityB))
        {
            gemEntity = triggerEvent.EntityA;
            playerEntity = triggerEvent.EntityB;
        }
        else if (GemLookup.HasComponent(triggerEvent.EntityB) && GemsCollectedLookup.HasComponent(triggerEvent.EntityA))
        {
            gemEntity = triggerEvent.EntityB;
            playerEntity = triggerEvent.EntityA;
        }
        else
        {
            return;
        }

        var gemsCollected = GemsCollectedLookup[playerEntity];
        gemsCollected.Value += 1;
        Interlocked.Increment(ref CollectGemSystem.s_PendingGold);
        GemsCollectedLookup[playerEntity] = gemsCollected;
        if (PlayerExperienceLookup.HasComponent(playerEntity))
        {
            var experience = PlayerExperienceLookup[playerEntity];
            experience.Current += 1;
            PlayerExperienceLookup[playerEntity] = experience;
        }
        UpdateGemUILookup.SetComponentEnabled(playerEntity, true);
        DestroyEntityFlagLookup.SetComponentEnabled(gemEntity, true);
    }
}