//Author: Nguyễn Văn Đức
//Date: 10/06/2026
//Description: Quản lý menu chính, bảng pause và chức năng restart game

using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    //chittp
    [SerializeField] private PlayerMovement pm;
    //end
    [Header("UI Panels")]
    public GameObject startPanel;
    public GameObject pausePanel;
    //c-0907
    public GameObject gameOverPanel;
    [Header("UI Elements")]
    public GameObject pauseButton;
    //end

    private static bool isRetrying = false;
    //chittp-0907
    private bool isGameOver = false;
    //end

    private void Start()
    {
        //chittp-0907
        isGameOver = false;
        //end
        if (pausePanel != null) pausePanel.SetActive(false);
        //chittp
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(true);
        //end
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
        Time.timeScale = 0f; 
        //chittp-0907
        if (isGameOver) return;
        //end
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
       
    }

    public void ResumeGame()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false); 
        }
        Time.timeScale = 1f; 
    }
    //chittp-0907
    public void TriggerGameOver()
    {
        isGameOver = true;
        if (pm != null) pm.FreezeOnDeath();
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }
    //end
}
