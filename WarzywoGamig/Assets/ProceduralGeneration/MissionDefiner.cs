using UnityEngine;
using TMPro;
using System.Collections;

public class MissionDefiner : MonoBehaviour
{
    [Header("Cel kamery podczas interakcji")]
    public Transform cameraTargetPosition;
    public float cameraMoveSpeed = 4f;

    [Header("UI Kanwy")]
    public GameObject missionCanvas;    // Canvas z ikonami lokacji
    public GameObject summaryCanvas;    // Canvas z nazw¹ i liczb¹ pokoi
    public TMP_Text summaryNameText;    // TMP_Text na nazwê lokacji
    public TMP_Text summaryRoomsText;   // TMP_Text na liczbê pokoi

    // Publiczne referencje, dynamicznie przypisywane jeœli puste
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

    public static bool IsAnyDefinerActive = false;

    void Start()
    {
        // Dynamiczne przypisanie jeœli puste
        if (playerMovementScript == null)
            playerMovementScript = Object.FindFirstObjectByType<PlayerMovement>();
        if (mouseLookScript == null)
            mouseLookScript = Object.FindFirstObjectByType<MouseLook>();
        if (playerInteraction == null)
            playerInteraction = Object.FindFirstObjectByType<PlayerInteraction>();
        if (inventoryUI == null)
            inventoryUI = Object.FindFirstObjectByType<InventoryUI>();
        if (inventory == null)
            inventory = Object.FindFirstObjectByType<Inventory>();
        if (crossHair == null)
            crossHair = GameObject.FindWithTag("Crosshair") ?? GameObject.Find("Crosshair");

        // NIE ukrywaj Canvasów!
    }

    public void UseDefiner()
    {
        if (!isInteracting)
        {
            isInteracting = true;
            IsAnyDefinerActive = true;
            originalCameraPosition = Camera.main.transform.position;
            originalCameraRotation = Camera.main.transform.rotation;

            // SCHOWAJ broñ (jeœli trzymasz)
            if (inventory != null && inventory.currentWeaponPrefab != null)
            {
                weaponWasActiveBefore = inventory.currentWeaponPrefab.activeSelf;
                inventory.currentWeaponPrefab.SetActive(false);
            }
            else
            {
                weaponWasActiveBefore = false;
            }

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

        // NIE ukrywaj missionCanvas ani summaryCanvas!

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

    public void ExitDefiner()
    {
        if (isInteracting)
        {
            StartCoroutine(MoveCameraBack());
        }
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

        // ODS£OÑ broñ jeœli by³a aktywna wczeœniej i NIE trzymasz lootu
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

        // NIE ukrywaj missionCanvas ani summaryCanvas!

        isCameraMoving = false;
        isInteracting = false;
        IsAnyDefinerActive = false;
    }

    void Update()
    {
        if (isInteracting && Input.GetKeyDown(KeyCode.Escape))
        {
            ExitDefiner();
        }
    }

    // --- WYBÓR LOKACJI ---

    public void OnLocationSelected(MissionLocationIcon icon)
    {
        MissionSettings.locationName = icon.locationName;
        MissionSettings.roomCount = icon.roomCount;
        ShowSummary(icon.locationName, icon.roomCount);

        // tutaj zmieniasz scenê swoim systemem
    }

    private void ShowSummary(string locationName, int roomCount)
    {
        // NIE ukrywaj/pokazuj canvasów!
        if (summaryNameText != null) summaryNameText.text = locationName;
        if (summaryRoomsText != null) summaryRoomsText.text = $"Liczba pokoi: {roomCount}";
    }
}