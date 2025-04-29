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
    public bool canInteract = false;  // Flaga, kt�ra pozwala na interakcj� po zako�czeniu log�w
    private bool isInteracting = false; // Czy gracz jest w trakcie interakcji
    private bool isCameraMoving = false; // Flaga sprawdzaj�ca, czy kamera jest w ruchu

    [Header("UI � Konsola monitora")]
    public TextMeshProUGUI consoleTextDisplay;
    private Queue<ConsoleMessage> messageQueue = new Queue<ConsoleMessage>();  // Kolejka wiadomo�ci
    public float messageDuration = 5f;  // Czas trwania ka�dej wiadomo�ci w sekundach
    public int maxMessages = 5;  // Maksymalna liczba wiadomo�ci w kolejce
    public float startNextLogBefore = 1;

    private bool isCursorVisible = true;  // Czy kursor (|) jest widoczny
    private float cursorBlinkInterval = 0.5f;  // Czas w sekundach mi�dzy miganiem kursora
    private float cursorBlinkTimer = 0f;  // Licznik czasu migania kursora
    private float currentTime = 0f;


    public List<LogEntry> logEntries; // Lista wpis�w log�w

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

        if (messageQueue.Count > 0)
        {
            ConsoleMessage msg = messageQueue.Peek();
            if (msg.IsExpired(Time.time))
            {
                messageQueue.Dequeue();
                UpdateConsoleText();
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

        ShowConsoleMessage(">>>Uruchamianie terminalu...",5,0.5f, "#00E700");
        StartLogSequence();
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

    public void ShowConsoleMessage(string rawMessage, float visibleDuration, float fadeDuration, string hexColor)
    {
        Color color = Color.white;
        if (ColorUtility.TryParseHtmlString(hexColor, out Color parsedColor))
        {
            color = parsedColor;
        }

        var newMessage = new ConsoleMessage(rawMessage, Time.time, visibleDuration, fadeDuration, color);
        messageQueue.Enqueue(newMessage);

        while (messageQueue.Count > maxMessages)
            messageQueue.Dequeue();
    }

    private void UpdateConsoleText()
    {
        string consoleText = "";

        string cursor = isCursorVisible ? "<color=#00E700>|</color>" : "<color=#00000000>|</color>";            /////   #00E700
        consoleText += cursor + " ";

        currentTime = Time.time;

        List<ConsoleMessage> activeMessages = new List<ConsoleMessage>();

        // Zbieranie tylko aktywnych wiadomo�ci
        foreach (var msg in messageQueue)
        {
            if (!msg.IsExpired(Time.time))
            {
                activeMessages.Add(msg);
            }
        }

        // Odwr�� kolejno�� � nowsze wiadomo�ci u g�ry
        activeMessages.Reverse();

        foreach (var msg in activeMessages)
        {
            float alpha = msg.GetFadeAlpha(currentTime);
            int alphaInt = Mathf.RoundToInt(alpha * 255);
            string hexAlpha = alphaInt.ToString("X2");

            // Konwertowanie koloru na hex
            Color msgColor = msg.color;
            string hexColor = ColorUtility.ToHtmlStringRGB(msgColor);

            // Dodanie wiadomo�ci z odpowiednim kolorem i przezroczysto�ci�
            consoleText += $"<color=#{hexColor}{hexAlpha}>{msg.message}</color>\n";
        }

        if (consoleTextDisplay != null)
        {
            consoleTextDisplay.text = consoleText;
        }
    }


    public void ClearMonitorConsole()
    {
        if (consoleTextDisplay != null)
        {
            consoleTextDisplay.text = "";
        }
    }

    [System.Serializable]
    public class ConsoleMessage
    {
        public string message;
        public float timeAdded;
        public float visibleDuration;
        public float fadeDuration;
        public Color color;

        public ConsoleMessage(string message, float timeAdded, float visibleDuration, float fadeDuration, Color color)
        {
            this.message = message;
            this.timeAdded = timeAdded;
            this.visibleDuration = visibleDuration;
            this.fadeDuration = fadeDuration;
            this.color = color;
        }

        public float GetFadeAlpha(float currentTime)
        {
            float elapsed = currentTime - timeAdded;

            if (elapsed <= visibleDuration)
                return 1f; // Pe�na widoczno��

            float fadeTime = elapsed - visibleDuration;
            return Mathf.Clamp01(1f - fadeTime / fadeDuration); // Od 1 do 0
        }

        public bool IsExpired(float currentTime)
        {
            return currentTime - timeAdded > (visibleDuration + fadeDuration);
        }
    }

    [System.Serializable]
    public class LogEntry
    {
        public string[] messages;   // Mo�liwe wiadomo�ci
        public float messageDuration;
        public float fadeDuration;
        [Range(0f, 100f)]  // Atrybut Range sprawi, �e b�dzie slider od 0 do 1
        public float probability;  // Prawdopodobie�stwo wy�wietlenia tego logu (od 0 do 1)
    }

    private void StartLogSequence()
    {
        // Zablokowanie interakcji przed rozpocz�ciem logowania
        canInteract = false;

        // B�dziemy losowa� wiadomo�ci z listy logEntries
        List<LogEntry> availableLogs = new List<LogEntry>(logEntries);  // Kopia listy, aby m�c usuwa� u�yte logi

        // Rozpoczynamy losowanie log�w przy w��czeniu monitora
        StartCoroutine(DisplayLogsSequence(availableLogs));
    }

    private IEnumerator DisplayLogsSequence(List<LogEntry> availableLogs)
    {
        while (availableLogs.Count > 0)
        {
            // Losowanie logu na podstawie prawdopodobie�stwa
            LogEntry logEntry = GetRandomLogWithProbability(availableLogs);

            // Ustawienie domy�lnego fadeDuration (np. 0.5f)
            float fadeDuration = 0.5f;

            // Dodaj wiadomo�� do konsoli
            ShowConsoleMessage(logEntry.messages[Random.Range(0, logEntry.messages.Length)], messageDuration, fadeDuration, "#00E700");

            // Usu� ten log z dost�pnej listy, �eby nie pojawi� si� ponownie
            availableLogs.Remove(logEntry);

            // Czekaj, zanim poka�emy kolejny log
            yield return new WaitForSeconds(messageDuration - startNextLogBefore);
        }

        // Tylko po zako�czeniu wszystkich log�w � dodaj wiadomo�� ko�cow�
        ShowConsoleMessage(">>>Terminal gotowy.", 5f, 0.5f, "#FFD200");

        canInteract = true;
    }


    private LogEntry GetRandomLogWithProbability(List<LogEntry> availableLogs)
    {
        float totalProbability = 0f;

        // Obliczamy sum� wszystkich prawdopodobie�stw
        foreach (var log in availableLogs)
        {
            totalProbability += log.probability;
        }

        // Losowanie, kt�re logi wybra� na podstawie prawdopodobie�stwa
        float randomValue = Random.Range(0f, totalProbability);
        float cumulativeProbability = 0f;

        foreach (var log in availableLogs)
        {
            cumulativeProbability += log.probability;
            if (randomValue <= cumulativeProbability)
            {
                return log;
            }
        }

        // W przypadku b��du, zwr�� pierwszy dost�pny log
        return availableLogs[0];
    }
}