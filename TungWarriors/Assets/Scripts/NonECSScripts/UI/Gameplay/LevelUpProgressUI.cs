using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class LevelProgressUI : MonoBehaviour
{
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TextMeshProUGUI levelText;

    private EntityManager em;
    private EntityQuery query;
    private int cachedLevel = -1;
    private int cachedExp = -1;

    private void Start()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null || !world.IsCreated) { enabled = false; return; }
        em = world.EntityManager;
        query = em.CreateEntityQuery(
            ComponentType.ReadOnly<PlayerTag>(),
            ComponentType.ReadOnly<PlayerLevel>(),
            ComponentType.ReadOnly<PlayerExperience>());
    }

    private void Update()
    {
        if (query.IsEmptyIgnoreFilter) 
            return;
        var entity = query.GetSingletonEntity();
        var level = em.GetComponentData<PlayerLevel>(entity);
        var exp = em.GetComponentData<PlayerExperience>(entity);
        if (level.Value == cachedLevel && exp.Current == cachedExp) 
            return;
        cachedLevel = level.Value;
        cachedExp = exp.Current;
        if (progressSlider != null)
        {
            progressSlider.maxValue = exp.RequiredForNext;
            progressSlider.value = exp.Current;
        }
        if (levelText != null)
            levelText.text = $"{LocalizationManager.Instance.Get(LocalizationCategories.game, "lvl")} {level.Value}";
    }
}