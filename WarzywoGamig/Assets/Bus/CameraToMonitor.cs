using System.Collections;
using UnityEngine;

public class CameraToMonitor : MonoBehaviour
{
    public PlayerMovement playerMovementScript; // Skrypt odpowiedzialny za ruch gracza
    public MouseLook mouseLookScript; // Skrypt odpowiedzialny za ruch kamery
    public Transform player; // Transform gracza
    public Transform finalCameraRotation; // Transform, który definiuje finaln¹ rotacjê i pozycjê kamery
    public float interactionRange = 5f; // Zasiêg, w którym gracz mo¿e zbli¿yæ siê do monitora
    public float cameraMoveSpeed = 5f; // Prêdkoœæ przesuwania kamery

    private Vector3 originalCameraPosition; // Pocz¹tkowa pozycja kamery
    private Quaternion originalCameraRotation; // Pocz¹tkowa rotacja kamery
    private bool isInteracting = false; // Czy gracz jest w trakcie interakcji
    private bool isCameraMoving = false; // Flaga sprawdzaj¹ca, czy kamera jest w ruchu

    void Update()
    {
        // Oblicz odleg³oœæ gracza od miejsca interakcji
        float distanceToInteraction = Vector3.Distance(player.position, finalCameraRotation.position);

        // Sprawdzenie, czy gracz znajduje siê w zasiêgu
        if (distanceToInteraction <= interactionRange)
        {
            if (Input.GetKeyDown(KeyCode.E) && !isInteracting && !isCameraMoving)
            {
                // Rozpoczêcie interakcji – zapamiêtanie pozycji kamery przed przesuniêciem
                originalCameraPosition = Camera.main.transform.position;
                originalCameraRotation = Camera.main.transform.rotation;

                // Rozpoczêcie interakcji – przesuwanie kamery do obiektu
                StartCoroutine(MoveCameraToPosition());
            }
            else if (Input.GetKeyDown(KeyCode.Q) && isInteracting && !isCameraMoving)
            {
                // Wyjœcie z interakcji – przywrócenie kamery do pierwotnej pozycji
                StartCoroutine(MoveCameraBackToOriginalPosition());
            }
        }
        // Je¿eli gracz oddali siê poza zasiêg, kamera wróci do pierwotnej pozycji
        else if (isInteracting && !isCameraMoving)
        {
            StartCoroutine(MoveCameraBackToOriginalPosition());
        }
    }

    // P³ynne przesuniêcie kamery do wskazanej pozycji
    IEnumerator MoveCameraToPosition()
    {
        // Zatrzymanie ruchu gracza i kamery
        DisablePlayerMovementAndMouseLook();
        isCameraMoving = true;

        isInteracting = true;

        // Okreœlamy pozycjê kamery wzglêdem obiektu finalCameraRotation (z uwzglêdnieniem odleg³oœci)
        Vector3 targetCameraPosition = new Vector3(finalCameraRotation.position.x, finalCameraRotation.position.y, finalCameraRotation.position.z);

        // Ustawiamy rotacjê kamery, patrz¹c w kierunku obiektu
        Quaternion targetCameraRotation = finalCameraRotation.rotation;

        // P³ynne przejœcie kamery
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetCameraPosition, elapsedTime);
            Camera.main.transform.rotation = Quaternion.Slerp(Camera.main.transform.rotation, targetCameraRotation, elapsedTime);
            elapsedTime += Time.deltaTime * cameraMoveSpeed;
            yield return null;
        }

        // Ustawiamy kamerê dok³adnie w docelowej pozycji i rotacji
        Camera.main.transform.position = targetCameraPosition;
        Camera.main.transform.rotation = targetCameraRotation;

        isCameraMoving = false;

        // Od³¹czenie kursora
        Cursor.lockState = CursorLockMode.None; // Kursor mo¿e byæ u¿ywany
        Cursor.visible = true; // Kursor jest widoczny
    }

    // P³ynne przesuniêcie kamery z powrotem do pierwotniej pozycji
    IEnumerator MoveCameraBackToOriginalPosition()
    {
        // Przywrócenie blokady kursora przed rozpoczêciem ruchu kamery
        Cursor.lockState = CursorLockMode.Locked; // Blokujemy kursor
        Cursor.visible = false; // Ukrywamy kursor

        // Zatrzymanie ruchu gracza i kamery
        DisablePlayerMovementAndMouseLook();
        isCameraMoving = true;

        // P³ynne przejœcie kamery do jej pierwotnej pozycji
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, originalCameraPosition, elapsedTime);
            Camera.main.transform.rotation = Quaternion.Slerp(Camera.main.transform.rotation, originalCameraRotation, elapsedTime);
            elapsedTime += Time.deltaTime * cameraMoveSpeed;
            yield return null;
        }

        // Po zakoñczeniu animacji, ustawiamy kamerê dok³adnie w pierwotniej pozycji
        Camera.main.transform.position = originalCameraPosition;
        Camera.main.transform.rotation = originalCameraRotation;

        // Po zakoñczeniu animacji, przywracamy mo¿liwoœæ poruszania siê
        EnablePlayerMovementAndMouseLook();
        isCameraMoving = false;

        isInteracting = false;
    }

    // Funkcja wy³¹czaj¹ca skrypty odpowiedzialne za ruch gracza i kamery
    private void DisablePlayerMovementAndMouseLook()
    {
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false; // Wy³¹cza skrypt odpowiedzialny za ruch gracza
        }

        if (mouseLookScript != null)
        {
            mouseLookScript.enabled = false; // Wy³¹cza skrypt odpowiedzialny za ruch kamery
        }
    }

    // Funkcja przywracaj¹ca skrypty odpowiedzialne za ruch gracza i kamery
    private void EnablePlayerMovementAndMouseLook()
    {
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true; // W³¹cza skrypt odpowiedzialny za ruch gracza
        }

        if (mouseLookScript != null)
        {
            mouseLookScript.enabled = true; // W³¹cza skrypt odpowiedzialny za ruch kamery
        }
    }
}