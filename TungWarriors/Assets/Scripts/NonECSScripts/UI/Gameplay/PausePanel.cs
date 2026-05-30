using UnityEngine;
using UnityEngine.UI;

public class PausePanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitButton;

    private void OnEnable()
    {
        resumeButton.onClick.AddListener(Hide);
        quitButton.onClick.AddListener(GameUIController.Instance.QuitToMainMenu);

        resumeButton.onClick.AddListener(AudioController.Instance.PlayButtonClick);
        quitButton.onClick.AddListener(AudioController.Instance.PlayButtonClick);
    }

    private void OnDisable()
    {
        resumeButton.onClick.RemoveListener(Hide);
        quitButton.onClick.RemoveListener(GameUIController.Instance.QuitToMainMenu);

        resumeButton.onClick.RemoveListener(AudioController.Instance.PlayButtonClick);
        quitButton.onClick.RemoveListener(AudioController.Instance.PlayButtonClick);
    }

    public void Show() => panelRoot.SetActive(true);

    private void Hide()
    {
        panelRoot.SetActive(false);
        GameUIController.Instance.TogglePause(false);
    }
}
