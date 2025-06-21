using UnityEngine;
using UnityEngine.EventSystems;

public class MissionLocationIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public string locationName;
    public int roomCount;
    public MissionDefiner missionDefiner;

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (missionDefiner != null && missionDefiner.tooltipPanel != null)
        {
            missionDefiner.tooltipPanel.ShowTooltip(locationName, roomCount, GetComponent<RectTransform>());
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