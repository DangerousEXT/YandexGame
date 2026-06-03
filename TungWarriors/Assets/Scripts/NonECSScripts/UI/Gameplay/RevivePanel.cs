using Assets.Scripts.DeathConsequencesSystems;
using GameAnalyticsSDK;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class RevivePanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button consentToAdvButton;
    [SerializeField] private Button rejectOfAdvButton;
    [SerializeField] private HUDPanel hudPanel;

    private void OnEnable()
    {
        consentToAdvButton.onClick.AddListener(OnConsentClicked);
        rejectOfAdvButton.onClick.AddListener(OnRejectClicked);

        consentToAdvButton.onClick.AddListener(AudioController.Instance.PlayButtonClick);
        rejectOfAdvButton.onClick.AddListener(AudioController.Instance.PlayButtonClick);
    }

    private void OnDisable()
    {
        consentToAdvButton.onClick.RemoveListener(OnConsentClicked);
        rejectOfAdvButton.onClick.RemoveListener(OnRejectClicked);

        consentToAdvButton.onClick.RemoveListener(AudioController.Instance.PlayButtonClick);
        rejectOfAdvButton.onClick.RemoveListener(AudioController.Instance.PlayButtonClick);
    }

    public void TogglePanel()
    {
        bool isShowing = !panelRoot.activeInHierarchy;
        panelRoot.SetActive(isShowing);
        GameUIController.Instance.SetRevivePause(isShowing);
        SetPlayerThinkingEcsState(isShowing);
    }

    private void OnConsentClicked()
    {
        var secondsSurvived = hudPanel.GetSurvivedSeconds();
        var minute = secondsSurvived / 60;
        GameAnalytics.NewDesignEvent($"Revive_Decision:Accepted:Minute_{minute}");
        RewardedAdvController.Instance.ShowRewardedAdv(RewardedAdvAwards.extraLife);
    }

    private void OnRejectClicked()
    {
        var secondsSurvived = hudPanel.GetSurvivedSeconds();
        var minute = secondsSurvived / 60;
        GameAnalytics.NewDesignEvent($"Revive_Decision:Declined:Minute_{minute}");
        TogglePanel();
    }

    private void SetPlayerThinkingEcsState(bool isThinking)
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null || !world.IsCreated)
            return;

        var playerQuery = world.EntityManager.CreateEntityQuery(typeof(PlayerTag));
        if (!playerQuery.IsEmpty)
        {
            var playerEntity = playerQuery.GetSingletonEntity();
            world.EntityManager.SetComponentEnabled<PlayerThinkingFlag>(playerEntity, isThinking);
        }

        playerQuery.Dispose();
    }
}
