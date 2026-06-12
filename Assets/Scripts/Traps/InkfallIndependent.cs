using UnityEngine;
using UnityEngine.Events; // BẮT BUỘC phải thêm thư viện này để dùng UnityEvent
using System.Collections;

public class InkfallIndependent  : MonoBehaviour
{
    [Header("Sự Kiện Kích Hoạt (Kéo thả ở đây)")]
    [Tooltip("Khi dũng sĩ trúng bẫy, tất cả các hàm kéo vào ô này sẽ tự động chạy")]
    public UnityEvent OnPlayerHitInkfall; // Nó sẽ hiện ra ô y hệt như sự kiện OnClick của Button

    [Header("Thời Gian Chu Kỳ")]
    public float openTime = 5f;
    public float activeTime = 2f;

    private float timer = 0f;
    private bool isFalling = false;
    private bool isHandlingHit = false;

    private Collider2D waterfallCollider;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        waterfallCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetInkfallState(false);
    }

    void Update()
    {
        if (isHandlingHit) return;

        timer += Time.deltaTime;

        if (!isFalling && timer >= openTime) SetInkfallState(true);
        else if (isFalling && timer >= activeTime) SetInkfallState(false);
    }

    void SetInkfallState(bool state)
    {
        isFalling = state;
        timer = 0f;
        waterfallCollider.enabled = state;

        if (state)
            spriteRenderer.color = new Color(0f, 0f, 0f, 1f);
        else
            spriteRenderer.color = new Color(0f, 0f, 0f, 0.1f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isFalling && !isHandlingHit)
        {
            isHandlingHit = true;

            // KÍCH HOẠT SỰ KIỆN: Gọi tất cả các hàm được kéo thả trong Inspector
            if (OnPlayerHitInkfall != null)
            {
                OnPlayerHitInkfall.Invoke();
            }

            StartCoroutine(PlayerHitRoutine(other.gameObject));
        }
    }

    private IEnumerator PlayerHitRoutine(GameObject playerObj)
    {
        SpriteRenderer playerSR = playerObj.GetComponent<SpriteRenderer>();
        Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
        Color originalColor = Color.white;

        if (playerSR != null) originalColor = playerSR.color;

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (playerSR != null)
        {
            playerSR.color = new Color(0f, 0f, 0f, 1f);

            float duration = 0.4f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float newAlpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                playerSR.color = new Color(0f, 0f, 0f, newAlpha);
                yield return null;
            }
        }

        if (playerSR != null) playerSR.color = originalColor;
        if (playerRb != null) playerRb.bodyType = RigidbodyType2D.Dynamic;

        isHandlingHit = false;
        SetInkfallState(false);
    }
}