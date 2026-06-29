using UnityEngine;
using UnityEngine.UI;

public class LetterUIHelper : MonoBehaviour
{
    [SerializeField] private Image displayImage; // Ô để kéo linh kiện Image con vào
    private char myLetter; // Biến lưu ký tự của quân chữ này (A, N, I, M, A, L...)

    // Hàm này sẽ được PlayerInventory gọi để cài đặt chữ và ảnh tương ứng
    public void SetLetter(char letter, LetterSpriteData spriteData)
    {
        myLetter = char.ToUpper(letter);
        if (displayImage != null && spriteData != null)
        {
            // 1. Tìm ảnh màu vàng tương ứng trong bộ sưu tập và thay thế vào UI
            displayImage.sprite = spriteData.GetSpriteForLetter(myLetter);

            // 2. ÉP BUỘC BẬT CHỐNG MÉO ẢNH (Thay vì phải tích tay ngoài Unity)
            displayImage.preserveAspect = true;

            // 3. Log kiểm tra xem code có thực sự chạy qua đây không
            Debug.Log($"[LetterUIHelper] Đã đổi ảnh thành công cho quân chữ: {myLetter}");
        }
        else
        {
            Debug.LogError("[LetterUIHelper] Thiếu liên kết! displayImage hoặc spriteData đang bị trống (Null)!");
        }
    }

    // Hàm trả về ký tự để script DropSlot đọc được khi kiểm tra kết quả Submit
    public char GetLetter()
    {
        return myLetter;
    }
}