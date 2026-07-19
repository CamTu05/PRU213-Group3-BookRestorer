using UnityEngine;

public class Enemy2 : Enemy
{
    [Header("Detection Settings")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask playerLayer;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2.0f;
    private float lastAttackTime = -999f;
    private bool isAttacking = false;

    // === Cài đặt cho Attack 2 ===
    [SerializeField] private float attack2Range = 1.8f;           // Có thể khác attack 1
    [SerializeField] private float attack2CooldownMultiplier = 1.1f; // Có thể cooldown lâu hơn một chút

    // Các biến tuần tra
    [SerializeField] private float patrolDistance = 5f;
    [SerializeField] private float speed = 2f;
    private Vector2 startPos;
    private bool movingRight = true;

    protected override void Start()
    {
        base.Start();
        startPos = transform.position;
    }

    private void Update()
    {
        if (isDead) return;
        if (isAttacking) return;                    // Đang tấn công thì không làm gì khác

        if (CanAttackPlayer())
        {
            PerformAttack();
        }
        else
        {
            Patrol();
        }
    }

    private bool CanAttackPlayer()
    {
        if (Time.time < lastAttackTime + attackCooldown) return false;

        // Kiểm tra trong tầm đánh của cả 2 attack
        Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerLayer);
        return hitPlayer != null;
    }

    // ====================== TẤN CÔNG RANDOM ======================
    private void PerformAttack()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;           // Dừng di chuyển khi đánh

        // Random chọn Attack hoặc Attack2 (50/50)
        bool useAttack2 = Random.value > 0.5f;

        if (useAttack2)
        {
            anim.SetBool("Is_Attack2", true);       // Trigger Attack 2
            lastAttackTime = Time.time + (attackCooldown * attack2CooldownMultiplier);
        }
        else
        {
            anim.SetBool("Is_Attack", true);        // Trigger Attack thường
            lastAttackTime = Time.time;
        }
        PlayAttackSound();
    }

    // ====================== ANIMATION EVENTS ======================

    // Gọi bởi Animation Event ở frame cuối của animation Attack
    public void EndAttack()
    {
        isAttacking = false;
        anim.SetBool("Is_Attack", false);
    }

    // Gọi bởi Animation Event ở frame cuối của animation Attack2
    public void EndAttack2()
    {
        isAttacking = false;
        anim.SetBool("Is_Attack2", false);
    }

    // Gọi bởi Animation Event ở frame đánh trúng của Attack 1
    public void DealDamageToPlayer()
    {
        DealDamage(attackRange);
    }

    // Gọi bởi Animation Event ở frame đánh trúng của Attack 2
    public void DealDamageToPlayer2()
    {
        DealDamage(attack2Range);
    }

    private void DealDamage(float range)
    {
        Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, range, playerLayer);
        if (hitPlayer != null)
        {
            PlayerHealth health = hitPlayer.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(1);   // Có thể thay đổi sát thương sau
            }
        }
    }

    // ====================== PATROL ======================
    private void Patrol()
    {
        transform.Translate(Vector2.right * speed * (movingRight ? 1 : -1) * Time.deltaTime);

        if (movingRight && transform.position.x >= startPos.x + patrolDistance) Flip();
        else if (!movingRight && transform.position.x <= startPos.x - patrolDistance) Flip();
    }

    private void Flip()
    {
        movingRight = !movingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }
}