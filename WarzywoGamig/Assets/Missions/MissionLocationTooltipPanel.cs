using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionLocationTooltipPanel : MonoBehaviour
{
    public TMP_Text typeText;
    public TMP_Text nameText;
    public TMP_Text roomCountText;
    public TMP_Text tooltipDistanceText;
    public TMP_Text tooltipDangerZoneText;

    [Header("Loot level stars (5 images, left to right)")]
    public Image[] lootLevelStars; // Przypnij 5 images w Inspectorze

    [Header("Kolory gwiazdek loot level")]
    public Color filledStarColor = Color.yellow;
    public Color emptyStarColor = Color.gray;

    [Header("0.01 +-")]
    public float margin = 0.01f;

    // Obs³uguje oba dystanse: totalDistanceKm i dangerZoneKm osobno + lootLevel
    public void ShowTooltip(
        string locationName,
        int roomCount,
        MissionLocationType locationType,
        float totalDistanceKm,
        float dangerZoneKm,
        int lootLevel,
        RectTransform targetRect)
    {
        if (typeText != null)
            typeText.text = "Typ: " + (locationType == MissionLocationType.ProceduralRaid ? "RAID" : "GRIND BUS");

        if (nameText != null)
            nameText.text = "Nazwa: " + locationName;

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

        if (tooltipDistanceText != null)
            tooltipDistanceText.text = $"Dystans: {totalDistanceKm:0.0} km";

        if (tooltipDangerZoneText != null)
            tooltipDangerZoneText.text = $"Danger zone: {dangerZoneKm:0.0} km";

        // Pokazuj gwiazdki tylko dla typu RAID, koloruj zamiast sprite
        if (lootLevelStars != null && lootLevelStars.Length == 5)
        {
            bool showStars = locationType == MissionLocationType.ProceduralRaid;
            for (int i = 0; i < lootLevelStars.Length; i++)
            {
                lootLevelStars[i].enabled = showStars;
                if (showStars)
                    lootLevelStars[i].color = (i < lootLevel) ? filledStarColor : emptyStarColor;
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