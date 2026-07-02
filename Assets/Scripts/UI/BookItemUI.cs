/* Author: TuLC
 * Date: 28/6/26
 * Description: This script controls one Book item in the Library UI.
 */

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookItemUI : MonoBehaviour
{
    [SerializeField] private Image coverImage;
    [SerializeField] private TMP_Text bookNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button button;

    private BookData bookData;
    private MainMenuManager mainMenuManager;

    // Khởi tạo dữ liệu hiển thị cho BookItem.
    public void Initialize(BookData data, MainMenuManager manager)
    {
        bookData = data;
        mainMenuManager = manager;

        if (coverImage != null)
        {
            coverImage.sprite = bookData.coverImage;
        }

        if (bookNameText != null)
        {
            bookNameText.text = bookData.bookName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = bookData.description;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickBook);
        }
    }

    // Gọi MainMenuManager để hiển thị chi tiết Book được chọn.
    private void OnClickBook()
    {
        if (mainMenuManager != null && bookData != null)
        {
            mainMenuManager.ShowBookDetail(bookData);
        }
    }
}