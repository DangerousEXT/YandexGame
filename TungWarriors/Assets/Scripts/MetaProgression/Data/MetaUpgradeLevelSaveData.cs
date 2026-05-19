using System;

[Serializable]
public class MetaUpgradeLevelSaveData
{
    public string upgradeId;
    public int level;

    public MetaUpgradeLevelSaveData Clone()
    {
        return new MetaUpgradeLevelSaveData
        {
            upgradeId = upgradeId,
            level = level
        };
    }
}
