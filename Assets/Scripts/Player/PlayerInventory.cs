using UnityEngine;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class PlayerInventory : MonoBehaviour
{
    [Header("Coin & UI Settings")]
    [SerializeField] private AudioClip coinSound;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject winPanel;

    private AudioSource audioSource;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private int score = 0;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        playerHealth = GetComponent<PlayerHealth>();
        playerMovement = GetComponent<PlayerMovement>();

        if (winPanel != null) winPanel.SetActive(false);
        UpdateScoreUI();
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
            if (playerHealth != null) playerHealth.TakeDamage(1);
        }

        if (collision.CompareTag("Finish"))
        {
            Debug.Log("Player đã chạm đích!");

            if (winPanel != null)
            {
                winPanel.SetActive(true);
            }

            if (playerMovement != null) playerMovement.FreezeOnWin();

            Time.timeScale = 0f;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trap"))
        {
            if (playerHealth != null) playerHealth.TakeDamage(1);
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