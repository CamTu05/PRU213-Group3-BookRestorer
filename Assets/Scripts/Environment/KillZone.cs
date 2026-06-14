using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player đã rơi xuống vực!");

            Player playerScript = collision.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(1); 
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RespawnPlayer(collision.gameObject);
            }
        }
    }
}