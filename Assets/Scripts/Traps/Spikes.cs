using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class Spikes : MonoBehaviour
{
    [Header("Linh Kiện Phát Âm Thanh")]
    public AudioSource trapAudioSource;

    [Header("Cấu Hình Hậu Quả")]
    public int damage = 3;
    public float knockbackForceX = 2f;
    public float knockbackForceY = 5f;
    public float flashDuration = 1.5f;
    public float flashInterval = 0.1f;

    private bool isPlayerInvincible = false;

    // 1. phát hiện vâthj thể đi vào vùng kích hoạt của bẫy

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        float dir = other.transform.position.x > transform.position.x ? 1f : -1f;
        Debug.Log("Spike gọi TakeDamage(3)");
        other.GetComponent<PlayerHealth>()
             ?.TakeDamage(damage, dir);

        trapAudioSource.Play();
    }
}