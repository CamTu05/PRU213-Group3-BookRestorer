/*
 * author chittp
 * description: tao bay co dinh cho nhan vat di qua bat lai va khoa input
 * last update day: 260613
 */
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Spikes : MonoBehaviour
{
    [Header("Sự Kiện Kích Hoạt")]
    public UnityEvent OnPlayerHitSpikes;

    [Header("Cấu Hình Phát Hiện Player")]
    [SerializeField] private float detectionRadius = 0.5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Cấu Hình Dịch Chuyển")]
    [SerializeField] private float teleportBackDistance = 3f;

    [Header("Cấu Hình Nhấp Nháy")]
    [SerializeField] private float flashDuration = 1.5f;
    [SerializeField] private float flashInterval = 0.1f;

    [Header("Cấu Hình Disable Input")]
    [SerializeField] private float inputDisableDuration = 1.0f;

    private const float DETECTION_COOLDOWN = 0.5f;
    private const float MIN_RADIUS = 0.1f;
    private const string PLAYER_TAG = "Player";

    private bool isPlayerInvincible = false;
    private float detectionCooldown = 0f;
    private Coroutine currentFlashRoutine = null;

    private void Start()
    {
        ValidateSettings();
    }

    private void ValidateSettings()
    {
        if (playerLayer.value == 0)
        {
            Debug.LogError($"[Spikes] ⚠️ playerLayer chưa được set! Hãy set Layer của Player trong Inspector.", gameObject);
            enabled = false;
            return;
        }
        if (detectionRadius < MIN_RADIUS) detectionRadius = MIN_RADIUS;
    }

    private void Update()
    {
        if (detectionCooldown > 0)
        {
            detectionCooldown -= Time.deltaTime;
            return;
        }

        if (!isPlayerInvincible)
        {
            DetectAndHitPlayer();
        }
    }

    private void DetectAndHitPlayer()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);

        if (playerCollider != null && playerCollider.CompareTag(PLAYER_TAG))
        {
            detectionCooldown = DETECTION_COOLDOWN;
            ExecuteHit(playerCollider.gameObject);
        }
    }

    private void ExecuteHit(GameObject playerObj)
    {
        if (playerObj == null) return;

        OnPlayerHitSpikes?.Invoke();

        // Trừ tim của Player thông qua hàm gốc
        Player playerScript = playerObj.GetComponent<Player>();
        if (playerScript != null)
        {
            playerScript.TakeDamage(1);
        }

        // Dịch chuyển Player ra xa bẫy
        TeleportPlayer(playerObj);

        // Khóa Input và Ép nhấp nháy ngay lập tức (Bỏ qua iframe của Player)
        SpriteRenderer playerSR = playerObj.GetComponent<SpriteRenderer>();
        if (playerSR != null)
        {
            if (currentFlashRoutine != null) StopCoroutine(currentFlashRoutine);
            currentFlashRoutine = StartCoroutine(PlayerImpactRoutine(playerSR, playerObj));
        }
    }

    private void TeleportPlayer(GameObject playerObj)
    {
        Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
        if (playerRb != null && playerRb.bodyType != RigidbodyType2D.Static)
        {
            playerRb.linearVelocity = Vector2.zero;
        }

        float direction = playerObj.transform.position.x > transform.position.x ? 1f : -1f;
        Vector3 targetPos = playerObj.transform.position;
        targetPos.x = transform.position.x + (direction * teleportBackDistance);
        playerObj.transform.position = targetPos;
    }

    private IEnumerator PlayerImpactRoutine(SpriteRenderer playerSR, GameObject playerObj)
    {
        isPlayerInvincible = true;

        Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
        Player playerMovement = playerObj.GetComponent<Player>();

        // KIỂM TRA CHỐNG LỖI: Nếu player đã chết hoặc script đã bị tắt trước đó thì thoát ngay
        if (playerMovement == null || !playerMovement.enabled || playerRb == null || playerRb.bodyType == RigidbodyType2D.Static)
        {
            isPlayerInvincible = false;
            yield break;
        }

        // KHÓA INPUT: Xóa sạch vận tốc ngang để không bị trượt đi khi bấm nút
        playerRb.linearVelocity = new Vector2(0, playerRb.linearVelocity.y);
        playerMovement.enabled = false;

        float elapsed = 0f;
        float inputTimer = 0f;
        bool isInputRestored = false;

        while (elapsed < flashDuration)
        {
            if (playerObj == null || playerSR == null) yield break;

            // Nhấp nháy cưỡng bức bằng cách ẩn/hiện thành phần hiển thị Sprite
            playerSR.enabled = false;
            yield return new WaitForSeconds(flashInterval);

            playerSR.enabled = true;
            yield return new WaitForSeconds(flashInterval);

            float step = flashInterval * 2;
            elapsed += step;
            inputTimer += step;

            // Khi hết thời gian disable input quy định, trả lại quyền điều khiển
            if (!isInputRestored && inputTimer >= inputDisableDuration)
            {
                if (playerMovement != null && playerRb != null && playerRb.bodyType != RigidbodyType2D.Static)
                {
                    playerMovement.enabled = true;
                }
                isInputRestored = true;
            }
        }

        if (playerSR != null) playerSR.enabled = true;
        if (!isInputRestored && playerMovement != null && playerRb != null && playerRb.bodyType != RigidbodyType2D.Static)
        {
            playerMovement.enabled = true;
        }

        isPlayerInvincible = false;
        currentFlashRoutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (detectionRadius > 0)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}