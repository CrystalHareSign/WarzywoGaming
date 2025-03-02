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

    private bool isRaised = false;   // Flaga, która informuje, czy wie¿yczka jest uniesiona
    private bool isUsingTurret = false; // Flaga, która informuje, czy gracz korzysta z wie¿yczki
    private bool isCooldown = false; // Flaga kontroluj¹ca opóŸnienie przy opuszczaniu

    void Start()
    {
        // Znajdujemy skrypt PlayerMovement w scenie
        playerMovement = Object.FindFirstObjectByType<PlayerMovement>();

        if (playerMovement == null)
        {
            Debug.LogError("Brak skryptu PlayerMovement w scenie.");
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
    private void ActivateTurretGun()
    {
        // Tutaj aktywujesz dzia³ko, np. poprzez w³¹czenie skryptu strzelania
        if (turretGun != null)
        {
            turretGun.SetActive(true); // Za³ó¿my, ¿e po podniesieniu wie¿yczki w³¹czamy dzia³ko
        }
        else
        {
            Debug.LogWarning("[WARNING] Dzia³ko nie zosta³o przypisane.");
        }
    }
    private IEnumerator RaiseTurret()
    {
        float targetHeight = turretBase.position.y + raiseHeight;
        float targetEnterAreaHeight = enterArea.position.y + raiseHeight;

        // Okreœlamy wektory docelowe
        Vector3 targetTurretBasePosition = new Vector3(turretBase.position.x, targetHeight, turretBase.position.z);
        Vector3 targetEnterAreaPosition = new Vector3(enterArea.position.x, targetEnterAreaHeight, enterArea.position.z);

        while (turretBase.position.y < targetHeight)
        {
            // P³ynne przesuwanie wie¿yczki
            turretBase.position = Vector3.MoveTowards(turretBase.position, targetTurretBasePosition, raiseSpeed * Time.deltaTime);

            // P³ynne przesuwanie EnterArea
            enterArea.position = Vector3.MoveTowards(enterArea.position, targetEnterAreaPosition, raiseSpeed * Time.deltaTime);

            // P³ynne przesuwanie gracza
            playerMovement.transform.position = Vector3.MoveTowards(playerMovement.transform.position, enterArea.position, raiseSpeed * Time.deltaTime);

            yield return null;
        }

        // Ustawienie finalnej pozycji
        turretBase.position = targetTurretBasePosition;
        enterArea.position = targetEnterAreaPosition;

        isRaised = true;
    }

    private IEnumerator LowerTurret()
    {
        isCooldown = true;
        float targetHeight = turretBase.position.y - raiseHeight;
        float targetEnterAreaHeight = enterArea.position.y - raiseHeight;

        // Okreœlamy wektory docelowe
        Vector3 targetTurretBasePosition = new Vector3(turretBase.position.x, targetHeight, turretBase.position.z);
        Vector3 targetEnterAreaPosition = new Vector3(enterArea.position.x, targetEnterAreaHeight, enterArea.position.z);

        while (turretBase.position.y > targetHeight)
        {
            // P³ynne przesuwanie wie¿yczki
            turretBase.position = Vector3.MoveTowards(turretBase.position, targetTurretBasePosition, lowerSpeed * Time.deltaTime);

            // P³ynne przesuwanie EnterArea
            enterArea.position = Vector3.MoveTowards(enterArea.position, targetEnterAreaPosition, lowerSpeed * Time.deltaTime);

            // P³ynne przesuwanie gracza
            playerMovement.transform.position = Vector3.MoveTowards(playerMovement.transform.position, enterArea.position, lowerSpeed * Time.deltaTime);

            yield return null;
        }

        // Ustawienie finalnej pozycji
        turretBase.position = targetTurretBasePosition;
        enterArea.position = targetEnterAreaPosition;

        // Po opuszczeniu wie¿yczki teleportacja gracza
        TeleportPlayer(exitArea);

        // W³¹czenie skryptu poruszania gracza
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // Wy³¹czenie dzia³ania dzia³ka
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
