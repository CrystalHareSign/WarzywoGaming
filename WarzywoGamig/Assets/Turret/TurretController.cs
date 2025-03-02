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

    private IEnumerator RaiseTurret()
    {
        float targetHeight = turretBase.position.y + raiseHeight;
        while (turretBase.position.y < targetHeight)
        {
            turretBase.position += Vector3.up * raiseSpeed * Time.deltaTime;
            yield return null;
        }
        turretBase.position = new Vector3(turretBase.position.x, targetHeight, turretBase.position.z);
        isRaised = true;
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

    private IEnumerator LowerTurret()
    {
        isCooldown = true;
        float targetHeight = turretBase.position.y - raiseHeight;
        while (turretBase.position.y > targetHeight)
        {
            turretBase.position -= Vector3.up * lowerSpeed * Time.deltaTime;
            yield return null;
        }
        turretBase.position = new Vector3(turretBase.position.x, targetHeight, turretBase.position.z);

        // Po opuszczeniu wie¿yczki teleportacja gracza
        TeleportPlayer(exitArea);

        // W³¹czenie z powrotem skryptu poruszania gracza
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
