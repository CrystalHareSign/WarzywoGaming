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
    public Transform sellArea;  // Transform strefy sprzeda¿y (SellArea)
    public GameObject rowPrefab;  // Prefab UI dla pojedynczego wiersza
    public Transform rowParent;  // Rodzic dla wszystkich wierszy w kanwie (powinien mieæ VerticalLayoutGroup)
    public Text totalValueText;  // Tekst dla ³¹cznej wartoœci wszystkich przedmiotów w SellArea

    private Dictionary<string, LootCategory> lootDictionary = new Dictionary<string, LootCategory>();

    void Start()
    {
        // Tworzenie s³ownika dla szybszego dostêpu
        foreach (var loot in lootCategories)
        {
            lootDictionary[loot.category] = loot;
        }

        // Aktualizacja UI na podstawie przedmiotów w SellArea
        UpdateSellAreaUI();
    }

    // Funkcja do aktualizacji UI dla SellArea
    public void UpdateSellAreaUI()
    {
        // Wyczyœæ poprzednie elementy UI
        foreach (Transform child in rowParent)
        {
            Destroy(child.gameObject);
        }

        float totalValue = 0f;
        var lootCounts = new Dictionary<string, int>();  // S³ownik do liczenia przedmiotów wg kategorii

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

        // Wyœwietlanie danych o przedmiotach w SellArea
        foreach (var entry in lootCounts)
        {
            if (lootDictionary.TryGetValue(entry.Key, out LootCategory lootInfo))
            {
                // Tworzenie nowego wiersza UI dla tej kategorii
                GameObject row = Instantiate(rowPrefab, rowParent);

                // Pobieranie komponentów tekstowych w wierszu
                Text[] rowTexts = row.GetComponentsInChildren<Text>();
                if (rowTexts.Length >= 3)
                {
                    // Ustawianie tekstów: kategoria, iloœæ oraz ³¹czna wartoœæ
                    rowTexts[0].text = lootInfo.category;  // Kategoria
                    rowTexts[1].text = "x " + entry.Value.ToString();  // Iloœæ przedmiotów
                    float totalCategoryValue = lootInfo.sellValue * entry.Value;
                    rowTexts[2].text = totalCategoryValue.ToString("F2") + " $";  // £¹czna wartoœæ

                    // Sumowanie wartoœci
                    totalValue += totalCategoryValue;
                }
            }
        }

        // Wyœwietlanie ³¹cznej wartoœci wszystkich przedmiotów w SellArea
        if (totalValueText != null)
        {
            totalValueText.text = "£¹czna wartoœæ: " + totalValue.ToString("F2") + " $";
        }
    }
}
