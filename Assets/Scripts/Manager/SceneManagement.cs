using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    public GameObject popupPanel;

    public void GoHome()
    {
        
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void ResumeGame()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false); 
        }

        Time.timeScale = 1f;
    }

    public void RetryGame()
    {   Time.timeScale = 1f;
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

}
