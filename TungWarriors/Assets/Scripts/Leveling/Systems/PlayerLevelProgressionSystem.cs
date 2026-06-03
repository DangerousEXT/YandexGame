using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct PlayerLevelProgressionSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (level, experience, availableCards, entity) in
                 SystemAPI.Query<RefRW<PlayerLevel>, RefRW<PlayerExperience>,
                         DynamicBuffer<AvailableLevelUpCardElement>>()
                        .WithAll<PlayerTag>()
                        .WithEntityAccess())
        {
            if (SystemAPI.IsComponentEnabled<ShowLevelUpSelectionFlag>(entity))
                continue;
            if (experience.ValueRO.Current < experience.ValueRO.RequiredForNext)
                continue;
            var newLevel = level.ValueRO.Value + 1;
            experience.ValueRW.Current -= experience.ValueRO.RequiredForNext;
            level.ValueRW.Value = newLevel;
            experience.ValueRW.RequiredForNext = 10 + (newLevel * 2) + ((newLevel * newLevel) / 4);
            if (availableCards.Length > 0)
            {
                SystemAPI.SetComponentEnabled<ShowLevelUpSelectionFlag>(entity, true);
            }
        }
    }
}