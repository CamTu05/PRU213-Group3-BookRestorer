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
            // Đổi ô cha mục tiêu thành ô trống này
            dragItem.parentToReturnTo = this.transform;

            // ĐỔI CHA NGAY LẬP TỨC: Ép quân chữ làm con của ô này luôn để Unity nhận diện diện vị trí mới
            dragItem.transform.SetParent(this.transform);
        }
    }

    // Hàm bổ trợ phục vụ nút Submit đọc kết quả ký tự
    public string GetLetter()
    {
        // Sử dụng GetComponentInChildren để tìm kiếm sâu xuống đối tượng con LetterDisplayImage
        LetterUIHelper helper = GetComponentInChildren<LetterUIHelper>();
        if (helper != null)
        {
            return helper.GetLetter().ToString();
        }
        return "";
    }
}