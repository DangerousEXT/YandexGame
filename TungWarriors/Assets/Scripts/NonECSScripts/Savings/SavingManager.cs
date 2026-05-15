using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using YG;

public class SaveManager : MonoBehaviour
{
    private static SaveManager _instance;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        YG2.onHideWindowGame += SaveGame;
        Application.wantsToQuit += OnQuit;
        YG2.onGetSDKData += LoadGame;
        YG2.onDefaultSaves += DefaultSaves;
    }

    private void OnDisable()
    {
        YG2.onHideWindowGame -= SaveGame;
        Application.wantsToQuit -= OnQuit;
        YG2.onGetSDKData -= LoadGame;
        YG2.onDefaultSaves -= DefaultSaves;
    }

    private void DefaultSaves()
    {
        Debug.Log("Start DefaultSave");
        YG2.saves.gold = 1000;
        YG2.saves.gems = 0;
        YG2.saves.rubies = 0;
        YG2.saves.inventory = new();
        YG2.saves.equipmentOnPlayer = new();
        Debug.Log("End DefaultSave");
    }

    private void SaveGame()
    {
        Debug.Log("Start SaveGame");
        YG2.saves.gold = PlayerData.Instance.Gold;
        YG2.saves.gems = PlayerData.Instance.Gems;
        YG2.saves.rubies = PlayerData.Instance.Rubies;
        YG2.saves.inventory = YG2.saves.SerializeInventory(PlayerData.Instance.Inventory);
        YG2.saves.equipmentOnPlayer = YG2.saves.SerializeEquipmentOnPlayer(PlayerData.Instance.EquipmentOnPlayer);
        
        YG2.SaveProgress();

        Debug.Log("End SaveGame");
    }
    private void LoadGame()
    {
        Debug.Log("Start LoadGame");
        while (PlayerData.Instance == null)
        {
            Invoke(nameof(LoadGame), 0.1f);  // повторяем через 0.1 сек
            return;
        }

        PlayerData.Instance.Gold = YG2.saves.gold;
        PlayerData.Instance.Gems = YG2.saves.gems;
        PlayerData.Instance.Rubies = YG2.saves.rubies;
        PlayerData.Instance.Inventory = YG2.saves.DeserializeInventory();
        PlayerData.Instance.EquipmentOnPlayer = YG2.saves.DeserializeEquipmentOnPlayer();
        Debug.Log("End LoadGame");
    }

    private bool OnQuit()
    {
        SaveGame();
        return true;
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
        }
    }
}