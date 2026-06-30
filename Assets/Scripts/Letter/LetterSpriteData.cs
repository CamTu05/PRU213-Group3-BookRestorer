using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LetterSpriteData", menuName = "ScriptableObjects/LetterSpriteData")]
public class LetterSpriteData : ScriptableObject
{
    [System.Serializable]
    public class LetterMapping
    {
        public char letter;
        public Sprite sprite;
    }

    [Header("Danh sách Sprite của các chữ cái")]
    public List<LetterMapping> alphabetSprites = new List<LetterMapping>();

    /// <summary>
    /// Lấy Sprite theo ký tự (A-Z)
    /// </summary>
    public Sprite GetSprite(char letter)
    {
        char upperLetter = char.ToUpper(letter);

        foreach (LetterMapping mapping in alphabetSprites)
        {
            if (char.ToUpper(mapping.letter) == upperLetter)
            {
                return mapping.sprite;
            }
        }

        Debug.LogWarning("Không tìm thấy Sprite của chữ: " + upperLetter);
        return null;
    }

    /// <summary>
    /// Hàm cũ để tương thích với các script đang dùng
    /// </summary>
    public Sprite GetSpriteForLetter(char letter)
    {
        return GetSprite(letter);
    }

    /// <summary>
    /// Kiểm tra có Sprite của chữ hay không
    /// </summary>
    public bool HasLetter(char letter)
    {
        return GetSprite(letter) != null;
    }
}