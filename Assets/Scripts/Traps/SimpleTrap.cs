//Author: Nguyễn Tín Nghĩa
//Date: 10/07/2026
//Description: Script đa năng dành cho các bẫy đứng im, tự tính hướng để đẩy Player văng ra

using UnityEngine;

public class SimpleTrap : MonoBehaviour
{
    [Header("Linh Kiện Phát Âm Thanh")]
    [SerializeField] private AudioSource trapAudioSource;

    [Header("Cấu Hình Hậu Quả")]
    [SerializeField] private int damage = 1;

    // Trường hợp 1: Bẫy để Is Trigger = True (Đi xuyên qua)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DealDamageToPlayer(other.gameObject);
        }
    }

    // Trường hợp 2: Bẫy để Is Trigger = False (Vật thể rắn chặn người)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            DealDamageToPlayer(collision.gameObject);
        }
    }

    private void DealDamageToPlayer(GameObject playerObj)
    {
        // Tính hướng văng dựa theo vị trí của bẫy so với Player
        float dir = playerObj.transform.position.x > transform.position.x ? 1f : -1f;

        // Gọi script PlayerHealth xịn trên người Player
        PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage, dir);
        }

        // Phát âm thanh
        if (trapAudioSource != null)
        {
            trapAudioSource.Play();
        }
    }
}