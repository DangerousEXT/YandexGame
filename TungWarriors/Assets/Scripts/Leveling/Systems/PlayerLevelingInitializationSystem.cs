using Unity.Entities;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct PlayerLevelingInitializationSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();
        state.RequireForUpdate<LevelUpCardPoolTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var poolEntity = SystemAPI.GetSingletonEntity<LevelUpCardPoolTag>();
        var initialCards = SystemAPI.GetBuffer<InitialLevelUpCardElement>(poolEntity);

        foreach (var (upgradeProgress, availableCards, offeredCards, initFlag) in
                 SystemAPI.Query<DynamicBuffer<PlayerUpgradeProgressElement>,
                         DynamicBuffer<AvailableLevelUpCardElement>,
                         DynamicBuffer<OfferedLevelUpCardElement>,
                         EnabledRefRW<InitializePlayerLevelingFlag>>()
                         .WithAll<PlayerTag>())
        {
            upgradeProgress.Clear();
            availableCards.Clear();
            offeredCards.Clear();
            for (int i = 0; i < initialCards.Length; i++)
            {
                availableCards.Add(new AvailableLevelUpCardElement
                {
                    Value = initialCards[i].Value
                });
            }
            initFlag.ValueRW = false;
        }
    }
}