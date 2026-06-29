//Author: Nguyễn Phương Nam//Date: 31/05/2026//Description: Quản lý hành vi của nhân vật người chơi, bao gồm di chuyển, nhảy//Author: Nguyễn Tín Nghĩa//Date: 03/06/2026//Description: Thêm xử lý UI liên quan đến điểm số và sức khỏe. Cũng bao gồm cơ chế bắt ledge để tăng tính tăng tương tác khi chơi.//Author: Nguyễn Văn Đức//Date: 07/06/2026//Description: Thêm xử lý va chạm để cập nhật máu, vàng using System;using TMPro;using UnityEngine;using UnityEngine.InputSystem;using UnityEngine.SceneManagement;

using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private GameObject winPanel;

    [Header("Health & Trap Settings")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private Transform healthContainer;
    [SerializeField] private float iframeDuration = 1.2f;
    private int currentHealth;
    private float iframeTimer;

    [Header("Ledge Grab Settings")]
    [SerializeField] private Transform ledgeCheck;
    [SerializeField] private float ledgeCheckRadius = 0.15f;
    [SerializeField] private float ledgeJumpForce = 12f;
    [SerializeField] private LayerMask wallLayer;

    [Header("Letter Chest System (New)")]
    [SerializeField] private GameObject letterPopupPanel;         // Bảng hiển thị chữ khi bấm vào rương
    [SerializeField] private TMPro.TextMeshProUGUI popupLettersText; // Chữ hiển thị danh sách đã nhặt inside popup
    [SerializeField] private string targetWord = "ANIMAL";           // Từ cần ghép của màn chơi
    private string currentLetters = "";                              // Chuỗi lưu các chữ đúng đã nhặt

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
        if (gameOverText != null) gameOverText.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (letterPopupPanel != null) letterPopupPanel.SetActive(false); // Tự động ẩn rương lúc đầu game

        UpdateHealthUI();
        UpdateScoreUI();
        UpdatePopupText(); // Cập nhật chữ ban đầu
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

    // ==========================================
    // LOGIC THU THẬP CHỮ CÁI
    // ==========================================
    public void CollectLetter(char newLetter)
    {
        if (IsLetterNeeded(newLetter))
        {
            currentLetters += newLetter;
            Debug.Log("==> RƯƠNG HIỆN TẠI ĐANG CHỨA: " + currentLetters);
            UpdatePopupText();
            CheckWinCondition();
        }
    }

    private bool IsLetterNeeded(char letter)
    {
        int countInTarget = CountOccurrences(targetWord, letter);
        int countInCurrent = CountOccurrences(currentLetters, letter);
        return countInTarget > countInCurrent;
    }

    private int CountOccurrences(string text, char letter)
    {
        int count = 0;
        foreach (char c in text) { if (c == letter) count++; }
        return count;
    }

    private void UpdatePopupText()
    {
        if (popupLettersText != null)
        {
            popupLettersText.text = "Từ cần ghép: <color=yellow>" + targetWord + "</color>\n\n" +
                                    "Đã nhặt được:\n<size=45><color=green>" + string.Join(" ", currentLetters.ToCharArray()) + "</color></size>";
        }
    }

    private void CheckWinCondition()
    {
        // ĐÃ KHỬ LỆNH DỪNG GAME TẠI ĐÂY - CHỈ CẢNH BÁO HOÀN THÀNH
        if (currentLetters.Length == targetWord.Length)
        {
            Debug.Log("==> ĐÃ NHẬT ĐỦ CHỮ! HÃY CHẠY ĐẾN CỬA MÀU ĐEN ĐỂ CHIẾN THẮNG!");
        }
    }

    public void TogglePopup(bool show)
    {
        if (letterPopupPanel != null)
        {
            // Bật hoặc tắt bảng Panel dựa vào giá trị show truyền vào
            letterPopupPanel.SetActive(show);

            // Nếu là lệnh bật bảng (show == true), thì đồng thời cập nhật chữ luôn
            if (show)
            {
                UpdatePopupText();
            }
        }
        else
        {
            Debug.LogError("LỖI: Chưa kéo thả LetterPopupPanel vào Script của Player ngoài Inspector!");
        }
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

        // --- CHỈNH SỬA PHẦN CHẠM CỬA ĐÍCH ---
        if (collision.CompareTag("Finish"))
        {
            // Kiểm tra xem người chơi đã gom đủ số lượng ký tự yêu cầu chưa
            if (currentLetters.Length == targetWord.Length)
            {
                Debug.Log("Player đã chạm đích và mang đủ chữ!");

                if (winPanel != null)
                {
                    winPanel.SetActive(true);
                }

                horizontalInput = 0f;
                rb.linearVelocity = Vector2.zero;
                Time.timeScale = 0f; // Dừng game tại đây
            }
            else
            {
                Debug.LogWarning("BẠN CHẠM CỬA ĐÍCH NHƯNG CHƯA GHÉP ĐỦ CHỮ '" + targetWord + "'!");
            }
        }

        // --- ĐOẠN XỬ LÝ VA CHẠM NHẶT CHỮ ---
        if (collision.CompareTag("Letter"))
        {
            Debug.Log("==> ĐÃ CHẠM VÀO VẬT THỂ CHỮ!");

            LetterItem letterItem = collision.GetComponent<LetterItem>();
            if (letterItem != null)
            {
                char pickedChar = letterItem.GetLetter();
                Debug.Log("==> ĐÃ ĐỌC ĐƯỢC CHỮ: " + pickedChar);

                CollectLetter(pickedChar);
                Destroy(collision.gameObject);
            }
            else
            {
                Debug.LogWarning("==> CHẠM CHỮ THÀNH CÔNG NHƯNG VẬT THỂ CHƯA GẮN SCRIPT LetterItem!");
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