/* Author: TuLC
 * Date: 30/6/26
 * Description: This script manages all music and sound effects in the game.
 */

using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip buttonHoverSound;
    [SerializeField] private AudioClip buttonClickSound;
    [Header("Gameplay Sounds")]
    [SerializeField] private AudioClip letterPickupSound;
    [SerializeField] private AudioClip chestOpenSound;
    [SerializeField] private AudioClip popupOpenSound;
    [SerializeField] private AudioClip popupCloseSound;
    [SerializeField] private AudioClip hintSound;
    [SerializeField] private AudioClip wrongAnswerSound;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip victorySound;

    // Khởi tạo Singleton và load volume đã lưu.
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadVolume();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Phát nhạc nền, nếu đang phát đúng clip thì không phát lại.
    public void PlayMusic(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || musicSource == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = volumeScale;
        musicSource.Play();
    }

    // Phát hiệu ứng âm thanh.
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || sfxSource == null) return;

        sfxSource.PlayOneShot(clip, volumeScale);
    }

    // Set âm lượng nhạc nền.
    public void SetMusicVolume(float value)
    {
        if (musicSource != null)
        {
            musicSource.volume = value;
        }

        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
        PlayerPrefs.Save();
    }

    // Set âm lượng hiệu ứng.
    public void SetSFXVolume(float value)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = value;
        }

        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, value);
        PlayerPrefs.Save();
    }

    // Lấy âm lượng nhạc nền đã lưu.
    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
    }

    // Lấy âm lượng hiệu ứng đã lưu.
    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
    }

    // Load volume từ PlayerPrefs.
    private void LoadVolume()
    {
        SetMusicVolume(GetMusicVolume());
        SetSFXVolume(GetSFXVolume());
    }

    // Phát âm thanh hover của button.
    public void PlayButtonHover()
    {
        PlaySFX(buttonHoverSound);
    }

    // Phát âm thanh click của button.
    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound);
    }
    public void PlayLetterPickup()
    {
        PlaySFX(letterPickupSound);
    }

    public void PlayChestOpen()
    {
        PlaySFX(chestOpenSound);
    }

    public void PlayPopupOpen()
    {
        PlaySFX(popupOpenSound);
    }

    public void PlayPopupClose()
    {
        PlaySFX(popupCloseSound);
    }

    public void PlayHint()
    {
        PlaySFX(hintSound);
    }

    public void PlayWrongAnswer()
    {
        PlaySFX(wrongAnswerSound);
    }

    public void PlaySuccess()
    {
        PlaySFX(successSound);
    }

    public void PlayVictory()
    {
        PlaySFX(victorySound);
    }
}