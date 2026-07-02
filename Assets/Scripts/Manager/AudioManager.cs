/* Author: TuLC
 * Date: 30 /6/26
 * Description: This script manages music and sound effect volume settings.
 */

using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

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

    // Cập nhật âm lượng nhạc nền.
    public void SetMusicVolume(float value)
    {
        if (musicSource != null)
        {
            musicSource.volume = value;
        }

        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
        PlayerPrefs.Save();
    }

    // Cập nhật âm lượng hiệu ứng.
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

    // Load âm lượng đã lưu vào AudioSource.
    private void LoadVolume()
    {
        SetMusicVolume(GetMusicVolume());
        SetSFXVolume(GetSFXVolume());
    }
}