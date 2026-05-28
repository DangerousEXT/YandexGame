using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class NewEquipmentPanelManager : MonoBehaviour
{
    [SerializeField] private EquipmentUIManager equipmentUIManager;
    [SerializeField] private GameObject newEquipmentPanel;
    [SerializeField] private Button sellButton;
    [SerializeField] private TextMeshProUGUI sellCost;
    [SerializeField] private Button takeButton;

    private Equipment _equipment;

    private void OnEnable()
    {
        sellButton.onClick.AddListener(SellButton);
        takeButton.onClick.AddListener(TakeButton);
    }

    private void OnDisable()
    {
        sellButton.onClick.RemoveListener(SellButton);
        takeButton.onClick.RemoveListener(TakeButton);
    }

    public void NewEquipment(Equipment equipment)
    {
        _equipment = equipment;
        newEquipmentPanel.SetActive(true);
        sellCost.text = $"{LocalizationManager.Instance.Get(LocalizationCategories.buttons, "sell_bt")}: {equipment.Cost}";
        equipmentUIManager.NewEquipment(equipment);
    }

    private void SellButton()
    {
        PlayerData.Instance.Gold += _equipment.Cost;
        newEquipmentPanel.SetActive(false);
    }

    private void TakeButton()
    {
        PlayerData.Instance.AddEquipment(_equipment);
        newEquipmentPanel.SetActive(false);
        Debug.Log($"{PlayerData.Instance.Inventory.Count}");
    }
}