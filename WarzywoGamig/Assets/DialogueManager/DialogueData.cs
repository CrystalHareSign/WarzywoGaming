using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Data", fileName = "NewDialogueData")]
public class DialogueData : ScriptableObject
{
    public DialogueNode[] nodes;
    public int startNode = 0;
}

[System.Serializable]
public class DialogueNode
{
    public string speakerName;

    [Header("Tekst w r�nych j�zykach")]
    [TextArea(2, 6)] public string textEnglish;
    [TextArea(2, 6)] public string textPolish;
    [TextArea(2, 6)] public string textGerman;

    [Header("Audio w r�nych j�zykach")]
    public AudioClip audioEnglish;
    public AudioClip audioPolish;
    public AudioClip audioGerman;

    public DialogueResponse[] responses;
}

[System.Serializable]
public class DialogueResponse
{
    [Header("Odpowied� w r�nych j�zykach")]
    public string textEnglish;
    public string textPolish;
    public string textGerman;

    public int nextNode = -1;
    public string action;
}