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
    public static bool CanUseMenu = true; // Flaga, kt�ra kontroluje, czy menu jest dost�pne
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

    // UnityEvents pozwalaj� na przypisanie funkcji w Inspectorze
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
    public string generatedPassword; // Wygenerowane has�o
    public string monitorFunctionText_EN;
    public string monitorFunctionText_PL;
    public string monitorFunctionText_DE;
    public string localizedFunctionText;

    [Header("Mini-Game Settings")]
    public int gridSize = 15; // Rozmiar siatki (np. 10x10)
    private string[,] grid; // Tablica reprezentuj�ca siatk�
    private Vector2Int targetCoordinate; // Cel w grze
    private int remainingAttempts;
    private bool isMiniGameActive;
    private GameObject cursor; // Obiekt kursora (pod�wietlenie)
    private Vector2Int cursorPosition = new Vector2Int(0, 0); // Pozycja kursora
    public bool hasWonGame = false;
    private List<string> logHistory = new List<string>(); // Lista do przechowywania log�w
    public bool isTimerEnabled = true;
    public float gameTime = 30f; // Czas gry w sekundach (np. 30s)
    private bool isGameForceEnded = false; // Zresetuj flag� na pocz�tku nowej gry
    private float timeRemaining;
    private Coroutine timerCoroutine;
    private Coroutine blinkCoroutine; // Przechowuje referencj� do korutyny BlinkTargetCell

    [Header("UI � Konsola monitora")]
    public TextMeshProUGUI consoleTextDisplay;
    public TMP_InputField inputField; // Pole tekstowe dla wpisywania komend
    public TextMeshProUGUI modelText; // Dodatkowy tekst, kt�ry ma zmieni� tre�� na losowe znaki
    private Queue<ConsoleMessage> messageQueue = new Queue<ConsoleMessage>();
    private Queue<(string messageKey, Color color)> messageProcessingQueue = new();
    private bool isProcessingMessage = false;
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

    private List<LogEntry> logEntries;
    public static List<LogEntry> sharedHelpLogs = new List<LogEntry>();

    // S�ownik komend do metod
    private Dictionary<string, CommandData> commandDictionary;

    private Coroutine logSequenceCoroutine = null;
    private string pendingCommand = null;
    private bool isTerminalInterrupted = false;

    private float errorVolume = 0.2f;
    // Lista wszystkich obiekt�w, kt�re posiadaj� PlaySoundOnObject
    private List<PlaySoundOnObject> playSoundObjects = new List<PlaySoundOnObject>();

    // zielony  "#00E700"
    // z�oty    "#FFD200"
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

        // Znajd� wszystkie obiekty posiadaj�ce PlaySoundOnObject i dodaj do listy
       playSoundObjects.AddRange(UnityEngine.Object.FindObjectsByType<PlaySoundOnObject>(FindObjectsSortMode.None));

    }
    private void GeneratePassword()
    {
        const string digits = "0123456789";
        generatedPassword = new string(Enumerable.Range(0, 4).Select(_ => digits[UnityEngine.Random.Range(0, digits.Length)]).ToArray());
    }

    void Awake()
    {
        UpdateLocalizedText(); // ustawia tekst zanim pojawi si� terminal
        UpdateInfoText();
    }

    void OnEnable()
    {
        // Subskrybujemy zmian� j�zyka
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
        // Tworzymy pusty s�ownik komend
        commandDictionary = new Dictionary<string, CommandData>();

        if (riddleMonitor && securedMonitor && !mainMonitor && !saveMonitor)
        {
            // Dodajemy komend� Hack/hakuj w zale�no�ci od j�zyka
            string localizedHack = LanguageManager.Instance.currentLanguage switch
            {
                LanguageManager.Language.Polski => "hakuj", // Polski
                LanguageManager.Language.Deutsch => "hacken", // Niemiecki
                _ => "hack" // Angielski
            };

            commandDictionary[localizedHack] = new CommandData(() => DisplayRandomPanels(), false);
        }

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

        // Dodanie komendy Start w zale�no�ci od j�zyka
        string localizedStart = LanguageManager.Instance.currentLanguage switch
        {
            LanguageManager.Language.Polski => "start", // Polski
            LanguageManager.Language.Deutsch => "start", // Niemiecki
            _ => "start" // Angielski
        };

        // Komenda Start dzia�a r�nie w zale�no�ci od blokady terminala
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

        // Dodanie komendy Start w zale�no�ci od j�zyka
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
            // Ustawienie komend zale�nie od j�zyka
            string localizedHome = LanguageManager.Instance.currentLanguage switch
            {
                LanguageManager.Language.Polski => "dom", // Polski
                LanguageManager.Language.Deutsch => "haus", // Niemiecki
                _ => "home" // Angielski
            };

            // Dodanie komend do s�ownika
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
        Debug.Log("wywo�ano akcje");
        onStartFunction?.Invoke(); // Wywo�anie przypisanych funkcji
    }

    public void SaveFunction()
    {
        // Wywo�anie zapisu gry na aktualnym slocie
        SaveManager.Instance.SavePlayerData();
    }

    public void HandleLanguageChanged()
    {
        // Ponowna inicjalizacja komend po zmianie j�zyka
        InitializeLocalizedCommands();
        UpdateLocalizedText();
        UpdateInfoText();
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

            foreach (var playSoundOnObject in playSoundObjects)
            {
                if (playSoundOnObject == null) continue;
                playSoundOnObject.PlaySound("TerminalError", errorVolume, false);
            }

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

            foreach (var playSoundOnObject in playSoundObjects)
            {
                if (playSoundOnObject == null) continue;
                playSoundOnObject.PlaySound("TerminalError", errorVolume, false);
            }

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
        // Blokujemy interakcj�, je�li canInteract jest false
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
            // Je�li w polu tekstowym co� jest wpisane
            if (!string.IsNullOrEmpty(inputField.text))
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    HandleCommandInput(inputField.text); // Obs�ugujemy wpisan� komend�
                }
            }
            else if (isMiniGameActive)
            {
                // Je�li pole tekstowe jest puste i mini-gra jest aktywna
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
            // --- Tu obs�uga latarki ---
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

        // --- Przywr�� latark� ---
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
            //Debug.Log($"[ShowConsoleMessage] Przet�umaczona wiadomo��: {translatedMessage}");
        }

        Color color = Color.white;
        if (ColorUtility.TryParseHtmlString(hexColor, out Color parsedColor))
        {
            color = parsedColor;
        }

        // Dodajemy do kolejki i odpalamy tylko je�li nic nie przetwarza
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

        // Pobranie wiadomo�ci z LanguageManager na podstawie messageKey
        string fullMessage = LanguageManager.Instance.GetLocalizedMessage(messageKey);

        string currentLine = "";

        // Iteracja po wszystkich znakach wiadomo�ci
        for (int i = 0; i < fullMessage.Length; i++)
        {
            // Sprawdzamy, czy terminal zosta� przerwany
            if (isTerminalInterrupted)
            {
                isProcessingMessage = false;
                yield break; // Przerwij wy�wietlanie, je�eli terminal zosta� przerwany
            }

            currentLine += fullMessage[i]; // Dodajemy nowy znak do linii
            UpdateConsolePreview(currentLine, color); // Aktualizujemy podgl�d konsoli

            if (i % 2 == 0)
            {
                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;
                    playSoundOnObject.PlaySound("TerminalLetter", 0.3f, false);
                }
            }

            // Op�nienie pomi�dzy literami
            yield return new WaitForSeconds(letterDisplayDelay);
        }

        // Je�eli terminal zosta� przerwany, przerywamy dalsze dzia�anie
        if (isTerminalInterrupted)
        {
            isProcessingMessage = false;
            yield break;
        }

        // Tworzymy obiekt finalnej wiadomo�ci
        var finalMessage = new ConsoleMessage(fullMessage, Time.time, color);

        // Dodajemy wiadomo�� do kolejki
        messageQueue.Enqueue(finalMessage);

        // Je�eli liczba wiadomo�ci przekroczy maksymaln� dozwolon�, usuwamy najstarsz�
        while (messageQueue.Count > maxMessages)
            messageQueue.Dequeue();

        foreach (var playSoundOnObject in playSoundObjects)
        {
            if (playSoundOnObject == null) continue;
            playSoundOnObject.PlaySound("TerminalLog", 0.4f, false);
        }

        // Aktualizujemy tekst na konsoli
        UpdateConsoleText();

        // Zako�czono przetwarzanie
        isProcessingMessage = false;

        // Je�li s� kolejne wiadomo�ci w kolejce, przetw�rz nast�pn�
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

        // Wyczyszczenie monitora bez wywo�ywania dodatkowych metod
        logHistory.Clear(); // Reset log�w
    }

    private void HandleCommandInput(string command)
    {
        // Ignorujemy komendy, je�li interakcja jest wy��czona
        if (!canInteract)
        {
            return;
        }

        // Je�li gra zako�czy�a si� wygran�, blokujemy "hack"
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
                // Komenda "exit" ko�czy mini-gr� jako przegran�
                EndMiniGame(false);

                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;
                    playSoundOnObject.PlaySound("TerminalExit", 0.4f, false);
                }

                ResetTerminalState(); // Reset terminala do stanu pocz�tkowego
            }

            ClearInputField();
            return;
        }

        // Je�li terminal jest zabezpieczony, sprawd� has�o lub obs�u� komendy
        if (securedMonitor)
        {
            if (commandDictionary.ContainsKey(command))
            {
                // Je�li komenda istnieje w s�owniku, wywo�aj j�
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
                // Je�li podano poprawne has�o
                securedMonitor = false; // Zdejmij zabezpieczenie
                ClearMonitorConsole(); // Wyczy�� konsol�
                ShowConsoleMessage($">>> {LanguageManager.Instance.GetLocalizedMessage("correctPassword")}", "#FFD200");

                foreach (var playSoundOnObject in playSoundObjects)
                {
                    if (playSoundOnObject == null) continue;

                    playSoundOnObject.PlaySound("TerminalCorrectPassword", 0.5f);
                }

                InitializeLocalizedCommands(); // Zainicjuj komendy po odblokowaniu
                StartLogSequence(); // Rozpocznij sekwencj� log�w

                if (modelText != null)
                {
                    modelText.text = "Siegdu & Babi v2.7_4_1998";
                }

                // Usu� komend� "hack" (lub jej odpowiednik w innym j�zyku)
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
                // Je�li has�o jest nieprawid�owe
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

        // Wyczy�� i aktywuj pierwszy panel
        randomPanelText.text = "";
        randomPanel.SetActive(true);
        StartCoroutine(TypeRandomCharacters(randomPanelText, numberOfRandomCharactersPanel1));

        // Wy�wietl drugi panel po okre�lonym op�nieniu
        StartCoroutine(ShowSecondPanelAfterDelay());
    }

    private void ResetTerminalState()
    {
        // Wy��cz panele i wyczy�� ich tekst
        randomPanel.SetActive(false);
        randomPanel2.SetActive(false);
        randomPanelText.text = "";
        randomPanel2Text.text = "";

        // Przywr�� widoczno�� consoleTextDisplay, je�li by�o ukryte
        consoleTextDisplay?.gameObject.SetActive(true);
    }

    // Wy�wietlanie losowych znak�w
    private IEnumerator TypeRandomCharacters(TextMeshProUGUI textComponent, int numberOfCharacters)
    {
        textComponent.text = ""; // Wyczy�� tekst na pocz�tku

        // Generowanie losowego tekstu
        string randomText = GenerateRandomText(numberOfCharacters); // Przekazujemy liczb� znak�w

        // Wy�wietlanie znak�w jeden po drugim
        foreach (char c in randomText)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(typingSpeed); // Odczekaj przed dodaniem kolejnego znaku
        }
    }

    // Obs�uga drugiego panelu z op�nieniem
    private IEnumerator ShowSecondPanelAfterDelay()
    {
        yield return new WaitForSeconds(panelDelay);

        // Wyczy�� i aktywuj drugi panel
        randomPanel2Text.text = "";
        randomPanel2.SetActive(true);
        StartCoroutine(TypeRandomCharacters(randomPanel2Text, numberOfRandomCharactersPanel2));

        yield return new WaitForSeconds(randomPanelDuration);

        // Dezaktywuj oba panele
        randomPanel.SetActive(false);
        randomPanel2.SetActive(false);

        // Przywr�� consoleTextDisplay
        consoleTextDisplay?.gameObject.SetActive(true);

        // Aktywuj pole tekstowe
        inputField?.ActivateInputField();

        StartMiniGame(); // Rozpocznij mini-gr�
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

        // Czyszczenie monitora i resetowanie log�w
        ClearMonitorConsole();
        logHistory.Clear();

        isGameForceEnded = false; // Zresetuj flag� na pocz�tku nowej gry

        grid = new string[gridSize, gridSize];
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                grid[i, j] = "X"; // Ka�de pole pocz�tkowo ma warto�� "X"
            }
        }

        // Obliczanie pocz�tkowej pozycji kursora
        int centerX = gridSize / 2;
        int centerY = gridSize / 2;

        // Je�li rozmiar siatki jest parzysty, przesuwamy kursor o jedno pole w lewo i do g�ry
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

        // Sprawdzenie czy grid zosta� zainicjalizowany
        if (grid == null)
        {
            Debug.LogError("Grid is null. Ensure it is initialized before calling UpdateGridDisplay.");
            return;
        }

        // Sprawdzenie czy cursorPosition mie�ci si� w zakresie grid
        if (cursorPosition.x < 0 || cursorPosition.x >= gridSize ||
            cursorPosition.y < 0 || cursorPosition.y >= gridSize)
        {
            Debug.LogError("Cursor position is out of bounds. Ensure cursorPosition is within grid limits.");
            return;
        }

        // Sprawdzenie czy logHistory zosta�a zainicjalizowana
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
                    gridBuilder.Append($" [<color=#FFFF00>{cellValue}</color>] "); // ��ty kolor dla kursora
                }
                else if (cellValue != "X")
                {
                    gridBuilder.Append($" (<color=#FFFFFF>{cellValue}</color>) "); // Bia�y kolor dla warto�ci kom�rek
                }
                else
                {
                    gridBuilder.Append(" (  ) "); // Domy�lne puste kom�rki
                }
            }
            gridBuilder.AppendLine();
        }

        // Dodanie pustej linii mi�dzy tabel� a logami
        gridBuilder.AppendLine();

        // Wy�wietlanie czasu
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
                timerColor = "#FFD200"; // z�oty
            else
                timerColor = "#00E700"; // zielony

            string localizedTimeLeft = LanguageManager.Instance.GetLocalizedMessage("miniGameTimer");
            gridBuilder.AppendLine($"<color={timerColor}> >>> {localizedTimeLeft}: {formattedTime}</color>");
        }

        // Dodanie pustej linii mi�dzy tabel� a logami
        gridBuilder.AppendLine();

        // Obs�uga log�w
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
            // Zatrzymaj korutyn� BlinkTargetCell, je�li dzia�a
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null; // Resetuj referencj�
                ResetTargetCellVisibility(); // Przywr�� widoczno�� celu
            }

            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isMiniGameActive)
                {
                    isGameForceEnded = true; // Ustaw flag� na true
                    EndMiniGame(false); // Zako�cz gr� jako przegran�
                }
            }
        }

        // Odtw�rz d�wi�k tylko je�li by� ruch
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
            // Przesu� obiekt kursora na aktualn� kom�rk�
            cursor.transform.position = GetCellPosition(cursorPosition.x, cursorPosition.y);
        }
    }

    private Vector3 GetCellPosition(int row, int col)
    {
        // Zak�adamy, �e kom�rki s� rozmieszczone w siatce 2D w przestrzeni
        // Zwraca pozycj� w przestrzeni w oparciu o siatk�
        return new Vector3(col * 1.1f, -row * 1.1f, 0); // Przyk�adowe rozmieszczenie
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
                yield break; // Zako�cz korutyn�
            }

            timeRemaining -= Time.deltaTime;
            if (timeRemaining < 0f)
                timeRemaining = 0f;

            UpdateGridDisplay(null); // Aktualizacja siatki z nowym czasem
            yield return null; // Czekaj jedn� klatk�
        }

        Debug.Log("Timer has reached zero.");

        if (isMiniGameActive)
        {
            Debug.Log("Ending mini-game because time is up.");
            EndMiniGame(false); // Je�li czas si� sko�czy� i gra trwa � przegrana
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

        isMiniGameActive = false; // Wy��czamy mini-gr�
        canInteract = false; // Blokujemy interakcj�

        if (isWin)
        {

            foreach (var playSoundOnObject in playSoundObjects)
            {
                if (playSoundOnObject == null) continue;
                playSoundOnObject.PlaySound("TerminalWin", 0.7F, false);
            }

            hasWonGame = true; // Oznaczamy, �e gra zako�czy�a si� wygran�
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
                commandDictionary.Remove(localizedHackCommand); // Usuwamy komend� z listy
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

        // Rozpoczynamy sekwencj� startow� po czasie migania tylko, je�li gra nie zosta�a wymuszenie zako�czona
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
            // Miganie: Na zmian� pokazuj/ukrywaj cel za pomoc� przezroczysto�ci
            grid[target.x, target.y] = isVisible ? "<color=#FFFFFF>0</color>" : "<color=#00000000>0</color>";
            UpdateGridDisplay(null); // Aktualizacja siatki z migaj�cym celem

            isVisible = !isVisible; // Zmie� stan widoczno�ci
            elapsedTime += 0.5f; // Odczekaj 0.5 sekundy
            yield return new WaitForSeconds(0.5f);

            // Sprawd�, czy korutyna zosta�a przerwana przez gracza
            if (blinkCoroutine == null)
            {
                yield break; // Natychmiast zako�cz korutyn�
            }
        }

        // Po zako�czeniu migania upewnij si�, �e cel pozostaje widoczny
        grid[target.x, target.y] = "<color=#FFFFFF>0</color>";
        UpdateGridDisplay(null); // Finalna aktualizacja siatki
        blinkCoroutine = null; // Resetuj referencj� po zako�czeniu
    }

    // Dodaj pomocnicz� metod� do resetowania widoczno�ci celu
    private void ResetTargetCellVisibility()
    {
        if (targetCoordinate != null)
        {
            grid[targetCoordinate.x, targetCoordinate.y] = "<color=#FFFFFF>0</color>";
            UpdateGridDisplay(null); // Finalna aktualizacja siatki z widocznym celem
        }
    }

    // Dodana metoda uruchamiaj�ca sekwencj� startow� po op�nieniu
    private IEnumerator StartSequenceAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Czy�cimy wy�wietlacz konsoli tu� przed rozpocz�ciem sekwencji
        // Czyszczenie monitora i resetowanie log�w
        ClearMonitorConsole();
        logHistory.Clear();

        // Rozpoczynamy sekwencj� startow�
        StartLogSequence();
        isGameForceEnded = false; // Zresetuj flag� na pocz�tku nowej gry
    }

    private void ModelNameRandomSymbols()
    {
        if (modelText != null)
        {
            //                    "Siegdu & Babi v2.7_4_1998";
            string baseTemplate = "******-&-****-*/.^_%_!##$";
            int templateLength = baseTemplate.Length;
            int passwordLength = generatedPassword.Length;

            // Wybierz losowe unikalne indeksy w bazowym tek�cie do wstawienia cyfr
            List<int> insertionIndices = Enumerable.Range(0, templateLength)
                .OrderBy(_ => UnityEngine.Random.value)
                .Take(passwordLength)
                .OrderBy(i => i) // Wa�ne: zachowaj kolejno�� od lewej do prawej
                .ToList();

            // Zamie� znaki w bazowym ci�gu na cyfry z has�a z kolorem
            char[] result = baseTemplate.ToCharArray();
            for (int i = 0; i < passwordLength; i++)
            {
                string coloredDigit = $"<color=#FFD200>{generatedPassword[i]}</color>";
                int index = insertionIndices[i];

                // Zamie� znak na znacznik koloru + cyfra
                // Uwaga: trzeba b�dzie u�y� StringBuilder zamiast char[] je�li liczysz na renderowanie tag�w
                result[index] = '\0'; // tymczasowo znak pusty, p�niej zast�pimy

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