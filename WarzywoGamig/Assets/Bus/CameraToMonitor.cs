using System.Collections;
using UnityEngine;

public class CameraToMonitor : MonoBehaviour
{
    public PlayerMovement playerMovementScript; // Skrypt odpowiedzialny za ruch gracza
    public MouseLook mouseLookScript; // Skrypt odpowiedzialny za ruch kamery
    public Transform player; // Transform gracza
    public Transform finalCameraRotation; // Transform, kt�ry definiuje finaln� rotacj� i pozycj� kamery
    public float interactionRange = 5f; // Zasi�g, w kt�rym gracz mo�e zbli�y� si� do monitora
    public float cameraMoveSpeed = 5f; // Pr�dko�� przesuwania kamery

    private Vector3 originalCameraPosition; // Pocz�tkowa pozycja kamery
    private Quaternion originalCameraRotation; // Pocz�tkowa rotacja kamery
    private bool isInteracting = false; // Czy gracz jest w trakcie interakcji
    private bool isCameraMoving = false; // Flaga sprawdzaj�ca, czy kamera jest w ruchu

    void Update()
    {
        // Oblicz odleg�o�� gracza od miejsca interakcji
        float distanceToInteraction = Vector3.Distance(player.position, finalCameraRotation.position);

        // Sprawdzenie, czy gracz znajduje si� w zasi�gu
        if (distanceToInteraction <= interactionRange)
        {
            if (Input.GetKeyDown(KeyCode.E) && !isInteracting && !isCameraMoving)
            {
                // Rozpocz�cie interakcji � zapami�tanie pozycji kamery przed przesuni�ciem
                originalCameraPosition = Camera.main.transform.position;
                originalCameraRotation = Camera.main.transform.rotation;

                // Rozpocz�cie interakcji � przesuwanie kamery do obiektu
                StartCoroutine(MoveCameraToPosition());
            }
            else if (Input.GetKeyDown(KeyCode.Q) && isInteracting && !isCameraMoving)
            {
                // Wyj�cie z interakcji � przywr�cenie kamery do pierwotnej pozycji
                StartCoroutine(MoveCameraBackToOriginalPosition());
            }
        }
        // Je�eli gracz oddali si� poza zasi�g, kamera wr�ci do pierwotnej pozycji
        else if (isInteracting && !isCameraMoving)
        {
            StartCoroutine(MoveCameraBackToOriginalPosition());
        }
    }

    // P�ynne przesuni�cie kamery do wskazanej pozycji
    IEnumerator MoveCameraToPosition()
    {
        // Zatrzymanie ruchu gracza i kamery
        DisablePlayerMovementAndMouseLook();
        isCameraMoving = true;

        isInteracting = true;

        // Okre�lamy pozycj� kamery wzgl�dem obiektu finalCameraRotation (z uwzgl�dnieniem odleg�o�ci)
        Vector3 targetCameraPosition = new Vector3(finalCameraRotation.position.x, finalCameraRotation.position.y, finalCameraRotation.position.z);

        // Ustawiamy rotacj� kamery, patrz�c w kierunku obiektu
        Quaternion targetCameraRotation = finalCameraRotation.rotation;

        // P�ynne przej�cie kamery
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetCameraPosition, elapsedTime);
            Camera.main.transform.rotation = Quaternion.Slerp(Camera.main.transform.rotation, targetCameraRotation, elapsedTime);
            elapsedTime += Time.deltaTime * cameraMoveSpeed;
            yield return null;
        }

        // Ustawiamy kamer� dok�adnie w docelowej pozycji i rotacji
        Camera.main.transform.position = targetCameraPosition;
        Camera.main.transform.rotation = targetCameraRotation;

        isCameraMoving = false;

        // Od��czenie kursora
        Cursor.lockState = CursorLockMode.None; // Kursor mo�e by� u�ywany
        Cursor.visible = true; // Kursor jest widoczny
    }

    // P�ynne przesuni�cie kamery z powrotem do pierwotniej pozycji
    IEnumerator MoveCameraBackToOriginalPosition()
    {
        // Przywr�cenie blokady kursora przed rozpocz�ciem ruchu kamery
        Cursor.lockState = CursorLockMode.Locked; // Blokujemy kursor
        Cursor.visible = false; // Ukrywamy kursor

        // Zatrzymanie ruchu gracza i kamery
        DisablePlayerMovementAndMouseLook();
        isCameraMoving = true;

        // P�ynne przej�cie kamery do jej pierwotnej pozycji
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, originalCameraPosition, elapsedTime);
            Camera.main.transform.rotation = Quaternion.Slerp(Camera.main.transform.rotation, originalCameraRotation, elapsedTime);
            elapsedTime += Time.deltaTime * cameraMoveSpeed;
            yield return null;
        }

        // Po zako�czeniu animacji, ustawiamy kamer� dok�adnie w pierwotniej pozycji
        Camera.main.transform.position = originalCameraPosition;
        Camera.main.transform.rotation = originalCameraRotation;

        // Po zako�czeniu animacji, przywracamy mo�liwo�� poruszania si�
        EnablePlayerMovementAndMouseLook();
        isCameraMoving = false;

        isInteracting = false;
    }

    // Funkcja wy��czaj�ca skrypty odpowiedzialne za ruch gracza i kamery
    private void DisablePlayerMovementAndMouseLook()
    {
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false; // Wy��cza skrypt odpowiedzialny za ruch gracza
        }

        if (mouseLookScript != null)
        {
            mouseLookScript.enabled = false; // Wy��cza skrypt odpowiedzialny za ruch kamery
        }
    }

    // Funkcja przywracaj�ca skrypty odpowiedzialne za ruch gracza i kamery
    private void EnablePlayerMovementAndMouseLook()
    {
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true; // W��cza skrypt odpowiedzialny za ruch gracza
        }

        if (mouseLookScript != null)
        {
            mouseLookScript.enabled = true; // W��cza skrypt odpowiedzialny za ruch kamery
        }
    }
}