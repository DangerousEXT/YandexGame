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

    [SerializeField] private Button toMainMenuPanel;
    [SerializeField] private Button toInventoryPanel;
    [SerializeField] private Button toShopPanel;
    [SerializeField] private Button toSkillTreePanel;

    private GameObject currentPanel;

    private void Awake()
    {
        EnsureSkillTreePanel();
        EnsureSkillTreeButton();

        currentPanel = mainMenuPanel;
        SetPanelState(mainMenuPanel, true);
        SetPanelState(inventoryPanel, false);
        SetPanelState(shopPanel, false);
        SetPanelState(skillTreePanel, false);

        AudioController.Instance.PlayMenuMusic();
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

        var rect = panelObject.GetComponent<RectTransform>();
        if (inventoryPanel != null && inventoryPanel.transform is RectTransform inventoryRect)
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

        skillTreePanel = panelObject;
        skillTreePanel.SetActive(false);
    }

    private void EnsureSkillTreeButton()
    {
        if (IsValidUniqueButton(toSkillTreePanel))
        {
            SetButtonLabel(toSkillTreePanel);
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

        SetButtonLabel(toSkillTreePanel);
    }

    private void EnsureMetaPanelComponent(GameObject panel)
    {
        if (panel == null)
            return;

        if (panel.GetComponent<MetaProgressionPanelManager>() == null)
            panel.AddComponent<MetaProgressionPanelManager>();
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

  

    private void AddButton(List<Button> buttons, Button button)
    {
        if (button != null && !buttons.Contains(button))
            buttons.Add(button);
    }

    private void SetButtonLabel(Button button)
    {
        if (button == null)
            return;

        var text = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text != null)
        {
            var localized = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.Get(LocalizationCategories.buttons, "to_meta_progression_bt")
                : null;
            text.text = string.IsNullOrWhiteSpace(localized) || localized == "to_meta_progression_bt"
                ? "Upgrades"
                : localized;
        }
    }

    private void SetPanelState(GameObject panel, bool isActive)
    {
        if (panel != null)
            panel.SetActive(isActive);
    }

    private void OnLanguageChanged(string _)
    {
        SetButtonLabel(toSkillTreePanel);
    }
}
