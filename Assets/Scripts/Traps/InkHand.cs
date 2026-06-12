using UnityEngine;
using System.Collections;

public class InkHand : MonoBehaviour
{
    [Header("Cấu Hình Bẫy")]
    public float trapDuration = 2f;

    private SpriteRenderer spriteRenderer;
    private Collider2D trapCollider;
    private bool isTriggered = false;

    private GameObject trappedPlayer;
    private Vector2 trapPosition;
    private bool isLockingPosition = false; // CHIẾC CÔNG TẮC ĐỂ GIẢI THOÁT

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        trapCollider = GetComponent<Collider2D>();

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            trappedPlayer = other.gameObject;
            isLockingPosition = true; // BẬT CÔNG TẮC: Khóa chân dũng sĩ

            trapPosition = new Vector2(transform.position.x, other.transform.position.y);

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                spriteRenderer.color = new Color(0f, 0f, 0f, 1f);
            }

            StartCoroutine(TrapPlayerRoutine());
        }
    }

    void LateUpdate()
    {
        // KIỂM TRA CÔNG TẮC: Nếu đang khóa thì mới ép tọa độ
        if (isLockingPosition && trappedPlayer != null)
        {
            trappedPlayer.transform.position = new Vector3(trapPosition.x, trapPosition.y, trappedPlayer.transform.position.z);

            Rigidbody2D playerRb = trappedPlayer.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
            }
        }
    }

    private IEnumerator TrapPlayerRoutine()
    {
        SpriteRenderer playerSR = trappedPlayer.GetComponent<SpriteRenderer>();
        Color originalPlayerColor = Color.white;

        if (playerSR != null) originalPlayerColor = playerSR.color;

        if (playerSR != null)
        {
            playerSR.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        }

        // Chờ đúng 2 giây hoảng loạn
        yield return new WaitForSeconds(trapDuration);

        // --- KHÚC NÀY QUAN TRỌNG: TẮT CÔNG TẮC TRƯỚC KHI THẢ NGƯỜI ---
        isLockingPosition = false;

        if (playerSR != null)
        {
            playerSR.color = originalPlayerColor;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        if (trapCollider != null)
        {
            trapCollider.enabled = false;
        }

        trappedPlayer = null;
    }
}