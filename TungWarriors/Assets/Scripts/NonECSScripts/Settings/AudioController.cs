using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using YG;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource soundSource;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string musicVolumeName = "MusicVolume";
    [SerializeField] private string soundVolumeName = "SoundVolume";

    [Header("Music Clips")]
    [SerializeField] private AudioClip menuBackgroundMusic = null;
    [SerializeField] private AudioClip gameBackgroundMusic = null;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip onButtonClickSound = null;
    [SerializeField] private AudioClip enemyDamagedSound = null;
    [SerializeField] private AudioClip playerDamagedSound = null;

    private const float minVolumeDB = -80;
    private const float maxVolumeDB = 0;

    #region PublicFields
    public AudioClip MenuBackgroundMusic => menuBackgroundMusic;
    public AudioClip GameBackgroundMusic => gameBackgroundMusic;
    public AudioClip OnButtonClickSound => onButtonClickSound;
    public AudioClip EnemyDamagedSound => enemyDamagedSound;
    public AudioClip PlayerDamagedSound => playerDamagedSound;

    public float MusicVolume => audioMixer.GetFloat(musicVolumeName, out var volume) ? Mathf.InverseLerp(minVolumeDB, maxVolumeDB, volume) : 0.5f;

    public float SoundVolume => audioMixer.GetFloat(soundVolumeName, out var volume) ? Mathf.InverseLerp(minVolumeDB, maxVolumeDB, volume) : 0.5f;
    #endregion

    private void Awake()
    {
        
        if (Instance != null)
        {
            Debug.LogWarning("Multiple AudioController instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadSaves();
    }

    public void LoadSaves()
    {
        if (YG2.saves == null) return;

        ChangeMusicVolume(YG2.saves.musicVolume);
        ChangeSoundVolume(YG2.saves.soundVolume);
    }

    public void Save()
    {
        YG2.saves.musicVolume = MusicVolume;

        YG2.saves.soundVolume = SoundVolume;
    }

    public void ChangeMusicVolume(float volume)
    {
        audioMixer.SetFloat(musicVolumeName, Mathf.Lerp(minVolumeDB, maxVolumeDB, volume));
    }

    public void ChangeSoundVolume(float volume)
    {
        audioMixer.SetFloat(soundVolumeName, Mathf.Lerp(minVolumeDB, maxVolumeDB, volume));
    }

    public void PlayMusic(AudioClip audioClip)
    {
        musicSource.clip = audioClip;
        musicSource.Play();
    }

    public void PlaySound(AudioClip audioClip)
    {
        soundSource.PlayOneShot(audioClip);
    }
}