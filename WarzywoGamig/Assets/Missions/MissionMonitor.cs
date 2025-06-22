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
    private float savedTotalDistanceKm;   // Ca�kowity dystans (info)
    private float savedDangerZoneKm;      // Danger zone (grywalny dystans)

    // Logika podr�y
    private float distanceLeft;
    private float travelSpeed = 0.02f;   // km/s (np. 72 km/h)
    private float timerAfterArrival = 59f;
    private bool isTraveling = false;
    private bool isTimerActive = false;

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

    // Wywo�aj przy zatwierdzeniu misji!
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
            if (!isTraveling && !isTimerActive)
            {
                string scene = SceneManager.GetActiveScene().name;
                if (scene == "Home")
                {
                    // Je�li jest wybrana misja (total > 0), pokazuj total, w przeciwnym razie czy�� pole
                    statusText.text = savedTotalDistanceKm > 0f ? $"{savedTotalDistanceKm:0.0} km" : "";
                }
                else if (scene == "Main")
                {
                    statusText.text = savedDangerZoneKm > 0f ? $"{savedDangerZoneKm:0.0} km" : "";
                }
                else if (scene == "ProceduralLevels")
                {
                    statusText.text = "0 km";
                }
                else
                {
                    statusText.text = ""; // domy�lnie nic
                }
            }
            // Reszta logiki (podczas podr�y/timera) obs�ugiwana w Update()
        }
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

    // WYWO�AJ PO ZMIANIE SCENY!
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
                // Wywo�aj docelow� metod�, np. OnArrivalTimerEnd();
            }
        }
        else
        {
            // Je�li nie podr�ujemy i nie jest aktywny timer, od�wie� UI (np. po zmianie sceny)
            UpdateUI();
        }
    }
}