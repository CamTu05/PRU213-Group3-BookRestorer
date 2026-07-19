using UnityEngine;

public class RotateTrap : MonoBehaviour
{
    [Header("Cài đặt tốc độ xoay")]
    [SerializeField] private float rotationSpeed = 100f; // Tốc độ quay tròn

    [Header("Chế độ quả lắc (Đu qua đu lại)")]
    [SerializeField] private bool usePendulumMode = false; // Tích chọn nếu muốn đu đưa kiểu đồng hồ
    [SerializeField] private float maxSwingAngle = 45f;    // Góc đu tối đa sang 2 bên

    private void Update()
    {
        if (usePendulumMode)
        {
            // CƠ CHẾ 1: Đu qua đu lại như quả lắc đồng hồ dùng hàm Sin toán học
            float angle = Mathf.Sin(Time.time * (rotationSpeed / 50f)) * maxSwingAngle;
            transform.localRotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            // CƠ CHẾ 2: Xoay tròn đều 360 độ tít thò lờ
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }
}