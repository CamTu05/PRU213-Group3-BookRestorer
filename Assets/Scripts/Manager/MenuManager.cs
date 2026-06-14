//Author: Nguyễn Văn Đức
//Date: 10/06/2026
//Description: Quản lý menu chính, bảng pause và chức năng restart game

using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject startPanel;
    public GameObject pausePanel; 

    private static bool isRetrying = false;

    private void Start()
    {
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