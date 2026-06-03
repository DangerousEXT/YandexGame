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
                if (!entityManager.Exists(cardEntity)) continue;
                if (!IsCardEligible(entityManager, cardEntity, playerUpgradeProgress)) continue;

                var weight = entityManager.HasComponent<LevelUpCardUpgradeTrack>(cardEntity)
                    ? math.max(1, entityManager.GetComponentData<LevelUpCardUpgradeTrack>(cardEntity).OfferWeight)
                    : 1;

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
                var cardEnt = offeredCards[i].Value;
                var meta = entityManager.GetComponentData<LevelUpCardMeta>(cardEnt);
                var track = entityManager.GetComponentData<LevelUpCardUpgradeTrack>(cardEnt);
                var currentLevel = GetPlayerUpgradeLevel(playerUpgradeProgress, track.UpgradeId);
                var nextLevel = currentLevel + 1;
                var scale = math.pow(1.5f, math.max(0, nextLevel - 1));
                var effectValue = 0f;
                var hasValue = false;
                if (entityManager.HasComponent<CardDamageBonusEffect>(cardEnt))
                {
                    effectValue = math.round(entityManager.GetComponentData<CardDamageBonusEffect>(cardEnt).Value * scale);
                    hasValue = true;
                }
                else if (entityManager.HasComponent<CardDefenseBonusEffect>(cardEnt))
                {
                    effectValue = math.round(entityManager.GetComponentData<CardDefenseBonusEffect>(cardEnt).Value * scale);
                    hasValue = true;
                }
                else if (entityManager.HasComponent<CardHealthRegenEffect>(cardEnt))
                {
                    effectValue = entityManager.GetComponentData<CardHealthRegenEffect>(cardEnt).ValuePerSecond * scale;
                    hasValue = true;
                }
                else if (entityManager.HasComponent<CardMoveSpeedBonusEffect>(cardEnt))
                {
                    effectValue = entityManager.GetComponentData<CardMoveSpeedBonusEffect>(cardEnt).Value * scale;
                    hasValue = true;
                }
                else if (entityManager.HasComponent<CardUnlockBatWeaponEffect>(cardEnt) && nextLevel > 1)
                {
                    effectValue = math.round(2 * scale);
                    hasValue = true;
                }
                cardsToShow.Add(new LevelUpCardViewData(
                    cardEnt,
                    meta.CardId.ToString(),
                    meta.Title.ToString(),
                    meta.Description.ToString(),
                    meta.Icon.Value,
                    nextLevel,
                    effectValue,
                    hasValue
                ));
            }
            GameUIController.Instance.ShowLevelUpPanel(cardsToShow);
        }
    }

    private static bool IsCardEligible(EntityManager entityManager, Entity cardEntity, DynamicBuffer<PlayerUpgradeProgressElement> progress)
    {
        if (entityManager.HasComponent<LevelUpCardRequirement>(cardEntity))
        {
            var req = entityManager.GetComponentData<LevelUpCardRequirement>(cardEntity);
            if (req.RequiredLevel > 0 && GetPlayerUpgradeLevel(progress, req.UpgradeId) < req.RequiredLevel)
                return false;
        }
        if (!entityManager.HasComponent<LevelUpCardUpgradeTrack>(cardEntity))
            return true;
        var track = entityManager.GetComponentData<LevelUpCardUpgradeTrack>(cardEntity);
        var currentLevel = GetPlayerUpgradeLevel(progress, track.UpgradeId);
        return currentLevel < track.MaxLevel;
    }

    private static int GetPlayerUpgradeLevel(DynamicBuffer<PlayerUpgradeProgressElement> progress, FixedString64Bytes upgradeId)
    {
        if (upgradeId.Length == 0) return 0;
        for (int i = 0; i < progress.Length; i++)
            if (progress[i].UpgradeId.Equals(upgradeId)) return progress[i].CurrentLevel;
        return 0;
    }

    private static int GetWeightedIndex(ref Unity.Mathematics.Random random, NativeList<int> weights)
    {
        var totalWeight = 0;
        for (int i = 0; i < weights.Length; i++) totalWeight += weights[i];

        var roll = random.NextInt(0, totalWeight);
        for (int i = 0; i < weights.Length; i++)
        {
            roll -= weights[i];
            if (roll < 0) return i;
        }
        return weights.Length - 1;
    }
}