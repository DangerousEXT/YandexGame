using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MetaUpgradeRowView : MonoBehaviour
{
    [SerializeField] private string upgradeId;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI bonusText;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Button buyButton;

    private MetaUpgradeDefinition definition;
    private Action<MetaUpgradeDefinition> onBuyClicked;

    public string UpgradeId => string.IsNullOrWhiteSpace(upgradeId) ? definition?.UpgradeId : upgradeId;

    public void Initialize(MetaUpgradeDefinition upgradeDefinition, Action<MetaUpgradeDefinition> onBuy)
    {
        definition = upgradeDefinition;
        onBuyClicked = onBuy;

        Refresh(null);

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);
    }

    public void Refresh(PlayerData playerData)
    {
        if (definition == null)
            return;

        titleText.text = GetLocalized(definition.TitleLocalizationKey, definition.DisplayName);

        if (playerData == null)
        {
            levelText.text = string.Format(GetLocalized("level_single_format", "Lvl {0}"), 0);
            bonusText.text = string.Empty;
            buttonText.text = "...";
            buyButton.interactable = false;
            return;
        }

        var level = playerData.GetMetaUpgradeLevel(definition.UpgradeId);
        var isMaxed = playerData.IsMetaUpgradeMaxed(definition);
        var currentBonus = definition.GetTotalBonusForLevel(level);
        var price = playerData.GetMetaUpgradePrice(definition);

        levelText.text = definition.HasMaxLevel
            ? string.Format(GetLocalized("level_format", "Lvl {0}/{1}"), level, definition.MaxLevel)
            : string.Format(GetLocalized("level_single_format", "Lvl {0}"), level);
        bonusText.text = string.Format(GetLocalized("bonus_format", "+{0}"), FormatBonus(currentBonus));
        buttonText.text = isMaxed
            ? GetLocalized("max_label", "Max")
            : string.Format(GetLocalized("buy_format", "Buy {0}"), price);
        buyButton.interactable = !isMaxed && playerData.Gold >= price;
    }

    private void OnBuyClicked()
    {
        onBuyClicked?.Invoke(definition);
    }

    private string FormatBonus(float value)
    {
        return Mathf.Approximately(value % 1f, 0f)
            ? value.ToString("0")
            : value.ToString("0.##");
    }


    private string GetLocalized(string key, string fallback)
    {
        if (LocalizationManager.Instance == null || string.IsNullOrWhiteSpace(key))
            return fallback;

        var localized = LocalizationManager.Instance.Get(LocalizationCategories.meta_progression, key);
        return localized == key ? fallback : localized;
    }

    

    
}
