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
    private MissionLocationType savedLocationType;

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

    public void SetSummary(string locationName, int roomCount, MissionLocationType locationType)
    {
        savedLocationName = locationName;
        savedRoomCount = roomCount;
        savedLocationType = locationType;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (summaryNameText != null)
            summaryNameText.text = savedLocationName ?? "";
        if (summaryRoomsText != null)
            summaryRoomsText.text = savedRoomCount > 0 ? $"Liczba pokoi: {savedRoomCount}" : "";
        // Jeœli chcesz dodaæ pole na typ misji, tu mo¿na je zaktualizowaæ
    }

    public bool HasSummary()
    {
        return !string.IsNullOrEmpty(savedLocationName) && savedRoomCount > 0;
    }

    public void ClearSummary()
    {
        savedLocationName = null;
        savedRoomCount = 0;
        savedLocationType = MissionLocationType.ProceduralRaid;
        if (summaryNameText != null) summaryNameText.text = "";
        if (summaryRoomsText != null) summaryRoomsText.text = "";
    }

    public MissionLocationType GetSavedLocationType()
    {
        return savedLocationType;
    }
}