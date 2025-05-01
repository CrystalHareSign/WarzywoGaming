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
    public static bool CanUseMenu = true; // Flaga, która kontroluje, czy menu jest dostêpne

    [Header("UI – Konsola monitora")]
    public TextMeshProUGUI consoleTextDisplay;
    public TMP_InputField inputField; // Pole tekstowe dla wpisywania komend
    private Queue<ConsoleMessage> messageQueue = new Queue<ConsoleMessage>();
    public int maxMessages = 5;

    public float letterDisplayDelay = 0.05f; // OpóŸnienie miêdzy literami w sekundach
    public float cursorBlinkInterval = 0.5f;

    public List<LogEntry> logEntries;
    // S³ownik komend do metod
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
            inputField.onEndEdit.AddListener(HandleCommandInput); // Dodaj obs³ugê zakoñczenia edycji
            inputField.characterLimit = 30;//Ogranicz liczbê znaków do 20 (zmieñ na wartoœæ, któr¹ chcesz)
        }

        commandDictionary = new Dictionary<string, CommandData>

        {
            { "main", new CommandData(() => ExitTerminalAndChangeScene("Main", 3f), true) },
            { "home", new CommandData(() => ExitTerminalAndChangeScene("Home", 3f), true) },
        };

        // Jeœli trzeba, przypisz referencjê do obiektu ButtonMethods (jeœli jeszcze nie zosta³o przypisane)
        if (sceneChanger == null)
        {
            sceneChanger = UnityEngine.Object.FindAnyObjectByType<SceneChanger>();
        }

    }

    public void ExitTerminalAndChangeScene(string targetSceneName, float delaySeconds = 3f)
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (currentScene == targetSceneName)
        {
            ShowConsoleMessage($">>> Ju¿ jesteœ w scenie \"{targetSceneName}\".", "#FF0000");
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
            ShowConsoleMessage(">>> Rafinacja w toku. Nie mo¿na zmieniæ sceny.", "#FF0000");
            yield break; // Zatrzymaj korutynê, jeœli rafinacja w toku
        }

        // Jeœli nie ma rafinacji, przechodzimy do dalszych operacji
        yield return StartCoroutine(MoveCameraBackToOriginalPosition());

        CanUseMenu = false;

        // Zablokuj mo¿liwoœæ interakcji
        DisablePlayerMovementAndMouseLook();

        ShowConsoleMessage($">>> Zmieniam scenê na {sceneName} za {delay} sekund...", "#FFD200");

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


    void Update()
    {
        float distanceToInteraction = Vector3.Distance(player.position, finalCameraRotation.position);

        if (distanceToInteraction <= interactionRange)
        {
            if (Input.GetKeyDown(KeyCode.E) && !isInteracting && !isCameraMoving)
            {
                originalCameraPosition = Camera.main.transform.position;
                originalCameraRotation = Camera.main.transform.rotation;
                StartCoroutine(MoveCameraToPosition());
                ClearMonitorConsole();
            }
            else if ((Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape)) && isInteracting && !isCameraMoving)
            {
                StartCoroutine(MoveCameraBackToOriginalPosition());
            }
        }
        else if (isInteracting && !isCameraMoving)
        {
            StartCoroutine(MoveCameraBackToOriginalPosition());
        }
    }

    IEnumerator MoveCameraToPosition()
    {
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

        ShowConsoleMessage(">>>Uruchamianie terminalu...", "#00E700");

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

    public void ShowConsoleMessage(string rawMessage, string hexColor)
    {
        isTerminalInterrupted = false;

        Color color = Color.white;
        if (ColorUtility.TryParseHtmlString(hexColor, out Color parsedColor))
        {
            color = parsedColor;
        }

        StartCoroutine(TypeMessageCoroutine(rawMessage, color));
    }

    private IEnumerator TypeMessageCoroutine(string fullMessage, Color color)
    {
        string currentLine = "";
        for (int i = 0; i < fullMessage.Length; i++)
        {
            if (isTerminalInterrupted)
                yield break; // Przerwij wyœwietlanie wiadomoœci jeœli terminal zosta³ zamkniêty

            currentLine += fullMessage[i];
            UpdateConsolePreview(currentLine, color);
            yield return new WaitForSeconds(letterDisplayDelay);
        }

        if (isTerminalInterrupted)
            yield break;

        var finalMessage = new ConsoleMessage(fullMessage, Time.time, color);
        messageQueue.Enqueue(finalMessage);

        while (messageQueue.Count > maxMessages)
            messageQueue.Dequeue();

        UpdateConsoleText();
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
    }
    private void HandleCommandInput(string command)
    {
        command = command.ToLower().Trim();

        if (!string.IsNullOrEmpty(pendingCommand))
        {
            if (command == "t")
            {
                ShowConsoleMessage($">>> Wykonujê: {pendingCommand}", "#00E700");
                commandDictionary[pendingCommand].command.Invoke();
            }
            else
            {
                ShowConsoleMessage(">>> Anulowano polecenie.", "#FF0000");
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

                // Sprawdzenie, czy gracz ju¿ jest w tej scenie
                string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

                if (data.requiresConfirmation && command != currentScene.ToLower())
                {
                    ShowConsoleMessage(">>> Czy jesteœ pewny? [T/N]", "#FFD200");
                    pendingCommand = command;
                }
                else
                {
                    data.command.Invoke();
                }
            }
            else
            {
                ShowConsoleMessage(">>> polecenie nieznane. spróbuj ponownie", "#FF0000");
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
        {
            StopCoroutine(logSequenceCoroutine); // zatrzymaj poprzedni¹ sekwencjê
        }
        logSequenceCoroutine = StartCoroutine(DisplayLogsSequence(availableLogs));
    }

    private IEnumerator DisplayLogsSequence(List<LogEntry> availableLogs)
    {
        while (availableLogs.Count > 0)
        {
            LogEntry logEntry = GetRandomLogWithProbability(availableLogs);

            // Debugowanie: wypisanie d³ugoœci tablicy messages
            //Debug.Log("D³ugoœæ tablicy messages: " + logEntry.messages.Length);

            // Sprawdzamy, czy tablica 'messages' w logEntry nie jest pusta
            if (logEntry.messages.Length > 0)
            {
                // Losujemy jeden z komunikatów w tablicy
                int randomIndex = UnityEngine.Random.Range(0, logEntry.messages.Length); // Poprawne losowanie
                //Debug.Log("Losowy indeks: " + randomIndex);  // Debugowanie: jaki indeks zosta³ wybrany
                ShowConsoleMessage(logEntry.messages[randomIndex], "#00E700");
            }
            else
            {
                // Jeœli tablica jest pusta, wyœwietlamy komunikat o b³êdzie
                ShowConsoleMessage(">>> Brak wiadomoœci do wyœwietlenia.", "#FF0000");
            }

            availableLogs.Remove(logEntry);

            yield return new WaitForSeconds(logEntry.delayAfterPrevious);
        }

        ShowConsoleMessage(">>> Terminal gotowy.", "#FFD200");

        yield return new WaitForSeconds(1f);

        canInteract = true;

        // Aktywuj pole tekstowe do wpisywania komend
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