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

    public List<LogEntry> logEntries;
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

        commandDictionary = new Dictionary<string, CommandData>

        {
            { "main", new CommandData(() => ExitTerminalAndChangeScene("Main", 3f), true) },
            { "home", new CommandData(() => ExitTerminalAndChangeScene("Home", 3f), true) },
        };

        // Je�li trzeba, przypisz referencj� do obiektu ButtonMethods (je�li jeszcze nie zosta�o przypisane)
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
            ShowConsoleMessage($">>> Ju� jeste� w scenie \"{targetSceneName}\".", "#FF0000");
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
            ShowConsoleMessage(">>> Rafinacja w toku. Nie mo�na zmieni� sceny.", "#FF0000");
            yield break; // Zatrzymaj korutyn�, je�li rafinacja w toku
        }

        // Je�li nie ma rafinacji, przechodzimy do dalszych operacji
        yield return StartCoroutine(MoveCameraBackToOriginalPosition());

        CanUseMenu = false;

        // Zablokuj mo�liwo�� interakcji
        DisablePlayerMovementAndMouseLook();

        ShowConsoleMessage($">>> Zmieniam scen� na {sceneName} za {delay} sekund...", "#FFD200");

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
                yield break; // Przerwij wy�wietlanie wiadomo�ci je�li terminal zosta� zamkni�ty

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

        if (!string.IsNullOrEmpty(pendingCommand))
        {
            if (command == "t")
            {
                ShowConsoleMessage($">>> Wykonuj�: {pendingCommand}", "#00E700");
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

                // Sprawdzenie, czy gracz ju� jest w tej scenie
                string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

                if (data.requiresConfirmation && command != currentScene.ToLower())
                {
                    ShowConsoleMessage(">>> Czy jeste� pewny? [T/N]", "#FFD200");
                    pendingCommand = command;
                }
                else
                {
                    data.command.Invoke();
                }
            }
            else
            {
                ShowConsoleMessage(">>> polecenie nieznane. spr�buj ponownie", "#FF0000");
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
            StopCoroutine(logSequenceCoroutine); // zatrzymaj poprzedni� sekwencj�
        }
        logSequenceCoroutine = StartCoroutine(DisplayLogsSequence(availableLogs));
    }

    private IEnumerator DisplayLogsSequence(List<LogEntry> availableLogs)
    {
        while (availableLogs.Count > 0)
        {
            LogEntry logEntry = GetRandomLogWithProbability(availableLogs);

            // Debugowanie: wypisanie d�ugo�ci tablicy messages
            //Debug.Log("D�ugo�� tablicy messages: " + logEntry.messages.Length);

            // Sprawdzamy, czy tablica 'messages' w logEntry nie jest pusta
            if (logEntry.messages.Length > 0)
            {
                // Losujemy jeden z komunikat�w w tablicy
                int randomIndex = UnityEngine.Random.Range(0, logEntry.messages.Length); // Poprawne losowanie
                //Debug.Log("Losowy indeks: " + randomIndex);  // Debugowanie: jaki indeks zosta� wybrany
                ShowConsoleMessage(logEntry.messages[randomIndex], "#00E700");
            }
            else
            {
                // Je�li tablica jest pusta, wy�wietlamy komunikat o b��dzie
                ShowConsoleMessage(">>> Brak wiadomo�ci do wy�wietlenia.", "#FF0000");
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