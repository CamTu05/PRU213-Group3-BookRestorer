using System.Runtime.CompilerServices;
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

        // Di chuyển lưỡi cưa từ vị trí hiện tại đến mục tiêu (Target)
        transform.position = Vector3.MoveTowards(transform.position, targetTarget.position, speed * Time.deltaTime);

        // Nếu đã chạy đến sát cột mốc mục tiêu (khoảng cách < 0.1)
        if (Vector3.Distance(transform.position, targetTarget.position) < 0.1f)
        {
            // Đổi mục tiêu ngược lại để quay đầu di chuyển
            if (targetTarget == pointB)
            {
                targetTarget = pointA;
            }
            else
            {
                targetTarget = pointB;
            }
        }
    }
}
