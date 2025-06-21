using UnityEngine;
using TMPro;

public class MissionMonitor : MonoBehaviour
{
    public static MissionMonitor Instance;

    [Header("UI - Canvas Fields")]
    public TMP_Text summaryNameText;
    public TMP_Text summaryRoomsText;

    // Persistent data
    private string savedLocationName;
    private int savedRoomCount;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetSummary(string locationName, int roomCount)
    {
        savedLocationName = locationName;
        savedRoomCount = roomCount;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (summaryNameText != null)
            summaryNameText.text = savedLocationName ?? "";
        if (summaryRoomsText != null)
            summaryRoomsText.text = savedRoomCount > 0 ? $"Liczba pokoi: {savedRoomCount}" : "";
    }

    public bool HasSummary()
    {
        return !string.IsNullOrEmpty(savedLocationName) && savedRoomCount > 0;
    }

    public void ClearSummary()
    {
        savedLocationName = null;
        savedRoomCount = 0;
        UpdateUI();
    }
}