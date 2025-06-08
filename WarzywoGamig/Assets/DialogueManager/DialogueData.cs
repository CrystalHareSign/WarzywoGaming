using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Data", fileName = "NewDialogueData")]
public class DialogueData : ScriptableObject
{
    [Header("Imiê postaci w ró¿nych jêzykach")]
    public string speakerNameEnglish;
    public string speakerNamePolish;
    public string speakerNameGerman;

    public DialogueNode[] nodes;
    public int startNode = 0;
}

[System.Serializable]
public class DialogueNode
{
    [Header("Tekst w ró¿nych jêzykach")]
    [TextArea(2, 6)] public string textEnglish;
    [TextArea(2, 6)] public string textPolish;
    [TextArea(2, 6)] public string textGerman;

    [Header("Audio w ró¿nych jêzykach")]
    public AudioClip audioEnglish;
    public AudioClip audioPolish;
    public AudioClip audioGerman;

    public DialogueResponse[] responses;
}

[System.Serializable]
public class DialogueResponse
{
    public string textEnglish;
    public string textPolish;
    public string textGerman;

    public int nextNode = -1; // domyœlnie -1: koniec rozmowy
    public string action; // opcjonalnie: akcja do wykonania
    public int setDialogueIndex = -1; // domyœlnie -1: nie zmieniaj dialogu
}