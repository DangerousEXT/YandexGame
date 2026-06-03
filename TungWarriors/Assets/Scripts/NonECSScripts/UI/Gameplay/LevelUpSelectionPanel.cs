using GameAnalyticsSDK;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpSelectionPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private CardButtonRefs[] cardButtons;

    private Action<Entity> OnCardSelected;

    [System.Serializable]
    public struct CardButtonRefs
    {
        public Button Button;
        public Image Icon;
        public TextMeshProUGUI Title;
        public TextMeshProUGUI Description;
        public TextMeshProUGUI LevelText;
    }

    public void Show(IReadOnlyList<LevelUpCardViewData> cards, Action<Entity> onCardSelected)
    {
        OnCardSelected = onCardSelected;
        panelRoot.SetActive(true);
        for (int i = 0; i < cardButtons.Length; i++)
        {
            if (i >= cards.Count)
            {
                cardButtons[i].Button.gameObject.SetActive(false);
                continue;
            }
            var card = cards[i];
            var btn = cardButtons[i];
            btn.Title.text = LocalizationManager.Instance.Get(LocalizationCategories.game, $"{card.CardId}_title");
            var rawDescription = LocalizationManager.Instance.Get(LocalizationCategories.game, $"{card.CardId}_description");
            if (card.HasEffectValue)
                btn.Description.text = rawDescription.Replace("{0}", card.PrimaryEffectValue.ToString("0.#"));
            else
                btn.Description.text = rawDescription;
            if (btn.LevelText != null)
                btn.LevelText.text = $"Lvl {card.Level}";
            if (btn.Icon != null)
            {
                btn.Icon.sprite = card.Icon;
                btn.Icon.enabled = card.Icon != null;
            }
            btn.Button.onClick.RemoveAllListeners();
            btn.Button.onClick.AddListener(() => OnButtonClicked(card.CardEntity, card.CardId));
            btn.Button.gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
        for (int i = 0; i < cardButtons.Length; i++)
        {
            cardButtons[i].Button.onClick.RemoveAllListeners();
            cardButtons[i].Button.gameObject.SetActive(false);
        }
        OnCardSelected = null;
    }

    private void OnButtonClicked(Entity cardEntity, string cardId)
    {
        GameAnalytics.NewDesignEvent($"Card_Picked:{cardId}");
        OnCardSelected?.Invoke(cardEntity);
    }
}