using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player đã rơi xuống vực!");

            GameManager.Instance.RespawnPlayer(collision.gameObject);
        }
    }
}