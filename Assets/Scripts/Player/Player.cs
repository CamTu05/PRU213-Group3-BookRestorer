using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 9f;
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float fallMultiplier = 3.5f;
    [SerializeField] private float lowJumpMultiplier = 3f;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Coin & UI Settings")]
    [SerializeField] private AudioClip coinSound;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject gameOverText;

    [Header("Health & Trap Settings")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private Transform healthContainer; // Ô kéo thả thanh tim vào đây
    [SerializeField] private float iframeDuration = 1.2f;
    private int currentHealth;
    private float iframeTimer;

    [Header("Ledge Grab Settings")]
    [SerializeField] private Transform ledgeCheck;
    [SerializeField] private float ledgeCheckRadius = 0.15f;
    [SerializeField] private float ledgeJumpForce = 12f;
    [SerializeField] private LayerMask wallLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private AudioSource audioSource;
    private float horizontalInput;
    private bool isFacingRight = true;
    private bool isGrounded;
    private bool shouldJump;
    private float ledgeGrabCooldown;

    private bool isTouchingLedge;
    private bool isLedgeGrabbing;
    private int score = 0;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 3f;
        }

        currentHealth = maxHealth;
        if (gameOverText != null)
        {
            gameOverText.SetActive(false);
        }
        UpdateHealthUI();
        UpdateScoreUI();
    }

    private void Update()
    {
        HandleInput();
        UpdateAnimations();
        CheckGround();
        CheckLedge();

        if (iframeTimer > 0)
        {
            iframeTimer -= Time.deltaTime;
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

            if (wasTouchingLedge && !isTouchingLedge && !isGrounded && rb.linearVelocity.y <= 1f)
            {
                isLedgeGrabbing = true;
            }
        }
    }

    private void FixedUpdate()
    {
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
        if (keyboard == null) return;

        horizontalInput = 0f;
        if (!isLedgeGrabbing)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontalInput = -1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontalInput = 1f;
            FlipController();
        }

        if (keyboard.spaceKey.wasPressedThisFrame)
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

    private void Move()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    private void Jump()
    {
        if (shouldJump)
        {
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

    private void UpdateAnimations()
    {
        if (anim != null)
        {
            anim.SetBool("IsRun", horizontalInput != 0);
            anim.SetBool("IsLedgeGrab", isLedgeGrabbing);
            anim.SetBool("IsGrounded", isGrounded);
            anim.SetFloat("VelocityY", rb.linearVelocity.y);
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
    }

    public void TakeDamage(int damage)
    {
        if (iframeTimer > 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthUI();

        iframeTimer = iframeDuration;

        if (anim != null)
        {
            anim.SetTrigger("isHurt");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthContainer == null) return;
        for (int i = 0; i < healthContainer.childCount; i++)
        {
            healthContainer.GetChild(i).gameObject.SetActive(i < currentHealth);
        }
    }

    private void Die()
    {
        Debug.Log("Player đã cạn máu!");
        if (gameOverText != null)
        {
            gameOverText.SetActive(true);
        }
        horizontalInput = 0;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static; 
        this.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
            if (audioSource != null && coinSound != null)
            {
                audioSource.PlayOneShot(coinSound);
            }
            score++;
            UpdateScoreUI();
            Destroy(collision.gameObject);
        }

        if (collision.CompareTag("Trap"))
        {
            TakeDamage(1);
        }
        if (collision.CompareTag("Finish"))
        {
            
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.Log("Đã phá đảo màn chơi! Tải lại màn 1.");
                SceneManager.LoadScene(0);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trap"))
        {
            TakeDamage(1);
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Coins: " + score;
        }
    }
}