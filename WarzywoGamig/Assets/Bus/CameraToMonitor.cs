using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CameraToMonitor : MonoBehaviour
{
    public PlayerMovement playerMovementScript;
    public MouseLook mouseLookScript;
    public SceneChanger sceneChanger;
    public HoverMessage monitorHoverMessage;
    public TreasureRefiner treasureRefiner;
    public GameObject crossHair;
    public GameObject monitorCanvas;

    public Transform player;
    public Transform finalCameraRotation;
    public float interactionRange = 5f;
    public float cameraMoveSpeed = 5f;

    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    public bool canInteract = false;
    private bool isInteracting = false;
    private bool isCameraMoving = false;
    public static bool CanUseMenu = true; // Flaga, kt�ra kontroluje, czy menu jest dost�pne

    [Header("UI � Konsola monitora")]
    public TextMeshProUGUI consoleTextDisplay;
    public TMP_InputField inputField; // Pole tekstowe dla wpisywania komend
    private Queue<ConsoleMessage> messageQueue = new Queue<ConsoleMessage>();
    public int maxMessages = 5;

    public float letterDisplayDelay = 0.05f; // Op�nienie mi�dzy literami w sekundach
    public float cursorBlinkInterval = 0.5f;

    [Header("Logi w r�nych j�zykach")]
    public List<LogEntry> logEntriesEnglish = new List<LogEntry>();
    public List<LogEntry> logEntriesPolish = new List<LogEntry>();
    public List<LogEntry> logEntriesGerman = new List<LogEntry>();

    public List<LogEntry> logEntries;
    public static List<LogEntry> sharedHelpLogs = new List<LogEntry>();

    // S�ownik komend do metod
    private Dictionary<string, CommandData> commandDictionary;

    private Coroutine logSequenceCoroutine = null;
    private string pendingCommand = null;
    private bool isTerminalInterrupted = false;

    private void Start()
    {
        if (monitorCanvas != null)
        {
            monitorCanvas.SetActive(false);
        }

        if (consoleTextDisplay == null)
        {
            Debug.LogError("Brak przypisanego TextMeshProUGUI dla konsoli!");
        }

        if (inputField != null)
        {
            inputField.gameObject.SetActive(false); // Ukryj pole edycji na starcie
            inputField.onEndEdit.AddListener(HandleCommandInput); // Dodaj obs�ug� zako�czenia edycji
            inputField.characterLimit = 30;//Ogranicz liczb� znak�w do 20 (zmie� na warto��, kt�r� chcesz)
        }

        // Je�li trzeba, przypisz referencj� do obiektu ButtonMethods (je�li jeszcze nie zosta�o przypisane)
        if (sceneChanger == null)
        {
            sceneChanger = UnityEngine.Object.FindAnyObjectByType<SceneChanger>();
        }

        // Subskrybuj zdarzenie zmiany j�zyka
        LanguageManager.Instance.OnLanguageChanged += HandleLanguageChanged;

        // Inicjalizuj komendy dla bie��cego j�zyka
        InitializeLocalizedCommands();

        UpdateLogEntriesLanguage(); // Ustaw pocz�tkowy j�zyk
    }

    private void InitializeLocalizedCommands()
    {
        // Tworzymy pusty s�ownik komend
        commandDictionary = new Dictionary<string, CommandData>();

        // Ustawienie komend zale�nie od j�zyka
        string localizedHome = LanguageManager.Instance.currentLanguage switch
        {
            LanguageManager.Language.Polski => "dom", // Polski
            LanguageManager.Language.Deutsch => "", // Niemiecki
            _ => "home" // Angielski
        };

        string localizedMain = LanguageManager.Instance.currentLanguage switch
        {
            LanguageManager.Language.Polski => "trasa", // Polski
            LanguageManager.Language.Deutsch => "", // Niemiecki
            _ => "route" // Angielski
        };

        string localizedHelp = LanguageManager.Instance.currentLanguage switch
        {
            LanguageManager.Language.Polski => "pomoc", // Polski
            LanguageManager.Language.Deutsch => "", // Niemiecki
            _ => "help" // Angielski
        };

        // Dodanie komend do s�ownika
        commandDictionary[localizedHome] = new CommandData(() => ExitTerminalAndChangeScene("Home", 3f), true);
        commandDictionary[localizedMain] = new CommandData(() => ExitTerminalAndChangeScene("Main", 3f), true);
        commandDictionary[localizedHelp] = new CommandData(() => StartCoroutine(DisplayHelpLogs()), false);
    }

    public void HandleLanguageChanged()
    {
        // Ponowna inicjalizacja komend po zmianie j�zyka
        InitializeLocalizedCommands();
    }

    private void OnDestroy()
    {
        // Usu� subskrypcj� zdarzenia, aby unikn�� b��d�w
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged -= HandleLanguageChanged;
        }
    }

    // Dynamiczne przypisanie log�w wg j�zyka
    private void UpdateLogEntriesLanguage()
    {
        switch (LanguageManager.Instance.currentLanguage)
        {
            case LanguageManager.Language.Polski:
                logEntries = new List<LogEntry>(logEntriesPolish);
                break;
            case LanguageManager.Language.Deutsch:
                logEntries = new List<LogEntry>(logEntriesGerman);
                break;
            default:
                logEntries = new List<LogEntry>(logEntriesEnglish);
                break;
        }
    }

    public void ExitTerminalAndChangeScene(string targetSceneName, float delaySeconds = 3f)
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (currentScene == targetSceneName)
        {
            ShowConsoleMessage(string.Format(LanguageManager.Instance.GetLocalizedMessage("alreadyInScene"), targetSceneName), "#FF0000");
            return; // Przerywamy � nie wychodzimy z terminala ani nie zmieniamy sceny
        }

        StartCoroutine(ExitAndDelaySceneChange(targetSceneName, delaySeconds));
    }

    private IEnumerator ExitAndDelaySceneChange(string sceneName, float delay)
    {
        // Znajd� TreasureRefiner w scenie
        TreasureRefiner treasureRefiner = UnityEngine.Object.FindFirstObjectByType<TreasureRefiner>();

        // Sprawd�, czy TreasureRefiner istnieje i czy rafinacja jest w toku
        if (treasureRefiner != null && treasureRefiner.isSpawning)
        {
            Debug.Log("Rafinacja w toku. Nie mo�na zmieni� sceny.");
            ShowConsoleMessage(LanguageManager.Instance.GetLocalizedMessage("refiningBlocked"), "#FF0000");
            yield break; // Zatrzymaj korutyn�, je�li rafinacja w toku
        }

        // Je�li nie ma rafinacji, przechodzimy do dalszych operacji
        yield return StartCoroutine(MoveCameraBackToOriginalPosition());

        CanUseMenu = false;

        // Zablokuj mo�liwo�� interakcji
        DisablePlayerMovementAndMouseLook();

        ShowConsoleMessage(string.Format(LanguageManager.Instance.GetLocalizedMessage("changingScene"), sceneName, delay), "#FFD200");

        yield return new WaitForSeconds(delay);

        // Zmie� scen�
        if (sceneChanger != null)
        {
            sceneChanger.TryChangeScene(sceneName);
        }
        else
        {
            Debug.LogError("Brak referencji do SceneChanger!");
        }

        // Po zmianie sceny, przywr�� kontrolki
        EnablePlayerMovementAndMouseLook();
        CanUseMenu = true;
    }

    private IEnumerator DisplayHelpLogs()
    {
        if (logSequenceCoroutine != null)
        {
            StopCoroutine(logSequenceCoroutine);
        }

        if (LanguageManager.Instance == null)
        {
            Debug.LogWarning("LanguageManager is not initialized.");
            yield break;
        }

        if (sharedHelpLogs.Count == 0)
        {
            // Pauza przed wy�wietleniem wiadomo�ci
            yield return new WaitForSeconds(1f);

            ShowConsoleMessage($">>> {LanguageManager.Instance.GetLocalizedMessage("command_home_desc")}", "#00E700");
            ShowConsoleMessage($">>> {LanguageManager.Instance.GetLocalizedMessage("command_main_desc")}", "#00E700");
            yield break;
        }

        // Skopiuj, �eby oryginalna lista by�a bezpieczna
        List<LogEntry> copiedLogs = new List<LogEntry>(sharedHelpLogs);
        logSequenceCoroutine = StartCoroutine(DisplayLogsSequence(copiedLogs));
    }


    public static void AddHelpLog(string[] messages, float delayAfterPrevious = 0.5f)
    {
        sharedHelpLogs.Add(new LogEntry
        {
            messages = messages,
            probability = 100f,
            delayAfterPrevious = delayAfterPrevious
        });
    }

    //void Start()              D O D A W A N I E
    //{
    //    CameraToMonitor.AddHelpLog(new string[]
    //    {
    //    ">>> Komenda 'refine' � rozpocznij rafinacj� skarb�w.",
    //    ">>> Komenda 'status' � sprawd� stan rafinacji."
    //    });
    //}


    void Update()
    {
        //float distanceToInteraction = Vector3.Distance(player.position, finalCameraRotation.position);

        //if (distanceToInteraction <= interactionRange)
        //{
        //    if (Input.GetKeyDown(KeyCode.E) && !isInteracting && !isCameraMoving)
        //    {
        //        originalCameraPosition = Camera.main.transform.position;
        //        originalCameraRotation = Camera.main.transform.rotation;
        //        StartCoroutine(MoveCameraToPosition());
        //        ClearMonitorConsole();
        //    }
        //    else if ((Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape)) && isInteracting && !isCameraMoving)
        //    {
        //        StartCoroutine(MoveCameraBackToOriginalPosition());
        //    }
        //}
        //else if (isInteracting && !isCameraMoving)
        //{
        //    StartCoroutine(MoveCameraBackToOriginalPosition());
        //}


        if ((Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape)) && isInteracting && !isCameraMoving)
        {
            StartCoroutine(MoveCameraBackToOriginalPosition());
        }
    }

    public void UseMonitor()
    {
        if (!isInteracting && !isCameraMoving)
        {
            originalCameraPosition = Camera.main.transform.position;
            originalCameraRotation = Camera.main.transform.rotation;
            StartCoroutine(MoveCameraToPosition());
            ClearMonitorConsole();
            UpdateLogEntriesLanguage();
        }
    }

    IEnumerator MoveCameraToPosition()
    {
        isTerminalInterrupted = false;
        CanUseMenu = false;
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

        DisablePlayerMovementAndMouseLook();
        isCameraMoving = true;

        Vector3 startPos = Camera.main.transform.position;
        Quaternion startRot = Camera.main.transform.rotation;

        Vector3 targetCameraPosition = finalCameraRotation.position;
        Quaternion targetCameraRotation = finalCameraRotation.rotation;

        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            Camera.main.transform.position = Vector3.Lerp(startPos, targetCameraPosition, elapsedTime);
            Camera.main.transform.rotation = Quaternion.Slerp(startRot, targetCameraRotation, elapsedTime);
            elapsedTime += Time.deltaTime * cameraMoveSpeed;
            yield return null;
        }

        Camera.main.transform.position = targetCameraPosition;
        Camera.main.transform.rotation = targetCameraRotation;

        isCameraMoving = false;

        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;

        ShowConsoleMessage($">>> {LanguageManager.Instance.GetLocalizedMessage("initializingTerminal")}", "#00E700");

        yield return new WaitForSeconds(1f);

        StartLogSequence();
    }

    IEnumerator MoveCameraBackToOriginalPosition()
    {
        isInteracting = false;
        isTerminalInterrupted = true;

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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        DisablePlayerMovementAndMouseLook();
        isCameraMoving = true;

        Vector3 startPos = Camera.main.transform.position;
        Quaternion startRot = Camera.main.transform.rotation;

        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            Camera.main.transform.position = Vector3.Lerp(startPos, originalCameraPosition, elapsedTime);
            Camera.main.transform.rotation = Quaternion.Slerp(startRot, originalCameraRotation, elapsedTime);
            elapsedTime += Time.deltaTime * cameraMoveSpeed;
            yield return null;
        }

        Camera.main.transform.position = originalCameraPosition;
        Camera.main.transform.rotation = originalCameraRotation;

        EnablePlayerMovementAndMouseLook();
        isCameraMoving = false;

        if (logSequenceCoroutine != null)
        {
            StopCoroutine(logSequenceCoroutine);
            logSequenceCoroutine = null;
        }

        ClearMonitorConsole();
        CanUseMenu = true;
    }

    private void DisablePlayerMovementAndMouseLook()
    {
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }

        if (mouseLookScript != null)
        {
            mouseLookScript.enabled = false;
        }
    }

    private void EnablePlayerMovementAndMouseLook()
    {
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }

        if (mouseLookScript != null)
        {
            mouseLookScript.enabled = true;
        }
    }

    public void ShowConsoleMessage(string key, string hexColor)
    {
        //Debug.Log($"[ShowConsoleMessage] Otrzymany klucz: {key}");

        string translatedMessage = LanguageManager.Instance.GetLocalizedMessage(key);

        if (translatedMessage.StartsWith("[[UNKNOWN KEY"))
        {
            Debug.Log($"[ShowConsoleMessage] Nieznany klucz: {key}");
        }
        else
        {
            //Debug.Log($"[ShowConsoleMessage] Przet�umaczona wiadomo��: {translatedMessage}");
        }

        Color color = Color.white;
        if (ColorUtility.TryParseHtmlString(hexColor, out Color parsedColor))
        {
            color = parsedColor;
        }

        // Wy�wietl wiadomo��
        StartCoroutine(TypeMessageCoroutine(translatedMessage, color));
    }

    private IEnumerator TypeMessageCoroutine(string messageKey, Color color)
    {
        // Pobranie wiadomo�ci z LanguageManager na podstawie messageKey
        string fullMessage = LanguageManager.Instance.GetLocalizedMessage(messageKey);

        string currentLine = "";

        // Iteracja po wszystkich znakach wiadomo�ci
        for (int i = 0; i < fullMessage.Length; i++)
        {
            // Sprawdzamy, czy terminal zosta� przerwany
            if (isTerminalInterrupted)
                yield break; // Przerwij wy�wietlanie, je�eli terminal zosta� przerwany

            currentLine += fullMessage[i]; // Dodajemy nowy znak do linii
            UpdateConsolePreview(currentLine, color); // Aktualizujemy podgl�d konsoli

            // Op�nienie pomi�dzy literami
            yield return new WaitForSeconds(letterDisplayDelay);
        }

        // Je�eli terminal zosta� przerwany, przerywamy dalsze dzia�anie
        if (isTerminalInterrupted)
            yield break;

        // Tworzymy obiekt finalnej wiadomo�ci
        var finalMessage = new ConsoleMessage(fullMessage, Time.time, color);

        // Dodajemy wiadomo�� do kolejki
        messageQueue.Enqueue(finalMessage);

        // Je�eli liczba wiadomo�ci przekroczy maksymaln� dozwolon�, usuwamy najstarsz�
        while (messageQueue.Count > maxMessages)
            messageQueue.Dequeue();

        // Aktualizujemy tekst na konsoli
        UpdateConsoleText();
    }

    private void UpdateConsolePreview(string typingLine, Color color)
    {
        if (consoleTextDisplay == null) return;

        List<string> lines = new List<string>();

        // Dodaj istniej�ce wiadomo�ci
        foreach (var msg in messageQueue)
        {
            string hexColor = ColorUtility.ToHtmlStringRGB(msg.color);
            lines.Add($"<color=#{hexColor}>{msg.message}</color>");
        }

        lines.Add($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{typingLine}</color>");

        // Ustaw tekst w UI
        consoleTextDisplay.text = string.Join("\n", lines);
    }

    private void UpdateConsoleText()
    {
        if (consoleTextDisplay == null) return;

        List<string> lines = new List<string>();

        // Dodaj wszystkie wiadomo�ci w kolejno�ci FIFO
        foreach (var msg in messageQueue)
        {
            string hexColor = ColorUtility.ToHtmlStringRGB(msg.color);
            lines.Add($"<color=#{hexColor}>{msg.message}</color>");
        }

        // ��cz tekst w jeden string
        consoleTextDisplay.text = string.Join("\n", lines);
    }

    public void ClearMonitorConsole()
    {
        messageQueue.Clear();

        if (consoleTextDisplay != null)
        {
            consoleTextDisplay.text = "";
        }
    }
    private void HandleCommandInput(string command)
    {
        command = command.ToLower().Trim();

        //Debug.Log($"[HandleCommandInput] Otrzymana komenda: {command}");

        // Sprawdzamy, czy gracz jest ju� w tej samej scenie
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToLower();

        if (command == currentScene)
        {
            // Gracz jest ju� w tej samej scenie, wy�wietlamy komunikat, ale nie wykonujemy komendy
            ShowConsoleMessage(LanguageManager.Instance.GetLocalizedMessage("alreadyInScene"), "#FFD200");
            inputField.text = "";
            inputField.ActivateInputField();
            return; // Ko�czymy funkcj�, bez dalszego przetwarzania
        }

        // Je�li mamy oczekuj�ce polecenie (potwierdzenie), sprawdzamy odpowied�
        if (!string.IsNullOrEmpty(pendingCommand))
        {
            //Debug.Log($"[HandleCommandInput] Oczekuj�ce polecenie: {pendingCommand}");

            string confirmYesKey = LanguageManager.Instance.GetLocalizedMessage("confirmYesKey").ToLower();
            string confirmNoKey = LanguageManager.Instance.GetLocalizedMessage("confirmNoKey").ToLower();

            // Sprawdzamy odpowied�
            if (command == confirmYesKey)
            {
                ShowConsoleMessage(string.Format(LanguageManager.Instance.GetLocalizedMessage("executingCommand"), pendingCommand), "#00E700");
                commandDictionary[pendingCommand].command.Invoke();
            }
            else if (command == confirmNoKey)
            {
                ShowConsoleMessage(LanguageManager.Instance.GetLocalizedMessage("commandCancelled"), "#FF0000");
            }
            else
            {
                ShowConsoleMessage(LanguageManager.Instance.GetLocalizedMessage("invalidResponse"), "#FF0000");
            }

            // Resetowanie oczekuj�cej komendy
            pendingCommand = null;
            inputField.text = "";
            inputField.ActivateInputField();
            return;
        }

        if (!string.IsNullOrEmpty(command))
        {
            //Debug.Log($"[HandleCommandInput] Sprawdzanie komendy: {command}");
            ShowConsoleMessage($">>> {command}", "#FFFFFF");

            if (commandDictionary.ContainsKey(command))
            {
                var data = commandDictionary[command];

                if (data.requiresConfirmation)
                {
                    // Sprawdzamy, czy gracz pr�buje przenie�� si� do tej samej sceny
                    if (command == currentScene)
                    {
                        // Je�li gracz ju� jest w tej scenie, pomijamy potwierdzenie i nie wykonujemy komendy
                        ShowConsoleMessage(LanguageManager.Instance.GetLocalizedMessage("alreadyInScene"), "#FFD200");
                    }
                    else
                    {
                        // Wy�wietlamy potwierdzenie, je�li komenda wymaga potwierdzenia
                        ShowConsoleMessage($"{LanguageManager.Instance.GetLocalizedMessage("confirmCommand")} [{LanguageManager.Instance.GetLocalizedMessage("confirmYesKey")}/{LanguageManager.Instance.GetLocalizedMessage("confirmNoKey")}]", "#FFD200");
                        pendingCommand = command;
                    }
                }
                else
                {
                    // Wykonujemy komend�, je�li nie wymaga potwierdzenia
                    data.command.Invoke();
                }
            }
            else
            {
                //Debug.Log($"[HandleCommandInput] Nieznana komenda: {command}");
                ShowConsoleMessage(LanguageManager.Instance.GetLocalizedMessage("unknownCommand"), "#FF0000");
            }

            inputField.text = "";
            inputField.ActivateInputField();
        }
    }

    private void StartLogSequence()
    {


        canInteract = false;
        List<LogEntry> availableLogs = new List<LogEntry>(logEntries);

        if (logSequenceCoroutine != null)
            StopCoroutine(logSequenceCoroutine);

        logSequenceCoroutine = StartCoroutine(DisplayLogsSequence(availableLogs));
    }

    private IEnumerator DisplayLogsSequence(List<LogEntry> availableLogs)
    {
        while (availableLogs.Count > 0)
        {
            LogEntry logEntry = GetRandomLogWithProbability(availableLogs);

            if (logEntry.messages.Length > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, logEntry.messages.Length);
                ShowConsoleMessage(logEntry.messages[randomIndex], "#00E700");
            }
            else
            {
                ShowConsoleMessage(">>> No messages to display.", "#FF0000");
            }

            availableLogs.Remove(logEntry);

            yield return new WaitForSeconds(logEntry.delayAfterPrevious);
        }

        // Wy�wietl komunikaty ko�cowe w odpowiednim j�zyku
        switch (LanguageManager.Instance.currentLanguage)
        {
            case LanguageManager.Language.Polski:
                ShowConsoleMessage(">>> Pomoc - lista dost�pnych komend.", "#00E700");
                yield return new WaitForSeconds(1f);
                ShowConsoleMessage(">>> Terminal gotowy.", "#FFD200");
                break;

            case LanguageManager.Language.Deutsch:
                ShowConsoleMessage(">>> Hilfe - verf�gbare Befehle.", "#00E700");
                yield return new WaitForSeconds(1f);
                ShowConsoleMessage(">>> Terminal bereit.", "#FFD200");
                break;

            default:
                ShowConsoleMessage(">>> Help - list of available commands.", "#00E700");
                yield return new WaitForSeconds(1f);
                ShowConsoleMessage(">>> Terminal ready.", "#FFD200");
                break;
        }

        yield return new WaitForSeconds(1f);

        canInteract = true;

        if (inputField != null)
        {
            inputField.gameObject.SetActive(true);
            inputField.ActivateInputField();
        }
    }

    private LogEntry GetRandomLogWithProbability(List<LogEntry> availableLogs)
    {
        // Sumujemy wszystkie prawdopodobie�stwa
        float totalProbability = 0f;
        foreach (var log in availableLogs)
        {
            totalProbability += log.probability;
        }

        // Generujemy losow� warto��
        float randomValue = UnityEngine.Random.Range(0f, totalProbability);
        float cumulativeProbability = 0f;

        // Iterujemy po dost�pnych logach, aby znale�� ten na podstawie losowego prawdopodobie�stwa
        foreach (var log in availableLogs)
        {
            cumulativeProbability += log.probability;

            // Je�li randomValue mie�ci si� w tym przedziale, zwr�� log
            if (randomValue <= cumulativeProbability)
            {
                return log;
            }
        }

        // Je�li nic nie znaleziono, zwr�� pierwszy log (to mo�e by� domy�lny przypadek)
        return availableLogs[0];
    }

    [System.Serializable]
    public class ConsoleMessage
    {
        public string message;
        public float timeAdded;
        public Color color;

        public ConsoleMessage(string message, float timeAdded, Color color)
        {
            this.message = message;
            this.timeAdded = timeAdded;
            this.color = color;
        }
    }

    [System.Serializable]
    public class LogEntry
    {
        public string[] messages;
        [Range(0f, 100f)]
        public float probability;
        public float delayAfterPrevious = 1f;
    }

    [System.Serializable]
    public class CommandData
    {
        public Action command;
        public bool requiresConfirmation;

        public CommandData(Action command, bool requiresConfirmation)
        {
            this.command = command;
            this.requiresConfirmation = requiresConfirmation;
        }
    }
}