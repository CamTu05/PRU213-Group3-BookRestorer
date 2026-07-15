using UnityEngine;
using TMPro;
using UnityEngine.UI; // Dùng để xử lý LayoutRebuilder ép đồng bộ UI khay chứa
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class PlayerInventory : MonoBehaviour

{
    [Header("Hint Content")]

    [TextArea(2, 4)]
    [SerializeField] private string hint1;

    [TextArea(2, 4)]
    [SerializeField] private string hint2;

    [TextArea(2, 4)]
    [SerializeField] private string hint3;

    private int currentHint = 0;
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private int hintCost = 5;
    [Header("Coin & UI Settings")]
    [SerializeField] private AudioClip coinSound;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject missingLetterPanel;

    [Header("Letter Drag & Drop System")]
    [SerializeField] private GameObject letterPopupPanel;         // Bảng gỗ lớn chứa UI
    [SerializeField] private string targetWord = "ANIMAL";         // Từ khóa cần hoàn thành
    [SerializeField] private GameObject letterUIPrefab;           // File Prefab quân chữ (màu xanh dưới Project)
    [SerializeField] private Transform lettersContainer;
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject dropSlotPrefab;// Hàng 1: Nơi chứa các chữ nhặt được lộn xộn (UI_Letters_Row)
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject submitButton;
    [SerializeField] private Button okButton;// Text hiển thị kết quả (Wrong! / Correct!) bên dưới nút Submit

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

        if (missingLetterPanel != null)
            missingLetterPanel.SetActive(false);

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

                if (canSolveWord)
                {
                    slotsContainer.gameObject.SetActive(true);

                    if (submitButton != null)
                        submitButton.SetActive(true);

                    CreateSlots();
                }
                else
                {
                    slotsContainer.gameObject.SetActive(false);

                    if (submitButton != null)
                        submitButton.SetActive(false);
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
        List<DropSlot> activeSlots = new List<DropSlot>();

        foreach (Transform child in slotsContainer)
        {
            DropSlot slot = child.GetComponent<DropSlot>();

            if (slot != null)
            {
                activeSlots.Add(slot);
            }
        }

        DropSlot[] slots = activeSlots.ToArray();

 

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
                statusText.text = "<color=white>Wrong!</color>";
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
                AudioManager.Instance.PlaySFX(coinSound);
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
            if (!HasCollectedAllLetters())
            {
                if (missingLetterPanel != null)
                {
                    missingLetterPanel.SetActive(true);
                }

                return;
            }

            // Đã nhặt đủ chữ
            Debug.Log("Đã nhặt đủ chữ, mở bảng ghép.");
            Debug.Log("Nhân vật đã chạm vào LevelExit_Portal! Tự động mở bảng ghép chữ.");
            canSolveWord = true;
            if (playerMovement != null)
                playerMovement.LockPlayer();
         
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
    public void OnHintButtonClicked()
    {
        hintPanel.SetActive(true);

        hintText.text = "Using a hint costs " + hintCost + " Coin.\n\nDo you want to continue?";

        okButton.interactable = true;
    }
    public void OnHintOKClicked()
    {
        if (score < hintCost)
        {
            hintText.text = "Not enough coins!";
            return;
        }

        if (currentHint >= 3)
        {
            hintText.text = "You have used all available hints!";
            return;
        }

        score -= hintCost;
        UpdateScoreUI();

        currentHint++;

        switch (currentHint)
        {
            case 1:
                hintText.text = hint1;
                break;

            case 2:
                hintText.text = hint2;
                break;

            case 3:
                hintText.text = hint3;
                break;
        }
        okButton.interactable = false;
    }
    public void CloseHintPanel()
    {
        hintPanel.SetActive(false);
    }
    private void CreateSlots()
    {
        if (slotsContainer == null || dropSlotPrefab == null)
        {
            Debug.LogError("Chưa gán SlotsContainer hoặc DropSlotPrefab!");
            return;
        }

        // Xóa Slot cũ
        foreach (Transform child in slotsContainer)
        {
            Destroy(child.gameObject);
        }

        // Tạo Slot mới
        for (int i = 0; i < targetWord.Length; i++)
        {
            GameObject slot = Instantiate(dropSlotPrefab, slotsContainer);

            RectTransform rect = slot.GetComponent<RectTransform>();

            if (rect != null)
            {
                rect.localScale = Vector3.one;
                rect.localPosition = Vector3.zero;
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            slotsContainer.GetComponent<RectTransform>());
    }
    public void CloseMissingLetterPanel()
    {
        if (missingLetterPanel != null)
        {
            missingLetterPanel.SetActive(false);
        }
    }

}