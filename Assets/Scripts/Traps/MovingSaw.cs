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
    [SerializeField] private AudioClip trapAudioSource;

    [Header("Cấu Hình Hậu Quả")]
    public int damage = 3;

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
        if (!collision.CompareTag("Player"))
            return;

        float dir = collision.transform.position.x > saw.position.x ? 1f : -1f;

        collision.GetComponent<PlayerHealth>()?.TakeDamage(damage, dir);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(trapAudioSource, 0.5f);
        }
    }

}