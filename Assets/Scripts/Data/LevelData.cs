/* Author: TuLC
 * Date: 27/6/26
 * Description: This script defines a ScriptableObject for storing level data in a Unity project.
 */

using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelId;

    // Level thứ mấy trong Book (1,2,3...)
    public int levelNumber;

    public string levelName;

    // Scene sẽ được load khi chơi level này
    public string sceneName;

    // Dữ liệu Puzzle của level
    //public PuzzleLevelData puzzleData;

    // Ảnh preview hiển thị trong Library 
    public Sprite previewImage;
}