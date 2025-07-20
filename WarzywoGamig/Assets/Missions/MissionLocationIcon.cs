using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MissionLocationIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
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
    public int lootLevel = 3; // enrichment

    [Header("Loot Rarity (1-5)")]
    [Range(1, 5)]
    public int lootRarity = 2;

    [Header("Dozwolone typy amunicji na tej lokacji")]
    public List<AmmoType> allowedAmmoTypes; // <-- TO DODAJ

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (missionDefiner != null && missionDefiner.tooltipPanel != null)
        {
            missionDefiner.tooltipPanel.ShowTooltip(
                locationName,
                roomCount,
                locationType,
                totalDistanceKm,
                dangerZoneKm,
                lootLevel,
                lootRarity,
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