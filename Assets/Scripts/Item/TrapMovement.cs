using UnityEngine;

public class TrapMovement : MonoBehaviour
{
    [Header("Cột mốc di chuyển")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Tốc độ di chuyển")]
    [SerializeField] private float speed = 2f;

    private Transform targetTarget;

    public void Start()
    {
        targetTarget = pointB;
    }

    private void Update()
    {
        if (pointA == null || pointB == null || targetTarget == null) return;

        // Di chuyển bục từ vị trí hiện tại đến mục tiêu
        transform.position = Vector3.MoveTowards(transform.position, targetTarget.position, speed * Time.deltaTime);

        // Nếu đã chạy đến sát cột mốc mục tiêu
        if (Vector3.Distance(transform.position, targetTarget.position) < 0.1f)
        {
            targetTarget = (targetTarget == pointB) ? pointA : pointB;
        }
    }

    // Khi người chơi nhảy lên bục
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra nếu vật thể va chạm có Tag là "Player"
        if (collision.CompareTag("Player"))
        {
            // Đặt bục này làm cha của Player
            collision.transform.SetParent(this.transform);
        }
    }

    // Khi người chơi nhảy ra khỏi bục
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Hủy quan hệ cha-con (trả Player về gốc của Scene)
            collision.transform.SetParent(null);
        }
    }
}