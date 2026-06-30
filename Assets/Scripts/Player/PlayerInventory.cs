using UnityEngine;
using TMPro;
using UnityEngine.UI; // Dùng để xử lý LayoutRebuilder ép đồng bộ UI khay chứa
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class PlayerInventory : MonoBehaviour
{
    [Header("Coin & UI Settings")]
    [SerializeField] private AudioClip coinSound;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject winPanel;

    [Header("Letter Drag & Drop System")]
    [SerializeField] private GameObject letterPopupPanel;         // Bảng gỗ lớn chứa UI
    [SerializeField] private string targetWord = "ANIMAL";         // Từ khóa cần hoàn thành
    [SerializeField] private GameObject letterUIPrefab;           // File Prefab quân chữ (màu xanh dưới Project)
    [SerializeField] private Transform lettersContainer;          // Hàng 1: Nơi chứa các chữ nhặt được lộn xộn (UI_Letters_Row)
    [SerializeField] private TextMeshProUGUI statusText;           // Text hiển thị kết quả (Wrong! / Correct!) bên dưới nút Submit

    [Header("Sprite Font Settings")]
    [SerializeField] private LetterSpriteData letterSpriteData; // Kho lưu trữ ảnh chữ cái ScriptableObject

    // Danh sách lưu ký tự thực tế nhặt được dưới đất
    private List<char> collectedLettersList = new List<char>();

    // Các biến phục vụ hiệu ứng đóng mở mượt mà
    private bool isAnimatingPopup = false;
    private Vector3 targetPopupScale = Vector3.one;

    private AudioSource audioSource;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private int score = 0;
    private bool canSolveWord = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        playerHealth = GetComponent<PlayerHealth>();
        playerMovement = GetComponent<PlayerMovement>();

        if (winPanel != null) winPanel.SetActive(false);
        if (letterPopupPanel != null) letterPopupPanel.SetActive(false);

        UpdateScoreUI();
    }

    // ========================================================
    // HÀM ĐÓNG MỞ BẢNG GỖ (ĐÃ FIX LỖI TRÀN CHỮ KHI MỞ LẠI)
    // ========================================================
    public void TogglePopup(bool show)
    {
        if (letterPopupPanel != null)
        {
            if (show)
            {
                letterPopupPanel.SetActive(true);

                // 1. Reset dòng chữ thông báo kết quả về trống khi mở bảng
                if (statusText != null) statusText.text = "";


                // 2. [SỬA LỖI QUAN TRỌNG]: Tìm và xóa sạch tất cả các quân chữ cũ còn sót lại trong các ô vuông gỗ (SlotsContainer)
                Transform boardTransform = lettersContainer.parent;
                Transform slotsParent = boardTransform.Find("SlotsContainer");
                if (slotsParent != null)
                {
                    // Duyệt qua từng ô vuông gỗ con
                    foreach (Transform slot in slotsParent)
                    {
                        // Nếu trong ô vuông gỗ đó có chứa quân chữ cái cũ, xóa nó ngay lập tức
                        foreach (Transform child in slot)
                        {
                            Destroy(child.gameObject);
                        }
                    }
                }

                // 3. HÀNG 1: Xóa sạch các quân chữ UI cũ trên khay chứa lộn xộn để làm mới dữ liệu
                if (lettersContainer != null)
                {
                    foreach (Transform child in lettersContainer)
                    {
                        Destroy(child.gameObject);
                    }

                    // Tự động sinh lại các ô vuông chứa chữ cái bằng ảnh Pixel Art màu vàng vào Hàng 1
                    foreach (char letter in collectedLettersList)
                    {
                        // Đúc quân chữ mẫu nhét thẳng vào khay chứa (Hàng 1)
                        GameObject newLetterUI = Instantiate(letterUIPrefab, lettersContainer);

                        // ÉP TỌA ĐỘ VÀ TỶ LỆ KÍCH THƯỚC VỀ CHUẨN
                        RectTransform rect = newLetterUI.GetComponent<RectTransform>();
                        if (rect != null)
                        {
                            rect.localPosition = Vector3.zero;
                            rect.localScale = Vector3.one;
                        }

                        // Gọi Helper đổi sang đúng chữ cái nhặt được dưới đất
                        LetterUIHelper uiHelper = newLetterUI.GetComponent<LetterUIHelper>();
                        if (uiHelper != null)
                        {
                            uiHelper.SetLetter(letter, letterSpriteData);
                        }
                        DragItem drag = newLetterUI.GetComponent<DragItem>();

                        if (drag != null)
                        {
                            drag.enabled = canSolveWord;
                        }
                    }

                    // Ép giao diện Unity tải và tính toán lại kích thước hiển thị ngay tức thì
                    Canvas.ForceUpdateCanvases();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(lettersContainer as RectTransform);
                }

                // Hiệu ứng mở bảng gỗ to dần lên
                letterPopupPanel.transform.localScale = Vector3.zero;
                targetPopupScale = Vector3.one;
                isAnimatingPopup = true;
            }
            else
            {
                targetPopupScale = Vector3.zero;
                isAnimatingPopup = true;
            }
        }
        else
        {
            Debug.LogError("LỖI: Chưa kéo LetterPopupPanel vào Script ngoài Inspector!");
        }
    }

    private void LateUpdate()
    {
        if (isAnimatingPopup && letterPopupPanel != null)
        {
            letterPopupPanel.transform.localScale = Vector3.Lerp(letterPopupPanel.transform.localScale, targetPopupScale, Time.unscaledDeltaTime * 12f);

            if (targetPopupScale == Vector3.zero && letterPopupPanel.transform.localScale.magnitude < 0.05f)
            {
                letterPopupPanel.transform.localScale = Vector3.zero;
                letterPopupPanel.SetActive(false);
                isAnimatingPopup = false;
            }
            else if (targetPopupScale == Vector3.one && Vector3.Distance(letterPopupPanel.transform.localScale, Vector3.one) < 0.01f)
            {
                letterPopupPanel.transform.localScale = Vector3.one;
                isAnimatingPopup = false;
            }
        }
    }

    // ========================================================
    // LOGIC THU THẬP CHỮ CÁI
    // ========================================================
    public void CollectLetter(char newLetter)
    {
        collectedLettersList.Add(newLetter);
        Debug.Log("==> Đã nhặt chữ: " + newLetter);
    }

    public bool HasCollectedAllLetters()
    {
        return collectedLettersList.Count >= targetWord.Length;
    }

    // ========================================================
    // KIỂM TRA ĐIỀU KIỆN KHI BẤM NÚT SUBMIT (ĐÃ SỬA LỖI QUÉT NHẦM)
    // ========================================================
    public void OnSubmitButtonClicked()
    {
        // 1. Tìm chính xác mục SlotsContainer chứa 6 ô vuông kết quả dựa trên vị trí của khay chứa cha
        Transform boardTransform = lettersContainer.parent;
        Transform slotsParent = boardTransform.Find("SlotsContainer");
        if (!canSolveWord)
        {
            if (statusText != null)
            {
                statusText.text = "<color=yellow>Hãy đến cổng cuối màn để ghép chữ!</color>";
            }

            return;
        }

        if (slotsParent == null)
        {
            Debug.LogError("LỖI: Không tìm thấy đối tượng 'SlotsContainer' trên bảng gỗ!");
            return;
        }

        // 2. Chỉ tìm các thành phần DropSlot nằm bên trong mục SlotsContainer này
        DropSlot[] slots = slotsParent.GetComponentsInChildren<DropSlot>();

        if (slots.Length == 0)
        {
            Debug.LogError("LỖI: Không tìm thấy linh kiện DropSlot nào trong SlotsContainer!");
            return;
        }

        // 3. Sắp xếp các ô trống theo thứ tự từ trái qua phải trong Hierarchy
        System.Array.Sort(slots, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

        string playerWord = "";

        // 4. Duyệt qua từng ô để kiểm tra chữ cái người chơi đã thả vào
        foreach (DropSlot slot in slots)
        {
            string letter = slot.GetLetter();

            if (string.IsNullOrEmpty(letter))
            {
                if (statusText != null)
                    statusText.text = "<color=orange>Hãy xếp đủ các chữ cái!</color>";

                return;
            }

            playerWord += letter;
        }

        Debug.Log("Từ người chơi xếp được: " + playerWord);

        // 5. So sánh kết quả với từ khóa mục tiêu (Ví dụ: ANIMAL)
        if (playerWord.ToUpper() == targetWord.ToUpper())
        {
            if (statusText != null)
                statusText.text = "<color=green>Correct!</color>";

            Invoke(nameof(ShowWinPanel), 0.5f);
        }
        else
        {
            if (statusText != null)
                statusText.text = "<color=red>Wrong!</color>";
        }
    }

    private void ShowWinPanel()
    {
        if (winPanel != null) winPanel.SetActive(true);
        if (playerMovement != null) playerMovement.FreezeOnWin();
        Time.timeScale = 0f; // Dừng game chiến thắng
        if (letterPopupPanel != null)
        {
            // Cách 1: Tắt trực tiếp Object (Biến mất ngay tức thì)
            letterPopupPanel.SetActive(false);

            // Cách 2: Nếu ông muốn nó dùng hiệu ứng thu nhỏ mượt mà của LateUpdate, hãy dùng 2 dòng dưới thay thế:
            // targetPopupScale = Vector3.zero;
            // isAnimatingPopup = true;
        }
    }

    // ========================================================
    // XỬ LÝ VA CHẠM TRIGGER
    // ========================================================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
            if (audioSource != null && coinSound != null)
            {
                audioSource.PlayOneShot(coinSound);
            }
            score++;
            UpdateScoreUI();
            Destroy(collision.gameObject);
        }

        if (collision.CompareTag("Trap"))
        {
            if (playerHealth != null) playerHealth.TakeDamage(1);
        }

        if (collision.CompareTag("Letter"))
        {
            LetterItem letterItem = collision.GetComponent<LetterItem>();
            if (letterItem != null)
            {
                CollectLetter(letterItem.GetLetter());
                Destroy(collision.gameObject);
            }
        }

        if (collision.CompareTag("Finish"))
        {
            Debug.Log("Nhân vật đã chạm vào LevelExit_Portal! Tự động mở bảng ghép chữ.");
            canSolveWord = true;
            foreach (DragItem drag in FindObjectsOfType<DragItem>())
    {
        drag.enabled = true;
    }


            // Gọi hàm mở bảng gỗ lên ngay khi chạm cửa
            TogglePopup(true);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trap"))
        {
            if (playerHealth != null) playerHealth.TakeDamage(1);
        }
    }
    public bool CanSolveWord()
    {
        return canSolveWord;
    }
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Coins: " + score;
        }
    }
}