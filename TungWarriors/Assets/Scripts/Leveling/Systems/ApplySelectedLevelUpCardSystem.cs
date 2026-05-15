using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct ApplySelectedLevelUpCardSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var entityManager = state.EntityManager;

        Entity playerEntity = Entity.Null;
        Entity selectedCardEntity = Entity.Null;

        foreach (var (selectedCard, availableCards, offeredCards, upgradeProgress, entity) in
                 SystemAPI.Query<RefRO<SelectedLevelUpCard>,
                         DynamicBuffer<AvailableLevelUpCardElement>,
                         DynamicBuffer<OfferedLevelUpCardElement>,
                         DynamicBuffer<PlayerUpgradeProgressElement>>()
                     .WithAll<PlayerTag>()
                     .WithEntityAccess())
        {
            if (!SystemAPI.IsComponentEnabled<SelectedLevelUpCard>(entity))
                continue;

            var selected = selectedCard.ValueRO.Value;

            entityManager.SetComponentData(entity, new SelectedLevelUpCard { Value = Entity.Null });
            entityManager.SetComponentEnabled<SelectedLevelUpCard>(entity, false);

            if (selected == Entity.Null || !Contains(offeredCards, selected))
                continue;
            if (!IsCardEligible(entityManager, selected, upgradeProgress))
                continue;

            playerEntity = entity;
            selectedCardEntity = selected;

            RemoveFromAvailable(availableCards, selected);
            ApplyUpgradeProgress(entityManager, upgradeProgress, selectedCardEntity);
            offeredCards.Clear();
        }

        if (playerEntity == Entity.Null || selectedCardEntity == Entity.Null)
            return;

        ApplyCardEffects(entityManager, playerEntity, selectedCardEntity);

        if (GameUIController.Instance != null)
        {
            GameUIController.Instance.HideLevelUpPanel();
            GameUIController.Instance.TogglePause(false);
        }
    }

    private static bool Contains(DynamicBuffer<OfferedLevelUpCardElement> buffer, Entity e)
    {
        for (int i = 0; i < buffer.Length; i++)
            if (buffer[i].Value == e) return true;
        return false;
    }

    private static void RemoveFromAvailable(DynamicBuffer<AvailableLevelUpCardElement> buffer, Entity e)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i].Value != e) continue;
            buffer.RemoveAt(i);
            return;
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

    private static void ApplyUpgradeProgress(EntityManager entityManager, DynamicBuffer<PlayerUpgradeProgressElement> progress, Entity cardEntity)
    {
        if (!entityManager.HasComponent<LevelUpCardUpgradeTrack>(cardEntity))
            return;

        var track = entityManager.GetComponentData<LevelUpCardUpgradeTrack>(cardEntity);
        if (track.UpgradeId.Length == 0)
            return;

        for (int i = 0; i < progress.Length; i++)
        {
            if (!progress[i].UpgradeId.Equals(track.UpgradeId))
                continue;

            progress[i] = new PlayerUpgradeProgressElement
            {
                UpgradeId = track.UpgradeId,
                CurrentLevel = track.UpgradeLevel
            };
            return;
        }

        progress.Add(new PlayerUpgradeProgressElement
        {
            UpgradeId = track.UpgradeId,
            CurrentLevel = track.UpgradeLevel
        });
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

    private static void ApplyCardEffects(EntityManager em, Entity player, Entity card)
    {
        if (em.HasBuffer<PlayerStatOperationElement>(player))
        {
            var statOperations = em.GetBuffer<PlayerStatOperationElement>(player);

            if (em.HasBuffer<CardStatModifierEffectElement>(card))
            {
                var cardModifiers = em.GetBuffer<CardStatModifierEffectElement>(card);
                for (int i = 0; i < cardModifiers.Length; i++)
                {
                    var modifier = cardModifiers[i];
                    EnqueueModifier(statOperations, modifier.Type, modifier.AddValue, modifier.MulValue);
                }
            }

            if (em.HasComponent<CardDamageBonusEffect>(card))
            {
                EnqueueModifier(statOperations, PlayerStatType.Damage, em.GetComponentData<CardDamageBonusEffect>(card).Value, 0f);
            }

            if (em.HasComponent<CardDefenseBonusEffect>(card))
            {
                EnqueueModifier(statOperations, PlayerStatType.Defense, em.GetComponentData<CardDefenseBonusEffect>(card).Value, 0f);
            }

            if (em.HasComponent<CardHealthRegenEffect>(card))
            {
                EnqueueModifier(statOperations, PlayerStatType.HealthRegen, em.GetComponentData<CardHealthRegenEffect>(card).ValuePerSecond, 0f);
            }

            if (em.HasComponent<CardMoveSpeedBonusEffect>(card))
            {
                EnqueueModifier(statOperations, PlayerStatType.MoveSpeedBonus, em.GetComponentData<CardMoveSpeedBonusEffect>(card).Value, 0f);
            }
        }

        if (em.HasComponent<CardUnlockBatWeaponEffect>(card) && !em.HasComponent<BatWeaponData>(player))
        {
            var effect = em.GetComponentData<CardUnlockBatWeaponEffect>(card);
            var batData = em.GetComponentData<BatWeaponData>(effect.BatPrefab);
            var batCooldown = em.GetComponentData<BatWeaponCooldown>(effect.BatPrefab);

            em.AddComponentData(player, batData);
            em.AddComponentData(player, batCooldown);

            Debug.Log("Bat weapon unlocked");
        }
    }

    private static void EnqueueModifier(DynamicBuffer<PlayerStatOperationElement> statOperations, PlayerStatType type, float addValue, float mulValue)
    {
        statOperations.Add(new PlayerStatOperationElement
        {
            Type = type,
            AddValue = addValue,
            MulValue = mulValue
        });
    }
}
