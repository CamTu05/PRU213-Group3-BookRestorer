using UnityEngine;
using System.Collections;

public class MovingSaw : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform pointA;
    public Transform pointB;
    public Transform saw;
    public float speed = 3f;


    [Header("Linh Kiện Phát Âm Thanh")]
    public AudioSource trapAudioSource;

    [Header("Cấu Hình Hậu Quả")]
    public float knockbackForceX = 2f;
    public float knockbackForceY = 5f;
    public float flashDuration = 1.5f;
    public float flashInterval = 0.1f;

    private Transform targetPoint;
    private static bool isPlayerInvincible = false; // ĐỔI THÀNH static để tất cả các bẫy dùng chung trạng thái bất tử này!

    void Start()
    {
        targetPoint = pointB;
        if (saw != null && pointA != null)
        {
            saw.position = pointA.position;
        }
    }

    void Update()
    {
        if (saw == null || pointA == null || pointB == null) return;

        // 1. Di chuyển cái cưa qua lại
        saw.position = Vector3.MoveTowards(saw.position, targetPoint.position, speed * Time.deltaTime);

        // 3. Đổi hướng
        if (Vector3.Distance(saw.position, targetPoint.position) < 0.1f)
        {
            targetPoint = (targetPoint == pointA) ? pointB : pointA;
        }
    }

    // 1. Phát hiện va chạm (Thay vì check ở Object cha, cái này gắn ở cưa nên nhận TriggerEnter)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Lấy vị trí của chính cái cưa đang di chuyển làm tâm đẩy vật lý
            ExecuteHit(collision.gameObject, saw.position);
        }
    }

    // 2. HÀM TRUNG TÂM (Kế thừa từ code Spikes)
    public void ExecuteHit(GameObject go, Vector3 trapPosition)
    {
        if (isPlayerInvincible) return;

        if (trapAudioSource != null) trapAudioSource.Play();

        // Trừ máu Player
        Player playerScript = go.GetComponent<Player>();
        if (playerScript != null) playerScript.TakeDamage(1);

        SpriteRenderer playerSR = go.GetComponent<SpriteRenderer>();
        Rigidbody2D playerRb = go.GetComponent<Rigidbody2D>();

        if (playerSR != null && playerScript != null && playerRb != null)
        {
            // Lực đẩy tính dựa trên vị trí cái CƯA (trapPosition) chứ không phải vị trí Object cha
            float direction = go.transform.position.x > trapPosition.x ? 1f : -1f;

            Vector2 knockbackVector = new Vector2(direction * knockbackForceX, knockbackForceY);

            StartCoroutine(PlayerImpactRoutine(playerSR, go, playerScript, playerRb, knockbackVector));
        }
    }

    private IEnumerator PlayerImpactRoutine(SpriteRenderer playerSR, GameObject playerObj, Player playerScript, Rigidbody2D playerRb, Vector2 knockbackVector)
    {
        isPlayerInvincible = true;
        playerScript.enabled = false;

        // ÁP DỤNG LỰC VẬT LÝ (Unity 2026 dùng linearVelocity thay cho velocity)
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