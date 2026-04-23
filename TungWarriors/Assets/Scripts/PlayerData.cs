using System;
using System.Collections.Generic;
using UnityEngine;
using YG;
using static YG.YG2;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    [Header("Resources")]
    [SerializeField] private int gold;
    [SerializeField] private int gems;
    [SerializeField] private int rubies;

    [Header("Shop")]
    [SerializeField] private List<ItemData> shopItems = new();

    [Header("Inventory")]
    [SerializeField] private List<Equipment> inventory = new();

    [Header("Equipment On Player")]
    [SerializeField] private Dictionary<EquipmentOnPlayerType, Equipment> equipmentOnPlayer = new();

    public event Action<int> OnGoldChanged;
    public event Action<int> OnGemsChanged;
    public event Action<Equipment, bool> OnInventoryChanged;

    public int Gold
    {
        get => gold;
        set
        {
            gold = Mathf.Max(0, value);
            OnGoldChanged?.Invoke(gold);
        }
    }

    public int Gems
    {
        get => gems;
        set
        {
            gems = Mathf.Max(0, value);
            OnGemsChanged?.Invoke(gems);
        }
    }

    public List<Equipment> Inventory => inventory;
    public Dictionary<EquipmentOnPlayerType, Equipment> EquipmentOnPlayer => equipmentOnPlayer;
    public List<ItemData> ShopItems => shopItems;

    private void Awake()
    {
        Debug.Log("Instance PlayerData");
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    
    public void AddEquipment(Equipment equip)
    {
        inventory.Add(equip);
        OnInventoryChanged?.Invoke(equip, true);
    }

    public void RemoveEquipment(Equipment equip)
    {
        inventory.Remove(equip);
        OnInventoryChanged?.Invoke(equip, false);
    }

    public void PutOnEquipment(Equipment equip, EquipmentOnPlayerType type)
    {
        if(equipmentOnPlayer.TryGetValue(type, out var equipment))
            if(equipment != null)
                AddEquipment(equipment);

        equipmentOnPlayer[type] = equip;
        RemoveEquipment(equip);
    }

    public void TakeOffEquipment(EquipmentOnPlayerType type)
    {
        if (equipmentOnPlayer.TryGetValue(type, out var equipment))
        {
            if (equipment == null)
                return;
            AddEquipment(equipment);
            equipmentOnPlayer[type] = null;
        }
    }
}