using UnityEngine;

public class InkPuddleIndependent : MonoBehaviour
{
    [Header("Slow Settings")]
    [Tooltip("Chỉ số lực cản khi ở trong mực. Càng cao thì càng chậm (Thử trong khoảng 10 đến 30)")]
    public float inkDrag = 20f;

    private Rigidbody2D playerRb;
    private float originalDrag;
    private float originalGravity;
    private bool isPlayerInside = false;

    // Khi Dũng Sĩ bước chân vào vũng mực
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerRb = other.GetComponent<Rigidbody2D>();

            if (playerRb != null && !isPlayerInside)
            {
                isPlayerInside = true;

                // Lưu lại chỉ số gốc của nhân vật để trả lại khi họ thoát ra
                originalDrag = playerRb.linearDamping; // Nếu dùng Unity cũ hơn phiên bản 6, đổi thành playerRb.drag
                originalGravity = playerRb.gravityScale;

                // Áp dụng lực cản cực lớn để làm chậm di chuyển ngang
                playerRb.linearDamping = inkDrag;

                // Tăng trọng lực lên một chút để dũng sĩ cảm thấy nặng nề, khó nhảy cao
                playerRb.gravityScale = originalGravity * 1.5f;
            }
        }
    }

    // Khi Dũng Sĩ nhảy ra hoặc chạy thoát khỏi vũng mực
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerRb != null && isPlayerInside)
            {
                // Trả lại mọi thông số vật lý nguyên bản cho bạn mình
                playerRb.linearDamping = originalDrag; // Nếu dùng Unity cũ, đổi thành playerRb.drag
                playerRb.gravityScale = originalGravity;

                // Reset trạng thái
                playerRb = null;
                isPlayerInside = false;
            }
        }
    }
}