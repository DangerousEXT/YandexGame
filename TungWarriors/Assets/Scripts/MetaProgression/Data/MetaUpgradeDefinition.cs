using UnityEngine;

[CreateAssetMenu(fileName = "MetaUpgradeDefinition", menuName = "Meta Progression/Upgrade Definition")]
public class MetaUpgradeDefinition : ScriptableObject
{
    [SerializeField] private string upgradeId;
    [SerializeField] private string displayName;
    [SerializeField] private string titleLocalizationKey;
    [SerializeField] private MetaUpgradeStatType statType;
    [SerializeField] private int basePrice = 100;
    [SerializeField] private int maxLevel = 20;
    [SerializeField] private float valuePerLevel = 1f;
    [SerializeField] private int arithmeticStep = 10;
    [SerializeField] private float geometricMultiplier = 1.05f;
    [SerializeField] private int sortOrder;

    public string UpgradeId => upgradeId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? statType.ToString() : displayName;
    public string TitleLocalizationKey => titleLocalizationKey;
    public MetaUpgradeStatType StatType => statType;
    public int BasePrice => basePrice;
    public int MaxLevel => maxLevel;
    public float ValuePerLevel => valuePerLevel;
    public int ArithmeticStep => arithmeticStep;
    public float GeometricMultiplier => geometricMultiplier;
    public int SortOrder => sortOrder;
    public bool HasMaxLevel => maxLevel > 0;

    public float GetTotalBonusForLevel(int level)
    {
        return Mathf.Max(0, level) * valuePerLevel;
    }

    public static MetaUpgradeDefinition CreateRuntime(
        string id,
        string title,
        MetaUpgradeStatType type,
        int price,
        int levelCap,
        float bonusPerLevel,
        int arithmetic,
        float geometric,
        int order)
    {
        var definition = CreateInstance<MetaUpgradeDefinition>();
        definition.upgradeId = id;
        definition.displayName = title;
        definition.titleLocalizationKey = id;
        definition.statType = type;
        definition.basePrice = price;
        definition.maxLevel = levelCap;
        definition.valuePerLevel = bonusPerLevel;
        definition.arithmeticStep = arithmetic;
        definition.geometricMultiplier = geometric;
        definition.sortOrder = order;
        return definition;
    }
}
