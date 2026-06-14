//Author: Lê Cẩm Tú
//Date: 07/06/2026
//Description: Định nghĩa Checkpoint để lưu lại vị trí của người chơi khi chạm vào nó, giúp người chơi có thể hồi sinh tại điểm này nếu bị chết

using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool isActivated = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.name + " vừa chạm vào Checkpoint!");
        if (collision.CompareTag("Player") && !isActivated)
        {
            isActivated = true;

            GameManager.Instance.UpdateCheckpoint(transform.position);
        }
    }
}