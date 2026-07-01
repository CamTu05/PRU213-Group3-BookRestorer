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
    [SerializeField] private float ledgeCheckRadius = 0.3f; 
    [SerializeField] private float ledgeJumpForce = 12f;
    [SerializeField] private LayerMask wallLayer;
    //start: ChiTTP_Tốc độ leo tường_010726
    [SerializeField] private float climbSpeed = 4f;
    //end

    [Header("Attack Settings")]
    [SerializeField] private Transform attackPoint;      
    [SerializeField] private float attackRange = 1.3f;   
    [SerializeField] private float damageAmount = 25f;

    private Rigidbody2D rb;
    private Animator anim;

    private float horizontalInput;
    //start: ChiTTP_Lấy thêm input dọc để leo tường_010726
    private float verticalInput;
    //end
    private bool isFacingRight = true;
    [SerializeField] private bool isGrounded;
    private bool shouldJump;
    private float ledgeGrabCooldown;
    private bool isTouchingLedge;
    private bool isLedgeGrabbing;
    //start
    //ChiTTP_Tam trang thai cui va leo_260626
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
            rb.linearVelocity = new Vector2(0f, verticalInput * climbSpeed);
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
        var mouse = Mouse.current;
        if (keyboard == null) return;


        horizontalInput = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontalInput = -1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontalInput = 1f;

 
        if (!isLedgeGrabbing)
        {
            FlipController();
        }

     
        verticalInput = 0f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) verticalInput = 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) verticalInput = -1f;

       
        if (isGrounded && !isLedgeGrabbing && (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed))
        {
            isCrouching = true;
        }
        else
        {
            isCrouching = false;
        }

  
        bool shiftAndUp = keyboard.shiftKey.isPressed;
        bool zPressed = keyboard.zKey.isPressed;
        wantsToClimb = shiftAndUp || zPressed;

       
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            Attack();
        }

   
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            if (isGrounded)
            {
                shouldJump = true;
            }
            else if (isLedgeGrabbing)
            {
                ledgeGrabCooldown = 0.3f;
                isLedgeGrabbing = false;
                rb.gravityScale = 3f;

              
                float jumpDirection = isFacingRight ? -1f : 1f;
                rb.linearVelocity = new Vector2(jumpDirection * moveSpeed * 0.8f, ledgeJumpForce);
            }
        }
    }

    private void Attack()
    {
        if (anim != null) anim.SetTrigger("Attack");
        if (attackPoint == null) return;

        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);
        foreach (Collider2D obj in hitObjects)
        {
            if (obj.CompareTag("Enemy"))
            {
                Enemy enemy = obj.GetComponent<Enemy>();
                if (enemy != null) enemy.TakeDamage(damageAmount);
            }
        }
    }

    private void Move()
    {
        float currentSpeed = isCrouching ? moveSpeed * crouchSpeedMultiplier : moveSpeed;
        rb.linearVelocity = new Vector2(horizontalInput * currentSpeed, rb.linearVelocity.y);
    }

    private void Jump()
    {
        if (shouldJump)
        {
            if (isCrouching) return;
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
            
            Vector2 rayDirection = new Vector2(transform.localScale.x, 0).normalized;
            RaycastHit2D hit = Physics2D.Raycast(ledgeCheck.position, rayDirection, ledgeCheckRadius + 0.2f, wallLayer);

            isTouchingLedge = (hit.collider != null);

            if (isTouchingLedge && !isGrounded && wantsToClimb)
            {
                isLedgeGrabbing = true;
            }

            if (isLedgeGrabbing && (!wantsToClimb || isGrounded || !isTouchingLedge))
            {
                isLedgeGrabbing = false;
                ledgeGrabCooldown = 0.2f;
            }
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
            anim.SetBool("IsCrouching", isCrouching);
            anim.SetFloat("ClimbSpeed", Mathf.Abs(verticalInput));
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
            Vector3 direction = new Vector3(transform.localScale.x, 0, 0).normalized;
            Gizmos.DrawLine(ledgeCheck.position, ledgeCheck.position + direction * (ledgeCheckRadius + 0.2f));
        }
        if (attackPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}