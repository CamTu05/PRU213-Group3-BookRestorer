using System.Collections;
using UnityEngine;

public class RockHeadTrap : MonoBehaviour
{
    [Header("Cột mốc di chuyển")]
    [SerializeField]private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Cài đặt tốc độ")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float waitTimeAtBottom = 0.8f;

    private Transform targetTarget;
    private Animator anim;
    private bool isWaiting = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
        if (pointA != null) targetTarget = pointA; // Bắt đầu ở trên cao
    }

    private void Update()
    {
        // Nếu đang trong thời gian đứng im chờ đợi thì đứng im, không chạy code di chuyển dưới
        if (isWaiting) return;

        if (pointA == null || pointB == null || targetTarget == null) return;

        // Di chuyển tảng đá
        transform.position = Vector3.MoveTowards(transform.position, targetTarget.position, speed * Time.deltaTime);

        // Khi chạm vào các điểm mốc
        if (Vector3.Distance(transform.position, targetTarget.position) < 0.1f)
        {
            if (targetTarget == pointB)
            {
                // NẾU CHẠM ĐẤT: Kích hoạt chuỗi đứng im dập bụi
                StartCoroutine(HitBottomRoutine());
            }
            else
            {
                // Nếu quay về đỉnh trần (Điểm A) thì chỉ cần đổi mục tiêu để rơi xuống tiếp
                targetTarget = pointB;
            }
        }
    }

    // Hàm xử lý thời gian chờ dập bụi bằng Coroutine
    private IEnumerator HitBottomRoutine()
    {
        isWaiting = true; // Khóa di chuyển lại

        if (anim != null)
        {
            // Bật Trigger chuyển sang hoạt ảnh Bottom Hit trong Animator
            anim.SetTrigger("HitBottom");
        }

        // Đứng im tại chỗ trong vài giây cho tảng đá nhìn thật nặng
        yield return new WaitForSeconds(waitTimeAtBottom);

        // Hết thời gian chờ: Đổi mục tiêu quay về Điểm A và cho phép di chuyển tiếp
        targetTarget = pointA;
        isWaiting = false;
    }
}
