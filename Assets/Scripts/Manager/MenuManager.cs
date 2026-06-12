using UnityEngine;
using UnityEngine.SceneManagement; 

public class MenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject startPanel; 

    private void Start()
    {
        
        if (startPanel != null)
        {
            startPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void StartGame()
    {
        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }
        Time.timeScale = 1f; 
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}