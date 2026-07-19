using UnityEngine;

public class LetterItem : MonoBehaviour
{
    [Header("Letter")]
    [SerializeField] private char letter;

    [Header("Sprite Data")]
    [SerializeField] private LetterSpriteData letterSpriteData;

    [Header("Audio")]
    [SerializeField] private AudioClip collectSFX;

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. Phát âm thanh thu thập qua AudioManager
            if (AudioManager.Instance != null && collectSFX != null)
            {
                AudioManager.Instance.PlaySFX(collectSFX);
            }
        }
    }
}