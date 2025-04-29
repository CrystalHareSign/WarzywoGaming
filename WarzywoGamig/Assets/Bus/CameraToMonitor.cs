using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CameraToMonitor : MonoBehaviour
{
    public PlayerMovement playerMovementScript; // Skrypt odpowiedzialny za ruch gracza
    public MouseLook mouseLookScript; // Skrypt odpowiedzialny za ruch kamery
    public HoverMessage monitorHoverMessage;
    public GameObject crossHair;
    public GameObject monitorCanvas; // UI z przyciskami

    public Transform player; // Transform gracza
    public Transform finalCameraRotation; // Transform, kt�ry definiuje finaln� rotacj� i pozycj� kamery
    public float interactionRange = 5f; // Zasi�g, w kt�rym gracz mo�e zbli�y� si� do monitora
    public float cameraMoveSpeed = 5f; // Pr�dko�� przesuwania kamery

    private Vector3 originalCameraPosition; // Pocz�tkowa pozycja kamery
    private Quaternion originalCameraRotation; // Pocz�tkowa rotacja kamery
    private bool isInteracting = false; // Czy gracz jest w trakcie interakcji
    private bool isCameraMoving = false; // Flaga sprawdzaj�ca, czy kamera jest w ruchu

    [Header("UI � Konsola monitora")]
    public TextMeshProUGUI consoleTextDisplay;
    private Queue<ConsoleMessage> messageQueue = new Queue<ConsoleMessage>();  // Kolejka wiadomo�ci
    public float messageDuration = 5f;  // Czas trwania ka�dej wiadomo�ci w sekundach
    public int maxMessages = 5;  // Maksymalna liczba wiadomo�ci w kolejce

    private bool isCursorVisible = true;  // Czy kursor (|) jest widoczny
    private float cursorBlinkInterval = 0.5f;  // Czas w sekundach mi�dzy miganiem kursora
    private float cursorBlinkTimer = 0f;  // Licznik czasu migania kursora
    private float currentTime = 0f;

    private void Start()
    {
        if (monitorCanvas != null)
        {
            monitorCanvas.SetActive(false);
        }

        // Inicjalizacja tekstu
        if (consoleTextDisplay == null)
        {
            Debug.LogError("Brak przypisanego TextMeshProUGUI dla konsoli!");
        }
    }

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

        // Sprawd�, czy min�� czas dla jakiejkolwiek wiadomo�ci i usu� j�, je�li wygas�a
        if (messageQueue.Count > 0)
        {
            if (messageQueue.Peek().expireTime <= Time.time)
            {
                // Usu� najstarsz� wiadomo��, je�li jej czas wyga�ni�cia min��
                messageQueue.Dequeue();
                UpdateConsoleText(); // Zaktualizuj konsol� po usuni�ciu wiadomo�ci
            }
        }

        if (isInteracting)
        {
            currentTime = Time.time;

            // Aktualizowanie tekstu konsoli
            UpdateConsoleText();

            // Migaj�cy kursor
            cursorBlinkTimer += Time.deltaTime;
            if (cursorBlinkTimer >= cursorBlinkInterval)
            {
                cursorBlinkTimer = 0f;
                isCursorVisible = !isCursorVisible;  // Zmiana stanu kursora (widoczny / niewidoczny)
            }
        }
    }

    // P�ynne przesuni�cie kamery do wskazanej pozycji
    IEnumerator MoveCameraToPosition()
    {
        isInteracting = true;
        if (monitorHoverMessage != null)
        {
            monitorHoverMessage.isInteracted = true;
        }

        if (crossHair != null)
        {
            crossHair.SetActive(false);
        }

        if (monitorCanvas != null)
        {
            monitorCanvas.SetActive(true);
        }

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

        ShowConsoleMessage("Uruchamianie terminalu...");
    }

    // P�ynne przesuni�cie kamery z powrotem do pierwotniej pozycji
    IEnumerator MoveCameraBackToOriginalPosition()
    {
        isInteracting = false;
        if (monitorHoverMessage != null)
        {
            monitorHoverMessage.isInteracted = false;
        }

        if (crossHair != null)
        {
            crossHair.SetActive(true);
        }

        if (monitorCanvas != null)
        {
            monitorCanvas.SetActive(false);
        }
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

        ClearMonitorConsole();
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

    public void ShowConsoleMessage(string message)
    {
        // Sprawd�, czy liczba wiadomo�ci przekroczy�a limit
        if (messageQueue.Count >= maxMessages)
        {
            messageQueue.Dequeue(); // Usu� najstarsz� wiadomo��, je�li przekroczono limit
        }

        // Dodaj wiadomo�� z czasem wyga�ni�cia
        ConsoleMessage newMessage = new ConsoleMessage(message, Time.time + messageDuration);
        messageQueue.Enqueue(newMessage);  // Dodaj now� wiadomo�� do kolejki

        // Zaktualizuj tekst w konsoli
        UpdateConsoleText();
    }

    private void UpdateConsoleText()
    {
        string consoleText = "";

        // Kursor: zawsze na pocz�tku, ale jego widoczno�� zmienia si�
        string cursor = isCursorVisible ? "<color=#00E700>|</color>" : "<color=#00000000>|</color>";  // Przezroczysty kursor

        // Dodaj migaj�cy kursor na pocz�tku tekstu
        consoleText += cursor + " ";

        // Zapisz aktualne wiadomo�ci do listy i odwr�� je
        List<ConsoleMessage> activeMessages = new List<ConsoleMessage>();
        foreach (var msg in messageQueue)
        {
            if (msg.expireTime > currentTime)
            {
                activeMessages.Add(msg);
            }
        }

        // Odwr�� kolejno��, by nowe by�y wy�ej
        activeMessages.Reverse();

        // Sklej wiadomo�ci
        foreach (var msg in activeMessages)
        {
            consoleText += msg.message + "\n";
        }

        if (consoleTextDisplay != null)
        {
            consoleTextDisplay.text = consoleText;
        }
    }

    private struct ConsoleMessage
    {
        public string message;  // Tre�� wiadomo�ci
        public float expireTime;  // Czas wyga�ni�cia wiadomo�ci

        public ConsoleMessage(string message, float expireTime)
        {
            this.message = message;
            this.expireTime = expireTime;
        }
    }

    public void ClearMonitorConsole()
    {
        if (consoleTextDisplay != null)
        {
            consoleTextDisplay.text = "";
        }
    }
}