using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class NavigationPanelManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject skillTreePanel;
    [SerializeField] private GameObject leaderboardPanel;

    [SerializeField] private Button toMainMenuPanel;
    [SerializeField] private Button toInventoryPanel;
    [SerializeField] private Button toShopPanel;
    [SerializeField] private Button toSkillTreePanel;
    [SerializeField] private Button toLeaderboardPanel;

    private GameObject currentPanel;

    private void Awake()
    {
        EnsureSkillTreePanel();
        EnsureSkillTreeButton();
        EnsureLeaderboardPanel();
        EnsureLeaderboardButton();
        LayoutNavigationButtons();

        currentPanel = mainMenuPanel;
        SetPanelState(mainMenuPanel, true);
        SetPanelState(inventoryPanel, false);
        SetPanelState(shopPanel, false);
        SetPanelState(skillTreePanel, false);
        SetPanelState(leaderboardPanel, false);
    }

    private void OnEnable()
    {
        YG2.onSwitchLang += OnLanguageChanged;
        if (toMainMenuPanel != null)
            toMainMenuPanel.onClick.AddListener(ChangePanelToMainMenu);
        if (toInventoryPanel != null)
            toInventoryPanel.onClick.AddListener(ChangePanelToInventory);
        if (toShopPanel != null)
            toShopPanel.onClick.AddListener(ChangePanelToShop);
        if (toSkillTreePanel != null)
            toSkillTreePanel.onClick.AddListener(ChangePanelToSkillTree);
        if (toLeaderboardPanel != null)
            toLeaderboardPanel.onClick.AddListener(ChangePanelToLeaderboard);
    }

    private void OnDisable()
    {
        YG2.onSwitchLang -= OnLanguageChanged;
        if (toMainMenuPanel != null)
            toMainMenuPanel.onClick.RemoveListener(ChangePanelToMainMenu);
        if (toInventoryPanel != null)
            toInventoryPanel.onClick.RemoveListener(ChangePanelToInventory);
        if (toShopPanel != null)
            toShopPanel.onClick.RemoveListener(ChangePanelToShop);
        if (toSkillTreePanel != null)
            toSkillTreePanel.onClick.RemoveListener(ChangePanelToSkillTree);
        if (toLeaderboardPanel != null)
            toLeaderboardPanel.onClick.RemoveListener(ChangePanelToLeaderboard);
    }

    private void ChangePanel(GameObject panel)
    {
        if (panel == null || currentPanel == panel)
            return;

        SetPanelState(currentPanel, false);
        SetPanelState(panel, true);
        currentPanel = panel;
    }

    private void ChangePanelToMainMenu()
    {
        ChangePanel(mainMenuPanel);
    }

    private void ChangePanelToInventory()
    {
        ChangePanel(inventoryPanel);
    }

    private void ChangePanelToShop()
    {
        ChangePanel(shopPanel);
    }

    private void ChangePanelToSkillTree()
    {
        ChangePanel(skillTreePanel);
    }

    private void ChangePanelToLeaderboard()
    {
        ChangePanel(leaderboardPanel);
    }

    private void EnsureSkillTreePanel()
    {
        if (IsValidUniquePanel(skillTreePanel))
        {
            EnsureMetaPanelComponent(skillTreePanel);
            return;
        }

        var panelParent = inventoryPanel != null ? inventoryPanel.transform.parent : transform.parent;
        if (panelParent == null)
            return;

        var existingPanel = panelParent.Find("MetaProgressionPanel");
        if (existingPanel != null)
        {
            skillTreePanel = existingPanel.gameObject;
            EnsureMetaPanelComponent(skillTreePanel);
            return;
        }

        var panelObject = new GameObject("MetaProgressionPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(MetaProgressionPanelManager));
        panelObject.transform.SetParent(panelParent, false);

        ConfigureRuntimePanelRect(panelObject);

        skillTreePanel = panelObject;
        skillTreePanel.SetActive(false);
    }

    private void EnsureLeaderboardPanel()
    {
        if (IsValidUniquePanel(leaderboardPanel))
        {
            EnsureLeaderboardPanelComponent(leaderboardPanel);
            return;
        }

        var panelParent = inventoryPanel != null ? inventoryPanel.transform.parent : transform.parent;
        if (panelParent == null)
            return;

        var existingPanel = panelParent.Find("LeaderboardPanel");
        if (existingPanel != null)
        {
            leaderboardPanel = existingPanel.gameObject;
            EnsureLeaderboardPanelComponent(leaderboardPanel);
            return;
        }

        var panelObject = new GameObject("LeaderboardPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObject.transform.SetParent(panelParent, false);
        ConfigureRuntimePanelRect(panelObject);

        leaderboardPanel = panelObject;
        leaderboardPanel.SetActive(false);
        EnsureLeaderboardPanelComponent(leaderboardPanel);
    }

    private void EnsureSkillTreeButton()
    {
        if (IsValidUniqueButton(toSkillTreePanel))
        {
            SetSkillTreeButtonLabel(toSkillTreePanel);
            return;
        }

        var sourceButton = toShopPanel ?? toInventoryPanel ?? toMainMenuPanel;
        if (sourceButton == null)
            return;

        var buttonParent = sourceButton.transform.parent;
        var existingButton = buttonParent.Find("ToMetaProgressionPanelButton");
        if (existingButton != null && existingButton.TryGetComponent(out Button existingSkillButton))
        {
            toSkillTreePanel = existingSkillButton;
        }
        else
        {
            var buttonObject = Instantiate(sourceButton.gameObject, buttonParent);
            buttonObject.name = "ToMetaProgressionPanelButton";
            toSkillTreePanel = buttonObject.GetComponent<Button>();
        }

        SetSkillTreeButtonLabel(toSkillTreePanel);
    }

    private void EnsureLeaderboardButton()
    {
        if (IsValidUniqueButton(toLeaderboardPanel))
        {
            SetLeaderboardButtonLabel(toLeaderboardPanel);
            return;
        }

        var sourceButton = toInventoryPanel ?? toMainMenuPanel ?? toShopPanel;
        if (sourceButton == null)
            return;

        var buttonParent = sourceButton.transform.parent;
        var existingButton = buttonParent.Find("ToLeaderboardPanelButton");
        if (existingButton != null && existingButton.TryGetComponent(out Button existingLeaderboardButton))
        {
            toLeaderboardPanel = existingLeaderboardButton;
        }
        else
        {
            var buttonObject = Instantiate(sourceButton.gameObject, buttonParent);
            buttonObject.name = "ToLeaderboardPanelButton";
            buttonObject.transform.SetAsFirstSibling();
            toLeaderboardPanel = buttonObject.GetComponent<Button>();
        }

        SetLeaderboardButtonLabel(toLeaderboardPanel);
    }

    private void EnsureMetaPanelComponent(GameObject panel)
    {
        if (panel == null)
            return;

        if (panel.GetComponent<MetaProgressionPanelManager>() == null)
            panel.AddComponent<MetaProgressionPanelManager>();
    }

    private void EnsureLeaderboardPanelComponent(GameObject panel)
    {
        if (panel == null)
            return;

    }

    private bool IsValidUniquePanel(GameObject panel)
    {
        return panel != null
            && panel != mainMenuPanel
            && panel != inventoryPanel
            && panel != shopPanel;
    }

    private bool IsValidUniqueButton(Button button)
    {
        return button != null
            && button != toMainMenuPanel
            && button != toInventoryPanel
            && button != toShopPanel;
    }

    private void ConfigureRuntimePanelRect(GameObject panelObject)
    {
        var rect = panelObject.GetComponent<RectTransform>();
        var inventoryRect = inventoryPanel != null ? inventoryPanel.transform as RectTransform : null;
        if (inventoryRect != null)
        {
            rect.anchorMin = inventoryRect.anchorMin;
            rect.anchorMax = inventoryRect.anchorMax;
            rect.pivot = inventoryRect.pivot;
            rect.anchoredPosition = inventoryRect.anchoredPosition;
            rect.sizeDelta = inventoryRect.sizeDelta;
            rect.offsetMin = inventoryRect.offsetMin;
            rect.offsetMax = inventoryRect.offsetMax;
            rect.SetSiblingIndex(inventoryPanel.transform.GetSiblingIndex());
        }
        else
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        var image = panelObject.GetComponent<Image>();
        if (image != null)
            image.color = new Color(1f, 1f, 1f, 0.392f);
    }

    private void LayoutNavigationButtons()
    {
        var buttons = new List<Button>();
        AddButton(buttons, toLeaderboardPanel);
        AddButton(buttons, toInventoryPanel);
        AddButton(buttons, toMainMenuPanel);
        AddButton(buttons, toSkillTreePanel);
        AddButton(buttons, toShopPanel);

        if (buttons.Count == 0)
            return;

        const float spacing = 0.012f;
        var width = (1f - spacing * (buttons.Count - 1)) / buttons.Count;

        for (var i = 0; i < buttons.Count; i++)
        {
            if (buttons[i] == null)
                continue;

            var rect = buttons[i].transform as RectTransform;
            if (rect == null)
                continue;

            var minX = i * (width + spacing);
            rect.anchorMin = new Vector2(minX, 0f);
            rect.anchorMax = new Vector2(minX + width, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }
    }

    private void AddButton(List<Button> buttons, Button button)
    {
        if (button != null && !buttons.Contains(button))
            buttons.Add(button);
    }

    private void SetSkillTreeButtonLabel(Button button)
    {
        SetButtonLabel(button, LocalizationCategories.buttons, "to_meta_progression_bt", "Upgrades");
    }

    private void SetLeaderboardButtonLabel(Button button)
    {
        SetButtonLabel(button, LocalizationCategories.buttons, "to_leaderboard_bt", "Leaderboard");
    }

    private void SetButtonLabel(Button button, LocalizationCategories category, string key, string fallback)
    {
        if (button == null)
            return;

        var text = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text == null)
            return;

        var localized = LocalizationManager.Instance != null
            ? LocalizationManager.Instance.Get(category, key)
            : null;

        text.text = string.IsNullOrWhiteSpace(localized) || localized == key
            ? fallback
            : localized;
    }

    private void SetPanelState(GameObject panel, bool isActive)
    {
        if (panel != null)
            panel.SetActive(isActive);
    }

    private void OnLanguageChanged(string _)
    {
        SetSkillTreeButtonLabel(toSkillTreePanel);
        SetLeaderboardButtonLabel(toLeaderboardPanel);
    }
}
