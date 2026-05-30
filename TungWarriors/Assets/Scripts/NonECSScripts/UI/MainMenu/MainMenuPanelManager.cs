using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenuPanelManager : MonoBehaviour
{
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _exitGame;

    private void OnEnable()
    {
        _exitGame.onClick.AddListener(OnExitButtonClicked);
        _playButton.onClick.AddListener(OnPlayButtonClicked);
    }

    private void OnDisable()
    {
        _exitGame.onClick.RemoveListener(OnExitButtonClicked);
        _playButton.onClick.RemoveListener(OnPlayButtonClicked);
    }

    private void OnPlayButtonClicked()
    {
        AudioController.Instance.PlayGameMusic(); //с отложением в 3 секунды
        SceneManager.LoadScene("DefaultLevel");
        StartCoroutine(StartGameMusic());
        
    }

    private IEnumerator StartGameMusic()
    {
        yield return new WaitForSecondsRealtime(3f);
        AudioController.Instance.PlayGameMusic(); //с отложением в 3 секунды
    }

    private void OnExitButtonClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
