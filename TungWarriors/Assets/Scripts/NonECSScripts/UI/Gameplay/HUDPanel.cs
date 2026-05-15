using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class HUDPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gemsCollected;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Button pauseButton;

    private EntityQuery _timerQuery;
    private EntityManager _entityManager;
    private bool _timerQueryInitialized;
    private int _lastTimerSeconds = -1;

    private void Awake()
    {
        EnsureTimerText();
        TryInitializeTimerQuery();
    }

    private void OnEnable() => pauseButton.onClick.AddListener(OnPauseClicked);
    private void OnDisable() => pauseButton.onClick.RemoveListener(OnPauseClicked);

    private void Update()
    {
        if (!TryInitializeTimerQuery() || _timerQuery.IsEmptyIgnoreFilter)
            return;

        var timerState = _timerQuery.GetSingleton<MatchTimerState>();
        var wholeSeconds = Mathf.FloorToInt(timerState.ElapsedSeconds);
        if (wholeSeconds == _lastTimerSeconds)
            return;

        _lastTimerSeconds = wholeSeconds;
        UpdateTimerText(wholeSeconds);
    }

    public void UpdateGemsText(int gems)
    {
        gemsCollected.text = $"{gems}";
    }

    private void UpdateTimerText(int wholeSeconds)
    {
        if (timerText == null)
            return;

        var minutes = wholeSeconds / 60;
        var seconds = wholeSeconds % 60;
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private bool TryInitializeTimerQuery()
    {
        if (_timerQueryInitialized)
            return true;

        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null || !world.IsCreated)
            return false;

        _entityManager = world.EntityManager;
        _timerQuery = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<MatchTimerState>());
        _timerQueryInitialized = true;
        return true;
    }

    private void EnsureTimerText()
    {
        if (timerText != null || gemsCollected == null)
            return;

        var timerObject = new GameObject("TimerText", typeof(RectTransform));
        timerObject.transform.SetParent(transform, false);

        var rectTransform = timerObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = new Vector2(0f, -16f);
        rectTransform.sizeDelta = new Vector2(220f, 48f);

        timerText = timerObject.AddComponent<TextMeshProUGUI>();
        timerText.font = gemsCollected.font;
        timerText.fontSharedMaterial = gemsCollected.fontSharedMaterial;
        timerText.fontSize = gemsCollected.fontSize;
        timerText.color = gemsCollected.color;
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.textWrappingMode = TextWrappingModes.NoWrap;
        timerText.raycastTarget = false;
        timerText.text = "00:00";
    }

    private void OnPauseClicked()
    {
        GameUIController.Instance.ShowPauseMenu();
    }
}
