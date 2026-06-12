using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool isActivated = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Có vật thể tên là " + collision.name + " vừa chạm vào Checkpoint!");
        if (collision.CompareTag("Player") && !isActivated)
        {
            isActivated = true;

            GameManager.Instance.UpdateCheckpoint(transform.position);
        }
    }
}