using UnityEngine;
using System.Collections;

public class Spikes : MonoBehaviour
{
    [Header("Linh Kiện Phát Âm Thanh")]
    public AudioSource trapAudioSource;

    [Header("Cấu Hình Hậu Quả")]
    public float knockbackForceX = 2f;
    public float knockbackForceY = 5f;
    public float flashDuration = 1.5f;
    public float flashInterval = 0.1f;

    private bool isPlayerInvincible = false;

    // 1. phát hiện vâthj thể đi vào vùng kích hoạt của bẫy

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ExecuteHit(collision.gameObject, transform.position);
        }
    }

    // 2. HÀM TRUNG TÂM
    public void ExecuteHit(GameObject go, Vector3 trapPosition)
    {
        if (isPlayerInvincible) return;
        trapAudioSource.Play();

        // Trừ máu Player
        Player playerScript = go.GetComponent<Player>();
        if (playerScript != null) playerScript.TakeDamage(1);

        SpriteRenderer playerSR = go.GetComponent<SpriteRenderer>();
        Rigidbody2D playerRb = go.GetComponent<Rigidbody2D>();

        if (playerSR != null && playerScript != null && playerRb != null)
        {
            // TÍNH TOÁN LỰC ĐẨY VẬT LÝ (KNOCKBACK FORCE)
            float direction = go.transform.position.x > trapPosition.x ? 1f : -1f;

            // Tạo Vector lực: đẩy sang trái/phải (direction * knockbackForceX) và hất nhẹ lên (knockbackForceY)
            Vector2 knockbackVector = new Vector2(direction * knockbackForceX, knockbackForceY);

            // Bắt đầu Coroutine xử lý hiệu ứng, truyền thêm lực đẩy vào
            StartCoroutine(PlayerImpactRoutine(playerSR, go, playerScript, playerRb, knockbackVector));
        }
    }

    private IEnumerator PlayerImpactRoutine(SpriteRenderer playerSR, GameObject playerObj, Player playerScript, Rigidbody2D playerRb, Vector2 knockbackVector)
    {
        isPlayerInvincible = true;
        playerScript.enabled = false;

        // ÁP DỤNG LỰC VẬT LÝ
        // Triệt tiêu vận tốc cũ để lực đẩy mới chính xác, không bị ảnh hưởng bởi vận tốc trước đó của Player
        playerRb.linearVelocity = Vector2.zero;
        // Sử dụng ForceMode2D.Impulse 
        playerRb.AddForce(knockbackVector, ForceMode2D.Impulse);

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            playerSR.enabled = false; // ẩn
            yield return new WaitForSeconds(flashInterval);
            playerSR.enabled = true; //hiện
            yield return new WaitForSeconds(flashInterval);
            elapsed += (flashInterval * 2);
        }

        playerSR.enabled = true;
        playerScript.enabled = true;
        isPlayerInvincible = false;
    }
}