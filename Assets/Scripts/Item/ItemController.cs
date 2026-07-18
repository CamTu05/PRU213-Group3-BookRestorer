using UnityEngine;

public class ItemController : MonoBehaviour
{
    [Header("Linh Kiện Phát Âm Thanh")]
    [SerializeField] private AudioClip itemAudioSource;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(itemAudioSource, 0.5f);
        }
    }
}
