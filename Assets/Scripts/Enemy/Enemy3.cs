using UnityEngine;

public class Enemy3 : Enemy
{
    [Header("Detection Settings")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask playerLayer;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2.0f;
    private float lastAttackTime = -999f;
    private bool isAttacking = false;

    // Các biến tuần tra cũ
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

        Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerLayer);
        return hitPlayer != null;
    }

    // ====================== TẤN CÔNG ======================
    private void PerformAttack()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;           // Dừng di chuyển khi đánh

        anim.SetBool("Is_Attack", true);            // Bật bool cho Animator

        lastAttackTime = Time.time;
    }

    // Hàm này được gọi bởi Animation Event ở frame cuối của animation Attack
    public void EndAttack()
    {
        isAttacking = false;
        anim.SetBool("Is_Attack", false);           // Tắt bool để quay về Idle/Run
    }

    // Hàm gây damage (gọi bởi Animation Event ở frame đánh trúng)
    public void DealDamageToPlayer()
    {
        Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerLayer);
        if (hitPlayer != null)
        {
            PlayerHealth health = hitPlayer.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(1);
            }
        }
    }

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