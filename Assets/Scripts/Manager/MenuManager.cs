using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject startPanel;
    public GameObject pausePanel; // <--- Ô mới: Để kéo thả bảng Pause vào đây

    private static bool isRetrying = false;

    private void Start()
    {
        // Tự động ẩn bảng Pause khi mới vào game đề phòng bạn quên tắt trong Unity
        if (pausePanel != null) pausePanel.SetActive(false);

        if (startPanel != null)
        {
            if (isRetrying)
            {
                startPanel.SetActive(false);
                Time.timeScale = 1f;
                isRetrying = false;
            }
            else
            {
                startPanel.SetActive(true);
                Time.timeScale = 0f;
            }
        }
    }

    public void StartGame()
    {
        if (startPanel != null) startPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        isRetrying = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void PauseGame()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true); 
        }
        Time.timeScale = 0f; 
    }

    public void ResumeGame()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false); 
        }
        Time.timeScale = 1f; 
    }
}