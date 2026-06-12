using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Spikes : MonoBehaviour
{
    [Header("Sự Kiện Kích Hoạt (Kéo thả GameManager vào đây)")]
    public UnityEvent OnPlayerHitSpikes;

    [Header("Cấu Hình Dịch Chuyển (Teleport)")]
    [Tooltip("Khoảng cách dũng sĩ bị giật lùi về phía bên trái từ vị trí bẫy")]
    public float teleportBackDistance = 3f;

    [Header("Cấu Hình Nhấp Nháy Bất Tử")]
    public float flashDuration = 1.5f;
    public float flashInterval = 0.1f;

    private bool isPlayerInvincible = false; // Khiên chống dính sát thương liên tục

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Chạm vào Player và Player không trong trạng thái bất tử nhấp nháy
        if (other.CompareTag("Player") && !isPlayerInvincible)
        {
            // 1. PHÁT TÍN HIỆU TRỪ MÁU
            if (OnPlayerHitSpikes != null)
            {
                OnPlayerHitSpikes.Invoke();
            }

            // 2. DỊCH CHUYỂN TỨC THỜI (TELEPORT) VỀ BÊN TRÁI
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();

            // Xóa sạch vận tốc cũ để dũng sĩ không bị đà trượt tiếp sau khi dịch chuyển
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
            }

            // Tính toán tọa độ mới: Lấy vị trí bẫy chông trừ đi khoảng cách an toàn về bên trái (trục X)
            float safeX = transform.position.x - teleportBackDistance;

            // Giữ nguyên cao độ Y và Z hiện tại của dũng sĩ để họ đáp đất an toàn
            other.transform.position = new Vector3(safeX, other.transform.position.y, other.transform.position.z);

            // 3. KÍCH HOẠT HIỆU ỨNG NHẤP NHÁY
            SpriteRenderer playerSR = other.GetComponent<SpriteRenderer>();
            if (playerSR != null)
            {
                StartCoroutine(PlayerFlashRoutine(playerSR));
            }
        }
    }

    // Coroutine xử lý nhấp nháy ẩn hiện
    private IEnumerator PlayerFlashRoutine(SpriteRenderer playerSprite)
    {
        isPlayerInvincible = true; // Bật bất tử tạm thời
        float elapsed = 0f;
        Color originalColor = playerSprite.color;

        while (elapsed < flashDuration)
        {
            // Mờ đi
            playerSprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.2f);
            yield return new WaitForSeconds(flashInterval);

            // Hiện rõ
            playerSprite.color = originalColor;
            yield return new WaitForSeconds(flashInterval);

            elapsed += (flashInterval * 2);
        }

        playerSprite.color = originalColor;
        isPlayerInvincible = false; // Tắt bất tử
    }
}