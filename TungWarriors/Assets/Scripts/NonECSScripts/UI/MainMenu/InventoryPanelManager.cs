using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;


public class InventoryPanelManager : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;

    [Header("Inventory")]
    [SerializeField] private ScrollRect inventory;
    [SerializeField] private List<EquipmentOnPlayerData> equipmentOnPlayerCells;
    [SerializeField] private Button closeInventory;

    [Header("Selected Equipment Panel")]
    [SerializeField] private GameObject selectedEquipmentPanel;
    [SerializeField] private EquipmentUIManager selectedEquipmentUI;
    [SerializeField] private Button putOnSelectedEquipment;
    [SerializeField] private Button sellSelectedEquipment;
    [SerializeField] private TextMeshProUGUI sellText;

    [Header("Warn Equipment Panel")]
    [SerializeField] private GameObject warnEquipmentPanel;
    [SerializeField] private EquipmentUIManager warnEquipmentUI;
    [SerializeField] private Button takeOffWarnEquipment;

    private EquipmentOnPlayerType currentEquipmentOnPlayerType;
    private EquipmentType currentEquipmentType;

    private void Awake()
    {
        selectedEquipmentPanel.SetActive(false);
        warnEquipmentPanel.SetActive(false);
        closeInventory.gameObject.SetActive(false);
        foreach (var x in equipmentOnPlayerCells)
        {
            x.cellPrefabData.Button.onClick.AddListener(() => OpenInventoryOfType(x.equipmentType, x.equipmentOnPlayerType));
            x.cellPrefabData.Button.onClick.AddListener(() => OpenWarnItemPanel(x.equipmentOnPlayerType));
            if(PlayerData.Instance.EquipmentOnPlayer.TryGetValue(x.equipmentOnPlayerType, out var item))
                x.cellPrefabData.Image.sprite = item.Icon;
        }
    }

    private void OnEnable()
    {
        Debug.Log($"Inventory Panel Enable");
        putOnSelectedEquipment.onClick.AddListener(PutOnSelectedEquipment);
        sellSelectedEquipment.onClick.AddListener(SellSelectedEquipment);
        takeOffWarnEquipment.onClick.AddListener(TakeOffWarnEquipment);
        closeInventory.onClick.AddListener(CloseInventory);

        PlayerData.Instance.OnInventoryChanged += OnInventoryChanged;
    }

    private void OnDisable()
    {
        Debug.Log($"Inventory Panel Disable");
        putOnSelectedEquipment.onClick.RemoveListener(PutOnSelectedEquipment);
        sellSelectedEquipment.onClick.RemoveListener(SellSelectedEquipment);
        takeOffWarnEquipment.onClick.RemoveListener(TakeOffWarnEquipment);
        closeInventory.onClick.RemoveListener(CloseInventory);

        PlayerData.Instance.OnInventoryChanged -= OnInventoryChanged;
    }

    private void OpenInventoryOfType(EquipmentType equipmentType, EquipmentOnPlayerType equipmentOnPlayerType)
    {
        ClearInventory();
        closeInventory.gameObject.SetActive(true);

        currentEquipmentOnPlayerType = equipmentOnPlayerType;
        currentEquipmentType = equipmentType;
        foreach (var equipment in PlayerData.Instance.Inventory.Where(e => e.Type == equipmentType))
            AddCell(equipment);
    }

    private void CloseInventory()
    {
        closeInventory.gameObject.SetActive(false);
        CloseItemPanels();
        ClearInventory();
    }

    public void AddCell(Equipment equipment)
    {
        if (equipment.Type != currentEquipmentType)
            return;
        var newCeil = Instantiate(cellPrefab, inventory.content);
        var ceilData = newCeil.GetComponent<CellPrefabData>();

        ceilData.Equipment = equipment;

        ceilData.Image.sprite = equipment.Icon;
        ceilData.Image.preserveAspect = true;

        ceilData.Button.onClick.AddListener(() => OpenSelectedItemPanel(equipment));
    }

    public void RemoveCell(Equipment equipment)
    {
        var cellToRemove = inventory.content.GetComponentsInChildren<CellPrefabData>()
            .FirstOrDefault(cell => cell.Equipment == equipment);

        if (cellToRemove != null)
        {
            Destroy(cellToRemove.gameObject);
        }
    }

    private void OpenSelectedItemPanel(Equipment equipment)
    {
        selectedEquipmentPanel.SetActive(true);
        selectedEquipmentUI.NewEquipment(equipment);
        sellText.text = $"{LocalizationManager.Instance.Get(LocalizationCategories.buttons, "sell_bt")}: {equipment.Cost}";
    }

    private void OpenWarnItemPanel(EquipmentOnPlayerType type)
    {
        warnEquipmentPanel.SetActive(false);
        if (PlayerData.Instance.EquipmentOnPlayer.TryGetValue(type, out var equip))
        {
            if (equip == null)
                return;
            warnEquipmentPanel.SetActive(true);
            warnEquipmentUI.NewEquipment(equip);
        }
    }

    private void CloseItemPanels()
    {
        selectedEquipmentPanel.SetActive(false);
        warnEquipmentPanel.SetActive(false);
        ClearInventory();
    }

    private void PutOnSelectedEquipment()
    {
        PlayerData.Instance.PutOnEquipment(selectedEquipmentUI.Equipment, currentEquipmentOnPlayerType);
        var equipmentOnPlayerCell = equipmentOnPlayerCells.Where(x => x.equipmentOnPlayerType == currentEquipmentOnPlayerType).Select(x => x.cellPrefabData).FirstOrDefault();
        equipmentOnPlayerCell.Image.sprite = selectedEquipmentUI.Equipment.Icon;
        CloseItemPanels();
    }

    private void SellSelectedEquipment()
    {
        PlayerData.Instance.Gold += selectedEquipmentUI.Equipment.Cost;
        PlayerData.Instance.RemoveEquipment(selectedEquipmentUI.Equipment);
        CloseItemPanels();
    }

    private void TakeOffWarnEquipment()
    {
        PlayerData.Instance.TakeOffEquipment(currentEquipmentOnPlayerType);
        var ceil = equipmentOnPlayerCells.Where(cell => cell.equipmentOnPlayerType == currentEquipmentOnPlayerType).Select(ceil => ceil.cellPrefabData).FirstOrDefault();
        ceil.Image.sprite = null;
        CloseItemPanels();
    }

    private void OnInventoryChanged(Equipment equipment, bool type)
    {
        if(type)
        {
            AddCell(equipment);
        }
        else
        {
            RemoveCell(equipment);
        }
    }

    private void ClearInventory()
    {
        foreach (Transform child in inventory.content)
            Destroy(child.gameObject);
    }
}
