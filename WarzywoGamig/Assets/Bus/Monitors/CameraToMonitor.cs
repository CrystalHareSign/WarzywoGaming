using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;


public class CameraToMonitor : MonoBehaviour
{
    public TerminalEmission terminalEmission;
    public PlayerMovement playerMovementScript;
    public PlayerInteraction playerInteraction;
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
    public static bool CanUseMenu = true; // Flaga, która kontroluje, czy menu jest dostêpne
    public bool isUsingMonitor = false;
    private bool flashlightWasOnBeforeMonitor = false;

    [Header("Info")]
    public bool hasInfo = false;
    public float randomKB;
    public string monitorInfoText_EN;
    public string monitorInfoText_PL;
    public string monitorInfoText_DE;
    public string localizedInfoText;

    [Header("Main monitor")]
    public bool mainMonitor = false;

    // UnityEvents pozwalaj¹ na przypisanie funkcji w Inspectorze
    [Header("Custom Actions")]
    public UnityEvent onStartFunction;
    public UnityEvent onSaveFunction;

    [Header("Start")]
    public bool hasStartFunction = false;

    [Header("Save monitor")]
    public bool saveMonitor = false;

    [Header("ID monitora")]
    public string monitorID;
    public bool saveUnlockState = true;

    [Header("Riddle monitor")]
    public bool riddleMonitor = false;
    public bool securedMonitor = false;
    public string generatedPassword; // Wygenerowane has³o
    public string monitorFunctionText_EN;
    public string monitorFunctionText_PL;
    public string monitorFunctionText_DE;
    public string localizedFunctionText;

    [Header("Mini-Game Settings")]
    public int gridSize = 15; // Rozmiar siatki (np. 10x10)
    private string[,] grid; // Tablica reprezentuj¹ca siatkê
    private Vector2Int targetCoordinate; // Cel w grze
    private int remainingAttempts;
    private bool isMiniGameActive;
    private GameObject cursor; // Obiekt kursora (podœwietlenie)
    private Vector2Int cursorPosition = new Vector2Int(0, 0); // Pozycja kursora
    public bool hasWonGame = false;
    private List<string> logHistory = new List<string>(); // Lista do przechowywania logów
    public bool isTimerEnabled = true;
    public float gameTime = 30f; // Czas gry w sekundach (np. 30s)
    private bool isGameForceEnded = false; // Zresetuj flagê na pocz¹tku nowej gry
    private float timeRemaining;
    private Coroutine timerCoroutine;
    private Coroutine blinkCoroutine; // Przechowuje referencjê do korutyny BlinkTargetCell

    [Header("UI – Konsola monitora")]
    public TextMeshProUGUI consoleTextDisplay;
    public TMP_InputField inputField; // Pole tekstowe dla wpisywania komend
    public TextMeshProUGUI modelText; // Dodatkowy tekst, który ma zmieniæ treœæ na losowe znaki
    private Queue<ConsoleMessage> messageQueue = new Queue<ConsoleMessage>();
    private Queue<(string messageKey, Color color)> messageProcessingQueue = new();
    private bool isProcessingMessage = false;
    public int maxMessages = 5;

    public float letterDisplayDelay = 0.05f; // OpóŸnienie miêdzy literami w sekundach
    public float cursorBlinkInterval = 0.5f;

    [Header("Panele losowych znaków")]
    public GameObject randomPanel; // Pierwszy panel
    public TextMeshProUGUI randomPanelText; // Tekst na pierwszym panelu
    public int numberOfRandomCharactersPanel1 = 50; // Liczba znaków na pierwszym panelu

    public GameObject randomPanel2; // Drugi panel
    public TextMeshProUGUI randomPanel2Text; // Tekst na drugim panelu
    public int numberOfRandomCharactersPanel2 = 100; // Liczba znaków na drugim panelu

    public float panelDelay = 2f; // OpóŸnienie przed wyœwietleniem drugiego panelu
    public float randomPanelDuration = 5f; // Czas trwania obu paneli (liczony od pojawienia siê drugiego panelu)
    public float typingSpeed = 0.05f; // Prêdkoœæ "pisania" tekstu

    [Header("Logi w ró¿nych jêzykach")]
    public List<LogEntry> logEntriesEnglish = new List<LogEntry>();
    public List<LogEntry> logEntriesPolish = new List<LogEntry>();
    public List<LogEntry> logEntriesGerman = new List<LogEntry>();

    private List<LogEntry> logEntries;
    public static List<LogEntry> sharedHelpLogs = new List<LogEntry>();

    // S³ownik komend do metod
    private Dictionary<string, CommandData> commandDictionary;

    private Coroutine logSequenceCoroutine = null;
    private string pendingCommand = null;
    private bool isTerminalInterrupted = false;

    private float errorVolume = 0.2f;
    // Lista wszystkich obiektów, które posiadaj¹ PlaySoundOnObject
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();

    // zielony  "#00E700"
    // z³oty    "#FFD200"
    // czerwony "#FF0000"
    private void Start()
    {
        if (riddleMonitor)
        {
            securedMonitor = true;
        }

        if (mainMonitor)
        {
            securedMonitor = false;
        }

        if (modelText != null)
        {
            modelText.text = "Siegdu & Babi v2.7_4_1998";
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
            inputField.onEndEdit.AddListener(HandleCommandInput); // Dodaj obs³ugê zakoñczenia edycji
            inputField.characterLimit = 30;//Ogranicz liczbê znaków do 20 (zmieñ na wartoœæ, któr¹ chcesz)
        }

        // Jeœli trzeba, przypisz referencjê do obiektu ButtonMethods (jeœli jeszcze nie zosta³o przypisane)
        if (sceneChanger == null)
        {
            sceneChanger = UnityEngine.Object.FindAnyObjectByType<SceneChanger>();
        }

        if (securedMonitor)
        {
            GeneratePassword(); // Generuj has³o przy starcie terminala
        }

        // Subskrybuj zdarzenie zmiany jêzyka
        LanguageManager.Instance.OnLanguageChanged += HandleLanguageChanged;

        // Inicjalizuj komendy dla bie¿¹cego jêzyka
        InitializeLocalizedCommands();

        UpdateLogEntriesLanguage(); // Ustaw pocz¹tkowy jêzyk

        // ZnajdŸ wszystkie obiekty posiadaj¹ce PlaySoundOnObject i dodaj do listy
       playSoundObjects.AddRange(UnityEngine.Object.FindObjectsByType<PlaySoundOnObject>(FindObjectsSortMode.None));

    }
    private void GeneratePassword()
    {
        const string digits = "0123456789";
        generatedPassword = new string(Enumerable.Range(0, 4).Select(_ => digits[UnityEngine.Random.Range(0, digits.Length)]).ToArray());
    }

