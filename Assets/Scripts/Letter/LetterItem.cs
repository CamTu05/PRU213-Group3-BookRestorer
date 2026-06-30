using UnityEngine;

public class LetterItem : MonoBehaviour
{
    [Header("Letter")]
    [SerializeField] private char letter;

    [Header("Sprite Data")]
    [SerializeField] private LetterSpriteData letterSpriteData;

    public char GetLetter()
    {
        return letter;
    }

    public Sprite GetSprite()
    {
        if (letterSpriteData == null)
            return null;

        return letterSpriteData.GetSprite(letter);
    }
}