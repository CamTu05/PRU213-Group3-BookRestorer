using UnityEngine;
using UnityEngine.EventSystems;

public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Transform parentToReturnTo = null;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private PlayerInventory inventory;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();

        inventory = FindObjectOfType<PlayerInventory>();
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
        // Khi buông chuột, đặt quân chữ làm con của ô cha mới (DropSlot hoặc khay chứa UI_Letters_Row)
        this.transform.SetParent(parentToReturnTo);

        // KHÓA CHẶT VÀO TÂM: Ép tọa độ cục bộ và kích thước về chuẩn của ô trống mới
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector2.zero; // Ép về vị trí (0,0) dựa trên Anchor của ô chứa
            rectTransform.localPosition = Vector3.zero;    // Khóa chặt tâm chống lệch văng lên trên
            rectTransform.localScale = Vector3.one;        // Giữ nguyên tỷ lệ hiển thị chuẩn (1,1,1)
        }

        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
    }
}