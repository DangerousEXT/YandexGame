using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct ApplySelectedLevelUpCardSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var entityManager = state.EntityManager;
        var playerEntity = Entity.Null;
        var selectedCardEntity = Entity.Null;
        var newLevel = 1;
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
            var currentLvl = 0;
            if (entityManager.HasComponent<LevelUpCardUpgradeTrack>(selectedCardEntity))
            {
                var track = entityManager.GetComponentData<LevelUpCardUpgradeTrack>(selectedCardEntity);
                currentLvl = GetPlayerUpgradeLevel(upgradeProgress, track.UpgradeId);

                if (currentLvl + 1 >= track.MaxLevel)
                {
                    RemoveFromAvailable(availableCards, selected);
                }
            }

            newLevel = currentLvl + 1;

            ApplyUpgradeProgress(entityManager, upgradeProgress, selectedCardEntity);
            offeredCards.Clear();
        }

        if (playerEntity == Entity.Null || selectedCardEntity == Entity.Null)
            return;

        ApplyCardEffects(entityManager, playerEntity, selectedCardEntity, newLevel);
        if (GameUIController.Instance != null)
            GameUIController.Instance.HideLevelUpPanel();
    }

    private static void ApplyCardEffects(EntityManager em, Entity player, Entity card, int newLevel)
    {
        var scale = math.pow(1.5f, math.max(0, newLevel - 1));
        if (em.HasBuffer<PlayerStatOperationElement>(player))
        {
            var statOperations = em.GetBuffer<PlayerStatOperationElement>(player);

            if (em.HasBuffer<CardStatModifierEffectElement>(card))
            {
                var cardModifiers = em.GetBuffer<CardStatModifierEffectElement>(card);
                for (int i = 0; i < cardModifiers.Length; i++)
                {
                    var scaledAdd = cardModifiers[i].AddValue * scale;
                    var scaledMul = cardModifiers[i].MulValue * scale;
                    EnqueueModifier(statOperations, cardModifiers[i].Type, scaledAdd, scaledMul);
                }
            }
            if (em.HasComponent<CardDamageBonusEffect>(card))
            {
                var scaledDmg = (int)math.round(em.GetComponentData<CardDamageBonusEffect>(card).Value * scale);
                EnqueueModifier(statOperations, PlayerStatType.Damage, scaledDmg, 0f);
            }
            if (em.HasComponent<CardDefenseBonusEffect>(card))
            {
                var scaledDef = (int)math.round(em.GetComponentData<CardDefenseBonusEffect>(card).Value * scale);
                EnqueueModifier(statOperations, PlayerStatType.Defense, scaledDef, 0f);
            }
            if (em.HasComponent<CardHealthRegenEffect>(card))
            {
                var scaledRegen = em.GetComponentData<CardHealthRegenEffect>(card).ValuePerSecond * scale;
                EnqueueModifier(statOperations, PlayerStatType.HealthRegen, scaledRegen, 0f);
            }
            if (em.HasComponent<CardMoveSpeedBonusEffect>(card))
            {
                var scaledSpd = em.GetComponentData<CardMoveSpeedBonusEffect>(card).Value * scale;
                EnqueueModifier(statOperations, PlayerStatType.MoveSpeedBonus, scaledSpd, 0f);
            }
        }

        if (em.HasComponent<CardUnlockBatWeaponEffect>(card))
        {
            if (!em.HasComponent<BatWeaponData>(player))
            {
                var effect = em.GetComponentData<CardUnlockBatWeaponEffect>(card);
                em.AddComponentData(player, em.GetComponentData<BatWeaponData>(effect.BatPrefab));
                em.AddComponentData(player, em.GetComponentData<BatWeaponCooldown>(effect.BatPrefab));
                Debug.Log("Bat weapon unlocked!");
            }
            else
            {
                var batQuery = em.CreateEntityQuery(typeof(BatOrbitData));
                var bats = batQuery.ToEntityArray(Allocator.Temp);
                var batDamageBuff = (int)math.round(2 * scale);

                for (int j = 0; j < bats.Length; j++)
                {
                    var batEntity = bats[j];
                    var orbit = em.GetComponentData<BatOrbitData>(batEntity);
                    if (orbit.Owner == player)
                    {
                        orbit.AngularSpeed += math.radians(45f);
                        orbit.Damage += batDamageBuff;
                        em.SetComponentData(batEntity, orbit);
                        Debug.Log($"Bat weapon UPGRADED! Dmg +{batDamageBuff}");
                    }
                }
                bats.Dispose();
            }
        }
    }

    private static void ApplyUpgradeProgress(EntityManager entityManager, DynamicBuffer<PlayerUpgradeProgressElement> progress, Entity cardEntity)
    {
        if (!entityManager.HasComponent<LevelUpCardUpgradeTrack>(cardEntity)) return;

        var track = entityManager.GetComponentData<LevelUpCardUpgradeTrack>(cardEntity);
        if (track.UpgradeId.Length == 0) return;

        for (int i = 0; i < progress.Length; i++)
        {
            if (progress[i].UpgradeId.Equals(track.UpgradeId))
            {
                progress[i] = new PlayerUpgradeProgressElement
                {
                    UpgradeId = track.UpgradeId,
                    CurrentLevel = progress[i].CurrentLevel + 1
                };
                return;
            }
        }
        progress.Add(new PlayerUpgradeProgressElement { UpgradeId = track.UpgradeId, CurrentLevel = 1 });
    }

    private static bool Contains(DynamicBuffer<OfferedLevelUpCardElement> buffer, Entity e)
    {
        for (int i = 0; i < buffer.Length; i++) if (buffer[i].Value == e) return true; return false;
    }
    private static void RemoveFromAvailable(DynamicBuffer<AvailableLevelUpCardElement> buffer, Entity e)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i].Value == e) { buffer.RemoveAt(i); return; }
        }
    }
    private static int GetPlayerUpgradeLevel(DynamicBuffer<PlayerUpgradeProgressElement> progress, FixedString64Bytes upgradeId)
    {
        if (upgradeId.Length == 0) return 0;
        for (int i = 0; i < progress.Length; i++)
            if (progress[i].UpgradeId.Equals(upgradeId)) return progress[i].CurrentLevel;
        return 0;
    }
    private static void EnqueueModifier(DynamicBuffer<PlayerStatOperationElement> ops, PlayerStatType type, float add, float mul)
    {
        ops.Add(new PlayerStatOperationElement { Type = type, AddValue = add, MulValue = mul });
    }
    private static bool IsCardEligible(EntityManager em, Entity card, DynamicBuffer<PlayerUpgradeProgressElement> progress)
    {
        if (em.HasComponent<LevelUpCardRequirement>(card))
        {
            var req = em.GetComponentData<LevelUpCardRequirement>(card);
            if (req.RequiredLevel > 0 && GetPlayerUpgradeLevel(progress, req.UpgradeId) < req.RequiredLevel) return false;
        }
        if (!em.HasComponent<LevelUpCardUpgradeTrack>(card)) return true;
        var track = em.GetComponentData<LevelUpCardUpgradeTrack>(card);
        return GetPlayerUpgradeLevel(progress, track.UpgradeId) < track.MaxLevel;
    }
}