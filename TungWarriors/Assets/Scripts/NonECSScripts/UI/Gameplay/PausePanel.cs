using UnityEngine;
using UnityEngine.UI;

public class PausePanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitButton;

    private void OnEnable()
    {
        resumeButton.onClick.AddListener(OnResumeClicked);
        quitButton.onClick.AddListener(GameUIController.Instance.QuitToMainMenu);

        resumeButton.onClick.AddListener(AudioController.Instance.PlayButtonClick);
        quitButton.onClick.AddListener(AudioController.Instance.PlayButtonClick);
    }

    private void OnDisable()
    {
        resumeButton.onClick.RemoveListener(OnResumeClicked);
        quitButton.onClick.RemoveListener(GameUIController.Instance.QuitToMainMenu);

        resumeButton.onClick.RemoveListener(AudioController.Instance.PlayButtonClick);
        quitButton.onClick.RemoveListener(AudioController.Instance.PlayButtonClick);
    }

    public void Show() => panelRoot.SetActive(true);

    public void Hide() => panelRoot.SetActive(false);

    private void OnResumeClicked()
    {
        GameUIController.Instance.HidePauseMenu();
    }
}
