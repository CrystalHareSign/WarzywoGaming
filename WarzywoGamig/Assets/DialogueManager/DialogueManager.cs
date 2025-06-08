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
    public Button[] answerButtons;             // 4 przyciski
    public TMP_Text[] answerButtonTexts;       // 4 TMP_Texty do opis�w tych przycisk�w
    public AudioSource audioSource;

    private DialogueData currentDialogue;
    private int currentNodeIndex;

    // Dodane: referencje do skrypt�w od ruchu i kamery
    private PlayerMovement playerMovement;
    private MouseLook mouseLook;

    // Statyczna flaga aktywno�ci dialogu
    public static bool DialogueActive { get; private set; } = false;

    // Singleton (opcjonalnie je�li chcesz mie� Instance)
    public static DialogueManager Instance { get; private set; }

    // Dodane: referencja do aktualnego hovera rozm�wcy
    private HoverMessage currentNpcHover;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Znajd� komponenty w scenie (mo�esz te� zrobi� to przez publiczne pola i przeci�gn�� w Inspectorze)
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        mouseLook = FindAnyObjectByType<MouseLook>();
    }

    // Dodane: teraz przyjmujemy referencj� do HoverMessage NPC
    public void StartDialogue(DialogueData dialogue, HoverMessage npcHover)
    {
        DialogueActive = true;
        currentNpcHover = npcHover;
        if (currentNpcHover != null)
            currentNpcHover.isInteracted = true;

        currentDialogue = dialogue;
        currentNodeIndex = dialogue.startNode;

        // Wy��cz ruch i kamer� oraz odblokuj kursor
        if (playerMovement != null) playerMovement.enabled = false;
        if (mouseLook != null) mouseLook.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Poka� panel dialogu
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ShowCurrentNode();
    }

    void ShowCurrentNode()
    {
        var node = currentDialogue.nodes[currentNodeIndex];
        speakerText.text = node.speakerName;
        dialogueText.text = GetLocalizedNodeText(node);

        // Audio
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

        // Przewi� na g�r� scrolla
        Canvas.ForceUpdateCanvases();
        if (dialogueScrollRect)
            dialogueScrollRect.verticalNormalizedPosition = 1f;

        // Ustaw przyciski
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

        // W��cz ruch i kamer� oraz zablokuj kursor
        if (playerMovement != null) playerMovement.enabled = true;
        if (mouseLook != null) mouseLook.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Przywr�� hover NPC je�li by� przekazany
        if (currentNpcHover != null)
        {
            currentNpcHover.isInteracted = false;
            currentNpcHover = null;
        }

        // Flaga nieaktywny dialog
        DialogueActive = false;
    }

    void PerformAction(string action)
    {
        Debug.Log("NPC Action: " + action);
        // Dodaj tu w�asn� obs�ug� np. if (action == "OpenGate") { ... }
    }
}