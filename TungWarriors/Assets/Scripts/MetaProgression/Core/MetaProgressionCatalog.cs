using System.Linq;
using UnityEngine;

public static class MetaProgressionCatalog
{
    private const string ResourcePath = "MetaProgression";
    private static MetaUpgradeDefinition[] cachedDefinitions;

    public static MetaUpgradeDefinition[] GetDefinitions()
    {
        if (cachedDefinitions != null && cachedDefinitions.Length > 0)
            return cachedDefinitions;

        cachedDefinitions = Resources.LoadAll<MetaUpgradeDefinition>(ResourcePath)
            .Where(definition => definition != null && !string.IsNullOrWhiteSpace(definition.UpgradeId))
            .OrderBy(definition => definition.SortOrder)
            .ThenBy(definition => definition.DisplayName)
            .ToArray();

        if (cachedDefinitions.Length == 0)
        {
            cachedDefinitions = new[]
            {
                MetaUpgradeDefinition.CreateRuntime("meta_health", "Health", MetaUpgradeStatType.MaxHitPoints, 100, 20, 10f, 10, 1.05f, 0),
                MetaUpgradeDefinition.CreateRuntime("meta_damage", "Damage", MetaUpgradeStatType.Damage, 100, 20, 1f, 10, 1.05f, 1),
                MetaUpgradeDefinition.CreateRuntime("meta_move_speed", "Move Speed", MetaUpgradeStatType.MoveSpeed, 100, 20, 0.25f, 10, 1.05f, 2)
            };

            Debug.LogWarning("");
        }

        return cachedDefinitions;
    }

    public static MetaUpgradeDefinition GetDefinition(string upgradeId)
    {
        if (string.IsNullOrWhiteSpace(upgradeId))
            return null;

        return GetDefinitions().FirstOrDefault(definition => definition.UpgradeId == upgradeId);
    }
}
