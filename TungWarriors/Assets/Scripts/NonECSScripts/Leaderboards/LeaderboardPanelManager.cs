using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;



//public class LeaderboardPanelManager : MonoBehaviour
//{
//    private sealed class LeaderboardRowWidgets
//    {
//        public GameObject Root;
//        public TextMeshProUGUI RankText;
//        public TextMeshProUGUI NameText;
//        public TextMeshProUGUI ScoreText;
//    }

//    private readonly List<GameObject> _spawnedTopRows = new();

//    private TextMeshProUGUI _titleText;
//    private TextMeshProUGUI _statusText;
//    private TextMeshProUGUI _selfSectionLabel;
//    private GameObject _headerRow;
//    private LeaderboardRowWidgets _headerWidgets;
//    private RectTransform _topRowsContainer;
//    private LeaderboardRowWidgets _selfRowWidgets;
//    private SurvivalLeaderboardEntryData[] _lastTopEntries = System.Array.Empty<SurvivalLeaderboardEntryData>();
//    private bool _leaderboardConfigChecked;

//    private bool _uiBuilt;

//    private void Awake()
//    {
//        EnsureUi();
//    }

//    private void OnEnable()
//    {
//        YG2.onSwitchLang += OnLanguageChanged;
//        YG2.onGetSDKData += Refresh;
//        SurvivalLeaderboardService.LeaderboardDataChanged += Refresh;
//        Refresh();
//    }

//    private void OnDisable()
//    {
//        YG2.onSwitchLang -= OnLanguageChanged;
//        YG2.onGetSDKData -= Refresh;
//        SurvivalLeaderboardService.LeaderboardDataChanged -= Refresh;
//    }

//    private void Refresh()
//    {
//        EnsureUi();
//        UpdateStaticTexts();

//        if (!YG2.player.auth)
//        {
//            ClearTopRows();
//            SetHeaderVisible(false);
//            SetSelfRowVisible(false);
//            SetSelfSectionVisible(false);
//            _statusText.gameObject.SetActive(true);
//            _statusText.text = GetText("authorize_required", "Authorize to submit your record");
//            return;
//        }

//        SurvivalLeaderboardService.TrySyncBestWithServer();

//        ClearTopRows();
//        SetHeaderVisible(true);
//        SetSelfRowVisible(false);
//        SetSelfSectionVisible(false);
//        _statusText.gameObject.SetActive(true);
//        _statusText.text = GetText("loading", "Loading...");

//        SurvivalLeaderboardService.GetTopEntries(
//            onSuccess: ApplyLeaderboardSnapshot,
//            onError: error =>
//            {
//                Debug.LogWarning($"Failed to load leaderboard top entries: {error}");
//                ApplyLeaderboardSnapshot(new SurvivalLeaderboardEntriesResponse
//                {
//                    entries = new SurvivalLeaderboardEntryData[0]
//                });
//            },
//            includeCurrentPlayer: true);
//    }

//    private void ApplyLeaderboardSnapshot(SurvivalLeaderboardEntriesResponse response)
//    {
//        ValidateLeaderboardConfiguration(response);
//        ClearTopRows();

//        var entries = response != null && response.entries != null
//            ? response.entries
//            : new SurvivalLeaderboardEntryData[0];
//        _lastTopEntries = entries;

//        if (entries.Length == 0)
//        {
//            _statusText.gameObject.SetActive(true);
//            _statusText.text = GetText("empty", "No leaderboard entries yet");
//        }
//        else
//        {
//            _statusText.gameObject.SetActive(false);
//            for (var i = 0; i < entries.Length; i++)
//            {
//                var rowWidgets = CreateRow(_topRowsContainer, highlight: false, $"TopRow_{i + 1}");
//                FillRow(rowWidgets, entries[i]);
//                _spawnedTopRows.Add(rowWidgets.Root);
//            }
//        }

//        if (response == null || !response.playerEntryFound || response.playerEntry == null)
//        {
//            ShowNoPlayerEntry();
//            return;
//        }

//        ApplyPlayerEntry(response.playerEntry);
//    }

