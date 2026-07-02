/* Author: TuLC
 * Date: 27/6/26
 * Description: This script defines a ScriptableObject for storing book data in a Unity project.
 */

using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BookData", menuName = "Game/Book Data")]
public class BookData : ScriptableObject
{
    public string bookId;

    public string bookName;

    [TextArea]
    public string description;

    public Sprite coverImage;

    public List<LevelData> levels;
}