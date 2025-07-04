using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class DriverSeatInteraction : MonoBehaviour
{
    [Header("Referencje gracza")]
    public Camera playerCamera;
    public Transform cameraTarget;

    [Header("Animacja podr�y")]
    [Tooltip("Czas trwania animacji siadania i wstawania (sekundy)")]
    public float animationDuration = 1.2f;
    [Tooltip("Czas oczekiwania po zako�czeniu animacji siadania zanim zmieni si� scena (sekundy)")]
    public float pauseAfterSit = 3.0f;
    [Tooltip("Czas oczekiwania po zmianie sceny zanim odpali si� animacja zsiadania (sekundy)")]
    public float pauseBeforeExitAnim = 1.0f;

    [Header("Podr�")]
    public CameraToMonitor cameraToMonitor;

    private MonoBehaviour playerMovementScript;
    private MonoBehaviour mouseLookScript;
    private Coroutine travelCoroutine;

    private bool flashlightWasOnBeforeTravel = false;

    // --- ZAPISYWANIE POZYCJI DO ANIMACJI POWROTU ---
    private static Vector3 lastPlayerPosition;
    private static Quaternion lastPlayerRotation;
    private static Vector3 lastCameraPosition;
    private static Quaternion lastCameraRotation;
    private static bool shouldPlayExitAnim = false;
    private static float cachedAnimationDuration = 1.2f;
    private static float cachedPauseBeforeExitAnim = 1.0f;

    private bool needToPlayExitAnimAfterSpawn = false;

    // --- HOVER MESSAGE INTERACTIVITY ---
    private HoverMessage hoverMessage;

    // --- FLAGA BLOKUJ�CA INTERAKCJE W CA�EJ GRZE ---
    public static bool IsAnyDriverSeatActive = false;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if (playerMovementScript == null)
            playerMovementScript = FindFirstObjectByType<PlayerMovement>();
        if (mouseLookScript == null)
            mouseLookScript = FindFirstObjectByType<MouseLook>();
        hoverMessage = GetComponent<HoverMessage>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneChanger.OnPlayerSpawned += OnPlayerSpawnedAndReady;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneChanger.OnPlayerSpawned -= OnPlayerSpawnedAndReady;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (shouldPlayExitAnim)
        {
            shouldPlayExitAnim = false;
            cachedPauseBeforeExitAnim = Mathf.Max(0.01f, cachedPauseBeforeExitAnim);
            needToPlayExitAnimAfterSpawn = true;
        }
    }

    private void OnPlayerSpawnedAndReady()
    {
        if (!needToPlayExitAnimAfterSpawn) return;
        needToPlayExitAnimAfterSpawn = false;

        StartCoroutine(DelayedExitLogicAfterSceneLoad(cachedPauseBeforeExitAnim));
    }

    private IEnumerator DelayedExitLogicAfterSceneLoad(float delay)
    {
        yield return new WaitForSeconds(delay);

        var player = FindFirstObjectByType<PlayerInteraction>();
        Camera cam = Camera.main;
        if (player != null && cam != null)
        {
            player.transform.position = lastPlayerPosition;
            player.transform.rotation = lastPlayerRotation;
            cam.transform.position = lastCameraPosition;
            cam.transform.rotation = lastCameraRotation;

            UnblockPlayerControl();
            // Odblokuj flag� po powrocie do gry
            IsAnyDriverSeatActive = false;

            if (MissionMonitor.Instance != null)
                MissionMonitor.Instance.StartTravel();
        }
    }

    public void StartTravel()
    {
        if (string.IsNullOrEmpty(CameraToMonitor.pendingTravelScene))
        {
            HoverMessage hoverMsg = GetComponent<HoverMessage>();
            if (hoverMsg != null && HoverMessageManager.Instance != null)
            {
                HoverMessageManager.Instance.ShowInfoPopup(
                    hoverMsg.infoMessage,
                    hoverMsg.infoFontSize,
                    hoverMsg.duration
                );
            }
            return;
        }

        // --- Blokada tras niedozwolonych ---
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string targetScene = CameraToMonitor.pendingTravelScene;
        string proceduralScene = "ProceduralLevels"; // Zmie� na nazw� swojej proceduralnej sceny, lub pobierz z konfiguracji

        bool allowed = false;
        if (currentScene == "Home" && targetScene == "Main")
            allowed = true;
        else if (currentScene == "Main" && (targetScene == "Home" || targetScene == proceduralScene))
            allowed = true;
        else if (currentScene == proceduralScene && targetScene == "Home")
            allowed = true;

        if (!allowed)
        {
            // Wy�wietl popup z uniwersaln� tre�ci�, kt�r� ustawisz w t�umaczeniach lub w inspectorze HoverMessage
            if (hoverMessage != null && HoverMessageManager.Instance != null)
            {
                HoverMessageManager.Instance.ShowInfoPopup(
                    hoverMessage.infoMessage,
                    hoverMessage.infoFontSize,
                    hoverMessage.duration
                );
            }
            return;
        }

        if (travelCoroutine == null)
            travelCoroutine = StartCoroutine(SeatTravelCoroutine());
    }

    private IEnumerator SeatTravelCoroutine()
    {
        var player = FindFirstObjectByType<PlayerInteraction>();
        Camera cam = playerCamera != null ? playerCamera : Camera.main;
        if (player != null && cam != null)
        {
            lastPlayerPosition = player.transform.position;
            lastPlayerRotation = player.transform.rotation;
            lastCameraPosition = cam.transform.position;
            lastCameraRotation = cam.transform.rotation;
        }

        if (Inventory.Instance != null && Inventory.Instance.flashlight != null)
        {
            flashlightWasOnBeforeTravel = Inventory.Instance.flashlight.enabled;
            Inventory.Instance.FlashlightOff();
        }

        IsAnyDriverSeatActive = true;

        // Ukryj UI i bro� natychmiast po rozpocz�ciu podr�y
        if (player != null)
        {
            // Ukrycie crosshaira
            if (player.crosshair != null)
                player.crosshair.SetActive(false);

            // Ukrycie broni
            if (player.inventory != null && player.inventory.currentWeaponPrefab != null)
                player.inventory.currentWeaponPrefab.SetActive(false);

            if (player.inventoryUI != null)
            {
                player.inventoryUI.HideWeaponUI();
                player.inventoryUI.HideItemUI();
            }
        }

        BlockPlayerControl();

        // BLOKADA HOVER MESSAGE: nie mo�na wchodzi� w interakcj� z fotelem podczas podr�y
        if (hoverMessage != null)
            hoverMessage.isInteracted = true;

        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;
        Vector3 targetPos = cameraTarget.position;
        Quaternion targetRot = cameraTarget.rotation;

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;
            cam.transform.position = Vector3.Lerp(startPos, targetPos, t);
            cam.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cam.transform.position = targetPos;
        cam.transform.rotation = targetRot;

        cachedAnimationDuration = animationDuration;
        cachedPauseBeforeExitAnim = pauseBeforeExitAnim;

        InputBlocker.Active = true;

        yield return new WaitForSeconds(pauseAfterSit);

        if (!string.IsNullOrEmpty(CameraToMonitor.pendingTravelScene))
        {
            shouldPlayExitAnim = true;
            cameraToMonitor.ConfirmTravel();
        }
        // NIE odblokowuj flagi tutaj ��odblokowanie dopiero po powrocie do gry!
        travelCoroutine = null;
    }

    private void BlockPlayerControl()
    {
        if (playerMovementScript != null)
            playerMovementScript.enabled = false;
        if (mouseLookScript != null)
            mouseLookScript.enabled = false;
    }

    public void UnblockPlayerControl()
    {
        if (playerMovementScript != null)
            playerMovementScript.enabled = true;
        if (mouseLookScript != null)
            mouseLookScript.enabled = true;
        InputBlocker.Active = false;

        // ODBLOKOWANIE HOVER MESSAGE: z powrotem mo�na wej�� w interakcj� z fotelem po podr�y
        if (hoverMessage != null)
            hoverMessage.isInteracted = false;

        if (Inventory.Instance != null && Inventory.Instance.flashlight != null)
        {
            if (flashlightWasOnBeforeTravel)
                Inventory.Instance.FlashlightOn();
            else
                Inventory.Instance.FlashlightOff();
        }

        // --- PRZYWR�� CROSSHAIR ---
        var player = FindFirstObjectByType<PlayerInteraction>();
        if (player != null && player.crosshair != null)
            player.crosshair.SetActive(true);

        // Flaga IsAnyDriverSeatActive odblokowywana w DelayedExitLogicAfterSceneLoad
    }
}