//    private void ApplyPlayerEntry(SurvivalLeaderboardEntryData entry)
//    {
//        var playerShownInTop = ContainsEntry(_lastTopEntries, entry);
//        if (playerShownInTop && _lastTopEntries.Length > 0)
//        {
//            SetSelfSectionVisible(false);
//            SetSelfRowVisible(false);
//            return;
//        }

//        SetSelfSectionVisible(true);
//        _selfSectionLabel.text = GetText("your_place", "Your place");
//        FillRow(_selfRowWidgets, entry);
//        SetSelfRowVisible(true);
//    }

//    private void ShowNoPlayerEntry()
//    {
//        SetSelfSectionVisible(true);
//        _selfSectionLabel.text = GetText("no_personal_record", "You don't have a leaderboard record yet");
//        SetSelfRowVisible(false);
//    }

//    private void OnLanguageChanged(string _)
//    {
//        Refresh();
//    }

//    private void EnsureUi()
//    {
//        if (_uiBuilt)
//            return;

//        var rectTransform = transform as RectTransform;
//        if (rectTransform == null)
//            return;

//        _titleText = CreateText("LeaderboardTitle", rectTransform, new Vector2(0.07f, 0.86f), new Vector2(0.93f, 0.95f), 34, FontStyles.Bold, TextAlignmentOptions.Center);
//        _statusText = CreateText("LeaderboardStatus", rectTransform, new Vector2(0.08f, 0.40f), new Vector2(0.92f, 0.58f), 28, FontStyles.Normal, TextAlignmentOptions.Center);

//        _headerRow = new GameObject("HeaderRow", typeof(RectTransform), typeof(Image));
//        _headerRow.transform.SetParent(rectTransform, false);
//        var headerRect = (RectTransform)_headerRow.transform;
//        headerRect.anchorMin = new Vector2(0.07f, 0.78f);
//        headerRect.anchorMax = new Vector2(0.93f, 0.84f);
//        headerRect.offsetMin = Vector2.zero;
//        headerRect.offsetMax = Vector2.zero;
//        var headerImage = _headerRow.GetComponent<Image>();
//        headerImage.color = new Color(1f, 1f, 1f, 0.18f);
//        _headerWidgets = CreateRowTexts(headerRect, "HeaderTexts", 24, FontStyles.Bold);

//        var scrollRoot = new GameObject("LeaderboardScrollRoot", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
//        scrollRoot.transform.SetParent(rectTransform, false);
//        var scrollRectTransform = (RectTransform)scrollRoot.transform;
//        scrollRectTransform.anchorMin = new Vector2(0.07f, 0.24f);
//        scrollRectTransform.anchorMax = new Vector2(0.93f, 0.77f);
//        scrollRectTransform.offsetMin = Vector2.zero;
//        scrollRectTransform.offsetMax = Vector2.zero;

//        var scrollImage = scrollRoot.GetComponent<Image>();
//        scrollImage.color = new Color(1f, 1f, 1f, 0.08f);
//        var mask = scrollRoot.GetComponent<Mask>();
//        mask.showMaskGraphic = false;

//        var contentRoot = new GameObject("LeaderboardContent", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
//        contentRoot.transform.SetParent(scrollRoot.transform, false);
//        _topRowsContainer = (RectTransform)contentRoot.transform;
//        _topRowsContainer.anchorMin = new Vector2(0f, 1f);
//        _topRowsContainer.anchorMax = new Vector2(1f, 1f);
//        _topRowsContainer.pivot = new Vector2(0.5f, 1f);
//        _topRowsContainer.offsetMin = new Vector2(8f, 0f);
//        _topRowsContainer.offsetMax = new Vector2(-8f, 0f);


//        var layoutGroup = contentRoot.GetComponent<VerticalLayoutGroup>();
//        layoutGroup.childAlignment = TextAnchor.UpperCenter;
//        layoutGroup.childControlHeight = true;
//        layoutGroup.childControlWidth = true;
//        layoutGroup.childForceExpandHeight = true;
//        layoutGroup.childForceExpandWidth = true;
//        layoutGroup.spacing = 6f;
//        layoutGroup.padding = new RectOffset(0, 0, 8, 8);

