using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LootShop : MonoBehaviour
{
    [System.Serializable]
    public class LootCategory
    {
        public string category;
        public Sprite icon;
        public float sellValue;
    }

    public List<LootCategory> lootCategories = new List<LootCategory>();
    public Image lootImage;
    public Text lootPriceText;
    public Text lootCategoryText;

    // Nowe zmienne do SellArea
    public Transform sellArea;  // Transform strefy sprzeda�y (SellArea)
    public GameObject rowPrefab;  // Prefab UI dla pojedynczego wiersza
    public Transform rowParent;  // Rodzic dla wszystkich wierszy w kanwie (powinien mie� VerticalLayoutGroup)
    public Text totalValueText;  // Tekst dla ��cznej warto�ci wszystkich przedmiot�w w SellArea

    private Dictionary<string, LootCategory> lootDictionary = new Dictionary<string, LootCategory>();

    void Start()
    {
        // Tworzenie s�ownika dla szybszego dost�pu
        foreach (var loot in lootCategories)
        {
            lootDictionary[loot.category] = loot;
        }

        // Aktualizacja UI na podstawie przedmiot�w w SellArea
        UpdateSellAreaUI();
    }

    // Funkcja do aktualizacji UI dla SellArea
    public void UpdateSellAreaUI()
    {
        // Wyczy�� poprzednie elementy UI
        foreach (Transform child in rowParent)
        {
            Destroy(child.gameObject);
        }

        float totalValue = 0f;
        var lootCounts = new Dictionary<string, int>();  // S�ownik do liczenia przedmiot�w wg kategorii

        // Zbieranie danych o przedmiotach w SellArea
        foreach (Transform child in sellArea)
        {
            TreasureValue treasureValue = child.GetComponent<TreasureValue>();
            if (treasureValue != null)
            {
                if (lootCounts.ContainsKey(treasureValue.category))
                {
                    lootCounts[treasureValue.category]++;
                }
                else
                {
                    lootCounts[treasureValue.category] = 1;
                }
            }
        }

        // Wy�wietlanie danych o przedmiotach w SellArea
        foreach (var entry in lootCounts)
        {
            if (lootDictionary.TryGetValue(entry.Key, out LootCategory lootInfo))
            {
                // Tworzenie nowego wiersza UI dla tej kategorii
                GameObject row = Instantiate(rowPrefab, rowParent);

                // Pobieranie komponent�w tekstowych w wierszu
                Text[] rowTexts = row.GetComponentsInChildren<Text>();
                if (rowTexts.Length >= 3)
                {
                    // Ustawianie tekst�w: kategoria, ilo�� oraz ��czna warto��
                    rowTexts[0].text = lootInfo.category;  // Kategoria
                    rowTexts[1].text = "x " + entry.Value.ToString();  // Ilo�� przedmiot�w
                    float totalCategoryValue = lootInfo.sellValue * entry.Value;
                    rowTexts[2].text = totalCategoryValue.ToString("F2") + " $";  // ��czna warto��

                    // Sumowanie warto�ci
                    totalValue += totalCategoryValue;
                }
            }
        }

        // Wy�wietlanie ��cznej warto�ci wszystkich przedmiot�w w SellArea
        if (totalValueText != null)
        {
            totalValueText.text = "��czna warto��: " + totalValue.ToString("F2") + " $";
        }
    }
}
