using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public GameObject monitorMainPanel;
    public GameObject monitorBlackPanel;
    public GameObject monitorRiddlePanel;

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
    public TextMeshProUGUI modelText; // Dodatkowy tekst, kt�ry ma zmieni� tre�� na losowe znaki
    private Queue<ConsoleMessage> messageQueue = new Queue<ConsoleMessage>();
    public int maxMessages = 5;

    public float letterDisplayDelay = 0.05f; // Op�nienie mi�dzy literami w sekundach
    public float cursorBlinkInterval = 0.5f;

    [Header("Panele losowych znak�w")]
    public GameObject randomPanel; // Pierwszy panel
    public TextMeshProUGUI randomPanelText; // Tekst na pierwszym panelu
    public int numberOfRandomCharactersPanel1 = 50; // Liczba znak�w na pierwszym panelu

    public GameObject randomPanel2; // Drugi panel
    public TextMeshProUGUI randomPanel2Text; // Tekst na drugim panelu
    public int numberOfRandomCharactersPanel2 = 100; // Liczba znak�w na drugim panelu

    public float panelDelay = 2f; // Op�nienie przed wy�wietleniem drugiego panelu
    public float randomPanelDuration = 5f; // Czas trwania obu paneli (liczony od pojawienia si� drugiego panelu)
    public float typingSpeed = 0.05f; // Pr�dko�� "pisania" tekstu

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

    public bool securedMonitor = false;
    public string generatedPassword; // Wygenerowane has�o

    private void Start()
    {
        if (modelText != null)
        {
            modelText.text = "Siegdu v2.7_4_1998";
        }

        if (randomPanel != null)
        {
            randomPanel.SetActive(false);
        }

        if (randomPanel2 != null)
        {
            randomPanel2.SetActive(false);
        }

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

        if (securedMonitor)
        {
            GeneratePassword(); // Generuj has�o przy starcie terminala
        }

        // Subskrybuj zdarzenie zmiany j�zyka
        LanguageManager.Instance.OnLanguageChanged += HandleLanguageChanged;

        // Inicjalizuj komendy dla bie��cego j�zyka
        InitializeLocalizedCommands();

        UpdateLogEntriesLanguage(); // Ustaw pocz�tkowy j�zyk
    }
    private void GeneratePassword()
    {
        const string digits = "0123456789";
        generatedPassword = new string(Enumerable.Range(0, 4).Select(_ => digits[UnityEngine.Random.Range(0, digits.Length)]).ToArray());
    }

    private void InitializeLocalizedCommands()
    {
        // Tworzymy pusty s�ownik komend
        commandDictionary = new Dictionary<string, CommandData>();

        // Dodajemy komend� Hack/hakuj w zale�no�ci od j�zyka
        string localizedHack = LanguageManager.Instance.currentLanguage switch
        {
            LanguageManager.Language.Polski => "hakuj", // Polski
            LanguageManager.Language.Deutsch => "hacken", // Niemiecki
            _ => "hack" // Angielski
        };

        commandDictionary[localizedHack] = new CommandData(() => DisplayRandomPanels(), false);

        // Dodanie komendy wyj�cia zale�nej od j�zyka
        string localizedExit = LanguageManager.Instance.currentLanguage switch
        {
            LanguageManager.Language.Polski => "zamknij", // Polski
            LanguageManager.Language.Deutsch => "beenden", // Niemiecki
            _ => "exit" // Angielski
        };

        commandDictionary[localizedExit] = new CommandData(() => StartCoroutine(MoveCameraBackToOriginalPosition()), false);

        // Je�li terminal jest zabezpieczony, nie dodajemy innych komend
        if (securedMonitor)
        {
            //Debug.Log("Terminal jest zabezpieczony. Komendy ograniczone.");
            return;
        }

        // Ustawienie komend zale�nie od j�zyka
        string localizedHome = LanguageManager.Instance.currentLanguage switch
        {
            LanguageManager.Language.Polski => "dom", // Polski
            LanguageManager.Language.Deutsch => "haus", // Niemiecki
            _ => "home" // Angielski
        };

        string localizedMain = LanguageManager.Instance.currentLanguage switch
        {
            LanguageManager.Language.Polski => "trasa", // Polski
            LanguageManager.Language.Deutsch => "route", // Niemiecki
            _ => "route" // Angielski
        };

        string localizedHelp = LanguageManager.Instance.currentLanguage switch
        {
            LanguageManager.Language.Polski => "pomoc", // Polski
            LanguageManager.Language.Deutsch => "hilfe", // Niemiecki
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
            yield return new WaitForSeconds(0.2f);
            ShowConsoleMessage($">>> {LanguageManager.Instance.GetLocalizedMessage("terminalExit")}", "#00E700");
            yield return new WaitForSeconds(0.2f);
            ShowConsoleMessage($">>> {LanguageManager.Instance.GetLocalizedMessage("hack")}", "#00E700");
            yield return new WaitForSeconds(0.2f);
            ShowConsoleMessage($">>> {LanguageManager.Instance.GetLocalizedMessage("locations")}", "#00E700");
            yield return new WaitForSeconds(0.2f);
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
        if ((Input.GetKeyDown(KeyCode.Escape) && isInteracting && !isCameraMoving))
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
        // Je�li terminal jest zabezpieczony, sprawd� has�o
        if (securedMonitor)
        {
            // Obs�uga komendy wyj�cia
            if (commandDictionary.ContainsKey(command))
            {
                var data = commandDictionary[command];
                data.command.Invoke(); // Wywo�anie komendy wyj�cia
                inputField.text = "";  // Wyczy�� pole tekstowe
                inputField.ActivateInputField(); // Ponownie aktywuj pole tekstowe
                return; // Zako�cz dalsze przetwarzanie
            }

            if (command == generatedPassword) // Sprawdzenie poprawno�ci has�a
            {
                ShowConsoleMessage(">>> Password correct. Terminal unlocked.", "#00E700");
                securedMonitor = false; // Odblokowanie terminala
                ClearMonitorConsole(); // Wyczy�� logi
                InitializeLocalizedCommands(); // Inicjalizacja komend po odblokowaniu
                StartLogSequence(); // Rozpocznij sekwencj� od nowa

                if (modelText != null)
                {
                    modelText.text = "Siegdu v2.7_4_1998";
                }

                // Usu� komend� "hack" lub jej odpowiednik z listy komend
                string localizedHackCommand = LanguageManager.Instance.currentLanguage switch
                {
                    LanguageManager.Language.Polski => "hakuj", // Polski
                    LanguageManager.Language.Deutsch => "hacken", // Niemiecki
                    _ => "hack" // Angielski
                };

                if (commandDictionary.ContainsKey(localizedHackCommand))
                {
                    commandDictionary.Remove(localizedHackCommand);
                    ShowConsoleMessage(LanguageManager.Instance.GetLocalizedMessage("unknownCommand"), "#FF0000");
                }
            }
            else
            {
                ShowConsoleMessage(">>> Incorrect password. Try again.", "#FF0000");
            }

            inputField.text = ""; // Wyczy�� pole tekstowe
            inputField.ActivateInputField(); // Ponownie aktywuj pole tekstowe
            return; // Zako�cz dalsze przetwarzanie
        }

        // Reszta funkcji HandleCommandInput pozostaje bez zmian
        command = command.ToLower().Trim();

        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToLower();

        if (command == currentScene)
        {
            ShowConsoleMessage(LanguageManager.Instance.GetLocalizedMessage("alreadyInScene"), "#FFD200");
            inputField.text = "";
            inputField.ActivateInputField();
            return;
        }

        if (!string.IsNullOrEmpty(pendingCommand))
        {
            string confirmYesKey = LanguageManager.Instance.GetLocalizedMessage("confirmYesKey").ToLower();
            string confirmNoKey = LanguageManager.Instance.GetLocalizedMessage("confirmNoKey").ToLower();

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

            pendingCommand = null;
            inputField.text = "";
            inputField.ActivateInputField();
            return;
        }

        if (!string.IsNullOrEmpty(command))
        {
            ShowConsoleMessage($">>> {command}", "#FFFFFF");

            if (commandDictionary.ContainsKey(command))
            {
                var data = commandDictionary[command];

                if (data.requiresConfirmation)
                {
                    if (command == currentScene)
                    {
                        ShowConsoleMessage(LanguageManager.Instance.GetLocalizedMessage("alreadyInScene"), "#FFD200");
                    }
                    else
                    {
                        ShowConsoleMessage($"{LanguageManager.Instance.GetLocalizedMessage("confirmCommand")} [{LanguageManager.Instance.GetLocalizedMessage("confirmYesKey")}/{LanguageManager.Instance.GetLocalizedMessage("confirmNoKey")}]", "#FFD200");
                        pendingCommand = command;
                    }
                }
                else
                {
                    data.command.Invoke();
                }
            }
            else
            {
                ShowConsoleMessage(LanguageManager.Instance.GetLocalizedMessage("unknownCommand"), "#FF0000");
            }

            inputField.text = "";
            inputField.ActivateInputField();
        }
    }

    private void DisplayRandomPanels()
    {
        // Ukryj consoleTextDisplay
        if (consoleTextDisplay != null)
        {
            consoleTextDisplay.gameObject.SetActive(false);
        }

        // Aktywuj pierwszy panel
        randomPanel.SetActive(true);
        StartCoroutine(TypeRandomCharacters(randomPanelText, numberOfRandomCharactersPanel1));

        // Wy�wietl drugi panel po okre�lonym op�nieniu
        StartCoroutine(ShowSecondPanelAfterDelay());
    }

    private IEnumerator ShowSecondPanelAfterDelay()
    {
        yield return new WaitForSeconds(panelDelay);

        // Aktywuj drugi panel
        randomPanel2.SetActive(true);

        // Wy�wietlaj losowe znaki na drugim panelu
        StartCoroutine(TypeRandomCharacters(randomPanel2Text, numberOfRandomCharactersPanel2));

        ModelNameRandomSymbols();

        // Po zako�czeniu animacji drugiego panelu, odlicz czas trwania obu paneli
        yield return new WaitForSeconds(randomPanelDuration);

        // Ukryj oba panele
        randomPanel.SetActive(false);
        randomPanel2.SetActive(false);

        // Aktywuj pole tekstowe po zamkni�ciu paneli
        if (inputField != null)
        {
            inputField.ActivateInputField();
        }

        // Przywr�� consoleTextDisplay
        if (consoleTextDisplay != null)
        {
            consoleTextDisplay.gameObject.SetActive(true);
        }
    }

    private IEnumerator TypeRandomCharacters(TextMeshProUGUI textComponent, int numberOfCharacters)
    {
        // Generuj losowe znaki
        string randomText = GenerateRandomText(numberOfCharacters);

        // Wy�wietlaj znaki jeden po drugim
        foreach (char c in randomText)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(typingSpeed); // Odczekaj przed dodaniem kolejnego znaku
        }
    }

    private string GenerateRandomText(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < length; i++)
        {
            char randomChar = chars[UnityEngine.Random.Range(0, chars.Length)];
            sb.Append(randomChar);
        }

        return sb.ToString();
    }

    private void ModelNameRandomSymbols()
    {
        if (modelText != null)
        {
            modelText.text = "******+^!./_%_"+ generatedPassword+"";
                           //"Siegdu v2.7_4_1998";
        }
    }

    private void StartLogSequence()
    {
        if (inputField != null)
        {
            inputField.gameObject.SetActive(false);
        }

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
                if (securedMonitor)
                {
                    ShowConsoleMessage(">>> Terminal gotowy. Wprowad� swoje has�o.", "#FFD200");
                }
                else
                {
                    ShowConsoleMessage(">>> Terminal gotowy.", "#FFD200");
                }
                break;

            case LanguageManager.Language.Deutsch:
                ShowConsoleMessage(">>> Hilfe - verf�gbare Befehle.", "#00E700");
                yield return new WaitForSeconds(1f);
                if (securedMonitor)
                {
                    ShowConsoleMessage(">>> Terminal bereit. Geben Sie Ihr Passwort ein.", "#FFD200");
                }
                else
                {
                    ShowConsoleMessage(">>> Terminal bereit.", "#FFD200");
                }
                break;

            default:
                ShowConsoleMessage(">>> Help - list of available commands.", "#00E700");
                yield return new WaitForSeconds(1f);
                if (securedMonitor)
                {
                    ShowConsoleMessage(">>> Terminal ready. Enter your password.", "#FFD200");
                }
                else
                {
                    ShowConsoleMessage(">>> Terminal ready.", "#FFD200");
                }
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