/* Author: TuLC
 * Date: 30/6/26
 * Description: This script plays the background music for the current scene.
 */

using UnityEngine;

public class SceneMusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip backgroundMusic;

    // Phát nhạc nền của scene khi scene được tải.
    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(backgroundMusic, 0.5f);
        }
    }
}