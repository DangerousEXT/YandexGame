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

    public float MusicVolume => audioMixer.GetFloat(musicVolumeName, out var volume) ? Mathf.InverseLerp(minVolumeDB, maxVolumeDB, volume) : 0.5f;

    public float SoundVolume => audioMixer.GetFloat(soundVolumeName, out var volume) ? Mathf.InverseLerp(minVolumeDB, maxVolumeDB, volume) : 0.5f;

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

    #region Play Methods
    public void PlayMusic(AudioClip audioClip)
    {
        if (musicSource == null || audioClip == null) return;
        musicSource.clip = audioClip;
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