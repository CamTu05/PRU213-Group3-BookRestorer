using System.Collections;
using UnityEngine;

public class MushroomAI : Enemy
{
    [Header("Patrol Settings")]
    [SerializeField] private float patrolDistance = 4f; 
    [SerializeField] private float patrolSpeed = 2f;      
    [SerializeField] private float restTime = 1.0f;       
    
    private Vector2 positionA;
    private Vector2 positionB; 
    private Vector2 positionC;
    
    private Vector2[] patrolRoute;
    private int currentWaypointIndex = 0;
    private bool isResting = false;                    

    [Header("Chase & Attack Settings")]
    private float chaseSpeed;                             
    [SerializeField] private float visionRange = 5f;       
    [SerializeField] private float attackRange = 1.2f;    
    
    [Header("Attack Cooldown Settings")]
    [SerializeField] private float attackCooldown = 2.0f;  
    private bool isCooldown = false;                        

    [Header("Tether Settings (Vùng Giới Hạn Đuổi)")]
    [SerializeField] private float extraLeashDistance = 2.0f; 

    [Header("Animation Durations")]
    [SerializeField] private float attackClipLength = 1.3f; 
    [SerializeField] private float stunClipLength = 1.0f;   

    [Header("Ground & Wall Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckDistance = 0.6f; 
    [SerializeField] private float wallCheckDistance = 0.3f; 
    [SerializeField] private LayerMask groundLayer;         

    private bool isFacingRight = false; 
    private bool isChasing = false;
    private bool isReturning = false;
    
    private bool isAttacking = false;
    private bool isStunned = false; 
    private bool hasHitPlayer = false; 

    protected override void Start()
    {
        // Gọi Start của lớp cha (Enemy) để lấy Component và tự tìm PlayerTag
        base.Start();

        chaseSpeed = patrolSpeed * 1.2f; 

        // Khởi tạo các điểm tuần tra cố định dựa trên vị trí quái được đặt trong Scene
        positionB = transform.position; 
        positionA = new Vector2(positionB.x - patrolDistance, positionB.y); 
        positionC = new Vector2(positionB.x + patrolDistance, positionB.y); 

        patrolRoute = new Vector2[] { positionA, positionC };
        currentWaypointIndex = 0; 
        isFacingRight = false; 
    }

    private void Update()
    {
        if (isDead || patrolRoute == null) return;
        
        // Cập nhật tầm nhìn xem Player có đến gần không
        CheckVision();
        HandleAnimations();
    }

    private void FixedUpdate()
    {
        if (isDead || patrolRoute == null) return;

        // Nếu quái đang thực hiện các hành động khựng lại, đứng yên tại chỗ
        if (isAttacking || isStunned || isResting)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // Quyết định trạng thái di chuyển
        if (isReturning) ReturnToPatrolZoneLogic();
        else if (isChasing) ChaseAndAttackPlayerLogic();
        else PatrolLogic(); // Tuần tra tự động hoàn toàn độc lập
    }

    // ==========================================
    // 1. TỰ ĐỘNG TUẦN TRA LIÊN TỤC KHU VỰC A <-> C
    // ==========================================
    private void PatrolLogic()
    {
        Vector2 targetPosition = patrolRoute[currentWaypointIndex];
        float directionToTarget = targetPosition.x - transform.position.x;

        if (directionToTarget > 0 && !isFacingRight) Flip();
        else if (directionToTarget < 0 && isFacingRight) Flip();

        if (groundCheckPoint != null)
        {
            RaycastHit2D groundInfo = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckDistance, groundLayer);
            Vector2 lookDir = isFacingRight ? Vector2.right : Vector2.left;
            RaycastHit2D wallInfo = Physics2D.Raycast(groundCheckPoint.position, lookDir, wallCheckDistance, groundLayer);

            if (wallInfo.collider == true || groundInfo.collider == false)
            {
                SwitchWaypoint();
                return; 
            }
        }

        float speed = isFacingRight ? patrolSpeed : -patrolSpeed;
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);

        if (Mathf.Abs(directionToTarget) < 0.4f) 
        {
            StartCoroutine(RestAtWaypointRoutine());
        }
    }

    private void SwitchWaypoint()
    {
        currentWaypointIndex = (currentWaypointIndex == 0) ? 1 : 0;
    }

