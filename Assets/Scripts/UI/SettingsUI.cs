/* Author: TuLC
 * Date: 30/6/26
 * Description: This script controls the settings panel UI.
 */

using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    // Khởi tạo slider theo volume đã lưu.
    private void Start()
    {
        musicSlider.value = AudioManager.Instance.GetMusicVolume();
        sfxSlider.value = AudioManager.Instance.GetSFXVolume();

        musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
    }
}