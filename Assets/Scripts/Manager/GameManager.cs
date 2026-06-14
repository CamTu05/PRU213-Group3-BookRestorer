//Author: Lê Cẩm Tú
//Date: 07/06/2026
//Description: Quản lý điểm hồi sinh của người chơi. Khi người chơi chạm vào Checkpoint, sẽ cập nhật điểm hồi sinh mới. Khi người chơi rơi vào KillZone, sẽ được hồi sinh tại điểm hồi sinh hiện tại

using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Home_Screen");
    }
}