using Assets.Scripts.DeathConsequencesSystems;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class RevivePanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button consentToAdvButton;
    [SerializeField] private Button rejectOfAdvButton;

    private void OnEnable()
    {
        consentToAdvButton.onClick.AddListener(OnConsentClicked);
        rejectOfAdvButton.onClick.AddListener(TogglePanel);

        consentToAdvButton.onClick.AddListener(AudioController.Instance.PlayButtonClick);
        rejectOfAdvButton.onClick.AddListener(AudioController.Instance.PlayButtonClick);
    }

    private void OnDisable()
    {
        consentToAdvButton.onClick.RemoveListener(OnConsentClicked);
        rejectOfAdvButton.onClick.RemoveListener(TogglePanel);

        consentToAdvButton.onClick.RemoveListener(AudioController.Instance.PlayButtonClick);
        rejectOfAdvButton.onClick.RemoveListener(AudioController.Instance.PlayButtonClick);
    }

    public void TogglePanel()
    {
        bool isShowing = !panelRoot.activeInHierarchy;
        panelRoot.SetActive(isShowing);
        GameUIController.Instance.TogglePause(isShowing);
        SetPlayerThinkingEcsState(isShowing);
    }

    private void OnConsentClicked()
    {
        RewardedAdvController.Instance.ShowRewardedAdv(RewardedAdvAwards.extraLife);
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
