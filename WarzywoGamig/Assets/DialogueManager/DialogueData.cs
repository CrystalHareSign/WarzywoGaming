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
    [Header("OdpowiedŸ w ró¿nych jêzykach")]
    public string textEnglish;
    public string textPolish;
    public string textGerman;

    public int nextNode = -1;
    public string action;
}