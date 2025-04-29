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
    public Transform finalCameraRotation; // Transform, który definiuje finaln¹ rotacjê i pozycjê kamery
    public float interactionRange = 5f; // Zasiêg, w którym gracz mo¿e zbli¿yæ siê do monitora
    public float cameraMoveSpeed = 5f; // Prêdkoœæ przesuwania kamery

    private Vector3 originalCameraPosition; // Pocz¹tkowa pozycja kamery
    private Quaternion originalCameraRotation; // Pocz¹tkowa rotacja kamery
    private bool isInteracting = false; // Czy gracz jest w trakcie interakcji
    private bool isCameraMoving = false; // Flaga sprawdzaj¹ca, czy kamera jest w ruchu

    [Header("UI – Konsola monitora")]
    public TextMeshProUGUI consoleTextDisplay;
    private Queue<ConsoleMessage> messageQueue = new Queue<ConsoleMessage>();  // Kolejka wiadomoœci
    public float messageDuration = 5f;  // Czas trwania ka¿dej wiadomoœci w sekundach
    public int maxMessages = 5;  // Maksymalna liczba wiadomoœci w kolejce

    private bool isCursorVisible = true;  // Czy kursor (|) jest widoczny
    private float cursorBlinkInterval = 0.5f;  // Czas w sekundach miêdzy miganiem kursora
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

        // SprawdŸ, czy min¹³ czas dla jakiejkolwiek wiadomoœci i usuñ j¹, jeœli wygas³a
        if (messageQueue.Count > 0)
        {
            if (messageQueue.Peek().expireTime <= Time.time)
            {
                // Usuñ najstarsz¹ wiadomoœæ, jeœli jej czas wygaœniêcia min¹³
                messageQueue.Dequeue();
                UpdateConsoleText(); // Zaktualizuj konsolê po usuniêciu wiadomoœci
            }
        }

        if (isInteracting)
        {
            currentTime = Time.time;

            // Aktualizowanie tekstu konsoli
            UpdateConsoleText();

            // Migaj¹cy kursor
            cursorBlinkTimer += Time.deltaTime;
            if (cursorBlinkTimer >= cursorBlinkInterval)
            {
                cursorBlinkTimer = 0f;
                isCursorVisible = !isCursorVisible;  // Zmiana stanu kursora (widoczny / niewidoczny)
            }
        }
    }

    // P³ynne przesuniêcie kamery do wskazanej pozycji
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

        ShowConsoleMessage("Uruchamianie terminalu...");
    }

    // P³ynne przesuniêcie kamery z powrotem do pierwotniej pozycji
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

        ClearMonitorConsole();
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

    public void ShowConsoleMessage(string message)
    {
        // SprawdŸ, czy liczba wiadomoœci przekroczy³a limit
        if (messageQueue.Count >= maxMessages)
        {
            messageQueue.Dequeue(); // Usuñ najstarsz¹ wiadomoœæ, jeœli przekroczono limit
        }

        // Dodaj wiadomoœæ z czasem wygaœniêcia
        ConsoleMessage newMessage = new ConsoleMessage(message, Time.time + messageDuration);
        messageQueue.Enqueue(newMessage);  // Dodaj now¹ wiadomoœæ do kolejki

        // Zaktualizuj tekst w konsoli
        UpdateConsoleText();
    }

    private void UpdateConsoleText()
    {
        string consoleText = "";

        // Kursor: zawsze na pocz¹tku, ale jego widocznoœæ zmienia siê
        string cursor = isCursorVisible ? "<color=#00E700>|</color>" : "<color=#00000000>|</color>";  // Przezroczysty kursor

        // Dodaj migaj¹cy kursor na pocz¹tku tekstu
        consoleText += cursor + " ";

        // Zapisz aktualne wiadomoœci do listy i odwróæ je
        List<ConsoleMessage> activeMessages = new List<ConsoleMessage>();
        foreach (var msg in messageQueue)
        {
            if (msg.expireTime > currentTime)
            {
                activeMessages.Add(msg);
            }
        }

        // Odwróæ kolejnoœæ, by nowe by³y wy¿ej
        activeMessages.Reverse();

        // Sklej wiadomoœci
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
        public string message;  // Treœæ wiadomoœci
        public float expireTime;  // Czas wygaœniêcia wiadomoœci

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