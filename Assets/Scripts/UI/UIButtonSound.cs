/* Author: TuLC
 * Date: 28/6/26
 * Description: This script plays UI sound effects when hovering or clicking buttons.
 */

using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour,
    IPointerEnterHandler,
    IPointerClickHandler
{
    // Phát âm thanh khi hover button.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonHover();
        }
    }

    // Phát âm thanh khi click button.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
}