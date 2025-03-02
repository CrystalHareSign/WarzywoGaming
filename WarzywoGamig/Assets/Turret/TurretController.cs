using System.Collections;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    public Transform enterArea;   // Punkt, do którego teleportuje siê gracz
    public Transform exitArea;    // Punkt, z którego teleportuje siê gracz po zakoñczeniu
    public Transform turretBase;  // Transform wie¿yczki (czêœæ, która bêdzie siê unosiæ)
    public float raiseHeight = 5f; // Wysokoœæ, na któr¹ wie¿yczka ma siê podnieœæ
    public float raiseSpeed = 2f; // Prêdkoœæ podnoszenia wie¿yczki
    public float lowerSpeed = 2f; // Prêdkoœæ opuszczania wie¿yczki
    public GameObject turretGun;  // Obiekt dzia³ka wie¿yczki
    public PlayerMovement playerMovement; // Skrypt odpowiadaj¹cy za poruszanie gracza
    public Inventory playerInventory;  // Skrypt odpowiadaj¹cy za inwentaryzacjê

    private bool isRaised = false;   // Flaga, która informuje, czy wie¿yczka jest uniesiona
    private bool isUsingTurret = false; // Flaga, która informuje, czy gracz korzysta z wie¿yczki
    private bool isCooldown = false; // Flaga kontroluj¹ca opóŸnienie przy opuszczaniu

    void Start()
    {
        // Znajdujemy skrypt PlayerMovement w scenie
        playerMovement = Object.FindFirstObjectByType<PlayerMovement>();
        playerInventory = Object.FindFirstObjectByType<Inventory>();  // Przypisujemy skrypt Inventory

        if (playerMovement == null || playerInventory == null)
        {
            Debug.LogError("Brak komponentów PlayerMovement lub Inventory w scenie.");
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
        }
    }

    public void UseTurret()
    {
        if (!isUsingTurret)
        {
            Debug.Log("Aktywujê wie¿yczkê.");

            // Teleportacja gracza do EnterArea
            TeleportPlayer(enterArea);

            // Wy³¹czenie skryptu poruszania gracza
            if (playerMovement != null)
            {
                Debug.Log("Wy³¹czam skrypt PlayerMovement.");
                playerMovement.enabled = false;
            }

            // Wy³¹czenie Inwentaryzacji
            if (playerInventory != null)
            {
                Debug.Log("Wy³¹czam inwentaryzacjê.");
                playerInventory.enabled = false;  // Wy³¹czamy skrypt inwentaryzacji
            }

            // Podniesienie wie¿yczki
            StartCoroutine(RaiseTurret());

            // Aktywowanie dzia³ania dzia³ka (np. za pomoc¹ skryptu do strzelania)
            ActivateTurretGun();

            isUsingTurret = true;
        }
    }

    private void TeleportPlayer(Transform targetArea)
    {
        if (targetArea != null)
        {
            // Teleportacja gracza do wyznaczonego obszaru
            playerMovement.transform.position = targetArea.position;
        }
        else
        {
            Debug.LogWarning("[WARNING] EnterArea jest niezdefiniowane.");
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
            playerMovement.transform.position = Vector3.MoveTowards(playerMovement.transform.position, enterArea.position, raiseSpeed * Time.deltaTime);
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
            turretGun.SetActive(true); // Za³ó¿my, ¿e po podniesieniu wie¿yczki w³¹czamy dzia³ko
        }
        else
        {
            Debug.LogWarning("[WARNING] Dzia³ko nie zosta³o przypisane.");
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
            playerMovement.transform.position = Vector3.MoveTowards(playerMovement.transform.position, enterArea.position, lowerSpeed * Time.deltaTime);
            yield return null;
        }

        turretBase.position = targetTurretBasePosition;
        enterArea.position = targetEnterAreaPosition;

        TeleportPlayer(exitArea);

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // Ponowne w³¹czenie Inwentaryzacji
        if (playerInventory != null)
        {
            Debug.Log("W³¹czam inwentaryzacjê.");
            playerInventory.enabled = true;  // W³¹czamy z powrotem inwentaryzacjê
        }

        DeactivateTurretGun();

        isRaised = false;
        isCooldown = false;
        isUsingTurret = false;
    }

    private void DeactivateTurretGun()
    {
        if (turretGun != null)
        {
            turretGun.SetActive(false); // Wy³¹czamy dzia³ko po opuszczeniu wie¿yczki
        }
    }
}
