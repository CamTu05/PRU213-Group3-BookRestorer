using UnityEngine;

public class Chest : MonoBehaviour
{
    private bool isPlayerInside = false;
    private Player playerScript;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem có đúng là Nhân vật đứng gần rương không
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = true;
            playerScript = collision.GetComponent<Player>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Khi nhân vật đi ra xa rương
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;

            // Tự động tắt bảng chữ đi khi người chơi bỏ chạy đi nơi khác
            if (playerScript != null)
            {
                playerScript.TogglePopup(false);
            }
            playerScript = null;
        }
    }

    // --- SỰ KIỆN CLICK CHUỘT VÀO RƯƠNG ---
    private void OnMouseDown()
    {
        // Chỉ cho phép bấm mở rương khi nhân vật đang đứng gần nó
        if (isPlayerInside && playerScript != null)
        {
            // Bật bảng popup lên màn hình (Hàm này đã có sẵn trong Player.cs)
            playerScript.TogglePopup(true);
            Debug.Log("==> ĐÃ CLICK MỞ RƯƠNG THÀNH CÔNG!");
        }
    }
}