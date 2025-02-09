using UnityEngine;

public class HoverMessage : MonoBehaviour
{
    public string message;
    public float interactionDistance = 5f;
    public bool isInteracted = false;
    public bool alwaysActive = false; // Czy obiekt jest zawsze aktywny
}