//        var sizeFitter = contentRoot.GetComponent<ContentSizeFitter>();
//        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
//        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

//        var scrollRect = scrollRoot.GetComponent<ScrollRect>();
//        scrollRect.content = _topRowsContainer;
//        scrollRect.viewport = scrollRectTransform;
//        scrollRect.horizontal = false;
//        scrollRect.vertical = true;
//        scrollRect.movementType = ScrollRect.MovementType.Clamped;
//        scrollRect.scrollSensitivity = 30f;

//        _selfSectionLabel = CreateText("LeaderboardSelfLabel", rectTransform, new Vector2(0.07f, 0.16f), new Vector2(0.93f, 0.22f), 24, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
//        _selfRowWidgets = CreateRow(rectTransform, highlight: true, "CurrentPlayerRow");
//        var selfRowRect = (RectTransform)_selfRowWidgets.Root.transform;
//        selfRowRect.anchorMin = new Vector2(0.07f, 0.07f);
//        selfRowRect.anchorMax = new Vector2(0.93f, 0.14f);
//        selfRowRect.offsetMin = Vector2.zero;
//        selfRowRect.offsetMax = Vector2.zero;

//        _uiBuilt = true;
//        UpdateStaticTexts();
//        SetSelfSectionVisible(false);
//        SetSelfRowVisible(false);
//    }

//    private void UpdateStaticTexts()
//    {
//        _titleText.text = GetText("panel_title", "Best survival time");
//        _headerWidgets.RankText.text = GetText("rank_header", "Rank");
//        _headerWidgets.NameText.text = GetText("name_header", "Name");
//        _headerWidgets.ScoreText.text = GetText("time_header", "Time");
//    }

//    private LeaderboardRowWidgets CreateRow(Transform parent, bool highlight, string objectName)
//    {
//        var rowObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
//        rowObject.transform.SetParent(parent, false);

//        var layoutElement = rowObject.GetComponent<LayoutElement>();
//        layoutElement.preferredHeight = 38f;

//        var rowImage = rowObject.GetComponent<Image>();
//        rowImage.color = highlight
//            ? new Color(1f, 0.96f, 0.76f, 0.22f)
//            : new Color(1f, 1f, 1f, 0.12f);

//        var rectTransform = (RectTransform)rowObject.transform;
//        rectTransform.offsetMin = Vector2.zero;
//        rectTransform.offsetMax = Vector2.zero;

//        var widgets = CreateRowTexts(rectTransform, $"{objectName}_Texts", 22, FontStyles.Normal);
//        widgets.Root = rowObject;
//        return widgets;
//    }

//    private LeaderboardRowWidgets CreateRowTexts(RectTransform rowRect, string objectPrefix, int fontSize, FontStyles fontStyle)
//    {
//        return new LeaderboardRowWidgets
//        {
//            RankText = CreateColumnText($"{objectPrefix}_Rank", rowRect, new Vector2(0f, 0f), new Vector2(0.16f, 1f), fontSize, fontStyle, TextAlignmentOptions.Center),
//            NameText = CreateColumnText($"{objectPrefix}_Name", rowRect, new Vector2(0.16f, 0f), new Vector2(0.70f, 1f), fontSize, fontStyle, TextAlignmentOptions.MidlineLeft),
//            ScoreText = CreateColumnText($"{objectPrefix}_Score", rowRect, new Vector2(0.70f, 0f), new Vector2(1f, 1f), fontSize, fontStyle, TextAlignmentOptions.Center)
//        };
//    }

//    private void FillRow(LeaderboardRowWidgets widgets, SurvivalLeaderboardEntryData entry)
//    {
//        widgets.RankText.text = SurvivalLeaderboardService.FormatRank(entry.rank);
//        widgets.NameText.text = SurvivalLeaderboardService.GetPlayerDisplayName(entry.publicName);
//        widgets.ScoreText.text = SurvivalLeaderboardService.FormatTime(entry.score);
//    }

//    private bool ContainsEntry(SurvivalLeaderboardEntryData[] entries, SurvivalLeaderboardEntryData targetEntry)
//    {
//        if (entries == null || targetEntry == null)
//            return false;

