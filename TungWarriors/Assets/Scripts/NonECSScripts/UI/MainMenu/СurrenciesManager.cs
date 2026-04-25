using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;

public class СurrenciesManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gold;
    [SerializeField] private TextMeshProUGUI gems;

    private bool isSubscribed = false;
    private void TrySubscribe()
    {
        if (isSubscribed) return;

        if (PlayerData.Instance == null)
        {
            Debug.LogWarning("PlayerData.Instance is null, will retry next frame");
            return;
        }

        PlayerData.Instance.OnGoldChanged += ChangeGold;
        PlayerData.Instance.OnGemsChanged += ChangeGems;
        isSubscribed = true;

        gold.text = PlayerData.Instance.Gold.ToString();
        gems.text = PlayerData.Instance.Gems.ToString();

    }

    private void Update()
    {
        if (!isSubscribed)
        {
            TrySubscribe();
        }
    }

    private void OnDisable()
    {
        PlayerData.Instance.OnGoldChanged -= ChangeGold; 
        PlayerData.Instance.OnGemsChanged -= ChangeGems;
    }

    private void ChangeGold(int value)
    {
        gold.text = value.ToString();
    }

    private void ChangeGems(int value)
    {
        gems.text = value.ToString();
    }
}
