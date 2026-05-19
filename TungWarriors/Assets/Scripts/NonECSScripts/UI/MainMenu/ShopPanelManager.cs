using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelManager : MonoBehaviour 
{
    [Header("Chests")]
    [SerializeField] private Button buyChestForGold;
    [SerializeField] private int chestForGoldCost;

    [SerializeField] private Button buyChestForDiamond;
    [SerializeField] private int chestForDiamondCost;

    [SerializeField] private Button buyChestForAdv;

    [Header("Buying Gold")]
    [SerializeField] private Button buyGoldForDiamond;
    [SerializeField] private int goldForDiamondCost;
    [SerializeField] private int goldForDiamondCount;

    [SerializeField] private Button buyGoldForMoney;
    [SerializeField] private float goldForMoneyCost;
    [SerializeField] private int goldForMoneyCount;

    [SerializeField] private Button buyGoldForAdv;
    [SerializeField] private int goldForAdvCount;

    [Header("Buying Diamonds")]
    [SerializeField] private Button buyDiamondsForAdv;
    [SerializeField] private int diamondsForAdvCount;

    [SerializeField] private Button buyDiamondsForMoneyLittle;
    [SerializeField] private float diamondsForMoneyLittleCost;
    [SerializeField] private int diamondsForMoneyLittleCount;

    [SerializeField] private Button buyDiamondsForMoneyLarge;
    [SerializeField] private float diamondsForMoneyLargeCost;
    [SerializeField] private int diamondsForMoneyLargeCount;

    [Header("Message")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeMessagePanel;

    [Header("New Equipment")]
    [SerializeField] private NewEquipmentPanelManager newEquipmentPanel;


    private EquipmentFactory equipmentFactory;

    private void Start()
    {
        equipmentFactory = new(PlayerData.Instance.ShopItems);
        messagePanel.SetActive(false);
    }

    private void OnEnable()
    {
        buyChestForGold.onClick.AddListener(BuyChestForGold);
        buyChestForDiamond.onClick.AddListener(BuyChestForDiamond);
        buyChestForAdv.onClick.AddListener(BuyChestForAdv);

        buyGoldForDiamond.onClick.AddListener(BuyGoldForDiamond);
        buyGoldForMoney.onClick.AddListener(BuyGoldForMoney);
        buyGoldForAdv.onClick.AddListener(BuyGoldForAdv);

        buyDiamondsForAdv.onClick.AddListener(BuyDiamondsForAdv);
        buyDiamondsForMoneyLittle.onClick.AddListener(BuyDiamondsForMoneyLittle);
        buyDiamondsForMoneyLarge.onClick.AddListener(BuyDiamondsForMoneyLarge);

        closeMessagePanel.onClick.AddListener(CloseMessagePanel);
    }

    private void OnDisable()
    {
        buyChestForGold.onClick.RemoveListener(BuyChestForGold);
        buyChestForDiamond.onClick.RemoveListener(BuyChestForDiamond);
        buyChestForAdv.onClick.RemoveListener(BuyChestForAdv);

        buyGoldForDiamond.onClick.RemoveListener(BuyGoldForDiamond);
        buyGoldForMoney.onClick.RemoveListener(BuyGoldForMoney);
        buyGoldForAdv.onClick.RemoveListener(BuyGoldForAdv);

        buyDiamondsForAdv.onClick.RemoveListener(BuyDiamondsForAdv);
        buyDiamondsForMoneyLittle.onClick.RemoveListener(BuyDiamondsForMoneyLittle);
        buyDiamondsForMoneyLarge.onClick.RemoveListener(BuyDiamondsForMoneyLarge);

        closeMessagePanel.onClick.RemoveListener(CloseMessagePanel);
    }

    private void BuyChestForGold()
    {
        if(PlayerData.Instance.Gold >= chestForGoldCost)
        {
            PlayerData.Instance.Gold -= chestForGoldCost;
            GetNewEquipment();
        }
        else
        {
            OpenMessagePanel(string.Format(LocalizationManager.Instance.Get(LocalizationCategories.shop, "not_enough_gold"), chestForGoldCost - PlayerData.Instance.Gold));
        }
    }

    private void BuyChestForDiamond()
    {
        if (PlayerData.Instance.Gems >= chestForDiamondCost)
        {
            PlayerData.Instance.Gems -= chestForDiamondCost;
            GetNewEquipment();
        }
        else
        {
            OpenMessagePanel(string.Format(LocalizationManager.Instance.Get(LocalizationCategories.shop, "not_enough_gems"), chestForDiamondCost - PlayerData.Instance.Gems));
        }
    }

    private void BuyChestForAdv()
    {
        RewardedAdvController.Instance.ShowRewardedAdv(
            RewardedAdvAwards.chest,
            () => GetNewEquipment(),
            () => OpenMessagePanel(LocalizationManager.Instance.Get(LocalizationCategories.shop, "ad_error")));
        
    }

    private void BuyGoldForDiamond()
    {
        if (PlayerData.Instance.Gems >= goldForDiamondCost)
        {
            PlayerData.Instance.Gems -= goldForDiamondCost;
            PlayerData.Instance.Gold += goldForDiamondCount;
        }
        else
        {
            OpenMessagePanel(string.Format(LocalizationManager.Instance.Get(LocalizationCategories.shop, "not_enough_gems"), goldForDiamondCost - PlayerData.Instance.Gems));
        }
    }

    private void BuyGoldForMoney()
    {
        Debug.Log($"Buy Gold For Money");
    }

    private void BuyGoldForAdv()
    {
        RewardedAdvController.Instance.ShowRewardedAdv(
            RewardedAdvAwards.gold,
            () =>
            {
                PlayerData.Instance.Gold += goldForAdvCount;
            },
            () => OpenMessagePanel(LocalizationManager.Instance.Get(LocalizationCategories.shop, "ad_error")));
    }
    
    private void BuyDiamondsForAdv()
    {
        RewardedAdvController.Instance.ShowRewardedAdv(
            RewardedAdvAwards.gems,
            () =>
            {
                PlayerData.Instance.Gems += diamondsForAdvCount;
            },
            () => OpenMessagePanel(LocalizationManager.Instance.Get(LocalizationCategories.shop, "ad_error")));
    }

    private void BuyDiamondsForMoneyLittle()
    {
        Debug.Log($"Buy Diamonds For MoneyLittle");
    }

    private void BuyDiamondsForMoneyLarge()
    {
        Debug.Log($"Buy Diamonds For MoneyLarge");
    }

    private void GetNewEquipment()
    {
        newEquipmentPanel.NewEquipment(equipmentFactory.CreateRandomEquipment());
    }

    private void OpenMessagePanel(string message)
    {
        messagePanel.SetActive(true);
        messageText.text = message;
    }

    private void CloseMessagePanel()
    {
        messagePanel.SetActive(false);
    }
}