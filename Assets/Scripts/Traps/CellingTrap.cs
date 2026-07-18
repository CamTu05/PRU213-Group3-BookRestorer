using UnityEngine;
using System.Collections;

public class CeilingTrap : MonoBehaviour
{
    [Header("Components")]
    public Animator animator;
    public BoxCollider2D trapCollider; // Kéo BoxCollider2D của chính Object này vào đây

    [Header("Linh Kiện Phát Âm Thanh")]
    [SerializeField] private AudioClip trapAudioSource;

    [Header("Cấu Hình Thời Gian (Chu Kỳ)")]
    public float idleDelay = 2f; // Thời gian chờ trên trần (giây)

    [Header("Cấu Hình Hậu Quả")]
    public int damage = 3; 

    private static bool isPlayerInvincible = false;

    void Start()
    {
   
        if (trapCollider != null) trapCollider.enabled = true;

        // Chạy chu kỳ dập tự động
        StartCoroutine(TrapCycleRoutine());
    }

    private IEnumerator TrapCycleRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(idleDelay);
            animator.SetTrigger("Activate");
            if (AudioManager.Instance != null && trapAudioSource != null)
            {
                AudioManager.Instance.PlaySFX(trapAudioSource);
            }
            else if (AudioManager.Instance == null)
            {
                Debug.LogWarning("AudioManager.Instance chưa được khởi tạo hoặc thiếu trong Scene!");
            }
            yield return new WaitForSeconds(2.0f); // Chờ chạy xong animation (bạn tự chỉnh cho khớp nhé)
        }
    }


    // Xử lý va chạm trực tiếp khi bẫy đập trúng Player
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        float dir = collision.transform.position.x > transform.position.x ? 1f : -1f;

        collision.GetComponent<PlayerHealth>()?.TakeDamage(damage, dir);

       
    }
   
}