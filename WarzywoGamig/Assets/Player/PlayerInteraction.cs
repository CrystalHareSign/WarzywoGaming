using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionRange = 4f;
    public LayerMask interactableLayer;
    public Camera playerCamera;
    public Image progressCircle;
    public TMP_Text messageText;

    private float interactionTimer = 0f;
    private InteractableItem currentInteractableItem = null;
    private float requiredHoldTime = 5f;

    void Update()
    {
        if (playerCamera == null)
        {
            Debug.LogError("PlayerCamera is not assigned in the Inspector.");
            return;
        }

        // Sprawdzamy, czy gracz trzyma loot
        Inventory playerInventory = Object.FindFirstObjectByType<Inventory>();
        if (playerInventory != null && playerInventory.lootParent != null && playerInventory.lootParent.childCount > 0)
        {
            HideUI();
            return; // Zatrzymujemy dalsze przetwarzanie, gdy gracz trzyma loot
        }

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, interactionRange, interactableLayer))
        {
            InteractableItem interactableItem = hit.collider.GetComponent<InteractableItem>();
            if (interactableItem != null && !interactableItem.hoverMessage.isInteracted)
            {
                if (messageText != null && interactableItem.hoverMessage != null)
                {
                    messageText.text = interactableItem.hoverMessage.message;
                    messageText.gameObject.SetActive(true);
                }

                if (progressCircle != null)
                {
                    progressCircle.gameObject.SetActive(true);
                }

                if (Input.GetKey(KeyCode.E))
                {
                    if (currentInteractableItem == interactableItem)
                    {
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
