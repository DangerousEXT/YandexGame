using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;

    [Header("Change Language")]
    [SerializeField] private LanguageChanger languageChanger;

    [Header("Change Volume")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider soundVolumeSlider;

    [Header("Close Button")]
    [SerializeField] private Button closePanelButton;

    private void Awake()
    {
        musicVolumeSlider.value = AudioController.Instance.MusicVolume;
        soundVolumeSlider.value = AudioController.Instance.SoundVolume;
    }

    private void OnEnable()
    {
        closePanelButton.onClick.AddListener(ClosePanel);
        musicVolumeSlider.onValueChanged.AddListener(AudioController.Instance.ChangeMusicVolume);
        soundVolumeSlider.onValueChanged.AddListener(AudioController.Instance.ChangeSoundVolume);
    }

    private void OnDisable()
    {
        closePanelButton.onClick.RemoveListener(ClosePanel);
        musicVolumeSlider.onValueChanged.RemoveListener(AudioController.Instance.ChangeMusicVolume);
        soundVolumeSlider.onValueChanged.RemoveListener(AudioController.Instance.ChangeSoundVolume);
    }

    private void ClosePanel()
    {
        settingsPanel.SetActive(false);
    }
}
