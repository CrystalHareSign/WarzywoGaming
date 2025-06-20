using UnityEngine;
using UnityEngine.UI;

public class MissionLocationIcon : MonoBehaviour
{
    [Header("Dane lokacji")]
    public string locationName;
    public int roomCount;

    [Header("UI")]
    public Image iconImage; // opcjonalnie, jeœli chcesz obrazek

    [Header("Manager")]
    public MissionDefiner missionDefiner;

    private void Awake()
    {
        if (missionDefiner == null)
            missionDefiner = Object.FindFirstObjectByType<MissionDefiner>();
    }

    // Mo¿esz podpi¹æ to do OnClick przez Unity Inspector
    public void OnIconClicked()
    {
        if (missionDefiner != null)
            missionDefiner.OnLocationSelected(this);
    }
}