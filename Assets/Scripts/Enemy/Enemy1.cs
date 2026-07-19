using UnityEngine;

public class Enemy1 : Enemy
{
    [Header("Chase Settings")]
    [SerializeField] private float visionRange = 8f;        // Tầm nhìn
    [SerializeField] private float chaseSpeed = 3f;         // Tốc độ truy đuổi
    [SerializeField] private float patrolSpeed = 1.8f;      // Tốc độ tuần tra khi không thấy Player

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float attackCooldown = 2.0f;

    [Header("Random Flip")]
    [SerializeField] private float minFlipInterval = 3f;    // Thời gian ngắn nhất giữa 2 lần flip
    [SerializeField] private float maxFlipInterval = 7f;    // Thời gian dài nhất

    private float lastAttackTime = -999f;
    private bool isAttacking = false;
    private float nextFlipTime;

    private Vector2 startPos;
    private bool isChasing = false;

    protected override void Start()
    {
        base.Start();
        startPos = transform.position;
        nextFlipTime = Time.time + Random.Range(minFlipInterval, maxFlipInterval);
    }

    private void Update()
    {
        if (isDead) return;
        if (isAttacking) return;

        CheckPlayerDistance();
        RandomFlipTimer();

        if (CanAttackPlayer())
        {
            PerformAttack();
        }
    }

    private void FixedUpdate()
    {
        if (isDead || isAttacking) return;

        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            SimplePatrol();        // Vẫn giữ tuần tra nhẹ khi không thấy Player
        }
    }

    // Kiểm tra khoảng cách với Player
    private void CheckPlayerDistance()
    {
        if (playerTransform == null) 
        {
            isChasing = false;
            return;
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= visionRange)
            isChasing = true;
        else if (distance > visionRange * 1.3f)
            isChasing = false;
    }

    private void ChasePlayer()
    {
        if (playerTransform == null) return;

        float direction = playerTransform.position.x - transform.position.x;

        // Tự động quay mặt về phía Player
        if (direction > 0 && transform.localScale.x < 0) Flip();
        if (direction < 0 && transform.localScale.x > 0) Flip();

        float speed = chaseSpeed * Mathf.Sign(direction);
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
    }

    private void SimplePatrol()
    {
        // Tuần tra đơn giản khi không thấy Player
        float speed = patrolSpeed * (transform.localScale.x > 0 ? 1 : -1);
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
    }

    // Flip ngẫu nhiên theo thời gian
    private void RandomFlipTimer()
    {
        if (Time.time >= nextFlipTime)
        {
            Flip();
            nextFlipTime = Time.time + Random.Range(minFlipInterval, maxFlipInterval);
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
            PlayerHealth health = hitPlayer.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(1);
            }
        }
    }

    private void Flip()
    {
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    private void OnDrawGizmosSelected()
    {
        // Tầm nhìn
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // Tầm tấn công
        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}