using UnityEngine;
using System.Collections;

public class CeilingTrap : MonoBehaviour
{
    [Header("Components")]
    public Animator animator;
    public BoxCollider2D trapCollider; // Kéo BoxCollider2D của chính Object này vào đây
    public AudioSource trapAudioSource;

    [Header("Cấu Hình Thời Gian (Chu Kỳ)")]
    public float idleDelay = 2f; // Thời gian chờ trên trần (giây)

    [Header("Cấu Hình Hậu Quả")]
    public float knockbackForceX = 4f;
    public float knockbackForceY = 6f;
    public float flashDuration = 1.5f;
    public float flashInterval = 0.1f;

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
            yield return new WaitForSeconds(2.0f); // Chờ chạy xong animation (bạn tự chỉnh cho khớp nhé)
        }
    }


    // Xử lý va chạm trực tiếp khi bẫy đập trúng Player
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ExecuteHit(collision.gameObject, transform.position);
        }
    }

    public void ExecuteHit(GameObject go, Vector3 trapPosition)
    {
        if (isPlayerInvincible) return;
        if (trapAudioSource != null) trapAudioSource.Play();

        Player playerScript = go.GetComponent<Player>();
        if (playerScript != null) playerScript.TakeDamage(1);

        SpriteRenderer playerSR = go.GetComponent<SpriteRenderer>();
        Rigidbody2D playerRb = go.GetComponent<Rigidbody2D>();

        if (playerSR != null && playerScript != null && playerRb != null)
        {
            float direction = go.transform.position.x > trapPosition.x ? 1f : -1f;
            Vector2 knockbackVector = new Vector2(direction * knockbackForceX, knockbackForceY);
            StartCoroutine(PlayerImpactRoutine(playerSR, playerScript, playerRb, knockbackVector));
        }
    }

    private IEnumerator PlayerImpactRoutine(SpriteRenderer playerSR, Player playerScript, Rigidbody2D playerRb, Vector2 knockbackVector)
    {
        isPlayerInvincible = true;
        playerScript.enabled = false;

        playerRb.linearVelocity = Vector2.zero;
        playerRb.AddForce(knockbackVector, ForceMode2D.Impulse);

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            playerSR.enabled = false;
            yield return new WaitForSeconds(flashInterval);
            playerSR.enabled = true;
            yield return new WaitForSeconds(flashInterval);
            elapsed += (flashInterval * 2);
        }

        playerSR.enabled = true;
        playerScript.enabled = true;
        isPlayerInvincible = false;
    }
}