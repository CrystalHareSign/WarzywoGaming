using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class MissionDefiner : MonoBehaviour
{
    [Header("Cel kamery podczas interakcji")]
    public Transform cameraTargetPosition;
    public float cameraMoveSpeed = 4f;

    [Header("UI Kanwy")]
    public GameObject missionCanvas;
    public GameObject summaryCanvas;
    public TMP_Text summaryNameText;
    public TMP_Text summaryTypeText;
    public TMP_Text summaryRoomsText;
    public TMP_Text summaryDistanceText;
    public TMP_Text summaryDangerZoneText;

    [Header("Loot Level UI")]
    public Transform summaryLootLevelContainer;
    [Tooltip("Kolor aktywnej ikonki loot level")]
    public Color lootLevelActiveColor = Color.yellow;
    [Tooltip("Kolor nieaktywnej ikonki loot level")]
    public Color lootLevelInactiveColor = Color.gray;

    [Header("UI Przycisków")]
    public Button confirmButton;
    public Button exitButton;

    [Header("Tooltip do ikon")]
    public MissionLocationTooltipPanel tooltipPanel;

    // Inne referencje
    public PlayerMovement playerMovementScript;
    public MouseLook mouseLookScript;
    public InventoryUI inventoryUI;
    public PlayerInteraction playerInteraction;
    public GameObject crossHair;
    public Inventory inventory;

    // Stany
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private bool isInteracting = false;
    private bool isCameraMoving = false;
    private bool flashlightWasOnBefore = false;
    private bool weaponWasActiveBefore = false;

    // Stan wyboru
    private string pendingLocationName = null;
    private int pendingRoomCount = 0;
    private MissionLocationType pendingLocationType = MissionLocationType.ProceduralRaid;
    private float pendingTotalDistanceKm = 0f;
    private float pendingDangerZoneKm = 0f;
    private int pendingLootLevel = 1;

    public static bool IsAnyDefinerActive = false;

    void Start()
    {
        if (playerMovementScript == null) playerMovementScript = Object.FindFirstObjectByType<PlayerMovement>();
        if (mouseLookScript == null) mouseLookScript = Object.FindFirstObjectByType<MouseLook>();
        if (playerInteraction == null) playerInteraction = Object.FindFirstObjectByType<PlayerInteraction>();
        if (inventoryUI == null) inventoryUI = Object.FindFirstObjectByType<InventoryUI>();
        if (inventory == null) inventory = Object.FindFirstObjectByType<Inventory>();
        if (crossHair == null) crossHair = GameObject.FindWithTag("Crosshair") ?? GameObject.Find("Crosshair");

        if (missionCanvas != null) missionCanvas.SetActive(true);
        if (summaryCanvas != null) summaryCanvas.SetActive(true);

        HideMissionDefinerButtons();
        ClearSummary();

        if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirmClicked);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitClicked);
    }

    public void UseDefiner()
    {
        if (!isInteracting)
        {
            isInteracting = true;
            IsAnyDefinerActive = true;
            originalCameraPosition = Camera.main.transform.position;
            originalCameraRotation = Camera.main.transform.rotation;

            if (inventory != null && inventory.currentWeaponPrefab != null)
            {
                weaponWasActiveBefore = inventory.currentWeaponPrefab.activeSelf;
                inventory.currentWeaponPrefab.SetActive(false);
            }
            else
            {
                weaponWasActiveBefore = false;
            }

            ShowMissionDefinerButtons();
            ClearSummary();
            StartCoroutine(MoveCameraToTarget());
        }
    }

    IEnumerator MoveCameraToTarget()
    {
        isCameraMoving = true;

        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (mouseLookScript != null) mouseLookScript.enabled = false;
        if (crossHair != null) crossHair.SetActive(false);
        if (inventoryUI != null)
        {
            inventoryUI.HideWeaponUI();
            inventoryUI.HideItemUI();
            inventoryUI.isInputBlocked = true;
        }

        if (inventory != null && inventory.flashlight != null)
        {
            flashlightWasOnBefore = inventory.flashlight.enabled;
            inventory.FlashlightOff();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Vector3 startPos = Camera.main.transform.position;
        Quaternion startRot = Camera.main.transform.rotation;
        Vector3 targetPos = cameraTargetPosition.position;
        Quaternion targetRot = cameraTargetPosition.rotation;

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            Camera.main.transform.position = Vector3.Lerp(startPos, targetPos, elapsed);
            Camera.main.transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsed);
            elapsed += Time.deltaTime * cameraMoveSpeed;
            yield return null;
        }
        Camera.main.transform.position = targetPos;
        Camera.main.transform.rotation = targetRot;

        isCameraMoving = false;
    }

    public void OnLocationSelected(MissionLocationIcon icon)
    {
        if (isCameraMoving)
        {
            return;
        }

        pendingLocationName = icon.locationName;
        pendingRoomCount = icon.roomCount;
        pendingLocationType = icon.locationType;
        pendingTotalDistanceKm = icon.totalDistanceKm;
        pendingDangerZoneKm = icon.dangerZoneKm;
        pendingLootLevel = icon.lootLevel;

        ShowSummary(pendingLocationName, pendingRoomCount, pendingLocationType, pendingTotalDistanceKm, pendingDangerZoneKm);

        if (confirmButton != null)
        {
            if (pendingLocationType == MissionLocationType.RouteOnly)
                confirmButton.interactable = true;
            else
                confirmButton.interactable = pendingRoomCount > 0;
        }

        ShowMissionDefinerButtons();
    }

    private void ShowSummary(string locationName, int roomCount, MissionLocationType locationType, float totalDistanceKm, float dangerZoneKm)
    {
        if (summaryTypeText != null)
            summaryTypeText.text = "Typ misji: " + (locationType == MissionLocationType.ProceduralRaid ? "RAID" : "GRIND BUS");
        if (summaryNameText != null)
            summaryNameText.text = locationName;
        if (summaryRoomsText != null)
            summaryRoomsText.text = roomCount > 0 ? $"Liczba pokoi: {roomCount}" : "";
        if (summaryDistanceText != null)
            summaryDistanceText.text = $"Dystans: {totalDistanceKm:0.0} km";
        if (summaryDangerZoneText != null)
            summaryDangerZoneText.text = $"Danger zone: {dangerZoneKm:0.0} km";
        ShowLootLevel(pendingLootLevel, locationType);
    }

    private void ShowLootLevel(int lootLevel, MissionLocationType locationType)
    {
        // Loot level TYLKO dla ProceduralRaid
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

    private void ClearSummary()
    {
        if (summaryTypeText != null) summaryTypeText.text = "";
        if (summaryNameText != null) summaryNameText.text = "";
        if (summaryRoomsText != null) summaryRoomsText.text = "";
        if (summaryDistanceText != null) summaryDistanceText.text = "";
        if (summaryDangerZoneText != null) summaryDangerZoneText.text = "";
        if (summaryLootLevelContainer != null)
            summaryLootLevelContainer.gameObject.SetActive(false);
    }

    public void OnConfirmClicked()
    {
        if (isCameraMoving) return;

        if (!string.IsNullOrEmpty(pendingLocationName))
        {
            MissionSettings.locationName = pendingLocationName;
            MissionSettings.roomCount = pendingRoomCount;
            MissionSettings.locationType = pendingLocationType;
            MissionSettings.totalDistanceKm = pendingTotalDistanceKm;
            MissionSettings.dangerZoneKm = pendingDangerZoneKm;
            MissionSettings.lootLevel = pendingLootLevel;

            if (MissionMonitor.Instance != null)
                MissionMonitor.Instance.SetSummary(pendingLocationName, pendingRoomCount, pendingLocationType, pendingTotalDistanceKm, pendingDangerZoneKm);

            HideMissionDefinerButtons();
            tooltipPanel?.HideTooltip();
            ExitDefiner();
        }
    }

    public void OnExitClicked()
    {
        if (isCameraMoving) return;

        pendingLocationName = null;
        pendingRoomCount = 0;
        pendingLocationType = MissionLocationType.ProceduralRaid;
        pendingTotalDistanceKm = 0f;
        pendingDangerZoneKm = 0f;
        pendingLootLevel = 1;

        ClearSummary();

        if (MissionMonitor.Instance != null)
            MissionMonitor.Instance.ClearSummary();

        HideMissionDefinerButtons();
        if (tooltipPanel != null)
            tooltipPanel.HideTooltip();

        ExitDefiner();
    }

    private void ShowMissionDefinerButtons()
    {
        if (confirmButton != null) confirmButton.gameObject.SetActive(true);
        if (exitButton != null) exitButton.gameObject.SetActive(true);
    }

    private void HideMissionDefinerButtons()
    {
        if (confirmButton != null) confirmButton.gameObject.SetActive(false);
        if (exitButton != null) exitButton.gameObject.SetActive(false);
    }

    public void ExitDefiner()
    {
        StartCoroutine(MoveCameraBack());
    }

    IEnumerator MoveCameraBack()
    {
        isCameraMoving = true;
        Vector3 startPos = Camera.main.transform.position;
        Quaternion startRot = Camera.main.transform.rotation;

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            Camera.main.transform.position = Vector3.Lerp(startPos, originalCameraPosition, elapsed);
            Camera.main.transform.rotation = Quaternion.Slerp(startRot, originalCameraRotation, elapsed);
            elapsed += Time.deltaTime * cameraMoveSpeed;
            yield return null;
        }
        Camera.main.transform.position = originalCameraPosition;
        Camera.main.transform.rotation = originalCameraRotation;

        if (playerMovementScript != null) playerMovementScript.enabled = true;
        if (mouseLookScript != null) mouseLookScript.enabled = true;
        if (crossHair != null) crossHair.SetActive(true);
        if (inventoryUI != null)
        {
            inventoryUI.isInputBlocked = false;
            inventoryUI.ShowWeaponUI();
            if (inventory != null)
                inventoryUI.ShowItemUI(inventory.items);
        }

        bool holdingLoot = false;
        if (inventory != null && inventory.lootParent != null && inventory.lootParent.childCount > 0)
            holdingLoot = true;

        if (inventory != null && inventory.currentWeaponPrefab != null && weaponWasActiveBefore && !holdingLoot)
        {
            inventory.currentWeaponPrefab.SetActive(true);
        }

        if (inventory != null && inventory.flashlight != null)
        {
            if (flashlightWasOnBefore)
                inventory.FlashlightOn();
            else
                inventory.FlashlightOff();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isCameraMoving = false;
        isInteracting = false;
        IsAnyDefinerActive = false;
    }

    void Update()
    {
        if (isInteracting && Input.GetKeyDown(KeyCode.Escape) && !isCameraMoving)
        {
            OnExitClicked();
        }
    }
}