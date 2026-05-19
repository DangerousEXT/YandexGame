using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class MetaProgressionPanelManager : MonoBehaviour
{
    private const string ContentRootName = "MetaProgressionContent";
    private const string RowsRootName = "MetaProgressionRows";

    private readonly List<MetaUpgradeRowView> rowViews = new();

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private Transform rowsRoot;

    private bool isSubscribed;

    private void Awake()
    {
        Refresh();
    }

    private void OnEnable()
    {
        TrySubscribe();
        Refresh();
    }

    private void Update()
    {
        if (!isSubscribed)
            TrySubscribe();
    }

    private void OnDisable()
    {
        if (!isSubscribed || PlayerData.Instance == null)
            return;

        PlayerData.Instance.OnGoldChanged -= OnGoldChanged;
        PlayerData.Instance.OnMetaProgressionChanged -= OnMetaProgressionChanged;
        YG2.onSwitchLang -= OnLanguageChanged;
        isSubscribed = false;
    }

    private void TrySubscribe()
    {
        if (isSubscribed || PlayerData.Instance == null)
            return;

        PlayerData.Instance.OnGoldChanged += OnGoldChanged;
        PlayerData.Instance.OnMetaProgressionChanged += OnMetaProgressionChanged;
        YG2.onSwitchLang += OnLanguageChanged;
        isSubscribed = true;
    }

    private void OnGoldChanged(int _)
    {
        Refresh();
    }

    private void OnMetaProgressionChanged()
    {
        Refresh();
    }

    private void OnLanguageChanged(string _)
    {
        Refresh();
    }

    private void Refresh()
    {

        var playerData = PlayerData.Instance;
        if (titleText != null)
            titleText.text = GetLocalized("panel_title", "Meta Progression");
        if (goldText != null)
            goldText.text = playerData == null
                ? string.Format(GetLocalized("gold_format", "Gold: {0}"), "...")
                : string.Format(GetLocalized("gold_format", "Gold: {0}"), playerData.Gold);
        if (hintText != null)
            hintText.text = GetLocalized("panel_hint", "Meta bonuses are added to base stats before equipment multipliers.");

        foreach (var rowView in rowViews)
            rowView.Refresh(playerData);
    }


    private string GetLocalized(string key, string fallback)
    {
        if (LocalizationManager.Instance == null)
            return fallback;

        var localized = LocalizationManager.Instance.Get(LocalizationCategories.meta_progression, key);
        return localized == key ? fallback : localized;
    }
}
