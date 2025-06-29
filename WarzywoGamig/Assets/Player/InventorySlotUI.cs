using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text slotNumberText;
    [SerializeField] private TMP_Text itemNameText; // Dodaj do wyœwietlania nazwy przedmiotu
    private InteractableItem currentItem;

    public bool IsEmpty => currentItem == null;

    public void SetUnlocked(bool unlocked)
    {
        if (backgroundImage != null)
            backgroundImage.color = unlocked ? Color.white : Color.gray;
    }

    public void SetSlotNumber(int? slotIndex)
    {
        if (slotNumberText != null)
        {
            if (slotIndex.HasValue)
            {
                slotNumberText.text = (slotIndex.Value + 1).ToString();
                slotNumberText.gameObject.SetActive(true);
            }
            else
            {
                slotNumberText.gameObject.SetActive(false);
            }
        }
    }

    public void HideSlotNumber()
    {
        if (slotNumberText != null)
            slotNumberText.gameObject.SetActive(false);
    }

    public void SetItem(InteractableItem item)
    {
        currentItem = item;
        if (itemNameText != null)
        {
            if (item != null)
            {
                itemNameText.text = item.itemName;
                itemNameText.gameObject.SetActive(true);
            }
            else
            {
                itemNameText.gameObject.SetActive(false);
            }
        }
    }
}