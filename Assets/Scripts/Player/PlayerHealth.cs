//chittp0807


using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health & UI Settings")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private Image healthBarFill; 
    [SerializeField] private float iframeDuration = 1.2f;
    [SerializeField] private GameObject gameOverPanel;

  

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
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        UpdateHealthUI();
    }

    private void Update()
    {
        if (iframeTimer > 0) iframeTimer -= Time.deltaTime;
    }

    public void TakeDamage(int damage, float knockbackDir)
    {
        Debug.Log($"Frame={Time.frameCount} | Damage={damage} | HP trước={currentHealth}");

        if (iframeTimer > 0 || currentHealth <= 0) return;
        currentHealth -= damage;
        Debug.Log($"Frame={Time.frameCount} | HP sau={currentHealth}");

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

    public void TakeDamage(int damage)
    {
        float dir = transform.localScale.x > 0 ? -1f : 1f;

        TakeDamage(damage, dir);
    }
    public void TakeDamage(int damage, Transform attacker)
    {
        float dir = transform.position.x < attacker.position.x ? -1f : 1f;

        TakeDamage(damage, dir);
    }

    private void UpdateHealthUI()
    {
        if (healthBarFill == null) return;
        healthBarFill.fillAmount = (float)currentHealth / maxHealth;
    }

    private void Die()
    {
        FindAnyObjectByType<MenuManager>()?.TriggerGameOver();

        if (movement != null)
            movement.FreezeOnDeath();
    }

    private IEnumerator HurtCoroutine()
    {
        float blinkInterval = 0.1f;
        float timer = 0f;

        while (timer < iframeDuration)
        {
            spriteRenderer.color = new Color(1f, 0f, 0f, 0.6f);
            yield return new WaitForSeconds(blinkInterval);

            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(blinkInterval);

            timer += blinkInterval * 2;
        }
           

        spriteRenderer.color = Color.white;

        if (movement != null && currentHealth > 0)
            movement.UnlockPlayer();
    }
}