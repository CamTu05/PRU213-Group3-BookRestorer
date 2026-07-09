using UnityEngine;
using TMPro;
using UnityEngine.UI; // Dùng để xử lý LayoutRebuilder ép đồng bộ UI khay chứa
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private int hintCost = 5;
    [Header("Hint Content")]

    [TextArea(2, 4)]
    [SerializeField] private string hint1;

    [TextArea(2, 4)]
    [SerializeField] private string hint2;

    [TextArea(2, 4)]
    [SerializeField] private string hint3;
    [Header("Coin & UI Settings")]
    [SerializeField] private AudioClip coinSound;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject winPanel;

    [Header("Letter Drag & Drop System")]
    [SerializeField] private GameObject letterPopupPanel;           // Bảng gỗ lớn chứa UI
    [SerializeField] private string targetWord = "ANIMAL";          // Từ khóa cần hoàn thành
    [SerializeField] private GameObject letterUIPrefab;           // File Prefab quân chữ (màu xanh dưới Project)
    [SerializeField] private Transform lettersContainer;
    [SerializeField] private Transform slotsContainer;
    [SerializeField] private GameObject dropSlotPrefab;          // Hàng 1: Nơi chứa các chữ nhặt được lộn xộn (UI_Letters_Row)
    [SerializeField] private Button submitButton;                // 👉 THÊM BIẾN NÀY ĐỂ ẨN HIỆN NÚT SUBMIT
    [SerializeField] private TextMeshProUGUI statusText;        // Text hiển thị kết quả (Wrong! / Correct!) bên dưới nút Submit

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
    private int currentHint = 0;

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
    // HÀM ĐÓNG MỞ BẢNG GỖ (TỐI ƯU HÓA GỌI LOCK/UNLOCK CHO PLAYER)
    // ========================================================
    public void TogglePopup(bool show)
    {
        if (letterPopupPanel != null)
        {
            if (show)
            {
                letterPopupPanel.SetActive(true);

                // Nếu canSolveWord = false (Dọc đường): Ẩn hoàn toàn ô trống kết quả và nút Submit
                // Nếu canSolveWord = true (Tại Finish): Bật hiện lên đầy đủ để xếp chữ giải đố
                if (slotsContainer != null)
                {
                    slotsContainer.gameObject.SetActive(canSolveWord);
                }

                if (submitButton != null)
                {
                    submitButton.gameObject.SetActive(canSolveWord);
                }

                // Chỉ khởi tạo sinh các ô trống kết quả hàng trên khi đã đứng ở cổng Finish
                if (canSolveWord)
                {
                    CreateSlots();
                }

                // Reset dòng chữ thông báo kết quả về trống khi mở bảng
                if (statusText != null) statusText.text = "";

                // Hiện dòng nhắc nhở nhỏ nếu người chơi tự mở bảng khi đang đi dọc đường
                if (!canSolveWord && statusText != null)
                {
                    statusText.text = "<color=yellow>Reach the final portal to solve!</color>";
                }

                // 🛑 GỌI HÀM KHÓA NHÂN VẬT GỌN GÀNG TỪ PLAYERMOVEMENT
                if (playerMovement != null)
                {
                    playerMovement.LockPlayer();
                }

                // Xóa sạch các quân chữ UI cũ trên khay chứa lộn xộn để làm mới dữ liệu
                if (lettersContainer != null)
                {
                    List<GameObject> childrenToDestroy = new List<GameObject>();
                    foreach (Transform child in lettersContainer)
                    {
                        childrenToDestroy.Add(child.gameObject);
                    }

                    // Ngắt kết nối ngay lập tức khỏi Layout
                    lettersContainer.DetachChildren();

                    foreach (GameObject child in childrenToDestroy)
                    {
                        if (child != null)
                        {
                            // Tắt hoàn toàn tương tác để EventSystem/Raycast của Unity UI buông tha cho linh kiện Image
                            child.SetActive(false);

                            // Đổi tên để Unity không xếp nó vào hàng đợi sự kiện UI nữa
                            child.name = "Destroyed_Letter";

                            // Xóa hoãn lại một nhịp rất nhỏ để Unity UI kịp cập nhật trạng thái trống
                            Destroy(child, 0.01f);
                        }
                    }

                    // Tự động sinh lại các ô vuông chứa chữ cái bằng ảnh Pixel Art màu vàng vào Hàng 1
                    foreach (char letter in collectedLettersList)
                    {
                        GameObject newLetterUI = Instantiate(letterUIPrefab, lettersContainer);

                        // ÉP TỌA ĐỘ VÀ TỶ LỆ KÍCH THƯỚC VỀ CHUẨN
                        RectTransform rect = newLetterUI.GetComponent<RectTransform>();
                        if (rect != null)
                        {
                            rect.localPosition = Vector3.zero;
                            rect.localScale = Vector3.one;
                        }

                        // Gọi Helper đổi sang đúng chữ cái nhặt được dưới đất
                        LetterUIHelper uiHelper = newLetterUI.GetComponentInChildren<LetterUIHelper>();
                        if (uiHelper != null)
                        {
                            uiHelper.SetLetter(letter, letterSpriteData);
                        }

                        DragItem drag = newLetterUI.GetComponent<DragItem>();
                        if (drag != null)
                        {
                            drag.enabled = canSolveWord; // Dọc đường khóa kéo thả, chỉ khi ở Finish mới mở
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
                Debug.Log("TogglePopup(false)");

                targetPopupScale = Vector3.zero;
                isAnimatingPopup = true;

                // 🟢 GỌI HÀM MỞ KHÓA NHÂN VẬT GỌN GÀNG TỪ PLAYERMOVEMENT
                if (playerMovement != null)
                {
                    playerMovement.UnlockPlayer();
                }
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
    // KIỂM TRA ĐIỀU KIỆN KHI BẤM NÚT SUBMIT
    // ========================================================
    public void OnSubmitButtonClicked()
    {
        if (!canSolveWord)
        {
            if (statusText != null)
            {
                statusText.text = "<color=yellow>Hãy đến cổng cuối màn để ghép chữ!</color>";
            }
            return;
        }

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

        if (slots.Length == 0)
        {
            Debug.LogError("LỖI: Không tìm thấy linh kiện DropSlot nào trong SlotsContainer!");
            return;
        }

        System.Array.Sort(slots, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

        string playerWord = "";

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
        Time.timeScale = 0f;
        if (letterPopupPanel != null)
        {
            letterPopupPanel.SetActive(false);
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
            if (!HasCollectedAllLetters())
            {
                Debug.Log("Bạn chưa nhặt đủ chữ!");
                if (statusText != null)
                {
                    statusText.text = "<color=yellow>Hãy nhặt đủ các chữ cái trước!</color>";
                }
                return;
            }

            Debug.Log("Đã nhặt đủ chữ, tiến hành khóa nhân vật và mở bảng ghép.");
            canSolveWord = true;

            // 1. Gọi mở bảng gỗ trước để kích hoạt LockPlayer() đóng băng nhân vật hoàn toàn
            TogglePopup(true);

            // 2. Sau khi bảng đã dựng xong, mới kích hoạt tính năng kéo thả cho các quân chữ UI
            DragItem[] allDragItems = FindObjectsOfType<DragItem>();
            foreach (DragItem drag in allDragItems)
            {
                if (drag != null)
                {
                    drag.enabled = true;
                }
            }
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
        hintText.text = "Mua gợi ý sẽ tốn " + hintCost + " Coin.\n\nBạn có muốn tiếp tục?";
    }

    public void OnHintOKClicked()
    {
        if (score < hintCost)
        {
            hintText.text = "Không đủ Coin!";
            return;
        }

        if (currentHint >= 3)
        {
            hintText.text = "Bạn đã sử dụng hết gợi ý!";
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

        if (slotsContainer != null)
        {
            List<GameObject> childrenToDestroy = new List<GameObject>();
            foreach (Transform child in slotsContainer)
            {
                childrenToDestroy.Add(child.gameObject);
            }

            slotsContainer.DetachChildren();

            foreach (GameObject child in childrenToDestroy)
            {
                if (child != null)
                {
                    child.SetActive(false);
                    child.name = "Destroyed_Slot";
                    Destroy(child, 0.01f);
                }
            }
        }

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

        LayoutRebuilder.ForceRebuildLayoutImmediate(slotsContainer.GetComponent<RectTransform>());
    }
}