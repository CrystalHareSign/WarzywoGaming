using UnityEngine;
using UnityEngine.UI;

public class DisableScroll : MonoBehaviour
{
    public ScrollRect scrollRect;

    void Update()
    {
        // SprawdŸ, czy content mieœci siê w viewport (nie ma co scrollowaæ)
        if (scrollRect.content.rect.height <= scrollRect.viewport.rect.height)
        {
            scrollRect.vertical = false;
        }
        else
        {
            scrollRect.vertical = true;
        }
    }
}