//        for (var i = 0; i < entries.Length; i++)
//        {
//            var entry = entries[i];
//            if (entry == null)
//                continue;

//            if (!string.IsNullOrWhiteSpace(targetEntry.uniqueId) &&
//                entry.uniqueId == targetEntry.uniqueId)
//                return true;

//            if (entry.rank == targetEntry.rank &&
//                entry.score == targetEntry.score &&
//                entry.publicName == targetEntry.publicName)
//                return true;
//        }

//        return false;
//    }

//    private void ValidateLeaderboardConfiguration(SurvivalLeaderboardEntriesResponse response)
//    {
//        if (_leaderboardConfigChecked || response == null || response.leaderboard == null)
//            return;

//        _leaderboardConfigChecked = true;
//        var leaderboard = response.leaderboard;
//        if (!string.IsNullOrWhiteSpace(leaderboard.sortOrder) &&
//            (!leaderboard.sortOrder.Equals("DESC", System.StringComparison.OrdinalIgnoreCase) ||
//             leaderboard.invertSortOrder))
//        {
//            Debug.LogWarning(
//                $"Leaderboard '{SurvivalLeaderboardService.TechnicalName}' is configured with sortOrder='{leaderboard.sortOrder}', invertSortOrder={leaderboard.invertSortOrder}. " +
//                "For survival time the leaderboard should be sorted in descending order so the longest run is first.");
//        }
//    }

//    private TextMeshProUGUI CreateColumnText(string objectName, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, int fontSize, FontStyles fontStyle, TextAlignmentOptions alignment)
//    {
//        var text = CreateText(objectName, parent, anchorMin, anchorMax, fontSize, fontStyle, alignment);
//        text.margin = new Vector4(8f, 0f, 8f, 0f);
//        return text;
//    }

//    private TextMeshProUGUI CreateText(string objectName, Transform parent, Vector2 anchorMin, Vector2 anchorMax, int fontSize, FontStyles fontStyle, TextAlignmentOptions alignment)
//    {
//        var textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
//        textObject.transform.SetParent(parent, false);

//        var rectTransform = (RectTransform)textObject.transform;
//        rectTransform.anchorMin = anchorMin;
//        rectTransform.anchorMax = anchorMax;
//        rectTransform.offsetMin = Vector2.zero;
//        rectTransform.offsetMax = Vector2.zero;

//        var text = textObject.GetComponent<TextMeshProUGUI>();
//        text.font = TMP_Settings.defaultFontAsset;
//        text.fontSize = fontSize;
//        text.fontStyle = fontStyle;
//        text.alignment = alignment;
//        text.color = new Color(0.16f, 0.16f, 0.16f, 1f);
//        text.enableWordWrapping = false;
//        text.overflowMode = TextOverflowModes.Ellipsis;
//        text.raycastTarget = false;
//        return text;
//    }

//    private void ClearTopRows()
//    {
//        for (var i = 0; i < _spawnedTopRows.Count; i++)
//        {
//            if (_spawnedTopRows[i] != null)
//                Destroy(_spawnedTopRows[i]);
//        }

//        _spawnedTopRows.Clear();
//    }

//    private void SetHeaderVisible(bool isVisible)
//    {
//        if (_headerRow != null)
//            _headerRow.SetActive(isVisible);
//    }

//    private void SetSelfSectionVisible(bool isVisible)
//    {
//        if (_selfSectionLabel != null)
//            _selfSectionLabel.gameObject.SetActive(isVisible);
//    }

//    private void SetSelfRowVisible(bool isVisible)
//    {
//        if (_selfRowWidgets != null && _selfRowWidgets.Root != null)
//            _selfRowWidgets.Root.SetActive(isVisible);
//    }

//    private string GetText(string key, string fallback)
//    {
//        if (LocalizationManager.Instance == null)
//            return fallback;

//        var localized = LocalizationManager.Instance.Get(LocalizationCategories.leaderboard, key);
//        return string.IsNullOrWhiteSpace(localized) || localized == key
//            ? fallback
//            : localized;
//    }
//}
