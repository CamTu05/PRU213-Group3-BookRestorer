//chittp0807

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class PlayerHealthController : MonoBehaviour
{
    [Header("Health & UI Settings")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private Image healthBarFill; 
    [SerializeField] private float iframeDuration = 1.2f;
    [SerializeField] private GameObject gameOverText;

    [Header("Damage Settings (Chỉnh lượng máu trừ ở đây)")]
    [SerializeField] private int enemyDamage = 1; 
    [SerializeField] private int trapDamage = 2;  

    private int currentHealth;
    private float iframeTimer;
    private Rigidbody2D rb;
    private Animator anim;
    private PlayerMovement movement;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackDistance = 0.4f;
    private SpriteRenderer spriteRenderer;
    [Header("Knockback")]
    [SerializeField] private float knockbackSpeed = 3f;
    [SerializeField] private float knockbackDuration = 0.15f;
    private void Start()
    {
        anim = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb=GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        if (gameOverText != null) gameOverText.SetActive(false);

        UpdateHealthUI();
    }

    private void Update()
    {
        if (iframeTimer > 0) iframeTimer -= Time.deltaTime;
    }

    public void TakeDamage(int damage, float knockbackDir)
    {
        if (iframeTimer > 0 || currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthUI();

        iframeTimer = iframeDuration;

        if (movement != null)
        {
            movement.LockPlayer();

            movement.PushBack(
                knockbackDir,
                knockbackSpeed,
                knockbackDuration
            );
        }
        if (anim != null)
            anim.SetTrigger("isHurt");
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(HurtCoroutine());
    }
    

    private void UpdateHealthUI()
    {
        if (healthBarFill == null) return;
        healthBarFill.fillAmount = (float)currentHealth / maxHealth;
    }

    private void Die()
    {
        FindFirstObjectByType<MenuManager>()?.TriggerGameOver();

        if (movement != null)
            movement.FreezeOnDeath();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            float dir = transform.position.x < collision.transform.position.x ? -1f : 1f;

            TakeDamage(enemyDamage, dir);
        }
        else if (collision.gameObject.CompareTag("Trap"))
        {
            float dir = transform.localScale.x > 0 ? -1f : 1f;
            TakeDamage(trapDamage, dir);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            float dir = transform.position.x < collision.transform.position.x ? -1f : 1f;
            TakeDamage(enemyDamage, dir);
        }
        else if (collision.CompareTag("Trap"))
        {
            float dir = transform.localScale.x > 0 ? -1f : 1f;
            TakeDamage(trapDamage, dir);
        }
    }
    private IEnumerator HurtCoroutine()
    {
        float totalLockTime = 1.0f;
        float timer = 0f;

        while (timer < iframeDuration)
        {
            spriteRenderer.color = new Color(1f, 0f, 0f, 0.6f);
            yield return new WaitForSeconds(0.1f);

            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);

            timer += 0.2f;
            if (timer >= totalLockTime && movement != null && currentHealth > 0)
            {
                movement.UnlockPlayer();
            }
        }

        spriteRenderer.color = Color.white;

        if (movement != null && currentHealth > 0)
            movement.UnlockPlayer();
    }
}