    private IEnumerator RestAtWaypointRoutine()
    {
        isResting = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); 
        SwitchWaypoint();
        yield return new WaitForSeconds(restTime); 
        isResting = false;
    }

    // ==========================================
    // 2. NHẬN DIỆN PLAYER (ĐÃ SỬA AN TOÀN - TỰ ĐỘNG CHUYỂN TUẦN TRA)
    // ==========================================
    private void CheckVision()
    {
        // Nếu đang bận giật mình/choáng/về tổ thì không quét tầm nhìn mới
        if (isReturning || isAttacking || isStunned) return;

        // SỬA ĐỔI QUAN TRỌNG: Nếu không thấy Player trong Scene, quái tự động chuyển sang đi tuần, KHÔNG đóng băng AI nữa
        if (playerTransform == null)
        {
            isChasing = false;
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        float maxAllowedDistance = patrolDistance + extraLeashDistance;
        float playerDistanceFromCenter = Vector2.Distance(playerTransform.position, positionB);

        // Player chạy vượt quá giới hạn xích quái -> Quái bỏ đuổi để quay lại vùng an toàn
        if (playerDistanceFromCenter > maxAllowedDistance)
        {
            if (isChasing)
            {
                isChasing = false;
                isReturning = true;
            }
            return;
        }

        // Phát hiện người chơi 360 độ (bất kể trước sau) khi lọt vào tầm mắt
        if (distanceToPlayer <= visionRange) 
        {
            isChasing = true;
        }
        else if (distanceToPlayer > visionRange * 1.2f) 
        {
            isChasing = false; // Player đi ra xa -> quái tiếp tục đi tuần tra
        }
    }

    private void ChaseAndAttackPlayerLogic()
    {
        if (playerTransform == null)
        {
            isChasing = false;
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange)
        {
            if (!isCooldown)
            {
                StartCoroutine(StopThenAttackRoutine());
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                anim.SetBool("IsRun", false);
            }
            return;
        }

        // Đuổi theo Player trực diện
        float directionX = playerTransform.position.x - transform.position.x;
        
        if (directionX > 0 && !isFacingRight) Flip();
        else if (directionX < 0 && isFacingRight) Flip();

        float moveDir = isFacingRight ? chaseSpeed : -chaseSpeed;
        rb.linearVelocity = new Vector2(moveDir, rb.linearVelocity.y);
    }

    // ==========================================
    // 3. TẤN CÔNG & QUAY VỀ TỔ KHU VỰC B
    // ==========================================
    private IEnumerator StopThenAttackRoutine()
    {
        isChasing = false;
        isAttacking = true;
        hasHitPlayer = false; 

        rb.linearVelocity = Vector2.zero;
        anim.SetBool("IsRun", false);

        yield return new WaitForFixedUpdate();
        anim.SetBool("IsAttack", true);

        yield return new WaitForSeconds(attackClipLength * 0.5f); 

        Vector2 scanDirection = isFacingRight ? Vector2.right : Vector2.left;
        Vector2 scanCenter = (Vector2)transform.position + scanDirection * (attackRange * 0.5f);
        Vector2 scanSize = new Vector2(attackRange, 2f); 

        Collider2D[] hitObjects = Physics2D.OverlapBoxAll(scanCenter, scanSize, 0f);
        foreach (Collider2D obj in hitObjects)
        {
            if (obj.CompareTag("Player"))
            {
                hasHitPlayer = true; 
                // Do PlayerHealth của bạn xử lý sát thương
                var playerHealth = obj.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageToPlayer);
                }
                break;
            }
        }

        yield return new WaitForSeconds(attackClipLength * 0.5f);

        anim.SetBool("IsAttack", false);
        isAttacking = false;

        StartCoroutine(AttackCooldownRoutine());

        if (hasHitPlayer)
        {
            isReturning = true; 
        }
        else
        {
            StartCoroutine(StunPenaltyRoutine());
        }
    }

    private IEnumerator AttackCooldownRoutine()
    {
        isCooldown = true; 
        yield return new WaitForSeconds(attackCooldown);
        isCooldown = false; 
    }

    private IEnumerator StunPenaltyRoutine()
    {
        isStunned = true;
        anim.SetBool("IsStun", true);
        yield return new WaitForSeconds(stunClipLength);
        anim.SetBool("IsStun", false);
        isStunned = false;
        isReturning = true; 
    }

    private void ReturnToPatrolZoneLogic()
    {
        float directionToB = positionB.x - transform.position.x;

        if (Mathf.Abs(directionToB) < 0.6f)
        {
            isReturning = false;
            currentWaypointIndex = 0; 
            return;
        }

        if (directionToB > 0 && !isFacingRight) Flip();
        else if (directionToB < 0 && isFacingRight) Flip();

        float speed = isFacingRight ? patrolSpeed : -patrolSpeed;
        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
    }

    // Đồng bộ hàm bị đánh từ Class cha Enemy (sử dụng override thay vì gán đè public thường)
    public override void TakeDamage(float damageAmount)
    {
        if (isDead) return;
        base.TakeDamage(damageAmount); // Giữ nguyên logic trừ máu, đổi màu của Enemy.cs
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        
        // Nhân ảnh ngược lại (-1) dựa theo quy chuẩn Asset gốc hướng trái của bạn
        localScale.x = Mathf.Abs(localScale.x) * (isFacingRight ? -1 : 1);
        
        transform.localScale = localScale;
    }

    private void HandleAnimations()
    {
        if (isAttacking || isStunned || isResting)
        {
            anim.SetBool("IsRun", false);
        }
        else
        {
            anim.SetBool("IsRun", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 scanDirection = isFacingRight ? Vector2.right : Vector2.left;
        Vector2 scanCenter = (Vector2)transform.position + scanDirection * (attackRange * 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(scanCenter, new Vector2(attackRange, 2f));

        Vector2 centerPoint = Application.isPlaying ? positionB : (Vector2)transform.position;

        Gizmos.color = Color.blue;
        Vector2 leftPatrolBound = new Vector2(centerPoint.x - patrolDistance, centerPoint.y);
        Vector2 rightPatrolBound = new Vector2(centerPoint.x + patrolDistance, centerPoint.y);
        Gizmos.DrawLine(leftPatrolBound + Vector2.up, leftPatrolBound + Vector2.down);
        Gizmos.DrawLine(rightPatrolBound + Vector2.up, rightPatrolBound + Vector2.down);
        Gizmos.DrawLine(leftPatrolBound, rightPatrolBound); 

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(centerPoint, patrolDistance + extraLeashDistance);
    }
}