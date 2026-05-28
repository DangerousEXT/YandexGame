using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button quitButton;

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
    }
}
