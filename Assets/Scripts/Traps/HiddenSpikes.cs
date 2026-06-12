using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class HiddenSpikes : MonoBehaviour
{
    [Header("Sự Kiện Kích Hoạt (Kéo thả GameManager vào đây)")]
    public UnityEvent OnPlayerHitSpikes;

    [Header("Collider Gây Sát Thương")]
    [Tooltip("Kéo cái cây kim CapsuleCollider2D ở đỉnh cao nhất vào đây")]
    public Collider2D damageCollider;

    [Header("Cấu Hình Chu Kỳ Chông")]
    public float warningTime = 0.4f;
    public float activeTime = 1.2f;
    public float retractTime = 0.5f;

    [Header("Cấu Hình Hất Văng Vật Lý")]
    [Tooltip("Vận tốc đẩy lùi dũng sĩ sang trái (Khoảng 3 mét)")]
    public float knockbackLeftSpeed = 9f;
    [Tooltip("Vận tốc hất tung dũng sĩ lên trời (Khoảng 3 mét)")]
    public float knockbackUpSpeed = 11f;

    [Header("Cấu Hình Nhấp Nháy Bất Tử")]
    public float flashDuration = 1.5f;
    public float flashInterval = 0.1f;

    private SpriteRenderer bẫySpriteRenderer;
    private Animator bẫyAnimator;
    private bool isTriggered = false;
    private bool isDangerous = false;
    private bool isPlayerInvincible = false;

    void Start()
    {
        bẫySpriteRenderer = GetComponent<SpriteRenderer>();
        bẫyAnimator = GetComponent<Animator>();

        // Mặc định tắt cây kim đi để tránh lỗi sát thương tàng hình khi chông đang ẩn
        if (damageCollider != null)
        {
            damageCollider.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // LUỒNG 1: Dũng sĩ đi vào vùng cảm biến dưới đất khi bẫy chưa kích hoạt
            if (!isTriggered)
            {
                isTriggered = true;
                StartCoroutine(SpikePopUpRoutine(other.gameObject));
            }

            // LUỒNG 2: Chông ĐANG DỰNG sẵn nguy hiểm, dũng sĩ lao đầu vào cây kim sát thương
            else if (isDangerous && !isPlayerInvincible)
            {
                if (damageCollider != null && damageCollider.enabled)
                {
                    ExecuteKnockback(other.gameObject);
                }
            }
        }
    }

    // Quản lý chu kỳ trồi sụt của bẫy chông ẩn
    private IEnumerator SpikePopUpRoutine(GameObject playerObj)
    {
        // Chạy hiệu ứng trồi lên
        if (bẫySpriteRenderer != null) bẫySpriteRenderer.enabled = true;
        if (bẫyAnimator != null) bẫyAnimator.Play("Spike_PopUp");

        // Chờ hết thời gian nhú lên
        yield return new WaitForSeconds(warningTime);

        // ĐÚNG FRAME ẢNH CAO NHẤT: Bật trạng thái nguy hiểm và mở cây kim sát thương
        isDangerous = true;
        if (damageCollider != null) damageCollider.enabled = true;

        // LUỒNG 3: Chông phóng trúng mông dũng sĩ đang đứng đợi ngay trên đầu
        CheckInstantDamage(playerObj);

        // Giữ nguyên trạng thái gây sát thương
        yield return new WaitForSeconds(activeTime);

        // Hết thời gian, rụt xuống và khóa sát thương lại
        isDangerous = false;
        if (damageCollider != null) damageCollider.enabled = false;

        // Chờ hồi bẫy hoàn toàn
        yield return new WaitForSeconds(retractTime);
        isTriggered = false;
    }

    // Kiểm tra va chạm đè nhau tại khoảnh khắc chông vừa phọt lên
    private void CheckInstantDamage(GameObject playerObj)
    {
        if (damageCollider == null || playerObj == null) return;

        Collider2D playerCollider = playerObj.GetComponent<Collider2D>();
        if (playerCollider != null && damageCollider.bounds.Intersects(playerCollider.bounds) && !isPlayerInvincible)
        {
            ExecuteKnockback(playerObj);
        }
    }

    // Thực hiện hình phạt: Hất văng vật lý + Nhấp nháy đổi màu
    private void ExecuteKnockback(GameObject playerObj)
    {
        // 1. PHÁT TÍN HIỆU TRỪ MÁU
        if (OnPlayerHitSpikes != null)
        {
            OnPlayerHitSpikes.Invoke();
        }

        // 2. TÁC DỤNG LỰC HẤT VĂNG 3M x 3M
        Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            // Reset vận tốc cũ để lực đẩy không bị sai lệch do đà chạy của dũng sĩ
            playerRb.linearVelocity = Vector2.zero;

            // Ép vận tốc bay: Sang trái (-X) tầm 3 mét, lên trời (+Y) tầm 3 mét
            playerRb.linearVelocity = new Vector2(-knockbackLeftSpeed, knockbackUpSpeed);
        }

        // 3. KÍCH HOẠT HIỆU ỨNG NHẤP NHÁY ĐỔI MÀU GỌN GÀNG
        SpriteRenderer playerSR = playerObj.GetComponent<SpriteRenderer>();
        if (playerSR != null)
        {
            StartCoroutine(PlayerFlashRoutine(playerSR));
        }
    }

    // Coroutine xử lý nhấp nháy mờ hiện chuẩn mẫu của ông
    private IEnumerator PlayerFlashRoutine(SpriteRenderer playerSprite)
    {
        isPlayerInvincible = true;
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
        isPlayerInvincible = false;
    }
}