using GameAnalyticsSDK;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button quitButton;
    [SerializeField] private HUDPanel hudPanel;

    private void OnEnable()
    {
        quitButton.onClick.AddListener(GameUIController.Instance.QuitToMainMenu);
        quitButton.onClick.AddListener(AudioController.Instance.PlayButtonClick);
    }
    private void OnDisable()
    {
        quitButton.onClick.RemoveListener(GameUIController.Instance.QuitToMainMenu);
        quitButton.onClick.RemoveListener(AudioController.Instance.PlayButtonClick);
    }

    public void ShowCanvas()
    {
        panelRoot.SetActive(true);
        var secondsSurvived = hudPanel.GetSurvivedSeconds();
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, "Endless_Run", secondsSurvived);
        GameAnalytics.NewDesignEvent("Run_Ended:Player_Died");
    }
}
