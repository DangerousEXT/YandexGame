using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct ShowLevelUpSelectionSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (GameUIController.Instance == null)
            return;

        var entityManager = state.EntityManager;
        foreach (var (randomState, availableCards, offeredCards, playerUpgradeProgress, entity) in
                 SystemAPI.Query<RefRW<PlayerCardRandom>,
                         DynamicBuffer<AvailableLevelUpCardElement>,
                         DynamicBuffer<OfferedLevelUpCardElement>,
                         DynamicBuffer<PlayerUpgradeProgressElement>>()
                     .WithAll<PlayerTag>()
                     .WithEntityAccess())
        {
            if (!SystemAPI.IsComponentEnabled<ShowLevelUpSelectionFlag>(entity))
                continue;

            SystemAPI.SetComponentEnabled<ShowLevelUpSelectionFlag>(entity, false);

            offeredCards.Clear();
            var eligibleCards = new NativeList<Entity>(availableCards.Length, Allocator.Temp);
            var eligibleWeights = new NativeList<int>(availableCards.Length, Allocator.Temp);

            for (int i = 0; i < availableCards.Length; i++)
            {
                var cardEntity = availableCards[i].Value;
                if (!entityManager.Exists(cardEntity))
                    continue;
                if (!IsCardEligible(entityManager, cardEntity, playerUpgradeProgress))
                    continue;

                var weight = 1;
                if (entityManager.HasComponent<LevelUpCardUpgradeTrack>(cardEntity))
                {
                    weight = math.max(1, entityManager.GetComponentData<LevelUpCardUpgradeTrack>(cardEntity).OfferWeight);
                }

                eligibleCards.Add(cardEntity);
                eligibleWeights.Add(weight);
            }

            if (eligibleCards.Length == 0)
            {
                eligibleCards.Dispose();
                eligibleWeights.Dispose();
                continue;
            }

            var random = randomState.ValueRW.Value;
            var offerCount = math.min(3, eligibleCards.Length);

            for (int i = 0; i < offerCount; i++)
            {
                var selectedIndex = GetWeightedIndex(ref random, eligibleWeights);
                offeredCards.Add(new OfferedLevelUpCardElement { Value = eligibleCards[selectedIndex] });
                eligibleCards.RemoveAtSwapBack(selectedIndex);
                eligibleWeights.RemoveAtSwapBack(selectedIndex);
            }

            randomState.ValueRW.Value = random;
            eligibleCards.Dispose();
            eligibleWeights.Dispose();

            var cardsToShow = new List<LevelUpCardViewData>(offeredCards.Length);
            for (int i = 0; i < offeredCards.Length; i++)
            {
                var meta = entityManager.GetComponentData<LevelUpCardMeta>(offeredCards[i].Value);
                cardsToShow.Add(new LevelUpCardViewData(
                    offeredCards[i].Value,
                    meta.Title.ToString(),
                    meta.Description.ToString(),
                    meta.Icon.Value
                ));
            }

            GameUIController.Instance.ShowLevelUpPanel(cardsToShow);
        }
    }

    private static bool IsCardEligible(EntityManager entityManager, Entity cardEntity, DynamicBuffer<PlayerUpgradeProgressElement> progress)
    {
        if (entityManager.HasComponent<LevelUpCardRequirement>(cardEntity))
        {
            var requirement = entityManager.GetComponentData<LevelUpCardRequirement>(cardEntity);
            if (requirement.RequiredLevel > 0)
            {
                var requirementCurrent = GetPlayerUpgradeLevel(progress, requirement.UpgradeId);
                if (requirementCurrent < requirement.RequiredLevel)
                    return false;
            }
        }

        if (!entityManager.HasComponent<LevelUpCardUpgradeTrack>(cardEntity))
            return true;

        var track = entityManager.GetComponentData<LevelUpCardUpgradeTrack>(cardEntity);
        var currentUpgradeLevel = GetPlayerUpgradeLevel(progress, track.UpgradeId);

        if (currentUpgradeLevel >= track.MaxLevel)
            return false;

        return currentUpgradeLevel + 1 == track.UpgradeLevel;
    }

    private static int GetPlayerUpgradeLevel(DynamicBuffer<PlayerUpgradeProgressElement> progress, FixedString64Bytes upgradeId)
    {
        if (upgradeId.Length == 0)
            return 0;

        for (int i = 0; i < progress.Length; i++)
        {
            if (progress[i].UpgradeId.Equals(upgradeId))
                return progress[i].CurrentLevel;
        }

        return 0;
    }

    private static int GetWeightedIndex(ref Unity.Mathematics.Random random, NativeList<int> weights)
    {
        var totalWeight = 0;
        for (int i = 0; i < weights.Length; i++)
            totalWeight += math.max(1, weights[i]);

        var roll = random.NextInt(0, totalWeight);
        for (int i = 0; i < weights.Length; i++)
        {
            roll -= math.max(1, weights[i]);
            if (roll < 0)
                return i;
        }

        return weights.Length - 1;
    }
}
