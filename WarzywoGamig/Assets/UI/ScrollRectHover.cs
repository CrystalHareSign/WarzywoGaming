using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollRectHover : ScrollRect, IPointerEnterHandler, IPointerExitHandler
{
    private bool isPointerOver = false;

    public override void OnScroll(PointerEventData data)
    {
        if (isPointerOver)
            base.OnScroll(data);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
    }
}