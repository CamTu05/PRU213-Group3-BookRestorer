/* Author: TuLC
 * Date: 27/6/26
 * Description: This script manages the game progress, including tracking completed levels and unlocked levels for each book.
 */

using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance;

    private const string LAST_COMPLETED_BOOK_KEY = "LastCompletedBookId";

    // Khởi tạo Singleton và giữ object khi chuyển Scene.
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Tạo key lưu level đã mở cao nhất của từng Book.
    private string GetUnlockedLevelKey(string bookId)
    {
        return $"Book_{bookId}_UnlockedLevel";
    }

    // Tạo key lưu trạng thái hoàn thành của từng level.
    private string GetCompletedLevelKey(string bookId, int levelNumber)
    {
        return $"Book_{bookId}_Level_{levelNumber}_Completed";
    }

    // Lấy level cao nhất đã được mở của Book.
    public int GetUnlockedLevel(string bookId)
    {
        return PlayerPrefs.GetInt(GetUnlockedLevelKey(bookId), 1);
    }

    // Kiểm tra level hiện tại đã được mở hay chưa.
    public bool IsLevelUnlocked(string bookId, int levelNumber)
    {
        return levelNumber <= GetUnlockedLevel(bookId);
    }

    // Kiểm tra level hiện tại đã hoàn thành hay chưa.
    public bool IsLevelCompleted(string bookId, int levelNumber)
    {
        return PlayerPrefs.GetInt(GetCompletedLevelKey(bookId, levelNumber), 0) == 1;
    }

    // Trả về trạng thái hiện tại của level.
    public LevelStatus GetLevelStatus(string bookId, int levelNumber)
    {
        if (IsLevelCompleted(bookId, levelNumber))
        {
            return LevelStatus.Completed;
        }

        if (IsLevelUnlocked(bookId, levelNumber))
        {
            return LevelStatus.Unlocked;
        }

        return LevelStatus.Locked;
    }

    // Cập nhật tiến trình khi người chơi hoàn thành một level.
    public void CompleteLevel(string bookId, int completedLevelNumber)
    {
        int currentUnlocked = GetUnlockedLevel(bookId);

        // Đánh dấu level hiện tại là đã hoàn thành.
        PlayerPrefs.SetInt(GetCompletedLevelKey(bookId, completedLevelNumber), 1); 

        // Chỉ mở khóa level tiếp theo nếu đây là level mới nhất.
        if (completedLevelNumber >= currentUnlocked)
        {
            PlayerPrefs.SetInt(GetUnlockedLevelKey(bookId), completedLevelNumber + 1);
        }

        // Ghi nhớ Book vừa hoàn thành để nút Play tự vào đúng Book.
        PlayerPrefs.SetString(LAST_COMPLETED_BOOK_KEY, bookId);

        PlayerPrefs.Save();
    }

    // Lấy Book mà người chơi hoàn thành gần nhất.
    public string GetLastCompletedBookId()
    {
        return PlayerPrefs.GetString(LAST_COMPLETED_BOOK_KEY, "");
    }

    // Trả về level tiếp theo cần chơi của Book.
    public LevelData GetNextLevel(BookData book)
    {
        if (book == null || book.levels == null || book.levels.Count == 0)
        {
            Debug.LogError("GameProgressManager: Book hoặc Levels chưa được config.");
            return null;
        }

        int unlockedLevel = GetUnlockedLevel(book.bookId);

        unlockedLevel = Mathf.Clamp(unlockedLevel, 1, book.levels.Count);

        return book.levels[unlockedLevel - 1];
    }

    public LevelData GetContinueLevel(BookData[] books)
    {
        if (books == null || books.Length == 0)
        {
            Debug.LogError("GameProgressManager: Books chưa được config.");
            return null;
        }

        string lastBookId = GetLastCompletedBookId();

        BookData targetBook = books[0];

        // Nếu đã có Book hoàn thành gần nhất thì tìm đúng Book đó.
        if (!string.IsNullOrEmpty(lastBookId))
        {
            foreach (BookData book in books)
            {
                if (book.bookId == lastBookId)
                {
                    targetBook = book;
                    break;
                }
            }
        }

        return GetNextLevel(targetBook);
    }

    // Xóa toàn bộ tiến trình game.
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}