using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialoguePanel;
    public TMP_Text speakerText;
    public TMP_Text dialogueText;
    public ScrollRect dialogueScrollRect;
    public Button[] answerButtons;
    public TMP_Text[] answerButtonTexts;
    public AudioSource audioSource;

    private DialogueData currentDialogue;
    private int currentNodeIndex;

    private PlayerMovement playerMovement;
    private MouseLook mouseLook;

    public static bool DialogueActive { get; private set; } = false;
    public static DialogueManager Instance { get; private set; }

    // Dodane: referencja do aktualnego hovera rozmówcy
    private HoverMessage currentNpcHover;

    // NOWE: referencja do InteractableItem, który wywo³a³ dialog
    private InteractableItem currentInteractableItem;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        playerMovement = FindAnyObjectByType<PlayerMovement>();
        mouseLook = FindAnyObjectByType<MouseLook>();
    }

    public void StartDialogue(DialogueData dialogue, HoverMessage npcHover, InteractableItem interactableItem = null)
    {
        DialogueActive = true;
        currentNpcHover = npcHover;
        if (currentNpcHover != null)
            currentNpcHover.isInteracted = true;

        currentDialogue = dialogue;
        currentNodeIndex = dialogue.startNode;
        currentInteractableItem = interactableItem;

        if (playerMovement != null) playerMovement.enabled = false;
        if (mouseLook != null) mouseLook.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ShowCurrentNode();
    }

    void ShowCurrentNode()
    {
        var node = currentDialogue.nodes[currentNodeIndex];
        speakerText.text = GetLocalizedSpeakerName(currentDialogue); // <-- poprawka!
        dialogueText.text = GetLocalizedNodeText(node);

        if (audioSource)
        {
            audioSource.Stop();
            AudioClip localizedClip = GetLocalizedAudioClip(node);
            if (localizedClip != null)
            {
                audioSource.clip = localizedClip;
                audioSource.Play();
            }
        }

        Canvas.ForceUpdateCanvases();
        if (dialogueScrollRect)
            dialogueScrollRect.verticalNormalizedPosition = 1f;

        int answersCount = node.responses != null ? node.responses.Length : 0;
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < answersCount)
            {
                answerButtons[i].gameObject.SetActive(true);
                answerButtons[i].interactable = true;
                answerButtonTexts[i].text = GetLocalizedResponseText(node.responses[i]);

                int idx = i; // closure
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => OnResponse(idx));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
                answerButtons[i].onClick.RemoveAllListeners();
            }
        }
    }

    // Nowa wersja – bierze imiê z DialogueData, nie z node!
    string GetLocalizedSpeakerName(DialogueData data)
    {
        if (LanguageManager.Instance != null)
        {
            switch (LanguageManager.Instance.currentLanguage)
            {
                case LanguageManager.Language.Polski:
                    return string.IsNullOrEmpty(data.speakerNamePolish) ? data.speakerNameEnglish : data.speakerNamePolish;
                case LanguageManager.Language.Deutsch:
                    return string.IsNullOrEmpty(data.speakerNameGerman) ? data.speakerNameEnglish : data.speakerNameGerman;
                default:
                    return data.speakerNameEnglish;
            }
        }
        return data.speakerNameEnglish;
    }

    string GetLocalizedNodeText(DialogueNode node)
    {
        if (LanguageManager.Instance != null)
        {
            switch (LanguageManager.Instance.currentLanguage)
            {
                case LanguageManager.Language.Polski:
                    return string.IsNullOrEmpty(node.textPolish) ? node.textEnglish : node.textPolish;
                case LanguageManager.Language.Deutsch:
                    return string.IsNullOrEmpty(node.textGerman) ? node.textEnglish : node.textGerman;
                default:
                    return node.textEnglish;
            }
        }
        return node.textEnglish;
    }

    string GetLocalizedResponseText(DialogueResponse response)
    {
        if (LanguageManager.Instance != null)
        {
            switch (LanguageManager.Instance.currentLanguage)
            {
                case LanguageManager.Language.Polski:
                    return string.IsNullOrEmpty(response.textPolish) ? response.textEnglish : response.textPolish;
                case LanguageManager.Language.Deutsch:
                    return string.IsNullOrEmpty(response.textGerman) ? response.textEnglish : response.textGerman;
                default:
                    return response.textEnglish;
            }
        }
        return response.textEnglish;
    }

    AudioClip GetLocalizedAudioClip(DialogueNode node)
    {
        if (LanguageManager.Instance != null)
        {
            switch (LanguageManager.Instance.currentLanguage)
            {
                case LanguageManager.Language.Polski:
                    return node.audioPolish != null ? node.audioPolish : node.audioEnglish;
                case LanguageManager.Language.Deutsch:
                    return node.audioGerman != null ? node.audioGerman : node.audioEnglish;
                default:
                    return node.audioEnglish;
            }
        }
        return node.audioEnglish;
    }

    void OnResponse(int responseIndex)
    {
        var response = currentDialogue.nodes[currentNodeIndex].responses[responseIndex];

        if (!string.IsNullOrEmpty(response.action))
        {
            PerformAction(response.action);
        }

        if (currentInteractableItem != null && response.setDialogueIndex >= 0)
        {
            currentInteractableItem.SetDialogueIndex(response.setDialogueIndex);
        }

        if (response.nextNode == -1)
        {
            EndDialogue();
        }
        else
        {
            currentNodeIndex = response.nextNode;
            ShowCurrentNode();
        }
    }

    void EndDialogue()
    {
        dialogueText.text = "";
        speakerText.text = "";
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].gameObject.SetActive(false);
            answerButtons[i].onClick.RemoveAllListeners();
        }
        if (audioSource) audioSource.Stop();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (playerMovement != null) playerMovement.enabled = true;
        if (mouseLook != null) mouseLook.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (currentNpcHover != null)
        {
            currentNpcHover.isInteracted = false;
            currentNpcHover = null;
        }

        currentInteractableItem = null;
        DialogueActive = false;
    }
    void PerformAction(string action)
    {
        if (!string.IsNullOrEmpty(action))
        {
            // Zak³adamy, ¿e masz DialogueActionHandler jako singleton lub komponent w scenie
            DialogueActionHandler.Instance?.HandleAction(action, currentInteractableItem);
        }
    }
}