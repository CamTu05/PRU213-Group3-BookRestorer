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
            Sprite sprite = spriteData.GetSpriteForLetter(myLetter);

            Debug.Log("Chữ: " + myLetter);
            Debug.Log("Sprite tìm được: " + sprite);

            displayImage.sprite = sprite;
            displayImage.preserveAspect = true;
        }
        else
        {
            Debug.LogError("displayImage hoặc spriteData bị Null");
        }
    }

    // Hàm trả về ký tự để script DropSlot đọc được khi kiểm tra kết quả Submit
    public char GetLetter()
    {
        return myLetter;
    }
}