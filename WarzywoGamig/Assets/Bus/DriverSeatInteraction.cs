using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class DriverSeatInteraction : MonoBehaviour
{
    [Header("Referencje gracza")]
    public Camera playerCamera;
    public Transform cameraTarget;

    [Header("Animacja podró¿y")]
    [Tooltip("Czas trwania animacji siadania i wstawania (sekundy)")]
    public float animationDuration = 1.2f;
    [Tooltip("Czas oczekiwania po zakoñczeniu animacji siadania zanim zmieni siê scena (sekundy)")]
    public float pauseAfterSit = 3.0f;
    [Tooltip("Czas oczekiwania po zmianie sceny zanim odpali siê animacja zsiadania (sekundy)")]
    public float pauseBeforeExitAnim = 1.0f;

    [Header("Podró¿")]
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

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if (playerMovementScript == null)
            playerMovementScript = FindFirstObjectByType<PlayerMovement>();
        if (mouseLookScript == null)
            mouseLookScript = FindFirstObjectByType<MouseLook>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (shouldPlayExitAnim)
        {
            shouldPlayExitAnim = false;
            float pause = cachedPauseBeforeExitAnim;
            StartCoroutine(DelayedExitAnimAfterSceneLoad(pause));
        }
    }

    private IEnumerator DelayedExitAnimAfterSceneLoad(float delay)
    {
        yield return new WaitForSeconds(delay);

        var player = FindFirstObjectByType<PlayerInteraction>();
        Camera cam = Camera.main;
        if (player != null && cam != null)
        {
            GameObject seatStart = GameObject.Find("DriverSeatStart");
            if (seatStart != null)
            {
                player.transform.position = seatStart.transform.position;
                player.transform.rotation = seatStart.transform.rotation;
                cam.transform.position = seatStart.transform.position;
                cam.transform.rotation = seatStart.transform.rotation;
            }
            player.StartCoroutine(ExitDriverSeatAnimation(player.transform, cam, cachedAnimationDuration, player));
        }
    }

    public static IEnumerator ExitDriverSeatAnimation(Transform player, Camera camera, float duration, PlayerInteraction playerInteraction)
    {
        Vector3 startPlayerPos = player.position;
        Quaternion startPlayerRot = player.rotation;
        Vector3 endPlayerPos = lastPlayerPosition;
        Quaternion endPlayerRot = lastPlayerRotation;

        Vector3 startCamPos = camera.transform.position;
        Quaternion startCamRot = camera.transform.rotation;
        Vector3 endCamPos = lastCameraPosition;
        Quaternion endCamRot = lastCameraRotation;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            player.position = Vector3.Lerp(startPlayerPos, endPlayerPos, t);
            player.rotation = Quaternion.Slerp(startPlayerRot, endPlayerRot, t);
            camera.transform.position = Vector3.Lerp(startCamPos, endCamPos, t);
            camera.transform.rotation = Quaternion.Slerp(startCamRot, endCamRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        player.position = endPlayerPos;
        player.rotation = endPlayerRot;
        camera.transform.position = endCamPos;
        camera.transform.rotation = endCamRot;

        var seat = FindFirstObjectByType<DriverSeatInteraction>();
        if (seat != null)
            seat.UnblockPlayerControl();
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
            Debug.Log("Najpierw wyznacz cel podró¿y na monitorze!");
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

        BlockPlayerControl();

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

        InputBlocker.Active = true; // InputBlocker w³¹czany na koñcu animacji siadania

        yield return new WaitForSeconds(pauseAfterSit);

        if (!string.IsNullOrEmpty(CameraToMonitor.pendingTravelScene))
        {
            shouldPlayExitAnim = true;
            cameraToMonitor.ConfirmTravel();
        }
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

        if (Inventory.Instance != null && Inventory.Instance.flashlight != null)
        {
            if (flashlightWasOnBeforeTravel)
                Inventory.Instance.FlashlightOn();
            else
                Inventory.Instance.FlashlightOff();
        }
    }
}