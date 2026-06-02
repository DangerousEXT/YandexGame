using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using YG;

public class AudioController : MonoBehaviour
{
    private const float RunningTimeScale = 1f;
    private const float PausedTimeScale = 0f;

    public static AudioController Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource soundSource;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string musicVolumeName = "MusicVolume";
    [SerializeField] private string soundVolumeName = "SoundVolume";

    [Header("Music Clips")]
    [SerializeField] private AudioClip menuBackgroundMusic = null;      // Музыка меню
    [SerializeField] private AudioClip gameBackgroundMusic = null;       // Музыка игры

    [Header("Sound Effects - Menu")]
    [SerializeField] private AudioClip onButtonClickSound = null;        // Нажатие кнопки
    [SerializeField] private AudioClip itemObtainedSound = null;         // Получение предмета
    [SerializeField] private AudioClip itemUnpackedSound = null;         // Разбор предмета
    [SerializeField] private AudioClip itemEquippedSound = null;         // Надевание предмета

    [Header("Sound Effects - Game")]
    [SerializeField] private AudioClip shootSound = null;                // Звук выстрела
    [SerializeField] private AudioClip enemyDamagedSound = null;         // Урон врагу
    [SerializeField] private AudioClip playerDamagedSound = null;        // Получение урона
    [SerializeField] private AudioClip experiencePickupSound = null;     // Подбор опыта
    [SerializeField] private AudioClip levelUpSound = null;              // Новый уровень
    [SerializeField] private AudioClip batHitSound = null;               // Удар битой
    [SerializeField] private AudioClip playerDeathSound = null;          // Смерть игрока
    [SerializeField] private AudioClip playerReviveSound = null;         // Воскрешение
    [SerializeField] private AudioClip roundEndSound = null;             // Конец раунда

    private const float minVolumeDB = -80;
    private const float maxVolumeDB = 0;

    private bool _isMenuFocusPaused;

    #region PublicFields
    public AudioClip MenuBackgroundMusic => menuBackgroundMusic;
    public AudioClip GameBackgroundMusic => gameBackgroundMusic;
    public AudioClip OnButtonClickSound => onButtonClickSound;
    public AudioClip ItemObtainedSound => itemObtainedSound;
    public AudioClip ItemUnpackedSound => itemUnpackedSound;
    public AudioClip ItemEquippedSound => itemEquippedSound;
    public AudioClip ShootSound => shootSound;
    public AudioClip EnemyDamagedSound => enemyDamagedSound;
    public AudioClip PlayerDamagedSound => playerDamagedSound;
    public AudioClip ExperiencePickupSound => experiencePickupSound;
    public AudioClip LevelUpSound => levelUpSound;
    public AudioClip BatHitSound => batHitSound;
    public AudioClip PlayerDeathSound => playerDeathSound;
    public AudioClip PlayerReviveSound => playerReviveSound;
    public AudioClip RoundEndSound => roundEndSound;

    #endregion

    public float MusicVolume
    {
        get
        {
            if (audioMixer.GetFloat(musicVolumeName, out var volume))
                return volume <= minVolumeDB ? 0f : Mathf.Clamp01(Mathf.Pow(10.0f, volume / 20.0f));
            return 0.5f;
        }
    }

    public float SoundVolume
    {
        get
        {
            if (audioMixer.GetFloat(soundVolumeName, out var volume))
                return volume <= minVolumeDB ? 0f : Mathf.Clamp01(Mathf.Pow(10.0f, volume / 20.0f));
            return 0.5f;
        }
    }

    public Action OnVolumesChanged;

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

    private void OnEnable()
    {
        YG2.onFocusWindowGame += Pause;
    }

    private void OnDisable()
    {
        YG2.onFocusWindowGame -= Pause;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        SetMenuFocusPause(!hasFocus);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        SetMenuFocusPause(pauseStatus);
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
        var clampedVol = Mathf.Clamp(volume, 0.0001f, 1f);
        audioMixer.SetFloat(musicVolumeName, 20f * Mathf.Log10(clampedVol));
        OnVolumesChanged?.Invoke();
    }

    public void ChangeSoundVolume(float volume)
    {
        var clampedVol = Mathf.Clamp(volume, 0.0001f, 1f);
        audioMixer.SetFloat(soundVolumeName, 20f * Mathf.Log10(clampedVol));
        OnVolumesChanged?.Invoke();
    }

    private void Pause(bool pause)
    {
        if(!pause)
        {
            musicSource.Pause();
            soundSource.Pause();
        }
        else
        {
            musicSource.UnPause();
            soundSource.UnPause();
        }
    }

    private void SetMenuFocusPause(bool pause)
    {
        if (GameUIController.Instance != null)
            return;

        if (_isMenuFocusPaused == pause)
            return;

        _isMenuFocusPaused = pause;

        PauseGameYG.SetState(
            pause ? PausedTimeScale : RunningTimeScale,
            pause,
            true);
    }

    #region Play Methods
    public void PlayMusic(AudioClip audioClip)
    {
        if (musicSource == null || audioClip == null) return;
        musicSource.clip = audioClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySound(AudioClip audioClip)
    {
        if (soundSource == null || audioClip == null) return;
        soundSource.PlayOneShot(audioClip);
    }

    
    public void PlayMenuMusic() => PlayMusic(menuBackgroundMusic);
    public void PlayGameMusic() => PlayMusic(gameBackgroundMusic);

    
    public void PlayButtonClick() => PlaySound(onButtonClickSound);
    public void PlayItemObtained() => PlaySound(itemObtainedSound);
    public void PlayItemUnpacked() => PlaySound(itemUnpackedSound);
    public void PlayItemEquipped() => PlaySound(itemEquippedSound);

    public void PlayShoot() => PlaySound(shootSound);
    public void PlayEnemyDamaged() => PlaySound(enemyDamagedSound);
    public void PlayPlayerDamaged() => PlaySound(playerDamagedSound);
    public void PlayExperiencePickup() => PlaySound(experiencePickupSound);
    public void PlayLevelUp() => PlaySound(levelUpSound);
    public void PlayBatHit() => PlaySound(batHitSound);
    public void PlayPlayerDeath() => PlaySound(playerDeathSound);
    public void PlayPlayerRevive() => PlaySound(playerReviveSound);
    public void PlayRoundEnd() => PlaySound(roundEndSound);
    #endregion
}