    void Awake()
    {
        UpdateLocalizedText(); // ustawia tekst zanim pojawi siê terminal
        UpdateInfoText();
    }

    void OnEnable()
    {
        // Subskrybujemy zmianê jêzyka
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged += UpdateLocalizedText;
            LanguageManager.Instance.OnLanguageChanged += UpdateInfoText;
        }
    }

    void OnDisable()
    {
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged -= UpdateLocalizedText;
            LanguageManager.Instance.OnLanguageChanged -= UpdateInfoText;
        }
    }

    public void UpdateLocalizedText()
    {
        if (LanguageManager.Instance == null) return;

        switch (LanguageManager.Instance.currentLanguage)
        {
            case LanguageManager.Language.Polski:
                localizedFunctionText = monitorFunctionText_PL;
                break;
            case LanguageManager.Language.Deutsch:
                localizedFunctionText = monitorFunctionText_DE;
                break;
            default:
                localizedFunctionText = monitorFunctionText_EN;
                break;
        }

        //Debug.Log($"[{gameObject.name}] StartText: {localizedFunctionText}");
    }

    public void UpdateInfoText()
    {
        if (LanguageManager.Instance == null) return;

        switch (LanguageManager.Instance.currentLanguage)
        {
            case LanguageManager.Language.Polski:
                localizedInfoText = monitorInfoText_PL;
                break;
            case LanguageManager.Language.Deutsch:
                localizedInfoText = monitorInfoText_DE;
                break;
            default:
                localizedInfoText = monitorInfoText_EN;
                break;
        }

        //Debug.Log($"[{gameObject.name}] StartText: {localizedInfoText}");
    }

    public void InitializeLocalizedCommands()
    {
        // Tworzymy pusty s³ownik komend
        commandDictionary = new Dictionary<string, CommandData>();

        if (riddleMonitor && securedMonitor && !mainMonitor && !saveMonitor)
        {
            // Dodajemy komendê Hack/hakuj w zale¿noœci od jêzyka
            string localizedHack = LanguageManager.Instance.currentLanguage switch
            {
                LanguageManager.Language.Polski => "hakuj", // Polski
                LanguageManager.Language.Deutsch => "hacken", // Niemiecki
                _ => "hack" // Angielski
            };

            commandDictionary[localizedHack] = new CommandData(() => DisplayRandomPanels(), false);
        }

        // Dodanie komendy wyjœcia zale¿nej od jêzyka
        string localizedExit = LanguageManager.Instance.currentLanguage switch
        {
            LanguageManager.Language.Polski => "zamknij", // Polski
            LanguageManager.Language.Deutsch => "beenden", // Niemiecki
            _ => "exit" // Angielski
        };

        commandDictionary[localizedExit] = new CommandData(() => StartCoroutine(MoveCameraBackToOriginalPosition()), false);

        // Jeœli terminal jest zabezpieczony, nie dodajemy innych komend
        if (securedMonitor)
        {
            //Debug.Log("Terminal jest zabezpieczony. Komendy ograniczone.");
            return;
        }

        // Dodanie komendy Start w zale¿noœci od jêzyka
        string localizedStart = LanguageManager.Instance.currentLanguage switch
        {
            LanguageManager.Language.Polski => "start", // Polski
            LanguageManager.Language.Deutsch => "start", // Niemiecki
            _ => "start" // Angielski
        };

        // Komenda Start dzia³a ró¿nie w zale¿noœci od blokady terminala
        commandDictionary[localizedStart] = new CommandData(() =>
        {
            if (hasStartFunction)
            {
                StartFunction();
                ShowConsoleMessage($">>> {LanguageManager.Instance.GetLocalizedMessage("start")}", "#FFD200");
                ShowConsoleMessage($"{LanguageManager.Instance.GetLocalizedMessage(localizedFunctionText)}", "#FFD200");
            }
            else
            {
                ShowConsoleMessage($"{LanguageManager.Instance.GetLocalizedMessage("commandMissing")}", "#FF0000");

                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;
                    playSoundOnObject.PlaySound("TerminalError", errorVolume, false);
                }
            }

        }, false);

        // Dodanie komendy Start w zale¿noœci od jêzyka
        string localizedInfo = LanguageManager.Instance.currentLanguage switch
        {
            LanguageManager.Language.Polski => "info", // Polski
            LanguageManager.Language.Deutsch => "", // Niemiecki
            _ => "info" // Angielski
        };

        commandDictionary[localizedInfo] = new CommandData(() =>
        {
            if (hasInfo)
            {
                ShowConsoleMessage($">>> {LanguageManager.Instance.GetLocalizedMessage("info")}", "#FFD200");
                ShowConsoleMessage($"{LanguageManager.Instance.GetLocalizedMessage(localizedInfoText)}", "#FFD200");
            }
            else
            {
                ShowConsoleMessage($"{LanguageManager.Instance.GetLocalizedMessage("commandMissing")}", "#FF0000");

                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;
                    playSoundOnObject.PlaySound("TerminalError", errorVolume, false);
                }
            }

        }, false);

        if (saveMonitor)
        {
            string localizedSave = LanguageManager.Instance.currentLanguage switch
            {
                LanguageManager.Language.Polski => "zapisz", // Polski
                LanguageManager.Language.Deutsch => "speichern", // Niemiecki
                _ => "save" // Angielski
            };

            commandDictionary[localizedSave] = new CommandData(() =>
            {
                SaveFunction();
                ShowConsoleMessage($">>> {LanguageManager.Instance.GetLocalizedMessage("saveMonitor")}", "#FFD200");
            }, false);
        }

        if (mainMonitor)
        {
            // Ustawienie komend zale¿nie od jêzyka
            string localizedHome = LanguageManager.Instance.currentLanguage switch
            {
                LanguageManager.Language.Polski => "dom", // Polski
                LanguageManager.Language.Deutsch => "haus", // Niemiecki
                _ => "home" // Angielski
            };

            // Dodanie komend do s³ownika
            commandDictionary[localizedHome] = new CommandData(() => ExitTerminalAndChangeScene("Home", 3f), true);

            string localizedMain = LanguageManager.Instance.currentLanguage switch
            {
                LanguageManager.Language.Polski => "trasa", // Polski
                LanguageManager.Language.Deutsch => "route", // Niemiecki
                _ => "route" // Angielski
            };

            commandDictionary[localizedMain] = new CommandData(() => ExitTerminalAndChangeScene("Main", 3f), true);
        }

        string localizedHelp = LanguageManager.Instance.currentLanguage switch
        {
            LanguageManager.Language.Polski => "pomoc", // Polski
            LanguageManager.Language.Deutsch => "hilfe", // Niemiecki
            _ => "help" // Angielski
        };

        commandDictionary[localizedHelp] = new CommandData(() => StartCoroutine(DisplayHelpLogs()), false);

    }
    public void StartFunction()
    {
        Debug.Log("wywo³ano akcje");
        onStartFunction?.Invoke(); // Wywo³anie przypisanych funkcji
    }

    public void SaveFunction()
    {
        // Wywo³anie zapisu gry na aktualnym slocie
        SaveManager.Instance.SavePlayerData();
    }

    public void HandleLanguageChanged()
    {
        // Ponowna inicjalizacja komend po zmianie jêzyka
        InitializeLocalizedCommands();
        UpdateLocalizedText();
        UpdateInfoText();
    }

    private void OnDestroy()
    {
        // Usuñ subskrypcjê zdarzenia, aby unikn¹æ b³êdów
        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.OnLanguageChanged -= HandleLanguageChanged;
        }
    }

    // Dynamiczne przypisanie logów wg jêzyka
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

            foreach (var playSoundOnObject in playSoundObjects)
            {
                if (playSoundOnObject == null) continue;
                playSoundOnObject.PlaySound("TerminalError", errorVolume, false);
            }

            return; // Przerywamy – nie wychodzimy z terminala ani nie zmieniamy sceny
        }

        StartCoroutine(ExitAndDelaySceneChange(targetSceneName, delaySeconds));
    }

    private IEnumerator ExitAndDelaySceneChange(string sceneName, float delay)
    {
        // ZnajdŸ TreasureRefiner w scenie
        TreasureRefiner treasureRefiner = UnityEngine.Object.FindFirstObjectByType<TreasureRefiner>();

        // SprawdŸ, czy TreasureRefiner istnieje i czy rafinacja jest w toku
        if (treasureRefiner != null && treasureRefiner.isSpawning)
        {
            Debug.Log("Rafinacja w toku. Nie mo¿na zmieniæ sceny.");
            ShowConsoleMessage(LanguageManager.Instance.GetLocalizedMessage("refiningBlocked"), "#FF0000");

            foreach (var playSoundOnObject in playSoundObjects)
            {
                if (playSoundOnObject == null) continue;
                playSoundOnObject.PlaySound("TerminalError", errorVolume, false);
            }

            yield break; // Zatrzymaj korutynê, jeœli rafinacja w toku
        }

        // Jeœli nie ma rafinacji, przechodzimy do dalszych operacji
        yield return StartCoroutine(MoveCameraBackToOriginalPosition());

        CanUseMenu = false;

        // Zablokuj mo¿liwoœæ interakcji
        DisablePlayerMovementAndMouseLook();

        ShowConsoleMessage(string.Format(LanguageManager.Instance.GetLocalizedMessage("changingScene"), sceneName, delay), "#FFD200");

        yield return new WaitForSeconds(delay);

        // Zmieñ scenê
        if (sceneChanger != null)
        {
            sceneChanger.TryChangeScene(sceneName);
        }
        else
        {
            Debug.LogError("Brak referencji do SceneChanger!");
        }

        // Po zmianie sceny, przywróæ kontrolki
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
            // Pauza przed wyœwietleniem wiadomoœci
            yield return new WaitForSeconds(0.2f);
            ShowConsoleMessage($">>> {LanguageManager.Instance.GetLocalizedMessage("terminalExit")}", "#00E700");
            yield return new WaitForSeconds(0.2f);

            if (riddleMonitor)
            {
                ShowConsoleMessage($">>> {LanguageManager.Instance.GetLocalizedMessage("hack")}", "#00E700");
                yield return new WaitForSeconds(0.2f);
            }
            if (hasStartFunction)
            {
                ShowConsoleMessage($">>> {LanguageManager.Instance.GetLocalizedMessage("startHelp")}", "#00E700");
                yield return new WaitForSeconds(0.2f);
            }
            if (hasInfo)
            {
                ShowConsoleMessage($">>> {LanguageManager.Instance.GetLocalizedMessage("infoHelp")}" + " " + randomKB + " KB" , "#00E700");
                yield return new WaitForSeconds(0.2f);
            }
            if (saveMonitor)
            {
                ShowConsoleMessage($">>> {LanguageManager.Instance.GetLocalizedMessage("saveMonitorHelp")}", "#00E700");
                yield return new WaitForSeconds(0.2f);
            }
            if (mainMonitor)
            {
                ShowConsoleMessage($">>> {LanguageManager.Instance.GetLocalizedMessage("locations")}", "#00E700");
                yield return new WaitForSeconds(0.2f);
                ShowConsoleMessage($">>> {LanguageManager.Instance.GetLocalizedMessage("command_home_desc")}", "#00E700");
                ShowConsoleMessage($">>> {LanguageManager.Instance.GetLocalizedMessage("command_main_desc")}", "#00E700");
            }

            yield break;
        }

        // Skopiuj, ¿eby oryginalna lista by³a bezpieczna
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
    //    ">>> Komenda 'refine' – rozpocznij rafinacjê skarbów.",
    //    ">>> Komenda 'status' – sprawdŸ stan rafinacji."
    //    });
    //}


    void Update()
    {
        // Blokujemy interakcjê, jeœli canInteract jest false
        if (!canInteract)
        {
            return; // Ignorujemy wszystkie akcje gracza
        }

        if ((Input.GetKeyDown(KeyCode.Escape) && isInteracting && !isCameraMoving))
        {
            StartCoroutine(MoveCameraBackToOriginalPosition());
        }

        if (inputField != null)
        {
            // Jeœli w polu tekstowym coœ jest wpisane
            if (!string.IsNullOrEmpty(inputField.text))
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    HandleCommandInput(inputField.text); // Obs³ugujemy wpisan¹ komendê
                }
            }
            else if (isMiniGameActive)
            {
                // Jeœli pole tekstowe jest puste i mini-gra jest aktywna
                HandleKeyboardInput();
            }
        }
    }

    public void UseMonitor()
    {
        if (!isInteracting && !isCameraMoving)
        {
            if (terminalEmission != null)
            {
                terminalEmission.FlashEmission();
            }
            // --- Tu obs³uga latarki ---
            if (playerInteraction != null && playerInteraction.inventory != null && playerInteraction.inventory.flashlight != null)
            {
                flashlightWasOnBeforeMonitor = playerInteraction.inventory.flashlight.enabled;
                playerInteraction.inventory.FlashlightOff();
            }

            isUsingMonitor = true;

            originalCameraPosition = Camera.main.transform.position;
            originalCameraRotation = Camera.main.transform.rotation;
            StartCoroutine(MoveCameraToPosition());
            ClearMonitorConsole();
            UpdateLogEntriesLanguage();
        }
    }

    IEnumerator MoveCameraToPosition()
    {
        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;
            playSoundOnObject.PlaySound("TerminalOpen", 0.7f, false);
        }

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
        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;
            playSoundOnObject.FadeOutSound("TerminalMusic", 0.5f);
        }

        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;
            playSoundOnObject.PlaySound("TerminalExit", 0.4f, false);
        }
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

        PlayerInteraction player = UnityEngine.Object.FindFirstObjectByType<PlayerInteraction>();
        if (player != null)
            player.ReactivateInventoryAndUI();

        // --- Przywróæ latarkê ---
        if (playerInteraction != null && playerInteraction.inventory != null && playerInteraction.inventory.flashlight != null)
        {
            if (flashlightWasOnBeforeMonitor)
                playerInteraction.inventory.FlashlightOn();
            else
                playerInteraction.inventory.FlashlightOff();
        }

        isCameraMoving = false;

        if (logSequenceCoroutine != null)
        {
            StopCoroutine(logSequenceCoroutine);
            logSequenceCoroutine = null;
        }

        ClearMonitorConsole();
        CanUseMenu = true;
        isUsingMonitor = false;

        if (terminalEmission != null)
        {
            terminalEmission.DisableEmission();
        }
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
            //Debug.Log($"[ShowConsoleMessage] Przet³umaczona wiadomoœæ: {translatedMessage}");
        }

        Color color = Color.white;
        if (ColorUtility.TryParseHtmlString(hexColor, out Color parsedColor))
        {
            color = parsedColor;
        }

        // Dodajemy do kolejki i odpalamy tylko jeœli nic nie przetwarza
        messageProcessingQueue.Enqueue((key, color));

        if (!isProcessingMessage)
        {
            var next = messageProcessingQueue.Dequeue();
            StartCoroutine(TypeMessageCoroutine(next.messageKey, next.color));
        }
    }


    private IEnumerator TypeMessageCoroutine(string messageKey, Color color)
    {

        isProcessingMessage = true;

        // Pobranie wiadomoœci z LanguageManager na podstawie messageKey
        string fullMessage = LanguageManager.Instance.GetLocalizedMessage(messageKey);

        string currentLine = "";

        // Iteracja po wszystkich znakach wiadomoœci
        for (int i = 0; i < fullMessage.Length; i++)
        {
            // Sprawdzamy, czy terminal zosta³ przerwany
            if (isTerminalInterrupted)
            {
                isProcessingMessage = false;
                yield break; // Przerwij wyœwietlanie, je¿eli terminal zosta³ przerwany
            }

            currentLine += fullMessage[i]; // Dodajemy nowy znak do linii
            UpdateConsolePreview(currentLine, color); // Aktualizujemy podgl¹d konsoli

            if (i % 2 == 0)
            {
                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;
                    playSoundOnObject.PlaySound("TerminalLetter", 0.3f, false);
                }
            }

            // OpóŸnienie pomiêdzy literami
            yield return new WaitForSeconds(letterDisplayDelay);
        }

        // Je¿eli terminal zosta³ przerwany, przerywamy dalsze dzia³anie
        if (isTerminalInterrupted)
        {
            isProcessingMessage = false;
            yield break;
        }

        // Tworzymy obiekt finalnej wiadomoœci
        var finalMessage = new ConsoleMessage(fullMessage, Time.time, color);

        // Dodajemy wiadomoœæ do kolejki
        messageQueue.Enqueue(finalMessage);

        // Je¿eli liczba wiadomoœci przekroczy maksymaln¹ dozwolon¹, usuwamy najstarsz¹
        while (messageQueue.Count > maxMessages)
            messageQueue.Dequeue();

        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;
            playSoundOnObject.PlaySound("TerminalLog", 0.4f, false);
        }

        // Aktualizujemy tekst na konsoli
        UpdateConsoleText();

        // Zakoñczono przetwarzanie
        isProcessingMessage = false;

        // Jeœli s¹ kolejne wiadomoœci w kolejce, przetwórz nastêpn¹
        if (messageProcessingQueue.Count > 0)
        {
            var next = messageProcessingQueue.Dequeue();
            StartCoroutine(TypeMessageCoroutine(next.messageKey, next.color));
        }
    }

    private void UpdateConsolePreview(string typingLine, Color color)
    {
        if (consoleTextDisplay == null) return;

        List<string> lines = new List<string>();

        // Dodaj istniej¹ce wiadomoœci
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

        // Dodaj wszystkie wiadomoœci w kolejnoœci FIFO
        foreach (var msg in messageQueue)
        {
            string hexColor = ColorUtility.ToHtmlStringRGB(msg.color);
            lines.Add($"<color=#{hexColor}>{msg.message}</color>");
        }

        // £¹cz tekst w jeden string
        consoleTextDisplay.text = string.Join("\n", lines);
    }

    public void ClearMonitorConsole()
    {
        messageQueue.Clear();

        if (consoleTextDisplay != null)
        {
            consoleTextDisplay.text = "";
        }

        // Wyczyszczenie monitora bez wywo³ywania dodatkowych metod
        logHistory.Clear(); // Reset logów
    }

    private void HandleCommandInput(string command)
    {
        // Ignorujemy komendy, jeœli interakcja jest wy³¹czona
        if (!canInteract)
        {
            return;
        }

        // Jeœli gra zakoñczy³a siê wygran¹, blokujemy "hack"
        if (!isMiniGameActive && command == "hack" && hasWonGame)
        {
            ShowConsoleMessage(LanguageManager.Instance.GetLocalizedMessage("unknownCommand"), "#FF0000");

            foreach (var playSoundOnObject in playSoundObjects)
            {
                if (playSoundOnObject == null) continue;
                playSoundOnObject.PlaySound("TerminalError", errorVolume, false);
            }

            ClearInputField();
            return;
        }

        if (isMiniGameActive)
        {
            if (command == "exit")
            {
                // Komenda "exit" koñczy mini-grê jako przegran¹
                EndMiniGame(false);

                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;
                    playSoundOnObject.PlaySound("TerminalExit", 0.4f, false);
                }

                ResetTerminalState(); // Reset terminala do stanu pocz¹tkowego
            }

            ClearInputField();
            return;
        }

        // Jeœli terminal jest zabezpieczony, sprawdŸ has³o lub obs³u¿ komendy
        if (securedMonitor)
        {
            if (commandDictionary.ContainsKey(command))
            {
                // Jeœli komenda istnieje w s³owniku, wywo³aj j¹
                var data = commandDictionary[command];
                data.command.Invoke();
                ClearInputField();

                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;
                    playSoundOnObject.PlaySound("TerminalAccept", 0.3f, false);
                }

                return;
            }

            if (command == generatedPassword)
            {
                // Jeœli podano poprawne has³o
                securedMonitor = false; // Zdejmij zabezpieczenie
                ClearMonitorConsole(); // Wyczyœæ konsolê
                ShowConsoleMessage($">>> {LanguageManager.Instance.GetLocalizedMessage("correctPassword")}", "#FFD200");

                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;

                    playSoundOnObject.PlaySound("TerminalCorrectPassword", 0.5f);
                }

                InitializeLocalizedCommands(); // Zainicjuj komendy po odblokowaniu
                StartLogSequence(); // Rozpocznij sekwencjê logów

                if (modelText != null)
                {
                    modelText.text = "Siegdu & Babi v2.7_4_1998";
                }

                // Usuñ komendê "hack" (lub jej odpowiednik w innym jêzyku)
                string localizedHackCommand = LanguageManager.Instance.currentLanguage switch
                {
                    LanguageManager.Language.Polski => "hakuj",
                    LanguageManager.Language.Deutsch => "hacken",
                    _ => "hack"
                };

                if (commandDictionary.ContainsKey(localizedHackCommand))
                {
                    commandDictionary.Remove(localizedHackCommand);
                    ShowConsoleMessage(LanguageManager.Instance.GetLocalizedMessage("unknownCommand"), "#FF0000");

                    foreach (var playSoundOnObject in playSoundObjects)
                    {
                        if (playSoundOnObject == null) continue;
                        playSoundOnObject.PlaySound("TerminalError", errorVolume, false);
                    }
                }
            }
            else
            {
                // Jeœli has³o jest nieprawid³owe
                ShowConsoleMessage(LanguageManager.Instance.GetLocalizedMessage("incorrectPassword"), "#FF0000");

                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;
                    playSoundOnObject.PlaySound("TerminalError", errorVolume, false);
                }
            }

            ClearInputField();
            return;
        }

        // Reszta funkcji HandleCommandInput pozostaje bez zmian
        command = command.ToLower().Trim();

        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToLower();

        if (command == currentScene && mainMonitor)
        {
            ShowConsoleMessage(LanguageManager.Instance.GetLocalizedMessage("alreadyInScene"), "#FFD200");

            foreach (var playSoundOnObject in playSoundObjects)
            {
                if (playSoundOnObject == null) continue;
                playSoundOnObject.PlaySound("TerminalError", errorVolume, false);
            }
            ClearInputField();
            return;
        }

        if (!string.IsNullOrEmpty(pendingCommand))
        {
            string confirmYesKey = LanguageManager.Instance.GetLocalizedMessage("confirmYesKey").ToLower();
            string confirmNoKey = LanguageManager.Instance.GetLocalizedMessage("confirmNoKey").ToLower();

            if (command == confirmYesKey)
            {
                ShowConsoleMessage($">>> {string.Format(LanguageManager.Instance.GetLocalizedMessage("executingCommand"), pendingCommand)}", "#00E700");

                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;
                    playSoundOnObject.PlaySound("TerminalAccept", 0.3f, false);
                }

                commandDictionary[pendingCommand].command.Invoke();
            }
            else if (command == confirmNoKey)
            {
                ShowConsoleMessage(LanguageManager.Instance.GetLocalizedMessage("commandCancelled"), "#FF0000");

                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;
                    playSoundOnObject.PlaySound("TerminalError", errorVolume, false);
                }
            }
            else
            {
                ShowConsoleMessage(LanguageManager.Instance.GetLocalizedMessage("invalidResponse"), "#FF0000");

                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;
                    playSoundOnObject.PlaySound("TerminalError", errorVolume, false);
                }
            }

            pendingCommand = null;
            ClearInputField();
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

                        foreach (var playSoundOnObject in playSoundObjects)
                        {
                            if (playSoundOnObject == null) continue;
                            playSoundOnObject.PlaySound("TerminalError", errorVolume, false);
                        }
                    }
                    else
                    {
                        ShowConsoleMessage($">>> {LanguageManager.Instance.GetLocalizedMessage("confirmCommand")} [{LanguageManager.Instance.GetLocalizedMessage("confirmYesKey")}/{LanguageManager.Instance.GetLocalizedMessage("confirmNoKey")}]", "#FFD200");
                        pendingCommand = command;
                    }
                }
                else
                {
                    data.command.Invoke();

                    foreach (var playSoundOnObject in playSoundObjects)
                    {
                        if (playSoundOnObject == null) continue;
                        playSoundOnObject.PlaySound("TerminalAccept", 0.2f, false);
                    }
                }
            }
            else
            {
                ShowConsoleMessage(LanguageManager.Instance.GetLocalizedMessage("unknownCommand"), "#FF0000");

                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;
                    playSoundOnObject.PlaySound("TerminalError", errorVolume, false);
                }
            }

            ClearInputField();
        }
    }

    private void ClearInputField()
    {
        inputField.text = "";
        inputField.ActivateInputField();
    }

    private void DisplayRandomPanels()
    {
        canInteract = false;

        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;
            playSoundOnObject.PlaySound("TerminalMusic", 0.2f, false);
        }

        // Zresetuj stan terminala
        ResetTerminalState();

        // Ukryj consoleTextDisplay
        consoleTextDisplay?.gameObject.SetActive(false);

        // Wyczyœæ i aktywuj pierwszy panel
        randomPanelText.text = "";
        randomPanel.SetActive(true);
        StartCoroutine(TypeRandomCharacters(randomPanelText, numberOfRandomCharactersPanel1));

        // Wyœwietl drugi panel po okreœlonym opóŸnieniu
        StartCoroutine(ShowSecondPanelAfterDelay());
    }

    private void ResetTerminalState()
    {
        // Wy³¹cz panele i wyczyœæ ich tekst
        randomPanel.SetActive(false);
        randomPanel2.SetActive(false);
        randomPanelText.text = "";
        randomPanel2Text.text = "";

        // Przywróæ widocznoœæ consoleTextDisplay, jeœli by³o ukryte
        consoleTextDisplay?.gameObject.SetActive(true);
    }

    // Wyœwietlanie losowych znaków
    private IEnumerator TypeRandomCharacters(TextMeshProUGUI textComponent, int numberOfCharacters)
    {
        textComponent.text = ""; // Wyczyœæ tekst na pocz¹tku

        // Generowanie losowego tekstu
        string randomText = GenerateRandomText(numberOfCharacters); // Przekazujemy liczbê znaków

        // Wyœwietlanie znaków jeden po drugim
        foreach (char c in randomText)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(typingSpeed); // Odczekaj przed dodaniem kolejnego znaku
        }
    }

    // Obs³uga drugiego panelu z opóŸnieniem
    private IEnumerator ShowSecondPanelAfterDelay()
    {
        yield return new WaitForSeconds(panelDelay);

        // Wyczyœæ i aktywuj drugi panel
        randomPanel2Text.text = "";
        randomPanel2.SetActive(true);
        StartCoroutine(TypeRandomCharacters(randomPanel2Text, numberOfRandomCharactersPanel2));

        yield return new WaitForSeconds(randomPanelDuration);

        // Dezaktywuj oba panele
        randomPanel.SetActive(false);
        randomPanel2.SetActive(false);

        // Przywróæ consoleTextDisplay
        consoleTextDisplay?.gameObject.SetActive(true);

        // Aktywuj pole tekstowe
        inputField?.ActivateInputField();

        StartMiniGame(); // Rozpocznij mini-grê
        canInteract = true;
    }

    private string GenerateRandomText(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var sb = new StringBuilder(length); // Initialize with expected capacity for efficiency

        for (int i = 0; i < length; i++)
        {
            // Use Random.Range to pick a random character from the available pool
            char randomChar = chars[UnityEngine.Random.Range(0, chars.Length)];
            sb.Append(randomChar);
        }

        return sb.ToString();
    }

    // Method to start the mini-game
    private void StartMiniGame()
    {
        if (isMiniGameActive) return; // Zapobiegaj ponownemu uruchomieniu mini-gry

        // Czyszczenie monitora i resetowanie logów
        ClearMonitorConsole();
        logHistory.Clear();

        isGameForceEnded = false; // Zresetuj flagê na pocz¹tku nowej gry

        grid = new string[gridSize, gridSize];
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                grid[i, j] = "X"; // Ka¿de pole pocz¹tkowo ma wartoœæ "X"
            }
        }

        // Obliczanie pocz¹tkowej pozycji kursora
        int centerX = gridSize / 2;
        int centerY = gridSize / 2;

        // Jeœli rozmiar siatki jest parzysty, przesuwamy kursor o jedno pole w lewo i do góry
        if (gridSize % 2 == 0)
        {
            centerX -= 1;
            centerY -= 1;
        }

        cursorPosition = new Vector2Int(centerX, centerY);

        targetCoordinate = new Vector2Int(UnityEngine.Random.Range(0, gridSize), UnityEngine.Random.Range(0, gridSize));
        remainingAttempts = 10;
        isMiniGameActive = true;

        UpdateGridDisplay($">>> {LanguageManager.Instance.GetLocalizedMessage("miniGameStart")}", "#FFD200");
        UpdateCursorPosition();

        if (isTimerEnabled)
        {
            timeRemaining = gameTime;

            if (timerCoroutine != null)
            {
                Debug.Log("Stopping existing timer coroutine before starting a new one.");
                StopCoroutine(timerCoroutine);
            }

            Debug.Log("Starting timer coroutine.");
            timerCoroutine = StartCoroutine(CountdownTimer());
        }
        else
        {
            Debug.Log("Timer is disabled, skipping timer setup.");
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
        }
    }

    private void UpdateGridDisplay(string additionalMessage, string hexColor = "#FFFFFF")
    {
        // Sprawdzenie consoleTextDisplay
        if (consoleTextDisplay == null) return;

        // Sprawdzenie czy grid zosta³ zainicjalizowany
        if (grid == null)
        {
            Debug.LogError("Grid is null. Ensure it is initialized before calling UpdateGridDisplay.");
            return;
        }

        // Sprawdzenie czy cursorPosition mieœci siê w zakresie grid
        if (cursorPosition.x < 0 || cursorPosition.x >= gridSize ||
            cursorPosition.y < 0 || cursorPosition.y >= gridSize)
        {
            Debug.LogError("Cursor position is out of bounds. Ensure cursorPosition is within grid limits.");
            return;
        }

        // Sprawdzenie czy logHistory zosta³a zainicjalizowana
        if (logHistory == null)
        {
            logHistory = new List<string>();
        }

        StringBuilder gridBuilder = new StringBuilder();

        // Budowanie siatki
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                string cellValue = grid[i, j];

                if (i == cursorPosition.x && j == cursorPosition.y)
                {
                    gridBuilder.Append($" [<color=#FFFF00>{cellValue}</color>] "); // ¯ó³ty kolor dla kursora
                }
                else if (cellValue != "X")
                {
                    gridBuilder.Append($" (<color=#FFFFFF>{cellValue}</color>) "); // Bia³y kolor dla wartoœci komórek
                }
                else
                {
                    gridBuilder.Append(" (  ) "); // Domyœlne puste komórki
                }
            }
            gridBuilder.AppendLine();
        }

        // Dodanie pustej linii miêdzy tabel¹ a logami
        gridBuilder.AppendLine();

        // Wyœwietlanie czasu
        if (isMiniGameActive && isTimerEnabled)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(timeRemaining);
            string formattedTime = string.Format("{0}:{1:D2}:{2:D3}",
                timeSpan.Minutes,
                timeSpan.Seconds,
                timeSpan.Milliseconds);

            string timerColor;
            if (timeRemaining <= 10f)
                timerColor = "#FF0000"; // czerwony
            else if (timeRemaining <= 20f)
                timerColor = "#FFD200"; // z³oty
            else
                timerColor = "#00E700"; // zielony

            string localizedTimeLeft = LanguageManager.Instance.GetLocalizedMessage("miniGameTimer");
            gridBuilder.AppendLine($"<color={timerColor}> >>> {localizedTimeLeft}: {formattedTime}</color>");
        }

        // Dodanie pustej linii miêdzy tabel¹ a logami
        gridBuilder.AppendLine();

        // Obs³uga logów
        if (!string.IsNullOrEmpty(additionalMessage))
        {
            string formattedMessage = $"<color={hexColor}> {additionalMessage}</color>";
            logHistory.Add(formattedMessage);

            if (logHistory.Count > 3)
            {
                logHistory.RemoveAt(0);
            }
        }

        foreach (string log in logHistory)
        {
            gridBuilder.AppendLine(log);
        }

        // Aktualizacja tekstu konsoli
        consoleTextDisplay.text = gridBuilder.ToString();
    }

    private void HandleKeyboardInput()
    {
        bool moved = false;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveCursor(-1, 0);
            moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveCursor(1, 0);
            moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveCursor(0, -1);
            moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveCursor(0, 1);
            moved = true;
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            if (string.IsNullOrEmpty(inputField.text))
            {
                HandleCursorSelection();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Zatrzymaj korutynê BlinkTargetCell, jeœli dzia³a
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null; // Resetuj referencjê
                ResetTargetCellVisibility(); // Przywróæ widocznoœæ celu
            }

            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isMiniGameActive)
                {
                    isGameForceEnded = true; // Ustaw flagê na true
                    EndMiniGame(false); // Zakoñcz grê jako przegran¹
                }
            }
        }

        // Odtwórz dŸwiêk tylko jeœli by³ ruch
        if (moved)
        {
            foreach (var playSoundOnObject in playSoundObjects)
            {
                if (playSoundOnObject == null) continue;
                playSoundOnObject.PlaySound("TerminalMove", 0.3f, false);
            }
        }
    }

    private void MoveCursor(int rowDelta, int colDelta)
    {
        int newRow = Mathf.Clamp(cursorPosition.x + rowDelta, 0, gridSize - 1);
        int newCol = Mathf.Clamp(cursorPosition.y + colDelta, 0, gridSize - 1);

        cursorPosition = new Vector2Int(newRow, newCol);

        UpdateGridDisplay(null); // Aktualizacja siatki
    }

    private void UpdateCursorPosition()
    {
        if (cursor != null)
        {
            // Przesuñ obiekt kursora na aktualn¹ komórkê
            cursor.transform.position = GetCellPosition(cursorPosition.x, cursorPosition.y);
        }
    }

    private Vector3 GetCellPosition(int row, int col)
    {
        // Zak³adamy, ¿e komórki s¹ rozmieszczone w siatce 2D w przestrzeni
        // Zwraca pozycjê w przestrzeni w oparciu o siatkê
        return new Vector3(col * 1.1f, -row * 1.1f, 0); // Przyk³adowe rozmieszczenie
    }

    private void HandleCursorSelection()
    {
        int row = cursorPosition.x;
        int col = cursorPosition.y;

        if (grid[row, col] != "X")
        {
            UpdateGridDisplay($">>> {LanguageManager.Instance.GetLocalizedMessage("miniGameAlreadyRevealed")}", "#FFD200");
            foreach (var playSoundOnObject in playSoundObjects)
            {
                if (playSoundOnObject == null) continue;
                playSoundOnObject.PlaySound("TerminalError", errorVolume, false);
            }
            return;
        }

        Vector2Int guess = new Vector2Int(row, col);
        if (guess == targetCoordinate)
        {
            grid[row, col] = "0"; // Trafienie
            EndMiniGame(true);
            return;
        }

        int distance = Mathf.Abs(targetCoordinate.x - row) + Mathf.Abs(targetCoordinate.y - col);
        grid[row, col] = distance.ToString();
        remainingAttempts--;

        if (remainingAttempts > 0)
        {
            UpdateGridDisplay($">>> {LanguageManager.Instance.GetLocalizedMessage("miniGameMissedTarget", distance, remainingAttempts)}", "#FFD200");

            foreach (var playSoundOnObject in playSoundObjects)
            {
                if (playSoundOnObject == null) continue;
                playSoundOnObject.PlaySound("TerminalShoot", 0.3F, false);
            }
            return;
        }
        else
        {
            UpdateGridDisplay($">>> {LanguageManager.Instance.GetLocalizedMessage("miniGameMissedTarget", distance, remainingAttempts)}", "#FFD200");
            EndMiniGame(false);
        }
    }

    private IEnumerator CountdownTimer()
    {
        Debug.Log("Timer coroutine started.");

        while (timeRemaining > 0f)
        {
            if (!isTimerEnabled)
            {
                Debug.Log("Timer coroutine stopped because timer is disabled.");
                yield break; // Zakoñcz korutynê
            }

            timeRemaining -= Time.deltaTime;
            if (timeRemaining < 0f)
                timeRemaining = 0f;

            UpdateGridDisplay(null); // Aktualizacja siatki z nowym czasem
            yield return null; // Czekaj jedn¹ klatkê
        }

        Debug.Log("Timer has reached zero.");

        if (isMiniGameActive)
        {
            Debug.Log("Ending mini-game because time is up.");
            EndMiniGame(false); // Jeœli czas siê skoñczy³ i gra trwa — przegrana
        }
    }

    private void EndMiniGame(bool isWin)
    {
        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;
            playSoundOnObject.FadeOutSound("TerminalMusic", 5.0f);
        }

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        isMiniGameActive = false; // Wy³¹czamy mini-grê
        canInteract = false; // Blokujemy interakcjê

        if (isWin)
        {

            foreach (var playSoundOnObject in playSoundObjects)
            {
                if (playSoundOnObject == null) continue;
                playSoundOnObject.PlaySound("TerminalWin", 0.7F, false);
            }

            hasWonGame = true; // Oznaczamy, ¿e gra zakoñczy³a siê wygran¹
            // Wygrana gra - cel miga przez 5 sekund
            ModelNameRandomSymbols();
            StartCoroutine(BlinkTargetCell(targetCoordinate, 5f));
            UpdateGridDisplay(LanguageManager.Instance.GetLocalizedMessage("miniGameWin"), "#FFD200");

            string localizedHackCommand = LanguageManager.Instance.currentLanguage switch
            {
                LanguageManager.Language.Polski => "hakuj",
                LanguageManager.Language.Deutsch => "hacken",
                _ => "hack"
            };

            if (commandDictionary.ContainsKey(localizedHackCommand))
            {
                commandDictionary.Remove(localizedHackCommand); // Usuwamy komendê z listy
            }
        }
        else
        {

            foreach (var playSoundOnObject in playSoundObjects)
            {
                if (playSoundOnObject == null) continue;
                playSoundOnObject.PlaySound("TerminalLose", 0.7F, false);
            }

            // Przegrana gra - ujawniamy cel graczowi
            grid[targetCoordinate.x, targetCoordinate.y] = "0"; // Oznaczamy cel jako `0`
            StartCoroutine(BlinkTargetCell(targetCoordinate, 5f)); // Miganie celu przez 5 sekund
            UpdateGridDisplay(LanguageManager.Instance.GetLocalizedMessage("miniGameOver"), "#FFD200");
        }

        // Rozpoczynamy sekwencjê startow¹ po czasie migania tylko, jeœli gra nie zosta³a wymuszenie zakoñczona
        if (!isGameForceEnded)
        {
            StartCoroutine(StartSequenceAfterDelay(5f));
        }
    }

    // Zmieniona metoda BlinkTargetCell
    private IEnumerator BlinkTargetCell(Vector2Int target, float duration)
    {
        float elapsedTime = 0f;
        bool isVisible = true; // Czy cel jest widoczny w danej chwili

        while (elapsedTime < duration)
        {
            // Miganie: Na zmianê pokazuj/ukrywaj cel za pomoc¹ przezroczystoœci
            grid[target.x, target.y] = isVisible ? "<color=#FFFFFF>0</color>" : "<color=#00000000>0</color>";
            UpdateGridDisplay(null); // Aktualizacja siatki z migaj¹cym celem

            isVisible = !isVisible; // Zmieñ stan widocznoœci
            elapsedTime += 0.5f; // Odczekaj 0.5 sekundy
            yield return new WaitForSeconds(0.5f);

            // SprawdŸ, czy korutyna zosta³a przerwana przez gracza
            if (blinkCoroutine == null)
            {
                yield break; // Natychmiast zakoñcz korutynê
            }
        }

        // Po zakoñczeniu migania upewnij siê, ¿e cel pozostaje widoczny
        grid[target.x, target.y] = "<color=#FFFFFF>0</color>";
        UpdateGridDisplay(null); // Finalna aktualizacja siatki
        blinkCoroutine = null; // Resetuj referencjê po zakoñczeniu
    }

    // Dodaj pomocnicz¹ metodê do resetowania widocznoœci celu
    private void ResetTargetCellVisibility()
    {
        if (targetCoordinate != null)
        {
            grid[targetCoordinate.x, targetCoordinate.y] = "<color=#FFFFFF>0</color>";
            UpdateGridDisplay(null); // Finalna aktualizacja siatki z widocznym celem
        }
    }

    // Dodana metoda uruchamiaj¹ca sekwencjê startow¹ po opóŸnieniu
    private IEnumerator StartSequenceAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Czyœcimy wyœwietlacz konsoli tu¿ przed rozpoczêciem sekwencji
        // Czyszczenie monitora i resetowanie logów
        ClearMonitorConsole();
        logHistory.Clear();

        // Rozpoczynamy sekwencjê startow¹
        StartLogSequence();
        isGameForceEnded = false; // Zresetuj flagê na pocz¹tku nowej gry
    }

    private void ModelNameRandomSymbols()
    {
        if (modelText != null)
        {
            //                    "Siegdu & Babi v2.7_4_1998";
            string baseTemplate = "******-&-****-*/.^_%_!##$";
            int templateLength = baseTemplate.Length;
            int passwordLength = generatedPassword.Length;

            // Wybierz losowe unikalne indeksy w bazowym tekœcie do wstawienia cyfr
            List<int> insertionIndices = Enumerable.Range(0, templateLength)
                .OrderBy(_ => UnityEngine.Random.value)
                .Take(passwordLength)
                .OrderBy(i => i) // Wa¿ne: zachowaj kolejnoœæ od lewej do prawej
                .ToList();

            // Zamieñ znaki w bazowym ci¹gu na cyfry z has³a z kolorem
            char[] result = baseTemplate.ToCharArray();
            for (int i = 0; i < passwordLength; i++)
            {
                string coloredDigit = $"<color=#FFD200>{generatedPassword[i]}</color>";
                int index = insertionIndices[i];

                // Zamieñ znak na znacznik koloru + cyfra
                // Uwaga: trzeba bêdzie u¿yæ StringBuilder zamiast char[] jeœli liczysz na renderowanie tagów
                result[index] = '\0'; // tymczasowo znak pusty, póŸniej zast¹pimy

                // Wstaw kolorowany znak do odpowiedniego miejsca w stringu
            }

            // Finalna budowa stringa z tagami <color>
            StringBuilder finalString = new StringBuilder();
            int passwordIndex = 0;
            for (int i = 0; i < templateLength; i++)
            {
                if (insertionIndices.Contains(i))
                {
                    finalString.Append($"<color=#FFD200>{generatedPassword[passwordIndex]}</color>");
                    passwordIndex++;
                }
                else
                {
                    finalString.Append(baseTemplate[i]);
                }
            }

            modelText.text = finalString.ToString();
        }

        //ClearMonitorConsole();
        //StartLogSequence();
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

        // Wyœwietl komunikaty koñcowe w odpowiednim jêzyku
        switch (LanguageManager.Instance.currentLanguage)
        {
            case LanguageManager.Language.Polski:
                ShowConsoleMessage(">>> Pomoc - lista dostêpnych komend.", "#00E700");
                yield return new WaitForSeconds(1f);
                if (securedMonitor)
                {
                    ShowConsoleMessage(">>> Terminal gotowy. WprowadŸ swoje has³o.", "#FFD200");
                }
                else
                {
                    ShowConsoleMessage(">>> Terminal gotowy.", "#FFD200");
                }
                break;

            case LanguageManager.Language.Deutsch:
                ShowConsoleMessage(">>> Hilfe - verfügbare Befehle.", "#00E700");
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
        // Sumujemy wszystkie prawdopodobieñstwa
        float totalProbability = 0f;
        foreach (var log in availableLogs)
        {
            totalProbability += log.probability;
        }

        // Generujemy losow¹ wartoœæ
        float randomValue = UnityEngine.Random.Range(0f, totalProbability);
        float cumulativeProbability = 0f;

        // Iterujemy po dostêpnych logach, aby znaleŸæ ten na podstawie losowego prawdopodobieñstwa
        foreach (var log in availableLogs)
        {
            cumulativeProbability += log.probability;

            // Jeœli randomValue mieœci siê w tym przedziale, zwróæ log
            if (randomValue <= cumulativeProbability)
            {
                return log;
            }
        }

        // Jeœli nic nie znaleziono, zwróæ pierwszy log (to mo¿e byæ domyœlny przypadek)
        return availableLogs[0];
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
    public class ConsoleMessage
    {
        public string message;
        public float timeAdded;
        public Color color;
        public bool canDisplay;

        public ConsoleMessage(string message, float timeAdded, Color color)
        {
            this.message = message;
            this.timeAdded = timeAdded;
            this.color = color;
        }
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