using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class PlayerHealthController : MonoBehaviour
{
    [Header("Health & UI Settings")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private Image healthBarFill; // Kéo ảnh màu thanh máu vào đây
    [SerializeField] private float iframeDuration = 1.2f;
    [SerializeField] private GameObject gameOverText;

    [Header("Damage Settings (Chỉnh lượng máu trừ ở đây)")]
    [SerializeField] private int enemyDamage = 1; // Quái cắn trừ mấy máu
    [SerializeField] private int trapDamage = 2;  // Dẫm bẫy trừ mấy máu

    private int currentHealth;
    private float iframeTimer;
    private Animator anim;
    private PlayerMovement movement;

    private void Start()
    {
        anim = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();

        currentHealth = maxHealth;
        if (gameOverText != null) gameOverText.SetActive(false);

        UpdateHealthUI();
    }

    private void Update()
    {
        if (iframeTimer > 0) iframeTimer -= Time.deltaTime;
    }

    public void TakeDamage(int damage)
    {
        if (iframeTimer > 0 || currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthUI();

        iframeTimer = iframeDuration;

        if (anim != null) anim.SetTrigger("isHurt");

        if (currentHealth <= 0) Die();
    }

    private void UpdateHealthUI()
    {
        if (healthBarFill == null) return;
        healthBarFill.fillAmount = (float)currentHealth / maxHealth;
    }

    private void Die()
    {
        if (gameOverText != null) gameOverText.SetActive(true);
        if (movement != null) movement.FreezeOnDeath();
    }

    // TỰ ĐỘNG TRỪ MÁU THEO TAG KHI VA CHẠM KHỐI CỨNG (Collision)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy")) TakeDamage(enemyDamage);
        else if (collision.gameObject.CompareTag("Trap")) TakeDamage(trapDamage);
    }

    // TỰ ĐỘNG TRỪ MÁU THEO TAG KHI ĐI XUYÊN QUA (Trigger)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy")) TakeDamage(enemyDamage);
        else if (collision.CompareTag("Trap")) TakeDamage(trapDamage);
    }
}