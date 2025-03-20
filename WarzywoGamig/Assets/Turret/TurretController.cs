using System.Collections;
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

    private Quaternion initialEnterAreaRotation; // Początkowa rotacja enterArea

    public bool isRaised = false;   // Flaga, która informuje, czy wieżyczka jest uniesiona
    public bool isLowering = false; // Flaga informująca, czy wieżyczka jest w trakcie opuszczania
    public bool isUsingTurret = false; // Flaga, która informuje, czy gracz korzysta z wieżyczki
    private bool isCooldown = false; // Flaga kontrolująca opóźnienie przy opuszczaniu

    void Start()
    {
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
    }


    void Update()
    {
        if (isUsingTurret)
        {
            if (Input.GetKeyDown(KeyCode.Q) && isRaised && !isCooldown && !harpoonController.isReturning && harpoonController.canShoot)
            {
                StartCoroutine(LowerTurret());
            }

            RotateEnterAreaWithPlayer();
            RotateBarrelWithMouse();
        }
    }

    private void RotateEnterAreaWithPlayer()
    {
        if (playerMovement != null && enterArea != null)
        {
            enterArea.rotation = Quaternion.Euler(0, playerMovement.transform.eulerAngles.y, 0);
        }
    }

    private void RotateBarrelWithMouse()
    {
        if (barrelPivot == null || playerCamera == null)
            return;

        // Pobieramy kąt X kamery gracza
        float cameraAngleX = NormalizeAngle(playerCamera.transform.localEulerAngles.x);

        // Ograniczamy kąt do podanego zakresu
        float clampedAngle = Mathf.Clamp(cameraAngleX, minBarrelAngle, maxBarrelAngle);

        // Ustawiamy nowy kąt dla lufy (obrót tylko w osi X)
        barrelPivot.localRotation = Quaternion.Euler(clampedAngle, 0, 0);

        // Sprawdzamy, czy kąt jest w dozwolonym zakresie
        if (clampedAngle == minBarrelAngle || clampedAngle == maxBarrelAngle)
        {
            // Jeśli kąt lufy przekracza zakres, blokujemy strzelanie
            if (harpoonController != null)
            {
                harpoonController.canShoot = false;
            }
        }
        else
        {
            // Jeśli kąt jest w dozwolonym zakresie, umożliwiamy strzelanie
            if (harpoonController != null)
            {
                harpoonController.canShoot = true;
            }
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
            //Debug.Log("Aktywuję wieżyczkę.");

            TeleportPlayer(enterArea);

            if (playerMovement != null)
            {
                playerMovement.enabled = false;
            }

            if (inventory != null && inventory.currentWeaponPrefab != null)
            {
                inventory.currentWeaponPrefab.SetActive(false);
            }

            if (inventory != null)
            {
                inventory.enabled = false;
            }

            StartCoroutine(RaiseTurret());

            ActivateWeapon();

            isUsingTurret = true;
        }
    }

    private void TeleportPlayer(Transform targetArea)
    {
        if (targetArea != null)
        {
            playerMovement.transform.position = targetArea.position;
        }
    }

    private IEnumerator RaiseTurret()
    {
        float targetHeight = turretBase.position.y + raiseHeight;
        float targetEnterAreaHeight = enterArea.position.y + raiseHeight;

        Vector3 targetTurretBasePosition = new Vector3(turretBase.position.x, targetHeight, turretBase.position.z);
        Vector3 targetEnterAreaPosition = new Vector3(enterArea.position.x, targetEnterAreaHeight, enterArea.position.z);

        while (turretBase.position.y < targetHeight)
        {
            turretBase.position = Vector3.MoveTowards(turretBase.position, targetTurretBasePosition, raiseSpeed * Time.deltaTime);
            enterArea.position = Vector3.MoveTowards(enterArea.position, targetEnterAreaPosition, raiseSpeed * Time.deltaTime);
            playerMovement.transform.position = Vector3.Lerp(playerMovement.transform.position, enterArea.position, 0.9f); // Płynniejszy ruch gracza
            yield return null;
        }

        turretBase.position = targetTurretBasePosition;
        enterArea.position = targetEnterAreaPosition;

        isRaised = true;
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
        isLowering = true; // Rozpocznij opuszczanie
        isCooldown = true;
        float targetHeight = turretBase.position.y - raiseHeight;
        float targetEnterAreaHeight = enterArea.position.y - raiseHeight;

        Vector3 targetTurretBasePosition = new Vector3(turretBase.position.x, targetHeight, turretBase.position.z);
        Vector3 targetEnterAreaPosition = new Vector3(enterArea.position.x, targetEnterAreaHeight, enterArea.position.z);

        // Zapisz początkową rotację pivotu
        Quaternion initialBarrelPivotRotation = barrelPivot.localRotation;

        while (turretBase.position.y > targetHeight)
        {
            turretBase.position = Vector3.MoveTowards(turretBase.position, targetTurretBasePosition, lowerSpeed * Time.deltaTime);
            enterArea.position = Vector3.MoveTowards(enterArea.position, targetEnterAreaPosition, lowerSpeed * Time.deltaTime);
            playerMovement.transform.position = Vector3.Lerp(playerMovement.transform.position, enterArea.position, 0.9f); // Lepsza synchronizacja gracza z enterArea
            yield return null;
        }

        turretBase.position = targetTurretBasePosition;
        enterArea.position = targetEnterAreaPosition;

        TeleportPlayer(exitArea);

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        if (enterArea != null)
        {
            StartCoroutine(ResetEnterAreaRotation());
        }

        if (inventory != null)
        {
            inventory.enabled = true;
        }

        if (inventory != null && inventory.currentWeaponPrefab != null)
        {
            inventory.currentWeaponPrefab.SetActive(true);
        }

        // Resetowanie rotacji barrelPivot po zakończeniu
        barrelPivot.localRotation = initialBarrelPivotRotation;

        isRaised = false;
        isCooldown = false;
        isUsingTurret = false;
        isLowering = false;
    }

    private IEnumerator ResetEnterAreaRotation()
    {
        float elapsedTime = 0f;
        Quaternion startRotation = enterArea.rotation;

        while (elapsedTime < 1f)
        {
            enterArea.rotation = Quaternion.Lerp(startRotation, initialEnterAreaRotation, elapsedTime);
            elapsedTime += Time.deltaTime * rotationResetSpeed;
            yield return null;
        }

        enterArea.rotation = initialEnterAreaRotation;
    }

    private void DeactivateWeapon()
    {
        if (weapon != null)
        {
            weapon.SetActive(false);
        }
    }
}