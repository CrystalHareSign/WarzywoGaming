using UnityEngine;
using TMPro;

public class MissionLocationTooltipPanel : MonoBehaviour
{
    public TMP_Text typeText;
    public TMP_Text nameText;
    public TMP_Text roomCountText;
    [Header("0.01 +-")]
    public float margin = 0.01f; // Edytowalny margines

    public void ShowTooltip(string locationName, int roomCount, MissionLocationType locationType, RectTransform targetRect)
    {
        // Ustaw typ
        if (typeText != null)
            typeText.text = "Typ: " + (locationType == MissionLocationType.ProceduralRaid ? "RAID" : "GRIND BUS");

        // Ustaw nazwê
        if (nameText != null)
            nameText.text = "Nazwa: " + locationName;

        // Poka¿ lub ukryj iloœæ pokoi w zale¿noœci od typu lokacji
        if (roomCountText != null)
        {
            if (locationType == MissionLocationType.ProceduralRaid)
            {
                roomCountText.text = "Iloœæ pokoi: " + roomCount;
                roomCountText.gameObject.SetActive(true);
            }
            else
            {
                roomCountText.gameObject.SetActive(false);
            }
        }

        gameObject.SetActive(true);

        RectTransform tooltipRect = GetComponent<RectTransform>();
        RectTransform parent = targetRect.parent as RectTransform;
        tooltipRect.SetParent(parent, false);

        Vector2 iconPos = targetRect.anchoredPosition;
        Vector2 iconSize = targetRect.sizeDelta;
        Vector2 tooltipSize = tooltipRect.sizeDelta;
        Vector2 parentSize = parent.rect.size;

        // 8 mo¿liwych pozycji (prawo, lewo, góra, dó³, oraz po skosie)
        Vector2[] candidates = new Vector2[]
        {
            iconPos + new Vector2(iconSize.x / 2 + tooltipSize.x / 2 + margin, 0),
            iconPos + new Vector2(-iconSize.x / 2 - tooltipSize.x / 2 - margin, 0),
            iconPos + new Vector2(0, iconSize.y / 2 + tooltipSize.y / 2 + margin),
            iconPos + new Vector2(0, -iconSize.y / 2 - tooltipSize.y / 2 - margin),
            iconPos + new Vector2(iconSize.x / 2 + tooltipSize.x / 2 + margin, iconSize.y / 2 + tooltipSize.y / 2 + margin),
            iconPos + new Vector2(iconSize.x / 2 + tooltipSize.x / 2 + margin, -iconSize.y / 2 - tooltipSize.y / 2 - margin),
            iconPos + new Vector2(-iconSize.x / 2 - tooltipSize.x / 2 - margin, iconSize.y / 2 + tooltipSize.y / 2 + margin),
            iconPos + new Vector2(-iconSize.x / 2 - tooltipSize.x / 2 - margin, -iconSize.y / 2 - tooltipSize.y / 2 - margin)
        };

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