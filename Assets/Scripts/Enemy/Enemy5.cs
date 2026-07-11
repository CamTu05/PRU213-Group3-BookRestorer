using UnityEngine;

public class Enemy5 : Enemy
{
    [Header("Detection Settings")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask playerLayer;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 2.0f;
    private float lastAttackTime = -999f;
    private bool isAttacking = false;

    // Các biến tuần tra
    [SerializeField] private float patrolDistance = 5f;
    [SerializeField] private float speed = 2f;
    private Vector2 startPos;
    private bool movingRight = false;        // ← Đổi thành false

    protected override void Start()
    {
        base.Start();
        startPos = transform.position;

        // Flip ngay từ đầu vì sprite hướng trái
        FlipSpriteAtStart();
    }

    private void FlipSpriteAtStart()
    {
        // Lật sprite sang phải để đồng bộ với các enemy khác
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;           // Lật sang phải
        transform.localScale = scaler;

        // Đồng bộ biến movingRight
        movingRight = true;
    }

    private void Update()
    {
        if (isDead) return;
        if (isAttacking) return;

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

    private void PerformAttack()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("Is_Attack", true);
        lastAttackTime = Time.time;
    }

    public void EndAttack()
    {
        isAttacking = false;
        anim.SetBool("Is_Attack", false);
    }

    public void DealDamageToPlayer()
    {
        Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerLayer);
        if (hitPlayer != null)
        {
            PlayerHealthController health = hitPlayer.GetComponent<PlayerHealthController>();
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