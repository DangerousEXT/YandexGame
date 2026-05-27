using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YG;

public class MainMenuPanelManager : MonoBehaviour
{
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _exitGame;
    [SerializeField] private TextMeshProUGUI _bestTimeText;
    [SerializeField] private GameObject _bestTimeTextPrefab;
    [SerializeField] private Transform _bestTimeTextParent;

    private void Awake()
    {
        EnsureBestTimeLabel();
        //UpdateBestTimeLabel();
    }

    private void OnEnable()
    {
        _exitGame.onClick.AddListener(OnExitButtonClicked);
        _playButton.onClick.AddListener(OnPlayButtonClicked);
        //YG2.onSwitchLang += OnLanguageChanged;

        //if (PlayerData.Instance != null)
        //    PlayerData.Instance.OnBestSurvivalTimeChanged += OnBestSurvivalTimeChanged;

        //UpdateBestTimeLabel();
    }

    private void OnDisable()
    {
        _exitGame.onClick.RemoveListener(OnExitButtonClicked);
        _playButton.onClick.RemoveListener(OnPlayButtonClicked);
        //YG2.onSwitchLang -= OnLanguageChanged;

        //if (PlayerData.Instance != null)
        //    PlayerData.Instance.OnBestSurvivalTimeChanged -= OnBestSurvivalTimeChanged;
    }

    private void OnPlayButtonClicked()
    {
        SceneManager.LoadScene("DefaultLevel");
    }

    private void OnExitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void EnsureBestTimeLabel()
    {
        if (_bestTimeText != null)
            return;

        if (_bestTimeTextPrefab != null)
        {
            var parent = _bestTimeTextParent != null ? _bestTimeTextParent : transform;
            var instance = Instantiate(_bestTimeTextPrefab, parent, false);
            instance.name = _bestTimeTextPrefab.name;

            _bestTimeText = instance.GetComponent<TextMeshProUGUI>();
            if (_bestTimeText == null)
                _bestTimeText = instance.GetComponentInChildren<TextMeshProUGUI>(true);

            if (_bestTimeText != null)
                return;

            Debug.LogWarning($"Prefab '{_bestTimeTextPrefab.name}' does not contain TextMeshProUGUI component.", this);
        }

        var existing = transform.Find("BestSurvivalTimeText");
        if (existing != null)
        {
            if (existing.TryGetComponent(out TextMeshProUGUI existingText))
            {
                _bestTimeText = existingText;
                return;
            }

            var childText = existing.GetComponentInChildren<TextMeshProUGUI>(true);
            if (childText != null)
            {
                _bestTimeText = childText;
                return;
            }
        }

        var labelObject = new GameObject("BestSurvivalTimeText", typeof(RectTransform));
        labelObject.transform.SetParent(transform, false);

        var rectTransform = (RectTransform)labelObject.transform;
        rectTransform.anchorMin = new Vector2(0.18f, 0.47f);
        rectTransform.anchorMax = new Vector2(0.82f, 0.56f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        var textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(labelObject.transform, false);

        var textRectTransform = (RectTransform)textObject.transform;
        textRectTransform.anchorMin = Vector2.zero;
        textRectTransform.anchorMax = Vector2.one;
        textRectTransform.offsetMin = Vector2.zero;
        textRectTransform.offsetMax = Vector2.zero;

        _bestTimeText = textObject.GetComponent<TextMeshProUGUI>();
        var templateText = _playButton != null ? _playButton.GetComponentInChildren<TextMeshProUGUI>(true) : null;
        _bestTimeText.font = templateText != null ? templateText.font : TMP_Settings.defaultFontAsset;
        _bestTimeText.fontSize = templateText != null ? Mathf.Max(22f, templateText.fontSize - 8f) : 28f;
        _bestTimeText.color = templateText != null ? templateText.color : new Color(0.16f, 0.16f, 0.16f, 1f);
        _bestTimeText.alignment = TextAlignmentOptions.Center;
        _bestTimeText.enableWordWrapping = false;
        _bestTimeText.raycastTarget = false;
    }

    //private void UpdateBestTimeLabel()
    //{
    //    if (_bestTimeText == null)
    //        return;

    //    var bestTimeMilliseconds = PlayerData.Instance != null
    //        ? PlayerData.Instance.BestSurvivalTimeMilliseconds
    //        : 0;
    //    var formattedTime = SurvivalLeaderboardService.FormatTime(bestTimeMilliseconds);

    //    var format = LocalizationManager.Instance != null
    //        ? LocalizationManager.Instance.Get(LocalizationCategories.leaderboard, "best_time_format")
    //        : null;

    //    if (string.IsNullOrWhiteSpace(format) || format == "best_time_format")
    //        format = "Best survival time {0}";

    //    _bestTimeText.text = string.Format(format, formattedTime);
    //}

    //private void OnBestSurvivalTimeChanged(int _)
    //{
    //    UpdateBestTimeLabel();
    //}

    //private void OnLanguageChanged(string _)
    //{
    //    UpdateBestTimeLabel();
    //}
}
