using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // Thêm thư viện này để chạy lệnh LayoutRebuilder căn chỉnh khay chứa

public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Transform parentToReturnTo = null;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private PlayerInventory inventory;
    private Transform lettersContainer; // Biến lưu trữ khay chứa chữ lộn xộn ban đầu

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        inventory = FindObjectOfType<PlayerInventory>();

        // Tự động tìm khay chứa lộn xộn "UI_Letters_Row" ngoài giao diện dựa theo cấu trúc bảng của ông
        GameObject containerObj = GameObject.Find("UI_Letters_Row");
        if (containerObj != null)
        {
            lettersContainer = containerObj.transform;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (inventory != null && !inventory.CanSolveWord())
        {
            return;
        }

        // Lưu lại ô cha cũ phòng trường hợp người chơi thả ra ngoài khoảng không
        parentToReturnTo = this.transform.parent;

        // Đưa quân chữ lên thẳng Canvas gốc để khi kéo nó không bị che khuất dưới các UI khác
        this.transform.SetParent(this.transform.root);

        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (inventory != null && !inventory.CanSolveWord())
        {
            return;
        }
        // Khi đang di chuột, quân chữ liên tục bám theo vị trí con trỏ màn hình
        this.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (inventory != null && !inventory.CanSolveWord())
        {
            return;
        }

        // ========================================================
        // ĐOẠN SỬA LỖI: KIỂM TRA XEM CÓ THẢ CHUỘT RA NGOÀI KHOẢNG TRỐNG KHÔNG
        // ========================================================
        if (this.transform.parent == this.transform.root)
        {
            // Nếu thả ở khoảng trống (không có DropSlot nào nhận nuôi), ta kiểm tra xem nó có rời đi từ ô kết quả không
            if (lettersContainer != null)
            {
                // Ép quân chữ chuyển hộ khẩu bay thẳng về khay chứa lộn xộn ban đầu
                parentToReturnTo = lettersContainer;
            }
        }

        // Khi buông chuột, đặt quân chữ làm con của ô cha mới (DropSlot hoặc được trả lại khay chứa)
        this.transform.SetParent(parentToReturnTo);

        // KHÓA CHẶT VÀO TÂM: Ép tọa độ cục bộ và kích thước về chuẩn của ô trống mới
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector2.zero; // Ép về vị trí (0,0) dựa trên Anchor của ô chứa
            rectTransform.localPosition = Vector3.zero;    // Khóa chặt tâm chống lệch văng lên trên
            rectTransform.localScale = Vector3.one;        // Giữ nguyên tỷ lệ hiển thị chuẩn (1,1,1)
        }

        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;

        // ÉP KHAY CHỨA SẮP XẾP LẠI: Nếu chữ được trả về khay, ép khay tự động dàn hàng ngang ngăn nắp lại ngay
        if (parentToReturnTo == lettersContainer && lettersContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(lettersContainer as RectTransform);
        }
    }
}