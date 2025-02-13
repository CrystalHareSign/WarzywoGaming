using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionRange = 4f; // Zasiêg interakcji
    public LayerMask interactableLayer; // Warstwa interaktywnych obiektów (ustaw w inspektorze)
    public Camera playerCamera;
    public Image progressCircle;
    public TMP_Text messageText; // Odniesienie do komponentu TMP_Text

    private float interactionTimer = 0f; // Licznik czasu trzymania przycisku
    private InteractableItem currentInteractableItem = null; // Aktualnie celowany obiekt interaktywny
    private float requiredHoldTime = 5f; // Domyœlny czas trzymania przycisku interakcji w sekundach

    void Update()
    {
        // Check if playerCamera is assigned
        if (playerCamera == null)
        {
            Debug.LogError("PlayerCamera is not assigned in the Inspector.");
            return;
        }

        // Rzutowanie promienia z pozycji gracza do przodu
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, interactionRange, interactableLayer))
        {
            InteractableItem interactableItem = hit.collider.GetComponent<InteractableItem>();
            if (interactableItem != null && !interactableItem.hoverMessage.isInteracted)
            {
                // Wyœwietl komunikat po najechaniu kursorem na obiekt
                if (messageText != null && interactableItem.hoverMessage != null)
                {
                    messageText.text = interactableItem.hoverMessage.message;
                    messageText.gameObject.SetActive(true);
                }

                if (progressCircle != null)
                {
                    progressCircle.gameObject.SetActive(true);
                }

                // SprawdŸ, czy gracz trzyma przycisk interakcji
                if (Input.GetKey(KeyCode.E))
                {
                    if (currentInteractableItem == interactableItem)
                    {
                        // Ustaw wymagany czas trzymania przycisku dla aktualnego obiektu interaktywnego
                        requiredHoldTime = currentInteractableItem.requiredHoldTime;

                        interactionTimer += Time.deltaTime;
                        if (progressCircle != null)
                        {
                            progressCircle.fillAmount = interactionTimer / requiredHoldTime;
                        }

                        if (interactionTimer >= requiredHoldTime)
                        {
                            InteractWithObject(currentInteractableItem);
                            interactionTimer = 0f;
                            HideUI();
                        }
                    }
                    else
                    {
                        currentInteractableItem = interactableItem;
                        interactionTimer = 0f;
                        if (progressCircle != null)
                        {
                            progressCircle.fillAmount = 0f;
                        }
                    }
                }
                else
                {
                    interactionTimer = 0f;
                    if (progressCircle != null)
                    {
                        progressCircle.fillAmount = 0f;
                    }
                }
            }
            else
            {
                ResetInteraction();
            }
        }
        else
        {
            ResetInteraction();
        }
    }

    private void InteractWithObject(InteractableItem interactableItem)
    {
        if (interactableItem != null)
        {
            interactableItem.Interact();
        }
    }

    private void HideUI()
    {
        if (progressCircle != null)
        {
            progressCircle.gameObject.SetActive(false);
        }

        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
    }

    private void ResetInteraction()
    {
        interactionTimer = 0f;
        if (progressCircle != null)
        {
            progressCircle.fillAmount = 0f;
            progressCircle.gameObject.SetActive(false);
        }

        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }

        currentInteractableItem = null;
    }
}