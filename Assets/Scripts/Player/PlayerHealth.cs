using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health & Trap Settings")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private Transform healthContainer;
    [SerializeField] private float iframeDuration = 1.2f;
    [SerializeField] private GameObject gameOverText;

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
        if (iframeTimer > 0)
        {
            iframeTimer -= Time.deltaTime;
        }
    }

    public void TakeDamage(int damage)
    {
        if (iframeTimer > 0 || currentHealth <= 0) return;

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

        if (movement != null)
        {
            movement.FreezeOnDeath();
        }
    }
}
