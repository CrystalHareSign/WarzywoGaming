using UnityEngine;
using UnityEngine.EventSystems;

public class MissionLocationIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Dane lokacji")]
    public string locationName;
    public int roomCount;
    public MissionLocationType locationType = MissionLocationType.ProceduralRaid;

    [Header("Referencje")]
    public MissionDefiner missionDefiner;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (missionDefiner != null && missionDefiner.tooltipPanel != null)
        {
            // Przekazujemy typ lokacji, ¿eby tooltip wiedzia³ co pokazaæ
            missionDefiner.tooltipPanel.ShowTooltip(locationName, roomCount, locationType, GetComponent<RectTransform>());
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