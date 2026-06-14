//Author: Lê Cẩm Tú
//Date: 07/06/2026
//Description: Gắn vào vực sâu để xử lý khi người chơi rơi vào đó. Khi người chơi chạm vào KillZone, sẽ mất một mạng và được hồi sinh tại điểm hồi sinh

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