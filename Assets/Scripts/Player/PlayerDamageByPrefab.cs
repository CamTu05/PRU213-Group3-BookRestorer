//chittp-0807
using UnityEngine;
using System.Collections.Generic;

public class PlayerDamageByPrefab : MonoBehaviour
{
    private PlayerHealth playerHealth;

    [System.Serializable]
    public class DamageGroup
    {
        public string groupName; 
        public int damageAmount = 1; 
        public List<GameObject> prefabs;
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

        foreach (var group in damageGroups)
        {
            foreach (var prefab in group.prefabs)
            {
                if (prefab != null)
                {
                    
                    if (hitObject.name.StartsWith(prefab.name))
                    {
                        playerHealth.TakeDamage(group.damageAmount);
                        return;
                    }
                }
            }
        }
    }
}
