using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public class GameUIController : MonoBehaviour
{
    [Flags]
    private enum PauseReason
    {
        None = 0,
        PauseMenu = 1 << 0,
        LevelUp = 1 << 1,
        Revive = 1 << 2,
        FocusLost = 1 << 3
    }

    private const float RunningTimeScale = 1f;
    private const float PausedTimeScale = 0f;

    public static GameUIController Instance { get; private set; }

    [SerializeField] private HUDPanel hudPanel;
    [SerializeField] private PausePanel pausePanel;
    [SerializeField] private GameOverPanel gameOverPanel;
    [SerializeField] private RevivePanel revivePanel;
    [SerializeField] private LevelUpSelectionPanel levelUpSelectionPanel;

    public bool IsPaused { get; private set; }

    private PauseReason _requestedPauseReasons;
    private bool _pluginPauseState;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Multiple GameUIController instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        YG2.onPauseGame += OnPluginPauseStateChanged;
    }

    private void OnDisable()
    {
        YG2.onPauseGame -= OnPluginPauseStateChanged;
    }

    private void Start()
    {
        if (levelUpSelectionPanel != null)
            levelUpSelectionPanel.Hide();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        SetPauseReason(PauseReason.FocusLost, !hasFocus);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        SetPauseReason(PauseReason.FocusLost, pauseStatus);
    }

    public void TogglePause(bool pause)
    {
        SetPauseReason(PauseReason.PauseMenu, pause);
    }

    public void QuitToMainMenu()
    {
        ResetPauseState();
        SceneManager.LoadScene("MainMenu");
    }

    public void UpdateGemsCollectedText(int gems)
    {
        hudPanel.UpdateGemsText(gems);
    }

    public void ShowGameOverUI()
    {
        StartCoroutine(ShowGameOverUICoroutine());
    }

    private IEnumerator ShowGameOverUICoroutine()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        gameOverPanel.ShowCanvas();
    }

    public void SwitchDeathPanel()
    {
        revivePanel.TogglePanel();
    }

    public void ShowPauseMenu()
    {
        if (pausePanel != null)
            pausePanel.Show();

        SetPauseReason(PauseReason.PauseMenu, true);
    }

    public void HidePauseMenu()
    {
        if (pausePanel != null)
            pausePanel.Hide();

        SetPauseReason(PauseReason.PauseMenu, false);
    }

    public void ShowLevelUpPanel(List<LevelUpCardViewData> cards)
    {
        SetPauseReason(PauseReason.LevelUp, true);

        if (levelUpSelectionPanel != null)
        {
            AudioController.Instance.PlayLevelUp();
            levelUpSelectionPanel.Show(cards, OnLevelUpCardSelected);
        }
    }

    public void HideLevelUpPanel()
    {
        if (levelUpSelectionPanel != null)
            levelUpSelectionPanel.Hide();

        SetPauseReason(PauseReason.LevelUp, false);
    }

    public void SetRevivePause(bool pause)
    {
        SetPauseReason(PauseReason.Revive, pause);
    }

    private void OnLevelUpCardSelected(Entity selectedCardEntity)
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null || !world.IsCreated)
            return;

        var entityManager = world.EntityManager;

        var playerQuery = entityManager.CreateEntityQuery(
            ComponentType.ReadOnly<PlayerTag>());

        if (playerQuery.IsEmptyIgnoreFilter)
        {
            playerQuery.Dispose();
            return;
        }

        var playerEntity = playerQuery.GetSingletonEntity();

        entityManager.SetComponentData(playerEntity, new SelectedLevelUpCard
        {
            Value = selectedCardEntity
        });
        entityManager.SetComponentEnabled<SelectedLevelUpCard>(playerEntity, true);

        playerQuery.Dispose();
    }

    private void OnPluginPauseStateChanged(bool pause)
    {
        _pluginPauseState = pause;
        ApplyEcsPauseState(CalculatePauseState());
    }

    private void SetPauseReason(PauseReason reason, bool pause)
    {
        var nextReasons = pause
            ? _requestedPauseReasons | reason
            : _requestedPauseReasons & ~reason;

        if (nextReasons == _requestedPauseReasons)
            return;

        _requestedPauseReasons = nextReasons;
        ApplyRequestedPauseState();
    }

    private void ApplyRequestedPauseState()
    {
        var requestedPause = _requestedPauseReasons != PauseReason.None;
        var pauseAudio = (_requestedPauseReasons & PauseReason.FocusLost) != 0;

        PauseGameYG.SetState(
            requestedPause ? PausedTimeScale : RunningTimeScale,
            pauseAudio,
            true);

        ApplyEcsPauseState(CalculatePauseState());
    }

    private bool CalculatePauseState()
    {
        return _pluginPauseState || _requestedPauseReasons != PauseReason.None;
    }

    private void ApplyEcsPauseState(bool pause)
    {
        IsPaused = pause;

        var defaultWorld = World.DefaultGameObjectInjectionWorld;
        if (defaultWorld == null || !defaultWorld.IsCreated)
            return;

        var simGroup = defaultWorld.GetExistingSystemManaged<SimulationSystemGroup>();
        if (simGroup != null)
            simGroup.Enabled = !pause;

        var fixedGroup = defaultWorld.GetExistingSystemManaged<FixedStepSimulationSystemGroup>();
        if (fixedGroup != null)
            fixedGroup.Enabled = !pause;
    }

    private void ResetPauseState()
    {
        _requestedPauseReasons = PauseReason.None;
        _pluginPauseState = false;

        PauseGameYG.SetState(RunningTimeScale, false, true);
        ApplyEcsPauseState(false);
    }
}
