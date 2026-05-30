using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YG;


public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    [Header("Resources")]
    [SerializeField] private int gold;
    [SerializeField] private int gems;
    [SerializeField] private int rubies;
    [SerializeField] private int bestSurvivalTimeMilliseconds;

    [Header("Shop")]
    [SerializeField] private List<ItemData> shopItems = new();

    [Header("Inventory")]
    [SerializeField] private List<Equipment> inventory = new();

    [Header("Equipment On Player")]
    [SerializeField] private Dictionary<EquipmentOnPlayerType, Equipment> equipmentOnPlayer = new();

    [Header("Meta Progression")]
    [SerializeField] private List<MetaUpgradeLevelSaveData> metaUpgradeLevels = new();

    public event Action<int> OnGoldChanged;
    public event Action<int> OnGemsChanged;
    public event Action<int> OnBestSurvivalTimeChanged;
    public event Action<Equipment, bool> OnInventoryChanged;
    public event Action OnMetaProgressionChanged;

    public int Gold
    {
        get => gold;
        set
        {
            gold = Mathf.Max(0, value);
            OnGoldChanged?.Invoke(gold);
        }
    }

    public int Gems
    {
        get => gems;
        set
        {
            gems = Mathf.Max(0, value);
            OnGemsChanged?.Invoke(gems);
        }
    }

    public int Rubies
    {
        get => rubies;
        set
        {
            rubies = Mathf.Max(0, value);
        }
    }

    public int BestSurvivalTimeMilliseconds
    {
        get => bestSurvivalTimeMilliseconds;
        set
        {
            var sanitizedValue = Mathf.Max(0, value);
            if (bestSurvivalTimeMilliseconds == sanitizedValue)
                return;

            bestSurvivalTimeMilliseconds = sanitizedValue;
            OnBestSurvivalTimeChanged?.Invoke(bestSurvivalTimeMilliseconds);
        }
    }

    public List<Equipment> Inventory
    {
        get => inventory;
        set
        {
            inventory = value;
        }
    } 

    public Dictionary<EquipmentOnPlayerType, Equipment> EquipmentOnPlayer
    {
        get => equipmentOnPlayer;
        set
        {
            equipmentOnPlayer = value;
        }
    } 

    public List<ItemData> ShopItems => shopItems;

    private void Awake()
    {
        SpritesBase.LoadAllIcons();
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IReadOnlyList<MetaUpgradeLevelSaveData> MetaUpgradeLevels => metaUpgradeLevels;

    public void LoadMetaUpgradeLevels(List<MetaUpgradeLevelSaveData> levels)
    {
        metaUpgradeLevels = levels?
            .Where(data => data != null && !string.IsNullOrWhiteSpace(data.upgradeId))
            .GroupBy(data => data.upgradeId)
            .Select(group => new MetaUpgradeLevelSaveData
            {
                upgradeId = group.Key,
                level = Mathf.Max(0, group.Last().level)
            })
            .ToList()
            ?? new List<MetaUpgradeLevelSaveData>();

        OnMetaProgressionChanged?.Invoke();
    }

    public List<MetaUpgradeLevelSaveData> GetMetaUpgradeLevelsForSave()
    {
        return metaUpgradeLevels.Select(data => data.Clone()).ToList();
    }

    public int GetMetaUpgradeLevel(string upgradeId)
    {
        if (string.IsNullOrWhiteSpace(upgradeId))
            return 0;

        var upgradeData = metaUpgradeLevels.FirstOrDefault(data => data.upgradeId == upgradeId);
        return upgradeData == null ? 0 : Mathf.Max(0, upgradeData.level);
    }

    public int GetMetaUpgradePrice(MetaUpgradeDefinition definition)
    {
        return MetaProgressionFormula.CalculatePrice(definition, GetMetaUpgradeLevel(definition.UpgradeId));
    }

    public bool IsMetaUpgradeMaxed(MetaUpgradeDefinition definition)
    {
        if (definition == null)
            return true;

        return definition.HasMaxLevel && GetMetaUpgradeLevel(definition.UpgradeId) >= definition.MaxLevel;
    }

    public bool TryPurchaseMetaUpgrade(MetaUpgradeDefinition definition)
    {
        if (definition == null || IsMetaUpgradeMaxed(definition))
            return false;

        var price = GetMetaUpgradePrice(definition);
        if (Gold < price)
            return false;

        Gold -= price;

        var upgradeData = GetOrCreateMetaUpgradeData(definition.UpgradeId);
        upgradeData.level = Mathf.Max(0, upgradeData.level) + 1;

        OnMetaProgressionChanged?.Invoke();
        return true;
    }

    public float GetMetaUpgradeBonus(MetaUpgradeStatType statType)
    {
        float totalBonus = 0f;
        foreach (var definition in MetaProgressionCatalog.GetDefinitions())
        {
            if (definition.StatType != statType)
                continue;

            totalBonus += definition.GetTotalBonusForLevel(GetMetaUpgradeLevel(definition.UpgradeId));
        }

        return totalBonus;
    }

    public MetaProgressionSnapshot GetMetaProgressionSnapshot()
    {
        return new MetaProgressionSnapshot(
            GetMetaUpgradeBonus(MetaUpgradeStatType.MaxHitPoints),
            GetMetaUpgradeBonus(MetaUpgradeStatType.Damage),
            GetMetaUpgradeBonus(MetaUpgradeStatType.MoveSpeed));
    }

    public bool TrySetBestSurvivalTimeMilliseconds(int value)
    {
        if (value <= BestSurvivalTimeMilliseconds)
            return false;

        BestSurvivalTimeMilliseconds = value;
        return true;
    }

    private MetaUpgradeLevelSaveData GetOrCreateMetaUpgradeData(string upgradeId)
    {
        var upgradeData = metaUpgradeLevels.FirstOrDefault(data => data.upgradeId == upgradeId);
        if (upgradeData != null)
            return upgradeData;

        upgradeData = new MetaUpgradeLevelSaveData
        {
            upgradeId = upgradeId,
            level = 0
        };
        metaUpgradeLevels.Add(upgradeData);
        return upgradeData;
    }

    public void AddEquipment(Equipment equip)
    {
        inventory.Add(equip);
        OnInventoryChanged?.Invoke(equip, true);
    }

    public void RemoveEquipment(Equipment equip)
    {
        inventory.Remove(equip);
        OnInventoryChanged?.Invoke(equip, false);
    }

    public void PutOnEquipment(Equipment equip, EquipmentOnPlayerType type)
    {
        if(equipmentOnPlayer.TryGetValue(type, out var equipment))
            if(equipment != null)
                AddEquipment(equipment);

        equipmentOnPlayer[type] = equip;
        RemoveEquipment(equip);
    }

    public void TakeOffEquipment(EquipmentOnPlayerType type)
    {
        if (equipmentOnPlayer.TryGetValue(type, out var equipment))
        {
            if (equipment == null)
                return;
            AddEquipment(equipment);
            equipmentOnPlayer.Remove(type);
        }
    }
}
