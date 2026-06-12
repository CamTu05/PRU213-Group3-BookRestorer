using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private Vector2 respawnPosition;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            transform.SetParent(null);

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            respawnPosition = player.transform.position;
        }
    }

    public void UpdateCheckpoint(Vector2 newPosition)
    {
        respawnPosition = newPosition;
        Debug.Log("Đã cập nhật Checkpoint mới tại: " + newPosition);
    }

    public void RespawnPlayer(GameObject player)
    {
        player.transform.position = respawnPosition;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        Debug.Log("Player đã hồi sinh về vị trí an toàn!");
    }
}