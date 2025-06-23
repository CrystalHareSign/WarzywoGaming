using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MissionMonitor : MonoBehaviour
{
    public static MissionMonitor Instance;

    [Header("UI - Canvas Fields")]
    public TMP_Text summaryNameText;
    public TMP_Text summaryRoomsText;
    public TMP_Text statusText; // Tylko to! (KM + TIMER w jednym)

    // Persistent data
    private string savedLocationName;
    private int savedRoomCount;
    private MissionLocationType savedLocationType;
    private float savedTotalDistanceKm;   // Ca³kowity dystans (info)
    private float savedDangerZoneKm;      // Danger zone (grywalny dystans)

    // Logika podró¿y
    private float distanceLeft;
    private float travelSpeed = 0.02f;   // km/s (np. 72 km/h)
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
            // Reset po wejœciu do miejsca docelowego
            isTraveling = false;
            isTimerActive = false;
            distanceLeft = 0f;
            timerAfterArrival = 0f;
            UpdateUI();
        }
        else if (scene == "Home")
        {
            // Reset po powrocie do bazy
            isTraveling = false;
            isTimerActive = false;
            distanceLeft = 0f;
            timerAfterArrival = 0f;
            // Jeœli nie ma aktywnej misji, czyœæ UI
            if (!HasSummary())
            {
                if (summaryNameText != null) summaryNameText.text = "";
                if (summaryRoomsText != null) summaryRoomsText.text = "";
                if (statusText != null) statusText.text = "";
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

    // Wywo³aj przy zatwierdzeniu misji!
    public void SetSummary(string locationName, int roomCount, MissionLocationType locationType, float totalDistanceKm, float dangerZoneKm)
    {
        savedLocationName = locationName;
        savedRoomCount = roomCount;
        savedLocationType = locationType;
        savedTotalDistanceKm = totalDistanceKm;
        savedDangerZoneKm = dangerZoneKm;
        distanceLeft = savedDangerZoneKm; // TYLKO danger zone jest liczony!
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
                    // Jeœli jest wybrana misja (total > 0), pokazuj total, w przeciwnym razie czyœæ pole
                    statusText.text = savedTotalDistanceKm > 0f ? $"{savedTotalDistanceKm:0.0} km" : "";
                }
                else if (scene == "Main")
                {
                    statusText.text = savedDangerZoneKm > 0f ? $"{savedDangerZoneKm:0.0} km" : "";
                }
                else
                {
                    statusText.text = ""; // domyœlnie nic
                }
            }
            // Reszta logiki (podczas podró¿y/timera) obs³ugiwana w Update()
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
        if (summaryNameText != null) summaryNameText.text = "";
        if (summaryRoomsText != null) summaryRoomsText.text = "";
        if (statusText != null) statusText.text = "";
        isTraveling = false;
        isTimerActive = false;
    }

    public MissionLocationType GetSavedLocationType()
    {
        return savedLocationType;
    }

    // WYWO£AJ PO ZMIANIE SCENY!
    public void StartTravel()
    {
        // Licznik tylko w Main!
        if (SceneManager.GetActiveScene().name != "Main")
            return;

        distanceLeft = savedDangerZoneKm; // tylko danger zone!
        isTraveling = true;
        isTimerActive = false;
        if (statusText != null)
            statusText.gameObject.SetActive(true);
    }

    void Update()
    {
        string scene = SceneManager.GetActiveScene().name;

        // Wymuszenie 0 km i resetu licznika/timera w ProceduralLevels
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

        // Reset po powrocie do Home jeœli nie ma aktywnej misji
        if (scene == "Home" && !HasSummary())
        {
            if (summaryNameText != null) summaryNameText.text = "";
            if (summaryRoomsText != null) summaryRoomsText.text = "";
            if (statusText != null) statusText.text = "";
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
            // Jeœli nie podró¿ujemy i nie jest aktywny timer, odœwie¿ UI (np. po zmianie sceny)
            UpdateUI();
        }
    }
}