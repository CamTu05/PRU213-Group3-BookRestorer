using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Enemy : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected int damageToPlayer = 1;
    protected float currentHealth;
    protected bool isDead = false;

    [Header("Base Components")]
    protected Rigidbody2D rb;
    protected Animator anim;
    protected Transform playerTransform;

    [Header("Health setting")]
    [SerializeField] private EnemyHealthBarAnimate healthBar;
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;

        // Tự động tìm kiếm nhân vật Player trong bản đồ qua Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    // Hàm nhận sát thương dùng chung cho mọi loại quái
    public virtual void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} trúng đòn! Máu còn: {currentHealth}/{maxHealth}");

        // CẬP NHẬT Ở ĐÂY: Gọi thanh máu nhảy frame ảnh
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }

        // Kích hoạt animation bị đau (IsHit)
        StopAllCoroutines();
        StartCoroutine(HitVisualRoutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual IEnumerator HitVisualRoutine()
    {
        anim.SetBool("IsHit", true);
        yield return new WaitForSeconds(0.15f); // Khựng lại diễn cảnh đau một chút
        anim.SetBool("IsHit", false);
    }

    // Hàm xử lý khi quái hết máu
    protected virtual void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        // Kích hoạt trạng thái chết trong Animator
        anim.SetBool("IsDie", true);

        // Tắt va chạm vật lý để quái không cản đường Player sau khi chết
        GetComponent<Collider2D>().enabled = false;
        rb.simulated = false;

        Debug.Log($"{gameObject.name} đã chết!");
        Destroy(gameObject, 1.5f); // Xóa quái khỏi Game sau 1.5 giây
    }
}