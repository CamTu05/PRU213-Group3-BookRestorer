/* Author: TuLC
 * Date: 29/6/26
 * Description: This script controls one Level item in the Library UI.
 */

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text levelNameText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image statusIcon;
    [SerializeField] private Button button;

    [Header("Status Sprites")]
    [SerializeField] private Sprite completedSprite;
    [SerializeField] private Sprite unlockedSprite;
    [SerializeField] private Sprite lockedSprite;

    [Header("Background Colors")]
    [SerializeField] private Color completedColor = new Color(0.70f, 1f, 0.70f);
    [SerializeField] private Color unlockedColor = new Color(1f, 0.95f, 0.60f);
    [SerializeField] private Color lockedColor = new Color(0.75f, 0.75f, 0.75f);

    private LevelData levelData;

    // Khởi tạo dữ liệu hiển thị cho LevelItem.
    public void Initialize(LevelData data, LevelStatus status)
    {
        levelData = data;

        levelNameText.text = levelData.levelName;

        button.onClick.RemoveAllListeners();

        switch (status)
        {
            case LevelStatus.Completed:
                backgroundImage.color = completedColor;
                statusIcon.sprite = completedSprite;
                button.interactable = true;
                button.onClick.AddListener(LoadLevel);
                break;

            case LevelStatus.Unlocked:
                backgroundImage.color = unlockedColor;
                statusIcon.sprite = unlockedSprite;
                button.interactable = true;
                button.onClick.AddListener(LoadLevel);
                break;

            case LevelStatus.Locked:
                backgroundImage.color = lockedColor;
                statusIcon.sprite = lockedSprite;
                button.interactable = false;
                break;
        }
    }

    // Load scene của level được chọn.
    private void LoadLevel()
    {
        if (levelData == null || string.IsNullOrEmpty(levelData.sceneName))
        {
            Debug.LogError("LevelItemUI: LevelData hoặc SceneName bị thiếu.");
            return;
        }

        SceneManager.LoadScene(levelData.sceneName);
    }
}