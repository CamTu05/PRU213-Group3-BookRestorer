using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject inventory ;
    [SerializeField] private GameObject healthBar ;
    [SerializeField] private GameObject coin ;
    [SerializeField] private GameObject popUpLetter;
    [SerializeField] private GameObject winPanel;

    [Header("Level Information")]
    [SerializeField] private BookData currentBook;
    [SerializeField] private int levelNumber;


    private bool isGameOver;

    public void PauseGame()
    {
        if (isGameOver) return;
        inventory?.SetActive(false);
        healthBar?.SetActive(false);
        coin?.SetActive(false);
        pausePanel?.SetActive(true);
        popUpLetter?.SetActive(false);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        healthBar?.SetActive(true);
        inventory?.SetActive(true);
        coin?.SetActive(true);

        popUpLetter?.SetActive(true);
        pausePanel?.SetActive(false);
        Time.timeScale = 1f;
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        inventory?.SetActive(false);
        healthBar?.SetActive(false);
        coin?.SetActive(false);
        pausePanel?.SetActive(false);
        gameOverPanel?.SetActive(true);
        popUpLetter?.SetActive(false);
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetCheckpoint();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void TriggerWin()
    {
        if (isGameOver) return;

        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.CompleteLevel(currentBook.bookId, levelNumber);
        }

        inventory?.SetActive(false);
        healthBar?.SetActive(false);
        coin?.SetActive(false);
        popUpLetter?.SetActive(false);
        pausePanel?.SetActive(false);
        winPanel?.SetActive(true);

        Time.timeScale = 0f;
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;

        if (currentBook == null)
        {
            Debug.LogError("MenuManager: Current Book chưa được config.");
            return;
        }

        LevelData nextLevel = GameProgressManager.Instance.GetNextLevel(currentBook);

        if (nextLevel == null || string.IsNullOrEmpty(nextLevel.sceneName))
        {
            Debug.LogError("MenuManager: Không tìm thấy level tiếp theo.");
            return;
        }

        SceneManager.LoadScene(nextLevel.sceneName);
    }

}