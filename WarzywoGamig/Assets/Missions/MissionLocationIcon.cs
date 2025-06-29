using UnityEngine;
using UnityEngine.EventSystems;

public class MissionLocationIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("5 min = 6.0 km\n10 min = 12.0 km\n15 min = 18.0 km")]
    [Header("Dane lokacji")]
    public MissionLocationType locationType = MissionLocationType.ProceduralRaid;
    public string locationName;
    public float totalDistanceKm;
    public float dangerZoneKm;
    public int roomCount;

    [Header("Referencje")]
    public MissionDefiner missionDefiner;

    [Header("Loot Level (1-5)")]
    [Range(1, 5)]
    public int lootLevel = 3; // Dodaj to pole, jeœli chcesz przekazywaæ lootLevel do tooltipa

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (missionDefiner != null && missionDefiner.tooltipPanel != null)
        {
            // Przekazujemy lootLevel równie¿ do tooltipa, pamiêtaj o dostosowaniu funkcji ShowTooltip!
            missionDefiner.tooltipPanel.ShowTooltip(
                locationName,
                roomCount,
                locationType,
                totalDistanceKm,
                dangerZoneKm,
                lootLevel,
                GetComponent<RectTransform>()
            );
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (missionDefiner != null && missionDefiner.tooltipPanel != null)
        {
            missionDefiner.tooltipPanel.HideTooltip();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (missionDefiner != null)
            missionDefiner.OnLocationSelected(this);
    }
}