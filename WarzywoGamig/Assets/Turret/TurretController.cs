using System.Collections;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    public Transform enterArea;   // Punkt, do którego teleportuje się gracz
    public Transform exitArea;    // Punkt, z którego teleportuje się gracz po zakończeniu
    public Transform turretBase;  // Transform wieżyczki (część, która będzie się unosić)
    public float raiseHeight = 5f; // Wysokość, na którą wieżyczka ma się podnieść
    public float raiseSpeed = 5f; // Prędkość podnoszenia wieżyczki (zwiększona dla płynności)
    public float lowerSpeed = 5f; // Prędkość opuszczania wieżyczki (zwiększona dla płynności)
    public GameObject turretGun;  // Obiekt działka wieżyczki
    public PlayerMovement playerMovement; // Skrypt odpowiadający za poruszanie gracza
    public Inventory inventory;  // Skrypt odpowiadający za inwentaryzację

    public float rotationResetSpeed = 3f; // Prędkość resetowania rotacji
    private Quaternion initialEnterAreaRotation; // Początkowa rotacja enterArea

    private bool isRaised = false;   // Flaga, która informuje, czy wieżyczka jest uniesiona
    private bool isUsingTurret = false; // Flaga, która informuje, czy gracz korzysta z wieżyczki
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
    }

    void Update()
    {
        if (isUsingTurret)
        {
            if (Input.GetKeyDown(KeyCode.Q) && isRaised && !isCooldown)
            {
                StartCoroutine(LowerTurret());
            }

            RotateEnterAreaWithPlayer();
        }
    }

    private void RotateEnterAreaWithPlayer()
    {
        if (playerMovement != null && enterArea != null)
        {
            float playerRotationY = playerMovement.transform.rotation.eulerAngles.y;
            enterArea.rotation = Quaternion.Lerp(enterArea.rotation, Quaternion.Euler(0, playerRotationY, 0), Time.deltaTime * 5f); // Płynniejsza interpolacja
        }
    }

    public void UseTurret()
    {
        if (!isUsingTurret)
        {
            Debug.Log("Aktywuję wieżyczkę.");

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

            ActivateTurretGun();

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

    private void ActivateTurretGun()
    {
        if (turretGun != null)
        {
            turretGun.SetActive(true);
        }
    }

    private IEnumerator LowerTurret()
    {
        isCooldown = true;
        float targetHeight = turretBase.position.y - raiseHeight;
        float targetEnterAreaHeight = enterArea.position.y - raiseHeight;

        Vector3 targetTurretBasePosition = new Vector3(turretBase.position.x, targetHeight, turretBase.position.z);
        Vector3 targetEnterAreaPosition = new Vector3(enterArea.position.x, targetEnterAreaHeight, enterArea.position.z);

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

        DeactivateTurretGun();

        isRaised = false;
        isCooldown = false;
        isUsingTurret = false;
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

    private void DeactivateTurretGun()
    {
        if (turretGun != null)
        {
            turretGun.SetActive(false);
        }
    }
}
