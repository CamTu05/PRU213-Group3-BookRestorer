using UnityEngine;
using System.Collections.Generic; // Bắt buộc để dùng List

public class PlayerDamageByPrefab : MonoBehaviour
{
    private PlayerHealth playerHealth;

    [System.Serializable]
    public class DamageGroup
    {
        public string groupName; // Tên gợi nhớ (Ví dụ: "Bẫy Gai", "Quái Thường")
        public int damageAmount = 1; // Lượng sát thương của nhóm này
        public List<GameObject> prefabs; // Nơi kéo thả các Prefab vào đây
    }

    [Header("Cấu hình Sát thương ngoài GUI")]
    [SerializeField] private List<DamageGroup> damageGroups;

    private void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckAndApplyDamage(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckAndApplyDamage(collision.gameObject);
    }

    private void CheckAndApplyDamage(GameObject hitObject)
    {
        if (playerHealth == null) return;

        // Duyệt qua từng nhóm sát thương bạn cấu hình ngoài GUI
        foreach (var group in damageGroups)
        {
            foreach (var prefab in group.prefabs)
            {
                if (prefab != null)
                {
                    // Kiểm tra xem vật va chạm có phải là bản sao của Prefab này không
                    // Unity 6 sử dụng tên hoặc cấu trúc gốc để đối chiếu bản sao từ Prefab
                    if (hitObject.name.StartsWith(prefab.name))
                    {
                        playerHealth.TakeDamage(group.damageAmount);
                        return; // Trừ máu xong thì dừng lại luôn
                    }
                }
            }
        }
    }
}
