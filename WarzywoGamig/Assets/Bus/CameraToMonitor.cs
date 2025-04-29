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
    public bool canInteract = false;  // Flaga, która pozwala na interakcjê po zakoñczeniu logów
    private bool isInteracting = false; // Czy gracz jest w trakcie interakcji
    private bool isCameraMoving = false; // Flaga sprawdzaj¹ca, czy kamera jest w ruchu

    [Header("UI – Konsola monitora")]
    public TextMeshProUGUI consoleTextDisplay;
    private Queue<ConsoleMessage> messageQueue = new Queue<ConsoleMessage>();  // Kolejka wiadomoœci
    public float messageDuration = 5f;  // Czas trwania ka¿dej wiadomoœci w sekundach
    public int maxMessages = 5;  // Maksymalna liczba wiadomoœci w kolejce
    public float startNextLogBefore = 1;

    private bool isCursorVisible = true;  // Czy kursor (|) jest widoczny
    private float cursorBlinkInterval = 0.5f;  // Czas w sekundach miêdzy miganiem kursora
    private float cursorBlinkTimer = 0f;  // Licznik czasu migania kursora
    private float currentTime = 0f;


    public List<LogEntry> logEntries; // Lista wpisów logów

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

        ShowConsoleMessage(">>>Uruchamianie terminalu...",5,0.5f, "#00E700");
        StartLogSequence();
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

        // Zbieranie tylko aktywnych wiadomoœci
        foreach (var msg in messageQueue)
        {
            if (!msg.IsExpired(Time.time))
            {
                activeMessages.Add(msg);
            }
        }

        // Odwróæ kolejnoœæ – nowsze wiadomoœci u góry
        activeMessages.Reverse();

        foreach (var msg in activeMessages)
        {
            float alpha = msg.GetFadeAlpha(currentTime);
            int alphaInt = Mathf.RoundToInt(alpha * 255);
            string hexAlpha = alphaInt.ToString("X2");

            // Konwertowanie koloru na hex
            Color msgColor = msg.color;
            string hexColor = ColorUtility.ToHtmlStringRGB(msgColor);

            // Dodanie wiadomoœci z odpowiednim kolorem i przezroczystoœci¹
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
                return 1f; // Pe³na widocznoœæ

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
        public string[] messages;   // Mo¿liwe wiadomoœci
        public float messageDuration;
        public float fadeDuration;
        [Range(0f, 100f)]  // Atrybut Range sprawi, ¿e bêdzie slider od 0 do 1
        public float probability;  // Prawdopodobieñstwo wyœwietlenia tego logu (od 0 do 1)
    }

    private void StartLogSequence()
    {
        // Zablokowanie interakcji przed rozpoczêciem logowania
        canInteract = false;

        // Bêdziemy losowaæ wiadomoœci z listy logEntries
        List<LogEntry> availableLogs = new List<LogEntry>(logEntries);  // Kopia listy, aby móc usuwaæ u¿yte logi

        // Rozpoczynamy losowanie logów przy w³¹czeniu monitora
        StartCoroutine(DisplayLogsSequence(availableLogs));
    }

    private IEnumerator DisplayLogsSequence(List<LogEntry> availableLogs)
    {
        while (availableLogs.Count > 0)
        {
            // Losowanie logu na podstawie prawdopodobieñstwa
            LogEntry logEntry = GetRandomLogWithProbability(availableLogs);

            // Ustawienie domyœlnego fadeDuration (np. 0.5f)
            float fadeDuration = 0.5f;

            // Dodaj wiadomoœæ do konsoli
            ShowConsoleMessage(logEntry.messages[Random.Range(0, logEntry.messages.Length)], messageDuration, fadeDuration, "#00E700");

            // Usuñ ten log z dostêpnej listy, ¿eby nie pojawi³ siê ponownie
            availableLogs.Remove(logEntry);

            // Czekaj, zanim poka¿emy kolejny log
            yield return new WaitForSeconds(messageDuration - startNextLogBefore);
        }

        // Tylko po zakoñczeniu wszystkich logów — dodaj wiadomoœæ koñcow¹
        ShowConsoleMessage(">>>Terminal gotowy.", 5f, 0.5f, "#FFD200");

        canInteract = true;
    }


    private LogEntry GetRandomLogWithProbability(List<LogEntry> availableLogs)
    {
        float totalProbability = 0f;

        // Obliczamy sumê wszystkich prawdopodobieñstw
        foreach (var log in availableLogs)
        {
            totalProbability += log.probability;
        }

        // Losowanie, które logi wybraæ na podstawie prawdopodobieñstwa
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

        // W przypadku b³êdu, zwróæ pierwszy dostêpny log
        return availableLogs[0];
    }
}