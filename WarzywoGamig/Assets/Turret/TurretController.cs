using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    public PlayerMovement playerMovement; // Skrypt odpowiadający za poruszanie gracza
    public HarpoonController harpoonController;  // Referencja do skryptu HarpoonController
    public Inventory inventory;  // Skrypt odpowiadający za inwentaryzację

    public Transform enterArea;   // Punkt, do którego teleportuje się gracz
    public Transform exitArea;    // Punkt, z którego teleportuje się gracz po zakończeniu
    public Transform turretBase;  // Transform wieżyczki (część, która będzie się unosić)
    public Transform barrelPivot; // Nowy obiekt pivotu
    public Camera playerCamera; // Kamera gracza, używana do wykrywania kursora
    public GameObject weapon; // Obiekt broni na wieżyczce
    public GameObject harpoonGunPrefab; // Prefab HarpoonGun
    public float raiseHeight = 5f; // Wysokość, na którą wieżyczka ma się podnieść
    public float raiseSpeed = 5f; // Prędkość podnoszenia wieżyczki (zwiększona dla płynności)
    public float lowerSpeed = 5f; // Prędkość opuszczania wieżyczki (zwiększona dla płynności)
    public float rotationResetSpeed = 3f; // Prędkość resetowania rotacji
    public float minBarrelAngle = -30f; // Minimalny kąt pochylenia lufy
    public float maxBarrelAngle = 30f;  // Maksymalny kąt pochylenia lufy
    private bool isTurretLocked;
    public float entryRotateTime = 0.7f; // czas łagodnego obrotu przy wejściu
    public float entryMoveTime = 0.7f; // czas łagodnego przesuwu i obrotu przy wejściu

    private Quaternion initialEnterAreaRotation; // Początkowa rotacja enterArea

    public bool isRaised = false;   // Flaga, która informuje, czy wieżyczka jest uniesiona
    public bool isLowering = false; // Flaga informująca, czy wieżyczka jest w trakcie opuszczania
    public bool isUsingTurret = false; // Flaga, która informuje, czy gracz korzysta z wieżyczki
    private bool isCooldown = false; // Flaga kontrolująca opóźnienie przy opuszczaniu
    private bool flashlightWasOnBeforeTurret = false;

    public static TurretController Instance;

    // Lista wszystkich obiektów, które posiadają PlaySoundOnObject
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            //Debug.Log("TurretController initialized.");
        }
        else
        {
            //Debug.LogWarning("Another instance of TurretController found. Destroying this instance.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Znajdź wszystkie obiekty posiadające PlaySoundOnObject i dodaj do listy
        playSoundObjects.AddRange(Object.FindObjectsByType<PlaySoundOnObject>(FindObjectsSortMode.None));

        playerMovement = Object.FindFirstObjectByType<PlayerMovement>();
        inventory = Object.FindFirstObjectByType<Inventory>();

        if (playerMovement == null || inventory == null)
        {
            Debug.LogError("Brak komponentów PlayerMovement lub Inventory w scenie.");
        }

        if (enterArea != null)
        {
            initialEnterAreaRotation = enterArea.rotation;
        }

        if (harpoonGunPrefab != null && weapon != null)
        {
            GameObject harpoonGun = Instantiate(harpoonGunPrefab, weapon.transform);

            // Teraz przypisujemy referencję do HarpoonController
            harpoonController = harpoonGun.GetComponent<HarpoonController>();

            if (harpoonController == null)
            {
                Debug.LogError("Nie znaleziono skryptu HarpoonController w prefabie.");
            }
        }

        if (harpoonController != null)
            harpoonController.SetHarpoonLight(false);
        if (harpoonController != null)
            harpoonController.SetCabinLight(false);
    }


    void Update()
    {
        if (isUsingTurret && !isTurretLocked)
        {
            if (Input.GetKeyDown(KeyCode.Q) && isRaised && !isCooldown && !harpoonController.isReturning && harpoonController.canShoot)
            {
                StartCoroutine(LowerTurret());

                
            }

            RotateEnterAreaWithPlayer();
            //RotateBarrelWithMouse();
        }
    }

    private void RotateEnterAreaWithPlayer()
    {
        if (playerMovement != null && enterArea != null && playerCamera != null)
        {
            // Pobieramy kąt X z kamery gracza, który chcemy ograniczyć
            float cameraAngleX = NormalizeAngle(playerCamera.transform.localEulerAngles.x);
            // Ograniczamy kąt w pionie (X) w zakresie podanym przez minBarrelAngle i maxBarrelAngle
            float clampedAngleX = Mathf.Clamp(cameraAngleX, minBarrelAngle, maxBarrelAngle);

            // Pobieramy kąt Y z gracza (rotacja w poziomie)
            float yRotation = playerMovement.transform.eulerAngles.y;

            // Obracamy enterArea i gracza w osi X oraz Y
            enterArea.rotation = Quaternion.Euler(clampedAngleX, yRotation, 0);
            playerMovement.transform.rotation = Quaternion.Euler(clampedAngleX, yRotation, 0); // Obrót gracza
        }
    }

    // Zamiana kąta na zakres -180° do 180°
    private float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    public void UseTurret()
    {
        if (!isUsingTurret)
        {
            if (inventory != null && inventory.flashlight != null)
            {
                flashlightWasOnBeforeTurret = inventory.flashlight.enabled;
                inventory.FlashlightOff();
            }

            if (harpoonController != null)
                harpoonController.SetCabinLight(true);

            StartCoroutine(SmoothMoveAndAlignPlayerToSeatAndRaiseTurret());

            ActivateWeapon(); // Możesz zostawić tutaj lub przenieść do korutyny, jeśli broń ma się pojawić dopiero po wejściu

            isUsingTurret = true;
        }
    }

    private void TeleportPlayer(Transform targetArea)
    {
        if (playerMovement != null && targetArea != null)
        {
            var controller = playerMovement.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;

            playerMovement.transform.position = targetArea.position;
            playerMovement.transform.rotation = targetArea.rotation;

            if (controller != null) controller.enabled = true;
        }
    }

    private IEnumerator SmoothMoveAndAlignPlayerToSeatAndRaiseTurret()
    {
        // Zablokuj sterowanie w trakcie animacji wejścia
        isTurretLocked = true;

        Vector3 startPos = playerMovement.transform.position;
        Vector3 targetPos = enterArea.position;

        Quaternion startRot = playerMovement.transform.rotation;
        Quaternion targetRot = enterArea.rotation;

        Quaternion weaponStartRot = weapon.transform.rotation;
        Quaternion weaponTargetRot = enterArea.rotation;

        float elapsed = 0f;
        while (elapsed < entryMoveTime)
        {
            float t = elapsed / entryMoveTime;
            // Możesz użyć SmoothStep zamiast liniowego Lerp dla ładniejszego efektu
            float smoothT = Mathf.SmoothStep(0, 1, t);

            playerMovement.transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
            playerMovement.transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);
            weapon.transform.rotation = Quaternion.Slerp(weaponStartRot, weaponTargetRot, smoothT);

            elapsed += Time.deltaTime;
            yield return null;
        }
        playerMovement.transform.position = targetPos;
        playerMovement.transform.rotation = targetRot;
        weapon.transform.rotation = weaponTargetRot;

        if (harpoonController != null)
            harpoonController.ResetReloadState();

        if (playerMovement != null)
            playerMovement.enabled = false;

        isTurretLocked = false;

        yield return StartCoroutine(RaiseTurret());
    }

    private IEnumerator RaiseTurret()
    {
        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;
            playSoundOnObject.PlaySound("TurretUp", 0.5f, false);
        }

        if (harpoonController != null)
            harpoonController.SetHarpoonLight(false);

        float targetHeight = turretBase.position.y + raiseHeight;
        float targetEnterAreaHeight = enterArea.position.y + raiseHeight;

        Vector3 targetTurretBasePosition = new Vector3(turretBase.position.x, targetHeight, turretBase.position.z);
        Vector3 targetEnterAreaPosition = new Vector3(enterArea.position.x, targetEnterAreaHeight, enterArea.position.z);

        float epsilon = 0.01f;
        while (Mathf.Abs(turretBase.position.y - targetHeight) > epsilon)
        {
            turretBase.position = Vector3.MoveTowards(turretBase.position, targetTurretBasePosition, raiseSpeed * Time.deltaTime);
            enterArea.position = Vector3.MoveTowards(enterArea.position, targetEnterAreaPosition, raiseSpeed * Time.deltaTime);

            if (playerMovement != null)
                playerMovement.transform.position = enterArea.position;

            yield return null;
        }

        turretBase.position = targetTurretBasePosition;
        enterArea.position = targetEnterAreaPosition;
        if (playerMovement != null)
            playerMovement.transform.position = enterArea.position;

        isRaised = true;

        if (harpoonController != null)
            harpoonController.SetHarpoonLight(true);
    }

    private void ActivateWeapon()
    {
        if (weapon != null)
        {
            weapon.SetActive(true);
        }
    }

    private IEnumerator LowerTurret()
    {
        // Przygotowanie do interpolacji rotacji
        float elapsedTime = 0f;
        Quaternion startRotation = enterArea.rotation;

        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;
            playSoundOnObject.PlaySound("TurretDown", 0.5f, false);
        }

        if (harpoonController != null)
            harpoonController.SetHarpoonLight(false);
        if (harpoonController != null)
            harpoonController.SetCabinLight(false);

        isLowering = true;
        isCooldown = true;
        float targetHeight = turretBase.position.y - raiseHeight;
        float targetEnterAreaHeight = enterArea.position.y - raiseHeight;

        Vector3 targetTurretBasePosition = new Vector3(turretBase.position.x, targetHeight, turretBase.position.z);
        Vector3 targetEnterAreaPosition = new Vector3(enterArea.position.x, targetEnterAreaHeight, enterArea.position.z);

        Quaternion initialBarrelPivotRotation = barrelPivot.localRotation;

        while (turretBase.position.y > targetHeight)
        {
            turretBase.position = Vector3.MoveTowards(turretBase.position, targetTurretBasePosition, lowerSpeed * Time.deltaTime);
            enterArea.position = Vector3.MoveTowards(enterArea.position, targetEnterAreaPosition, lowerSpeed * Time.deltaTime);
            playerMovement.transform.position = Vector3.Lerp(playerMovement.transform.position, enterArea.position, 0.9f);

            // Płynny reset rotacji działka
            if (elapsedTime < 1f)
            {
                enterArea.rotation = Quaternion.Lerp(startRotation, initialEnterAreaRotation, elapsedTime);
                elapsedTime += Time.deltaTime * rotationResetSpeed;
            }
            else
            {
                enterArea.rotation = initialEnterAreaRotation;
            }

            // Synchronizacja rotacji gracza z kabiną
            playerMovement.transform.rotation = enterArea.rotation;

            yield return null;
        }

        // Upewnij się, że finalna rotacja jest dokładnie docelowa
        enterArea.rotation = initialEnterAreaRotation;
        turretBase.position = targetTurretBasePosition;
        enterArea.position = targetEnterAreaPosition;

        Quaternion currentRotation = playerMovement.transform.rotation;
        playerMovement.transform.rotation = Quaternion.Euler(0f, currentRotation.eulerAngles.y, 0f);

        TeleportPlayer(exitArea);

        if (playerMovement != null)
            playerMovement.enabled = true;

        PlayerInteraction player = UnityEngine.Object.FindFirstObjectByType<PlayerInteraction>();
        if (player != null)
            player.ReactivateInventoryAndUI();

        if (harpoonController != null)
        {
            harpoonController.ResetReloadState();
        }

        barrelPivot.localRotation = initialBarrelPivotRotation;

        isRaised = false;
        isCooldown = false;
        isUsingTurret = false;
        isLowering = false;

        if (inventory != null && inventory.flashlight != null)
        {
            if (flashlightWasOnBeforeTurret)
                inventory.FlashlightOn();
            else
                inventory.FlashlightOff();
        }
    }
}