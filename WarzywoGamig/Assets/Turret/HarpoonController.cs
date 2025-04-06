using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using static TurretCollector;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class HarpoonController : MonoBehaviour
{
    public GameObject harpoonPrefab;
    public Transform firePoint;
    public Transform treasureMountPoint;
    [Header("HARPUN PARAMETRY")]
    public float shootSpeed = 20f;
    public float returnSpeed = 10f;
    public float maxRange = 50f;
    [Tooltip("treasureLifetime =    RT 50%\r\n        fullAnimationTime =   RT 50%\r\n        timeBeforeAnimation = RT 12,5%\r\n        pauseTime =           RT 12,5%\r\n        timeAfterAnimation =  RT 25%\r\n")]
    public float reloadTime = 2f; // Czas przeładowania
    public float treasureLifetime = 1f;
    [Header("! DRGANIE !")]
    public float returnTolerance = 3.0f;
    [Header("WYKRYWANIE")]
    public float maxHorizontalAngleToTarget = 30f;
    public float maxVerticalAngleToTarget = 30f;
    public float detectionRange = 100f;
    public float minDetectionRange = 10f;
    public bool showRangesInScene = true;

    private GameObject currentHarpoon;
    private Rigidbody harpoonRb;
    public bool isReturning = false;
    public bool canShoot = true;
    private Vector3 initialScale = new Vector3(1, 1, 1);
    private float currentShootDistance = 0f;
    private Vector3 shootPosition;

    private TurretController turretController;
    private TurretCollector turretCollector;
    private float reloadTimer = 0f; // Nowa zmienna do liczenia czasu przeładowania
    private bool isReloading;

    [Header("TUBE - Pamietej aby całkowity czas wszyskich etapow musi byc równy ReloadTime")]
    // Dodajemy referencję do obiektu, który ma się poruszać
    public GameObject movingObject;
    private Vector3 initialLocalPosition; // Zmienna do przechowywania początkowej lokalnej pozycji obiektu
    private Quaternion initialLocalRotation; // Zmienna do przechowywania początkowej lokalnej rotacji obiektu
    private bool isMoving = false;

    //private bool isMovingForward = true; // Nowa zmienna do śledzenia, w którą stronę porusza się obiekt

    public float fullAnimationTime = 2f; // Czas trwania całej animacji (ruch do przodu i do tyłu)
    public float timeBeforeAnimation = 1f; // Czas opóźnienia przed rozpoczęciem animacji
    public float pauseTime = 1f; // Czas przerwy pomiędzy ruchem do przodu i do tyłu
    public float timeAfterAnimation = 1f; // Czas, przez który obiekt pozostaje w miejscu po animacji
    public float moveDistance = 5f; // Odległość, na jaką obiekt ma się wysunąć

    [Header("TABLET")]
    public GameObject rotatingObject; // Obiekt, który ma się obracać
    private Quaternion initialRotation; // Początkowa rotacja obiektu
    private Quaternion targetRotation;  // Docelowa rotacja obiektu
    private bool isRotated = false; // Zmienna do śledzenia, czy obiekt jest obrócony wokół osi Z
    private bool isRotating = false; // Zmienna do sprawdzenia, czy obiekt jest w trakcie rotacji
    public float rotationDuration = 1f; // Czas trwania rotacji (można ustawić w inspektorze)

    public Canvas objectCanvas;  // Canvas na obiekcie

    public TextMeshProUGUI[] categoryTexts;  // Tablica dla tekstów kategorii
    public TextMeshProUGUI[] quantityTexts;  // Tablica dla tekstów ilości zasobów

    // Odwołania do pól tekstowych na kanwie
    public TextMeshProUGUI resourceText1;
    public TextMeshProUGUI resourceText2;
    public TextMeshProUGUI resourceText3;
    public TextMeshProUGUI resourceText4;

    // Lista wszystkich obiektów, które posiadają PlaySoundOnObject
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();

    void Start()
    {

        // Znajdź wszystkie obiekty posiadające PlaySoundOnObject i dodaj do listy
        playSoundObjects.AddRange(Object.FindObjectsOfType<PlaySoundOnObject>());

        // Zakładając, że zmienna Reload jest już zdefiniowana
        treasureLifetime = reloadTime * 0.5f;
        fullAnimationTime = reloadTime * 0.5f;
        timeBeforeAnimation = reloadTime * 0.125f;
        pauseTime = reloadTime * 0.125f;
        timeAfterAnimation = reloadTime * 0.25f;

        if (objectCanvas == null)
        {
            objectCanvas = GetComponentInChildren<Canvas>();
        }

        if (objectCanvas != null)
        {
            objectCanvas.gameObject.SetActive(false); // Na początku Canvas jest nieaktywny
        }

        // Znajdź TurretCollector (można również przypisać bezpośrednio w inspektorze)
        turretCollector = Object.FindFirstObjectByType<TurretCollector>();

        // Sprawdź, czy TurretCollector został znaleziony
        if (turretCollector == null)
        {
            Debug.LogError("TurretCollector not found!");
            return;
        }

        UpdateMaxResourceTexts();

        turretController = Object.FindFirstObjectByType<TurretController>();

        if (harpoonPrefab != null && firePoint != null)
        {
            currentHarpoon = Instantiate(harpoonPrefab, firePoint.position, firePoint.rotation);
            currentHarpoon.transform.SetParent(firePoint);
            harpoonRb = currentHarpoon.GetComponent<Rigidbody>();
            harpoonRb.isKinematic = true;
            currentHarpoon.transform.localScale = initialScale;

            Harpoon harpoonScript = currentHarpoon.GetComponent<Harpoon>();
            if (harpoonScript != null)
            {
                harpoonScript.treasureLifetime = treasureLifetime;
            }
        }
        else
        {
            Debug.LogError("harpoonPrefab lub firePoint nie jest przypisany.");
        }

        // Inicjalizacja, ustawienie początkowej lokalnej pozycji i rotacji obiektu
        if (movingObject != null)
        {
            initialLocalPosition = movingObject.transform.localPosition;
            initialLocalRotation = movingObject.transform.localRotation;
        }

        // Inicjalizujemy początkową rotację
        if (rotatingObject != null)
        {
            initialRotation = rotatingObject.transform.localRotation;
            targetRotation = initialRotation * Quaternion.Euler(0f, 90f, 0f); // Obrót o 90 stopni w prawo
        }
    }

    void Update()
    {
        if (turretController != null && turretController.isUsingTurret && turretController.isRaised)
        {
            if (Input.GetMouseButtonDown(0) && canShoot && !(turretController.isLowering) && reloadTimer <= 0f) // Sprawdzamy, czy czas przeładowania minął
            {
                // Zatrzymaj wszystkie odtwarzane dźwięki
                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;

                    playSoundOnObject.PlaySound("HarpoonFire", 0.6f, false);
                    playSoundOnObject.PlaySound("HarpoonChainFire", 0.6f, false);
                }

                GameObject target = FindClosestTreasureInView();
                if (target != null)
                {
                    Vector3 predictedPosition = PredictTargetPosition(target);
                    ShootHarpoon(predictedPosition);
                }
                else
                {
                    Vector3 shootDirection = GetMouseWorldPosition();
                    ShootHarpoon(shootDirection);
                }
            }

            if (Input.GetMouseButtonDown(1)) // Prawy przycisk myszy (PPM)
            {
                if (Input.GetMouseButtonDown(1) && !isRotating) // Prawy przycisk myszy (PPM) oraz sprawdzamy, czy rotacja się nie odbywa
                {
                    // Aktywujemy lub dezaktywujemy Canvas przed rozpoczęciem rotacji
                    if (objectCanvas != null)
                    {
                        objectCanvas.gameObject.SetActive(false);
                    }

                    if (isRotated)
                    {
                        // Jeśli obiekt jest obrócony, wróć do początkowej rotacji wokół osi Z
                        StartCoroutine(RotateObject(rotatingObject, rotatingObject.transform.localRotation, initialRotation));
                    }
                    else
                    {
                        // Jeśli obiekt nie jest obrócony, obróć go o 90 stopni wokół osi Z
                        StartCoroutine(RotateObject(rotatingObject, rotatingObject.transform.localRotation, initialRotation * Quaternion.Euler(0f, 0f, -90f)));
                    }

                    // Zmieniamy stan obrotu
                    isRotated = !isRotated;
                }
            }

            if (isReturning)
            {
                ReturnHarpoon();
            }

            if (currentShootDistance >= maxRange)
            {
                StartReturnHarpoon();
            }

            // Jeśli harpun wrócił, zaczynamy odliczać czas przeładowania
            if (reloadTimer > 0f)
            {
                // Pobieramy komponent Harpoon z currentHarpoon
                Harpoon harpoonScript = currentHarpoon.GetComponent<Harpoon>();

                if (harpoonScript != null)
                {
                    // Ustawiamy zmienną hasTreasureAttached na wartość z harpoonScript
                    bool hasTreasure = harpoonScript.hasTreasureAttached;
                    //Debug.Log("hasTreasureAttached: " + hasTreasure);

                    // Rozpoczynamy ruch obiektu podczas przeładowania tylko jeśli jest dołączony skarb
                    if (!isMoving && hasTreasure)
                    {
                        //Debug.Log("Startuję ruch obiektu!");
                        isMoving = true;
                        StartCoroutine(MoveObjectDuringReload());
                    }
                }
                else
                {
                    Debug.LogError("Nie znaleziono komponentu Harpoon na obiekcie currentHarpoon!");
                }

                reloadTimer -= Time.deltaTime;
            }

            else
            {
                isMoving = false;
            }
        }

        if (currentHarpoon != null && !harpoonRb.isKinematic)
        {
            currentShootDistance = Vector3.Distance(shootPosition, currentHarpoon.transform.position);
        }
    }

    private IEnumerator RotateObject(GameObject obj, Quaternion fromRotation, Quaternion toRotation)
    {
        isRotating = true; // Zaczynamy rotację, ustawiamy flagę na true
        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration) // Używamy publicznej zmiennej rotationDuration
        {
            obj.transform.localRotation = Quaternion.Slerp(fromRotation, toRotation, elapsedTime / rotationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        obj.transform.localRotation = toRotation; // Ustawiamy końcową rotację

        // Po zakończeniu rotacji, aktywujemy Canvas jeśli obiekt wrócił do pierwotnej rotacji
        if (objectCanvas != null)
        {
            objectCanvas.gameObject.SetActive(true);

            foreach (var playSoundOnObject in playSoundObjects)
            {
                if (playSoundOnObject == null) continue;

                playSoundOnObject.PlaySound("TabletOn", 0.6f, false);
            }
        }

        isRotating = false; // Rotacja zakończona, ustawiamy flagę na false
    }

    public void UpdateResourceUI(List<ResourceSlot> resourceSlots)
    {
        // Sprawdzamy, czy sloty są w ogóle dostępne
        if (resourceSlots == null || resourceSlots.Count == 0)
        {
            Debug.LogWarning("No resource slots found.");
            return;
        }

        // Dla każdego slotu wyświetlamy kategorię i ilość w UI
        for (int i = 0; i < resourceSlots.Count; i++)
        {
            if (i < categoryTexts.Length && i < quantityTexts.Length)
            {
                var slot = resourceSlots[i];

                // Jeśli slot zawiera kategorię, wyświetlamy ją
                categoryTexts[i].text = string.IsNullOrEmpty(slot.resourceCategory) ? "" : slot.resourceCategory;
                // Wyświetlamy ilość zasobów w slocie
                quantityTexts[i].text = slot.resourceCount.ToString();
            }
        }
    }
    void UpdateMaxResourceTexts()
    {
        if (turretCollector == null)
        {
            return;
        }

        // Zakładając, że maxResourcePerSlot to lista 4 wartości
        resourceText1.text = turretCollector.maxResourcePerSlot.ToString();
        resourceText2.text = turretCollector.maxResourcePerSlot.ToString();
        resourceText3.text = turretCollector.maxResourcePerSlot.ToString();
        resourceText4.text = turretCollector.maxResourcePerSlot.ToString();
    }

    private IEnumerator MoveObjectDuringReload()
    {
        // Czekamy na opóźnienie przed animacją
        yield return new WaitForSeconds(timeBeforeAnimation);

        float elapsedTime = 0f;
        Vector3 initialLocalPos = movingObject.transform.localPosition;
        Vector3 targetPosition = initialLocalPos + Vector3.forward * moveDistance; // Cel ruchu do przodu
        Vector3 reverseTargetPosition = initialLocalPos - Vector3.forward * moveDistance; // Cel ruchu do tyłu
        Quaternion initialRotation = movingObject.transform.localRotation;

        // Całkowity czas na animację
        float movementTime = fullAnimationTime;

        // Ruch do przodu (3/4 czasu)
        float forwardMovementTime = movementTime * 3f / 4f;

        // Ruch w tył (1/4 czasu)
        float reverseMovementTime = movementTime / 4f;

        // Czas na przerwę
        float pauseDuration = pauseTime;

        // Czas przeładowania minus czas animacji
        float remainingTimeAfterAnim = reloadTimer - fullAnimationTime;

        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;

            playSoundOnObject.PlaySound("Tube1", 0.5f, false);
        }

        // Ruch do przodu
        while (elapsedTime < forwardMovementTime)
        {
            float t = elapsedTime / forwardMovementTime;
            movingObject.transform.localPosition = Vector3.Lerp(initialLocalPos, targetPosition, t);
            movingObject.transform.localRotation = initialRotation;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Przerwa po zakończeniu ruchu do przodu
        yield return new WaitForSeconds(pauseDuration);

        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;

            playSoundOnObject.PlaySound("Tube2", 0.5f, false);
        }

        // Resetujemy czas, by rozpocząć ruch w tył
        elapsedTime = 0f;

        // Ruch w tył (1/4 czasu)
        while (elapsedTime < reverseMovementTime)
        {
            float t = elapsedTime / reverseMovementTime;
            movingObject.transform.localPosition = Vector3.Lerp(targetPosition, initialLocalPos, t);
            movingObject.transform.localRotation = initialRotation;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Zapewniamy, że po zakończeniu animacji obiekt wróci dokładnie na początkową pozycję
        movingObject.transform.localPosition = initialLocalPos;
        movingObject.transform.localRotation = initialRotation;

        // Czekamy na pozostały czas po zakończeniu animacji
        yield return new WaitForSeconds(remainingTimeAfterAnim);


        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;

            playSoundOnObject.PlaySound("Tube3", 0.5f, false);
        }

        // Kończymy animację
        isMoving = false;
    }

    void ShootHarpoon(Vector3 targetPosition)
    {
        if (currentHarpoon != null)
        {
            currentHarpoon.transform.SetParent(null);
            currentHarpoon.SetActive(true);

            Vector3 shootDirection = Camera.main.transform.forward;
            harpoonRb.isKinematic = false;
            harpoonRb.linearVelocity = shootDirection * shootSpeed;

            canShoot = false;
            shootPosition = firePoint.position;
            currentShootDistance = 0f;
        }
        else
        {
            Debug.LogError("currentHarpoon nie jest przypisany.");
        }
    }

    void ReturnHarpoon()
    {
        if (currentHarpoon != null)
        {
            Vector3 returnDirection = (firePoint.position - currentHarpoon.transform.position).normalized;
            harpoonRb.linearVelocity = returnDirection * returnSpeed;

            if (Vector3.Distance(currentHarpoon.transform.position, firePoint.position) < returnTolerance)
            {
                harpoonRb.linearVelocity = Vector3.zero;
                harpoonRb.isKinematic = true;
                currentHarpoon.transform.SetParent(firePoint);
                currentHarpoon.transform.localPosition = Vector3.zero;
                currentHarpoon.transform.localRotation = Quaternion.identity;
                currentHarpoon.transform.localScale = initialScale;

                foreach (Transform child in currentHarpoon.transform)
                {
                    child.localPosition = Vector3.zero;
                    child.localRotation = Quaternion.identity;
                }

                isReturning = false;

                // Po powrocie harpunu, zaczynamy czas przeładowania
                reloadTimer = reloadTime; // Zainicjuj czas przeładowania
                canShoot = true; // Pozwól na strzał po zakończeniu przeładowania

                // Zatrzymaj wszystkie odtwarzane dźwięki
                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;

                    //playSoundOnObject.StopSound("HarpoonFire");
                    //playSoundOnObject.StopSound("HarpoonChainFire");

                    //playSoundOnObject.PlaySound("HarpoonChainBack2", 0.3f, false);
                }
            }
        }
        else
        {
            Debug.LogError("currentHarpoon nie jest przypisany.");
        }
    }

    void StartReturnHarpoon()
    {
        if (harpoonRb != null)
        {
            harpoonRb.isKinematic = false;
            isReturning = true;
        }
        else
        {
            Debug.LogError("harpoonRb nie jest przypisany.");
        }

        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;

            playSoundOnObject.PlaySound("HarpoonChainBack2", 0.1f, false);
        }
    }

    public void OnHarpoonCollision()
    {
        if (harpoonRb != null)
        {
            StartReturnHarpoon();
        }
        else
        {
            Debug.LogError("harpoonRb nie jest przypisany.");
        }
    }

    GameObject FindClosestTreasureInView()
    {
        GameObject[] treasures = GameObject.FindGameObjectsWithTag("Treasure");
        GameObject closestTreasure = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject treasure in treasures)
        {
            Vector3 directionToTreasure = (treasure.transform.position - firePoint.position).normalized;
            float angleToTreasureHorizontal = Vector3.Angle(firePoint.forward, new Vector3(directionToTreasure.x, 0, directionToTreasure.z));
            float angleToTreasureVertical = Vector3.Angle(firePoint.forward, new Vector3(0, directionToTreasure.y, directionToTreasure.z));
            float distanceToTreasure = Vector3.Distance(firePoint.position, treasure.transform.position);

            if (angleToTreasureHorizontal <= maxHorizontalAngleToTarget && angleToTreasureVertical <= maxVerticalAngleToTarget && distanceToTreasure <= detectionRange && distanceToTreasure >= minDetectionRange)
            {
                if (distanceToTreasure < closestDistance)
                {
                    closestDistance = distanceToTreasure;
                    closestTreasure = treasure;
                }
            }
        }

        return closestTreasure;
    }

    Vector3 PredictTargetPosition(GameObject target)
    {
        TreasureTracker treasureTracker = target.GetComponent<TreasureTracker>();

        if (treasureTracker != null)
        {
            Vector3 targetVelocity = treasureTracker.CurrentVelocity;
            float distanceToTarget = Vector3.Distance(firePoint.position, target.transform.position);
            float timeToTarget = distanceToTarget / shootSpeed;
            Vector3 predictedPosition = target.transform.position + targetVelocity * timeToTarget;

            return predictedPosition;
        }

        return target.transform.position;
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            return hit.point;
        }

        return ray.origin + ray.direction * maxRange;
    }

    void OnDrawGizmos()
    {
        if (showRangesInScene)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, detectionRange);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(firePoint.position, minDetectionRange);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(firePoint.position, maxRange);
            Gizmos.color = Color.yellow;
            DrawViewCone(firePoint.position, firePoint.forward, maxHorizontalAngleToTarget, maxVerticalAngleToTarget, detectionRange);
        }
    }

    void DrawViewCone(Vector3 position, Vector3 direction, float horizontalAngle, float verticalAngle, float range)
    {
        Vector3 up = Quaternion.Euler(verticalAngle, 0, 0) * direction * range;
        Vector3 down = Quaternion.Euler(-verticalAngle, 0, 0) * direction * range;
        Vector3 left = Quaternion.Euler(0, -horizontalAngle, 0) * direction * range;
        Vector3 right = Quaternion.Euler(0, horizontalAngle, 0) * direction * range;

        Gizmos.DrawLine(position, position + up);
        Gizmos.DrawLine(position, position + down);
        Gizmos.DrawLine(position, position + left);
        Gizmos.DrawLine(position, position + right);

        Gizmos.DrawLine(position + up, position + left);
        Gizmos.DrawLine(position + up, position + right);
        Gizmos.DrawLine(position + down, position + left);
        Gizmos.DrawLine(position + down, position + right);
    }
}
