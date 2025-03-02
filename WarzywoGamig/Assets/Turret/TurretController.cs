using System.Collections;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    public Transform enterArea;   // Punkt, do kt�rego teleportuje si� gracz
    public Transform exitArea;    // Punkt, z kt�rego teleportuje si� gracz po zako�czeniu
    public Transform turretBase;  // Transform wie�yczki (cz��, kt�ra b�dzie si� unosi�)
    public float raiseHeight = 5f; // Wysoko��, na kt�r� wie�yczka ma si� podnie��
    public float raiseSpeed = 2f; // Pr�dko�� podnoszenia wie�yczki
    public float lowerSpeed = 2f; // Pr�dko�� opuszczania wie�yczki
    public GameObject turretGun;  // Obiekt dzia�ka wie�yczki
    public PlayerMovement playerMovement; // Skrypt odpowiadaj�cy za poruszanie gracza
    public Inventory playerInventory;  // Skrypt odpowiadaj�cy za inwentaryzacj�

    private bool isRaised = false;   // Flaga, kt�ra informuje, czy wie�yczka jest uniesiona
    private bool isUsingTurret = false; // Flaga, kt�ra informuje, czy gracz korzysta z wie�yczki
    private bool isCooldown = false; // Flaga kontroluj�ca op�nienie przy opuszczaniu

    void Start()
    {
        // Znajdujemy skrypt PlayerMovement w scenie
        playerMovement = Object.FindFirstObjectByType<PlayerMovement>();
        playerInventory = Object.FindFirstObjectByType<Inventory>();  // Przypisujemy skrypt Inventory

        if (playerMovement == null || playerInventory == null)
        {
            Debug.LogError("Brak komponent�w PlayerMovement lub Inventory w scenie.");
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
            Debug.Log("Aktywuj� wie�yczk�.");

            // Teleportacja gracza do EnterArea
            TeleportPlayer(enterArea);

            // Wy��czenie skryptu poruszania gracza
            if (playerMovement != null)
            {
                Debug.Log("Wy��czam skrypt PlayerMovement.");
                playerMovement.enabled = false;
            }

            // Wy��czenie Inwentaryzacji
            if (playerInventory != null)
            {
                Debug.Log("Wy��czam inwentaryzacj�.");
                playerInventory.enabled = false;  // Wy��czamy skrypt inwentaryzacji
            }

            // Podniesienie wie�yczki
            StartCoroutine(RaiseTurret());

            // Aktywowanie dzia�ania dzia�ka (np. za pomoc� skryptu do strzelania)
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
            turretGun.SetActive(true); // Za��my, �e po podniesieniu wie�yczki w��czamy dzia�ko
        }
        else
        {
            Debug.LogWarning("[WARNING] Dzia�ko nie zosta�o przypisane.");
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

        // Ponowne w��czenie Inwentaryzacji
        if (playerInventory != null)
        {
            Debug.Log("W��czam inwentaryzacj�.");
            playerInventory.enabled = true;  // W��czamy z powrotem inwentaryzacj�
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
            turretGun.SetActive(false); // Wy��czamy dzia�ko po opuszczeniu wie�yczki
        }
    }
}
