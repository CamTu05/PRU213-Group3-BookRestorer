using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarAnimate : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image healthBarImage;
    [SerializeField] private GameObject healthBarUI;

    [Header("Thứ tự các ảnh từ Đầy đến Hết máu")]
    // Bạn kéo danh sách các ảnh đã cắt từ GIF vào đây theo đúng thứ tự từ Đầy -> Cạn
    [SerializeField] private Sprite[] healthFrames;

    private void Start()
    {
        // Mặc định ẩn đi khi đầy máu
        if (healthBarUI != null) healthBarUI.SetActive(false);
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthFrames == null || healthFrames.Length == 0) return;

        // Bật thanh máu lên khi bị đánh
        if (healthBarUI != null && !healthBarUI.activeSelf)
        {
            healthBarUI.SetActive(true);
        }

        // Tính toán tỷ lệ máu hiện tại (từ 0.0 đến 1.0)
        float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);

        // Quy đổi tỷ lệ phần trăm sang số thứ tự Khung hình (Index) trong mảng ảnh
        // Vì ảnh đầu tiên (Index 0) là ĐẦY MÁU, ảnh cuối (Index cuối) là HẾT MÁU:
        int frameIndex = Mathf.RoundToInt((1f - healthPercent) * (healthFrames.Length - 1));

        // Giới hạn an toàn để không bị lỗi vượt quá mảng
        frameIndex = Mathf.Clamp(frameIndex, 0, healthFrames.Length - 1);

        // Đổi ảnh hiển thị trên UI sang đúng frame đó
        if (healthBarImage != null)
        {
            healthBarImage.sprite = healthFrames[frameIndex];
        }
    }
}