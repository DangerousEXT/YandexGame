using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIController : MonoBehaviour
{
    public static GameUIController Instance { get; private set; }
    [SerializeField] private HUDPanel hudPanel;
    [SerializeField] private PausePanel pausePanel;
    [SerializeField] private GameOverPanel gameOverPanel;
    [SerializeField] private RevivePanel revivePanel;
    [SerializeField] private LevelUpSelectionPanel levelUpSelectionPanel;

    public bool IsPaused { get; private set; }

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

    private void Start()
    {
        if (levelUpSelectionPanel != null) //
            levelUpSelectionPanel.Hide();
    }

    public void TogglePause(bool pause)
    {
        IsPaused = pause;
        var defaultWorld = World.DefaultGameObjectInjectionWorld;
        if (defaultWorld == null || !defaultWorld.IsCreated) return;
        var simGroup = defaultWorld.GetExistingSystemManaged<SimulationSystemGroup>();
        if (simGroup != null) simGroup.Enabled = !IsPaused;
        var fixedGroup = defaultWorld.GetExistingSystemManaged<FixedStepSimulationSystemGroup>();
        if (fixedGroup != null) fixedGroup.Enabled = !IsPaused;
    }

    public void QuitToMainMenu()
    {
        TogglePause(false);
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
        TogglePause(true);
        pausePanel.Show();
    }

    public void ShowLevelUpPanel(List<LevelUpCardViewData> cards)
    {
        TogglePause(true);
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
}