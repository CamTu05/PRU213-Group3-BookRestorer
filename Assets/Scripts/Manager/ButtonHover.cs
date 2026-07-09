//chittp-0807
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour,
IPointerEnterHandler,
IPointerExitHandler
{
    Vector3 original;

    void Start()
    {
        original = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = Vector3.one * 1.05f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = original;
    }
}