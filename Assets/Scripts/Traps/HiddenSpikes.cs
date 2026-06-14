/*
 * author chittp
 * description: tao bay dong cho nhan vat di qua bat lai va khoa input
 * last update day: 260613
 */
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class HiddenSpikes : MonoBehaviour
{
    [Header("Sự Kiện Kích Hoạt")]
    public UnityEvent OnPlayerHitSpikes;

    [Header("Collider Sát Thương")]
    [SerializeField] private Collider2D damageCollider;

    [Header("Cấu Hình Phát Hiện Player")]
    [SerializeField] private float detectionRadius = 0.5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Cấu Hình Chu Kỳ")]
    [SerializeField] private float warningTime = 0.4f;
    [SerializeField] private float activeTime = 1.2f;
    [SerializeField] private float retractTime = 0.5f;

    [Header("Cấu Hình Hất Văng Cưỡng Bức")]
    [SerializeField] private float knockbackXSpeed = 10f;
    [SerializeField] private float knockbackUpSpeed = 8f;
    [SerializeField] private float knockbackDuration = 0.3f; // Thời gian bay trên không

    [Header("Cấu Hình Disable Input")]
    [SerializeField] private float inputDisableDuration = 2.0f; // Vô hiệu hóa điều khiển 2 giây theo yêu cầu

    [Header("Cấu Hình Nhấp Nháy")]
    [SerializeField] private float flashDuration = 1.5f;
    [SerializeField] private float flashInterval = 0.1f;

    private const float MIN_RADIUS = 0.1f;
    private const string PLAYER_TAG = "Player";

    private SpriteRenderer trapSpriteRenderer;
    private Animator trapAnimator;
    private bool isTriggered = false;
    private bool isPlayerInvincible = false;
    private Coroutine currentPopupRoutine = null;
    private Coroutine currentFlashRoutine = null;

    private void Start()
    {
        trapSpriteRenderer = GetComponent<SpriteRenderer>();
        trapAnimator = GetComponent<Animator>();
        ValidateSettings();

        if (damageCollider != null)
        {
            damageCollider.enabled = false;
        }
    }

    private void ValidateSettings()
    {
        if (playerLayer.value == 0)
        {
            Debug.LogError($"[HiddenSpikes] ⚠️ playerLayer chưa được set!", gameObject);
            enabled = false;
            return;
        }
        if (detectionRadius < MIN_RADIUS) detectionRadius = MIN_RADIUS;
    }

    private void Update()
    {
        if (!isTriggered)
        {
            DetectAndTrigger();
        }
    }

    private void DetectAndTrigger()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);

        if (playerCollider != null && playerCollider.CompareTag(PLAYER_TAG))
        {
            isTriggered = true;

            if (currentPopupRoutine != null) StopCoroutine(currentPopupRoutine);
            currentPopupRoutine = StartCoroutine(SpikePopUpRoutine(playerCollider.gameObject));
        }
    }

    private IEnumerator SpikePopUpRoutine(GameObject playerObj)
    {
        if (trapSpriteRenderer != null) trapSpriteRenderer.enabled = true;
        if (trapAnimator != null) trapAnimator.Play("Spike_PopUp");

        yield return new WaitForSeconds(warningTime);

        if (damageCollider != null) damageCollider.enabled = true;

        if (playerObj != null && !isPlayerInvincible)
        {
            Collider2D playerCollider = playerObj.GetComponent<Collider2D>();
            if (playerCollider != null && damageCollider.bounds.Intersects(playerCollider.bounds))
            {
                ExecuteTrapImpact(playerObj);
            }
        }

        yield return new WaitForSeconds(activeTime);

        if (damageCollider != null) damageCollider.enabled = false;

        yield return new WaitForSeconds(retractTime);

        isTriggered = false;
        currentPopupRoutine = null;
    }

    private void ExecuteTrapImpact(GameObject playerObj)
    {
        OnPlayerHitSpikes?.Invoke();

        Player playerScript = playerObj.GetComponent<Player>();
        if (playerScript != null)
        {
            playerScript.TakeDamage(1);
        }

        // Kích hoạt chu kỳ khóa Input kéo dài đầy đủ 2 giây độc lập
        StartCoroutine(ForcedKnockbackRoutine(playerObj, playerScript));

        SpriteRenderer playerSR = playerObj.GetComponent<SpriteRenderer>();
        if (playerSR != null)
        {
            if (currentFlashRoutine != null) StopCoroutine(currentFlashRoutine);
            currentFlashRoutine = StartCoroutine(PlayerFlashRoutine(playerSR, playerObj));
        }
    }

    private IEnumerator ForcedKnockbackRoutine(GameObject playerObj, Player playerScript)
    {
        Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
        if (playerObj == null || playerRb == null || playerScript == null) yield break;
        if (playerRb.bodyType == RigidbodyType2D.Static) yield break;

        // Lấy quyền can thiệp biến private hệ thống di chuyển của Player
        var horizontalInputField = typeof(Player).GetField("horizontalInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var shouldJumpField = typeof(Player).GetField("shouldJump", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        float direction = playerObj.transform.position.x > transform.position.x ? 1f : -1f;
        float totalTimer = 0f;

        // Reset vận tốc ngay frame đầu tiên trúng bẫy
        playerRb.linearVelocity = Vector2.zero;

        // Sử dụng một vòng lặp tổng chạy đúng thời gian inputDisableDuration (2.0 giây)
        while (totalTimer < inputDisableDuration && playerObj != null)
        {
            if (playerRb.bodyType == RigidbodyType2D.Static) yield break;

            // LIÊN TỤC KHÓA CHẶT INPUT: Đè biến di chuyển và nhảy về 0 trong suốt cả 2 giây
            if (horizontalInputField != null) horizontalInputField.SetValue(playerScript, 0f);
            if (shouldJumpField != null) shouldJumpField.SetValue(playerScript, false);

            // Giai đoạn hất văng (Chỉ thực hiện dịch chuyển vị trí trong khoảng thời gian knockbackDuration ngắn ban đầu)
            if (totalTimer < knockbackDuration)
            {
                Vector2 forcedMove = new Vector2(direction * knockbackXSpeed, knockbackUpSpeed) * Time.deltaTime;
                playerRb.MovePosition(playerRb.position + forcedMove);
            }
            else
            {
                // Sau khi bay xong, ép vận tốc ngang về 0 để Player đứng im tại chỗ chịu phạt, không bị trượt đi
                playerRb.linearVelocity = new Vector2(0f, playerRb.linearVelocity.y);
            }

            totalTimer += Time.deltaTime;
            yield return null;
        }

        // Sau khi hết sạch 2 giây vô hiệu hóa, giải phóng vận tốc để người chơi di chuyển lại bình thường
        if (playerObj != null && playerRb.bodyType != RigidbodyType2D.Static)
        {
            playerRb.linearVelocity = new Vector2(0f, playerRb.linearVelocity.y);
        }
    }

    private IEnumerator PlayerFlashRoutine(SpriteRenderer playerSR, GameObject playerObj)
    {
        isPlayerInvincible = true;
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            if (playerObj == null || playerSR == null) yield break;

            playerSR.enabled = false;
            yield return new WaitForSeconds(flashInterval);

            playerSR.enabled = true;
            yield return new WaitForSeconds(flashInterval);

            elapsed += (flashInterval * 2);
        }

        if (playerSR != null) playerSR.enabled = true;
        isPlayerInvincible = false;
        currentFlashRoutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (detectionRadius > 0)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}