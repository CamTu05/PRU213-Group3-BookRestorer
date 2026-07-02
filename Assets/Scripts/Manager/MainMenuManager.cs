/* Author: TuLC
 * Date: 27/6/26
 * Description: This script manages the game progress, including tracking completed levels and unlocked levels for each book.
 */

using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Books")]
    [SerializeField] private BookData[] books;

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject libraryPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject instructionPanel;

    [Header("Library")]
    [SerializeField] private Transform bookContent;
    [SerializeField] private BookItemUI bookItemPrefab;

    [Header("Levels")]
    [SerializeField] private Transform levelContent;
    [SerializeField] private LevelItemUI levelItemPrefab;

    [Header("Reset")]
    [SerializeField] private GameObject resetConfirmPopup;
    [SerializeField] private GameObject successMessageText;

    // Khởi tạo danh sách Book trong Library.
    private void Start()
    {
        GenerateBooks();
    }

    // Load level cần chơi tiếp khi bấm Play.
    public void Play()
    {
        LevelData nextLevel = GameProgressManager.Instance.GetContinueLevel(books);

        if (nextLevel == null || string.IsNullOrEmpty(nextLevel.sceneName))
        {
            Debug.LogError("MainMenuManager: Không tìm thấy level tiếp theo hoặc SceneName bị trống.");
            return;
        }

        SceneManager.LoadScene(nextLevel.sceneName);
    }

    // Mở popup Library.
    public void OpenLibrary()
    {
        Debug.Log("Library button clicked");
        mainPanel.SetActive(false);
        libraryPanel.SetActive(true);
    }

    // Đóng popup Library.
    public void CloseLibrary()
    {
        libraryPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    // Tạo BookItem cho tất cả BookData.
    private void GenerateBooks()
    {
        foreach (BookData book in books)
        {
            BookItemUI item =
                Instantiate(bookItemPrefab, bookContent);

            item.Initialize(book, this);
        }
    }

    // Hiển thị danh sách level của Book được chọn.
    public void ShowBookDetail(BookData book)
    {
        Debug.Log("Selected book: " + book.bookName);
        Debug.Log("Level count: " + book.levels.Count);

        foreach (Transform child in levelContent)
        {
            Destroy(child.gameObject);
        }

        foreach (LevelData level in book.levels)
        {
            LevelItemUI item = Instantiate(levelItemPrefab, levelContent);

            LevelStatus status = GameProgressManager.Instance.GetLevelStatus(
                book.bookId,
                level.levelNumber
            );

            item.Initialize(level, status);
        }
    }

    // Mở popup Settings.
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    // Đóng popup Settings.
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    // Mở popup hướng dẫn chơi.
    public void OpenInstruction()
    {
        instructionPanel.SetActive(true);
    }

    // Đóng popup hướng dẫn chơi.
    public void CloseInstruction()
    {
        instructionPanel.SetActive(false);
    }

    // Thoát game khi build, dừng Play Mode khi chạy trong Editor.
    public void ExitGame()
    {
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Mở popup xác nhận reset progress.
    public void OpenResetConfirm()
    {
        resetConfirmPopup.SetActive(true);
    }

    // Đóng popup xác nhận reset progress.
    public void CloseResetConfirm()
    {
        resetConfirmPopup.SetActive(false);
    }

    // Xác nhận xóa toàn bộ tiến trình game.
    public void ConfirmResetProgress()
    {
        GameProgressManager.Instance.ResetProgress();

        resetConfirmPopup.SetActive(false);

        successMessageText.SetActive(true);

        Debug.Log("Reset progress successfully!");
    }
}