using UnityEngine;
using UnityEngine.UI;

public class SkillTreePanelManager : MonoBehaviour
{
    [Header("BaseStatsUpgrades")]
    [SerializeField] private Button buyDamageUpgrade;
    [SerializeField] private int damageUpgradeCost;

    [SerializeField] private Button buyHealthUpgrade;
    [SerializeField] private int healthUpgradeCost;

    [SerializeField] private Button buySpeedUpgrade;
    [SerializeField] private int speedUpgradeCost;


    public void Start()
    {
        buyDamageUpgrade.onClick.AddListener(() => BuyUpgrade("Damage", damageUpgradeCost));
    }


    public void BuyUpgrade(string statType, int cost)
    {
        Debug.Log("ffsdfsd");
        if (PlayerData.Instance.Gold < cost)
        {
            Debug.Log("Not enough gold to buy " + statType + " upgrade.");
            return;
        }
        PlayerData.Instance.Gold -= cost;
        switch (statType)
        {
            case "Damage":
                PlayerData.Instance.SkillsStats.SetBaseDamage(PlayerData.Instance.SkillsStats.GetBaseDamage() + 10);
                break;
            default:
                Debug.LogError("Unknown stat type: " + statType);
                break;
        }
        Debug.Log(statType + " upgrade purchased!");
    }
}
