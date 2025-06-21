using UnityEngine;
using TMPro;

public class MissionLocationTooltipPanel : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text roomCountText;
    [Header("0.01 +-")]
    public float margin = 0.01f; // Edytowalny margines

    public void ShowTooltip(string locationName, int roomCount, RectTransform targetRect)
    {
        if (nameText != null) nameText.text = locationName;
        if (roomCountText != null) roomCountText.text = $"Liczba pokoi: {roomCount}";
        gameObject.SetActive(true);

        RectTransform tooltipRect = GetComponent<RectTransform>();
        RectTransform parent = targetRect.parent as RectTransform;
        tooltipRect.SetParent(parent, false);

        Vector2 iconPos = targetRect.anchoredPosition;
        Vector2 iconSize = targetRect.sizeDelta;
        Vector2 tooltipSize = tooltipRect.sizeDelta;
        Vector2 parentSize = parent.rect.size;

        // 8 mo�liwych pozycji (prawo, lewo, g�ra, d�, oraz po skosie)
        Vector2[] candidates = new Vector2[]
        {
            // PRAWO
            iconPos + new Vector2(iconSize.x / 2 + tooltipSize.x / 2 + margin, 0),
            // LEWO
            iconPos + new Vector2(-iconSize.x / 2 - tooltipSize.x / 2 - margin, 0),
            // G�RA
            iconPos + new Vector2(0, iconSize.y / 2 + tooltipSize.y / 2 + margin),
            // Dӣ
            iconPos + new Vector2(0, -iconSize.y / 2 - tooltipSize.y / 2 - margin),
            // PRAWO-G�RA
            iconPos + new Vector2(iconSize.x / 2 + tooltipSize.x / 2 + margin, iconSize.y / 2 + tooltipSize.y / 2 + margin),
            // PRAWO-Dӣ
            iconPos + new Vector2(iconSize.x / 2 + tooltipSize.x / 2 + margin, -iconSize.y / 2 - tooltipSize.y / 2 - margin),
            // LEWO-G�RA
            iconPos + new Vector2(-iconSize.x / 2 - tooltipSize.x / 2 - margin, iconSize.y / 2 + tooltipSize.y / 2 + margin),
            // LEWO-Dӣ
            iconPos + new Vector2(-iconSize.x / 2 - tooltipSize.x / 2 - margin, -iconSize.y / 2 - tooltipSize.y / 2 - margin)
        };

        // Sprawd� ka�d� z pozycji
        foreach (Vector2 candidate in candidates)
        {
            bool inside =
                candidate.x - tooltipSize.x / 2 >= -parentSize.x / 2 &&
                candidate.x + tooltipSize.x / 2 <= parentSize.x / 2 &&
                candidate.y - tooltipSize.y / 2 >= -parentSize.y / 2 &&
                candidate.y + tooltipSize.y / 2 <= parentSize.y / 2;
            if (inside)
            {
                tooltipRect.anchoredPosition = candidate;
                return;
            }
        }

        tooltipRect.anchoredPosition = Vector2.zero;
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}