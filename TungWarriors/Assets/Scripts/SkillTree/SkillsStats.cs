using Unity.VisualScripting;
using UnityEngine;

public class SkillsStats : MonoBehaviour
{
    private static SkillsStats Instance;
    public static SkillsStats getInstance()
    {
        if (Instance == null)
            Instance = new SkillsStats();
        return Instance;
    }

    private float BaseDamageUp{ get; set; }
    private float BaseSpeedUp { get; set; }
    private float Health { get; set; }

    private float HealthPercent { get; set; }

    public void SetBaseDamage(float value)
    {
        BaseDamageUp = value;
    }
    public float GetBaseDamage()
    {
        return BaseDamageUp;
    }

    public void SetBaseSpeed(float value)
    {
        BaseSpeedUp = value;
    }
    public float GetBaseSpeed()
    {
        return BaseSpeedUp;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
