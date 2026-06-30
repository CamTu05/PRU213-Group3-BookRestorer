using UnityEngine;
using UnityEngine.EventSystems;

public class DropSlot : MonoBehaviour, IDropHandler
{
    private PlayerInventory inventory;
    private void Start()
    {
        inventory = FindObjectOfType<PlayerInventory>();
    }
    public void OnDrop(PointerEventData eventData)
    {
        if (inventory != null && !inventory.CanSolveWord())
        {
            return;
        }
        // Kiểm tra xem đối tượng được thả vào có script DragItem hay không
        DragItem dragItem = eventData.pointerDrag?.GetComponent<DragItem>();

        // Nếu ô này chưa có chữ nào gán vào, cho phép nhận chữ mới
        if (dragItem != null && transform.childCount == 0)
        {
            dragItem.parentToReturnTo = this.transform; // Đổi ô cha mục tiêu thành ô trống này
        }
    }

    // Hàm bổ trợ phục vụ nút Submit đọc kết quả ký tự
    public string GetLetter()
    {
        LetterUIHelper helper = GetComponentInChildren<LetterUIHelper>();
        if (helper != null)
        {
            return helper.GetLetter().ToString();
        }
        return "";
    }
}