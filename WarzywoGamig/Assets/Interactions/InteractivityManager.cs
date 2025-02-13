using System.Collections.Generic;
using UnityEngine;

public class InteractivityManager : MonoBehaviour
{
    public static InteractivityManager Instance;

    private Dictionary<GameObject, bool> interactivityStatus = new Dictionary<GameObject, bool>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Upewnij siê, ¿e obiekt nie zostanie zniszczony przy zmianie sceny
            Debug.Log("InteractivityManager initialized.");
        }
        else
        {
            Debug.LogWarning("Another instance of InteractivityManager found. Destroying this instance.");
            Destroy(gameObject);
        }
    }

    public void RegisterInteractable(GameObject interactable, bool alwaysInteractive)
    {
        if (!interactivityStatus.ContainsKey(interactable))
        {
            interactivityStatus[interactable] = alwaysInteractive;
            Debug.Log($"Registered interactable: {interactable.name}, always interactive: {alwaysInteractive}");
        }
    }

    public void UpdateInteractivityStatus(GameObject interactable, bool isInteractive)
    {
        if (interactivityStatus.ContainsKey(interactable))
        {
            // Nie aktualizujemy statusu interaktywnoœci jeœli obiekt jest zawsze interaktywny
            if (!interactivityStatus[interactable])
            {
                interactivityStatus[interactable] = isInteractive;
                Debug.Log($"Updated interactivity status for {interactable.name} to {isInteractive}");
            }
        }
    }

    public bool IsInteractable(GameObject interactable)
    {
        if (interactivityStatus.ContainsKey(interactable))
        {
            return interactivityStatus[interactable];
        }
        return false;
    }

    public void RestoreInteractivity(GameObject interactable)
    {
        if (interactivityStatus.ContainsKey(interactable))
        {
            interactivityStatus[interactable] = true;
            var hoverMessage = interactable.GetComponent<HoverMessage>();
            if (hoverMessage != null)
            {
                hoverMessage.isInteracted = false;
            }
            Debug.Log($"Restored interactivity for {interactable.name}");
        }
    }
}