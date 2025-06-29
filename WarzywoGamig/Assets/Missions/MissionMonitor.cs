using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MissionMonitor : MonoBehaviour
{
    public static MissionMonitor Instance;

    [Header("UI - Canvas Fields")]
    public TMP_Text summaryNameText;
    public TMP_Text summaryRoomsText;
    public TMP_Text statusText;
    [Header("Loot Level UI")]
    public Transform summaryLootLevelContainer;
    [Tooltip("Kolor aktywnej ikonki loot level")]
    public Color lootLevelActiveColor = Color.yellow;
    [Tooltip("Kolor nieaktywnej ikonki loot level")]
    public Color lootLevelInactiveColor = Color.gray;

    // Persistent data
    private string savedLocationName;
    private int savedRoomCount;
    private MissionLocationType savedLocationType;
    private float savedTotalDistanceKm;
    private float savedDangerZoneKm;
    private int savedLootLevel;

    // Logika podró¿y
    private float distanceLeft;
    private float travelSpeed = 0.02f;
    private float timerAfterArrival = 59f;
    private bool isTraveling = false;
    private bool isTimerActive = false;

    public float GetDistanceLeft() => distanceLeft;
    public bool IsTimerActive() => isTimerActive;

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

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleSceneChange();
    }

    private void HandleSceneChange()
    {
        string scene = SceneManager.GetActiveScene().name;

        if (scene == "ProceduralLevels")
        {
            isTraveling = false;
            isTimerActive = false;
            distanceLeft = 0f;
            timerAfterArrival = 0f;
            UpdateUI();
        }
        else if (scene == "Home")
        {
            isTraveling = false;
            isTimerActive = false;
            distanceLeft = 0f;
            timerAfterArrival = 0f;
            if (!HasSummary())
            {
                if (summaryNameText != null) summaryNameText.text = "";
                if (summaryRoomsText != null) summaryRoomsText.text = "";
                if (statusText != null) statusText.text = "";
                if (summaryLootLevelContainer != null)
                    summaryLootLevelContainer.gameObject.SetActive(false);
            }
            else
            {
                UpdateUI();
            }
        }
        else
        {
            UpdateUI();
        }
    }

    public void SetSummary(string locationName, int roomCount, MissionLocationType locationType, float totalDistanceKm, float dangerZoneKm)
    {
        savedLocationName = locationName;
        savedRoomCount = roomCount;
        savedLocationType = locationType;
        savedTotalDistanceKm = totalDistanceKm;
        savedDangerZoneKm = dangerZoneKm;
        savedLootLevel = MissionSettings.lootLevel;
        distanceLeft = savedDangerZoneKm;
        isTraveling = false;
        isTimerActive = false;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (summaryNameText != null)
            summaryNameText.text = savedLocationName ?? "";
        if (summaryRoomsText != null)
            summaryRoomsText.text = savedRoomCount > 0 ? $"Liczba pokoi: {savedRoomCount}" : "";

        ShowLootLevel(savedLootLevel, savedLocationType);

        if (statusText != null)
        {
            string scene = SceneManager.GetActiveScene().name;
            if (scene == "ProceduralLevels")
            {
                statusText.text = "0 km";
                return;
            }
            if (!isTraveling && !isTimerActive)
            {
                if (scene == "Home")
                {
                    statusText.text = savedTotalDistanceKm > 0f ? $"{savedTotalDistanceKm:0.0} km" : "";
                }
                else if (scene == "Main")
                {
                    statusText.text = savedDangerZoneKm > 0f ? $"{savedDangerZoneKm:0.0} km" : "";
                }
                else
                {
                    statusText.text = "";
                }
            }
        }
    }

    private void ShowLootLevel(int lootLevel, MissionLocationType locationType)
    {
        // Pokazuj loot level tylko dla ProceduralRaid
        if (summaryLootLevelContainer == null) return;
        if (locationType != MissionLocationType.ProceduralRaid || lootLevel <= 0)
        {
            summaryLootLevelContainer.gameObject.SetActive(false);
            return;
        }

        summaryLootLevelContainer.gameObject.SetActive(true);
        for (int i = 0; i < summaryLootLevelContainer.childCount; i++)
        {
            var icon = summaryLootLevelContainer.GetChild(i).GetComponent<Image>();
            if (icon == null) continue;
            icon.color = i < lootLevel ? lootLevelActiveColor : lootLevelInactiveColor;
        }
    }

    public bool HasSummary()
    {
        return !string.IsNullOrEmpty(savedLocationName);
    }

    public void ClearSummary()
    {
        savedLocationName = null;
        savedRoomCount = 0;
        savedLocationType = MissionLocationType.ProceduralRaid;
        savedTotalDistanceKm = 0f;
        savedDangerZoneKm = 0f;
        savedLootLevel = 0;
        if (summaryNameText != null) summaryNameText.text = "";
        if (summaryRoomsText != null) summaryRoomsText.text = "";
        if (statusText != null) statusText.text = "";
        if (summaryLootLevelContainer != null)
            summaryLootLevelContainer.gameObject.SetActive(false);
        isTraveling = false;
        isTimerActive = false;
    }

    public MissionLocationType GetSavedLocationType()
    {
        return savedLocationType;
    }

    public void StartTravel()
    {
        if (SceneManager.GetActiveScene().name != "Main")
            return;

        distanceLeft = savedDangerZoneKm;
        isTraveling = true;
        isTimerActive = false;
        if (statusText != null)
            statusText.gameObject.SetActive(true);
    }

    void Update()
    {
        string scene = SceneManager.GetActiveScene().name;

        if (scene == "ProceduralLevels")
        {
            if (statusText != null)
                statusText.text = "0 km";
            isTraveling = false;
            isTimerActive = false;
            distanceLeft = 0f;
            timerAfterArrival = 0f;
            return;
        }

        if (scene == "Home" && !HasSummary())
        {
            if (summaryNameText != null) summaryNameText.text = "";
            if (summaryRoomsText != null) summaryRoomsText.text = "";
            if (statusText != null) statusText.text = "";
            if (summaryLootLevelContainer != null)
                summaryLootLevelContainer.gameObject.SetActive(false);
            isTraveling = false;
            isTimerActive = false;
            distanceLeft = 0f;
            timerAfterArrival = 0f;
            return;
        }

        if (isTraveling)
        {
            distanceLeft -= Time.deltaTime * travelSpeed;
            if (distanceLeft <= 0f)
            {
                distanceLeft = 0f;
                isTraveling = false;
                isTimerActive = true;
                timerAfterArrival = 59f;
            }
            else if (statusText != null)
            {
                statusText.text = $"{distanceLeft:0.0} km";
            }
        }
        else if (isTimerActive)
        {
            timerAfterArrival -= Time.deltaTime;
            if (timerAfterArrival < 0f) timerAfterArrival = 0f;
            if (statusText != null)
                statusText.text = $"00:{timerAfterArrival:00}";
            if (timerAfterArrival <= 0f)
            {
                isTimerActive = false;
                // Wywo³aj docelow¹ metodê, np. OnArrivalTimerEnd();
            }
        }
        else
        {
            UpdateUI();
        }
    }
}