using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 9f;
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float fallMultiplier = 3.5f;
    [SerializeField] private float lowJumpMultiplier = 3f;
    //start: ChiTTP_Them toc do khi cui_260626
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;
    //end
    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.4f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Ledge Grab Settings")]
    [SerializeField] private Transform ledgeCheck;
    [SerializeField] private float ledgeCheckRadius = 0.15f;
    [SerializeField] private float ledgeJumpForce = 12f;
    [SerializeField] private LayerMask wallLayer;

    [Header("Attack Settings")]
    [SerializeField] private Transform attackPoint;      // Tâm của Hitbox đặt trước mặt Player
    [SerializeField] private float attackRange = 1.3f;    // Bán kính vùng đánh
    [SerializeField] private float damageAmount = 25f;

    private Rigidbody2D rb;
    private Animator anim;

    private float horizontalInput;
    private bool isFacingRight = true;
    [SerializeField] private bool isGrounded;
    private bool shouldJump;
    private float ledgeGrabCooldown;
    private bool isTouchingLedge;
    private bool isLedgeGrabbing;
    //start
    //ChiTTP_Them trang thai cui va leo_260626
    private bool isCrouching;
    private bool wantsToClimb;
    //end

    public float HorizontalInput => horizontalInput;
    public bool IsLedgeGrabbing => isLedgeGrabbing;
    public bool IsGrounded => isGrounded;
    //start
    //ChiTTP_Getter cho trangj thai cui
    public bool IsCrouching => isCrouching;
    //end

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 3f;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        isLedgeGrabbing = false;
        isCrouching = false;
        wantsToClimb = false;
    }

    private void Update()
    {
        HandleInput();
        UpdateAnimations();
       
    }

    private void FixedUpdate()
    {
        CheckGround();
        CheckLedge();
        
        if (isLedgeGrabbing)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            rb.gravityScale = 3f;
            Move();
            Jump();
            OptimizeJumpPhysics();
        }
    }

    private void HandleInput()
    {
        var keyboard = Keyboard.current;
        //start
        //ChiTTP_Lay thong tin chuot_260626
        var mouse = Mouse.current;
        //end
        if (keyboard == null) return;

        horizontalInput = 0f;
        if (!isLedgeGrabbing)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontalInput = -1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontalInput = 1f;
            FlipController();
        }
        //start
        //ChiTTP_Xu ly cui nguoi S or down
        if (isGrounded && (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed))
        {
            isCrouching = true;
        }
        else
        {
            isCrouching = false;
        }
        //end
        //start
        //ChiTTP_Ktra dk leo tuong Shift or Z
        bool shiftAndUp = keyboard.shiftKey.isPressed;
        bool zPressed = keyboard.zKey.isPressed;
        wantsToClimb = shiftAndUp || zPressed;
        //end
        //Start
        //ChiTTp_Tan caong bang chuot trai_260626
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            Attack();
        }
        //end

        if (keyboard.spaceKey.wasPressedThisFrame )
        {
            if (isGrounded)
            {
                shouldJump = true;
            }
            else if (isLedgeGrabbing)
            {
                ledgeGrabCooldown = 0.25f;
                isLedgeGrabbing = false;
                rb.gravityScale = 3f;

                float jumpDirection = isFacingRight ? 1f : -1f;
                rb.linearVelocity = new Vector2(jumpDirection * moveSpeed * 0.4f, ledgeJumpForce);
            }
        }
    }

    //ChiTTP_Kich hoatj Trigger tan cong_260626
   
       private void Attack()
    {
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        if (attackPoint == null) return;

        // Quét TẤT CẢ các Collider nằm trong vòng tròn Hitbox (không lọc theo Layer nữa)
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);

        // Duyệt qua từng đối tượng quét trúng
        foreach (Collider2D obj in hitObjects)
        {
            // Kiểm tra xem đối tượng đó có Tag là "Enemy" hay không
            if (obj.CompareTag("Enemy"))
            {
                // Lấy Component Enemy từ quái để trừ máu
                Enemy enemy = obj.GetComponent<Enemy>();

                if (enemy != null)
                {
                    enemy.TakeDamage(damageAmount);
                }
            }
        }
    }


    private void Move()
    {
        //start: ChiTTP_giam toc do khi cui nguoi + di chuyen_260626
        float currentSpeed = isCrouching ? moveSpeed * crouchSpeedMultiplier : moveSpeed;
        rb.linearVelocity = new Vector2(horizontalInput * currentSpeed, rb.linearVelocity.y);
        //end
    }

    private void Jump()
    {
        if (shouldJump)
        {
            //start:ChiTTP_ko cho nhay neu dang cui nguoi_260626
            if (isCrouching) return;
            //end
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            shouldJump = false;
        }
    }

    private void OptimizeJumpPhysics()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !keyboard.spaceKey.isPressed)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    private void CheckGround()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        }
    }

    private void CheckLedge()
    {
        if (ledgeGrabCooldown > 0)
        {
            ledgeGrabCooldown -= Time.deltaTime;
            isLedgeGrabbing = false;
            return;
        }

        if (ledgeCheck != null)
        {
            bool wasTouchingLedge = isTouchingLedge;
            isTouchingLedge = Physics2D.OverlapCircle(ledgeCheck.position, ledgeCheckRadius, wallLayer);
            //start:ChiTTP_CHo bam togn neus thoa man nut bam yeu cau
            if (isTouchingLedge && !isGrounded && wantsToClimb)
            {
                isLedgeGrabbing = true;
            }
            //dang bam maf buong ra thi roi
            if (isLedgeGrabbing && (!wantsToClimb|| isGrounded))
            {
                isLedgeGrabbing = false;
                ledgeGrabCooldown = 0.25f;
            }
            //end
        }
    }

    private void UpdateAnimations()
    {
        if (anim != null)
        {
            anim.SetBool("IsRun", horizontalInput != 0);
            anim.SetBool("IsLedgeGrab", isLedgeGrabbing);
            anim.SetBool("IsGrounded", isGrounded);
            anim.SetFloat("VelocityY", rb.linearVelocity.y);
            //start:ChiTTP_Dong bo cui nguoi_260626
            anim.SetBool("IsCrouching", isCrouching);
            //end
        }
    }

    private void FlipController()
    {
        if ((horizontalInput < 0 && isFacingRight) || (horizontalInput > 0 && !isFacingRight))
        {
            isFacingRight = !isFacingRight;
            Vector3 currentScale = transform.localScale;
            currentScale.x *= -1;
            transform.localScale = currentScale;
        }
    }

    public void FreezeOnDeath()
    {
        horizontalInput = 0;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }
        this.enabled = false;
    }

    public void FreezeOnWin()
    {
        horizontalInput = 0f;
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
        if (ledgeCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(ledgeCheck.position, ledgeCheckRadius);
        }
        if (attackPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}