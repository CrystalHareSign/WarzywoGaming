using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LootShop : MonoBehaviour
{
    [Header("Teksty kategorii (przypisane rêcznie na pocz¹tku)")]
    public TMP_Text[] categoryTexts;  // Lista tekstów dla kategorii

    [Header("Teksty iloœci (przypisane rêcznie na pocz¹tku)")]
    public TMP_Text[] amountTexts;    // Lista tekstów dla iloœci

    private Dictionary<string, float> lootContents = new Dictionary<string, float>();  // S³ownik przechowuj¹cy kategorie i ich iloœci
    private List<string> categories = new List<string>();  // Lista przechowuj¹ca kategorie, które zosta³y znalezione

    private void Start()
    {
        // Pocz¹tkowo wszystkie iloœci ustawiamy na 0
        foreach (var amountText in amountTexts)
        {
            amountText.text = "0";
            
        }
        // Pocz¹tkowo wszystkie iloœci ustawiamy na 0
        foreach (var categoryText in categoryTexts)
        {
            categoryText.text = "- - -";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TreasureValue treasure = other.GetComponent<TreasureValue>();

        if (treasure != null)
        {
            Debug.Log($"Znaleziono TreasureValue z kategori¹: {treasure.category}");

            // Jeœli kategoria jeszcze nie istnieje, dodajemy j¹ do s³ownika i kategorii
            if (!lootContents.ContainsKey(treasure.category))
            {
                lootContents[treasure.category] = 0;
                categories.Add(treasure.category); // Dodajemy kategoriê do listy

                // Sprawdzamy, czy mamy wystarczaj¹co du¿o miejsca na UI
                ResizeUI(categories.Count);

                // Ustawiamy nazwê kategorii w odpowiednim miejscu w UI
                int index = categories.IndexOf(treasure.category);
                if (index != -1 && index < categoryTexts.Length)
                {
                    categoryTexts[index].text = treasure.category; // Przypisanie nazwy kategorii
                }
            }

            // Aktualizacja iloœci tej kategorii
            lootContents[treasure.category] += treasure.amount;
            UpdateAmount(treasure.category);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        TreasureValue treasure = other.GetComponent<TreasureValue>();

        if (treasure != null && lootContents.ContainsKey(treasure.category))
        {
            // Odejmujemy iloœæ od danej kategorii
            lootContents[treasure.category] -= treasure.amount;

            // Jeœli iloœæ spadnie poni¿ej zera, ustawiamy j¹ na 0
            if (lootContents[treasure.category] <= 0)
            {
                lootContents[treasure.category] = 0;

                // Usuwamy kategoriê z listy i UI
                int index = categories.IndexOf(treasure.category);
                if (index != -1)
                {
                    categories.RemoveAt(index); // Usuwamy kategoriê z listy
                    lootContents.Remove(treasure.category); // Usuwamy kategoriê ze s³ownika

                    // Aktualizujemy UI
                    RemoveCategoryFromUI(index);
                }
            }
            else
            {
                // Aktualizujemy iloœæ w interfejsie u¿ytkownika
                UpdateAmount(treasure.category);
            }
        }
    }

    private void RemoveCategoryFromUI(int index)
    {
        // Przesuwamy pozosta³e elementy w dó³ w UI
        for (int i = index; i < categories.Count; i++)
        {
            categoryTexts[i].text = categories[i];
            amountTexts[i].text = lootContents[categories[i]].ToString("0.#");
        }

        // Czyszczymy ostatni element UI, poniewa¿ kategoria zosta³a usuniêta
        if (categories.Count < categoryTexts.Length)
        {
            categoryTexts[categories.Count].text = "- - -";
            amountTexts[categories.Count].text = "0";
        }
    }

    private void UpdateAmount(string category)
    {
        int index = categories.IndexOf(category);
        if (index != -1)
        {
            amountTexts[index].text = lootContents[category].ToString("0.#");
        }
    }

    private void ResizeUI(int newCategoryCount)
    {
        // Powiêkszamy listy UI w miarê potrzeby
        if (newCategoryCount > categoryTexts.Length)
        {
            // Tworzymy nowe miejsce w UI, zwiêkszamy tablice
            TMP_Text[] newCategoryTexts = new TMP_Text[newCategoryCount];
            TMP_Text[] newAmountTexts = new TMP_Text[newCategoryCount];

            for (int i = 0; i < categoryTexts.Length; i++)
            {
                newCategoryTexts[i] = categoryTexts[i];
                newAmountTexts[i] = amountTexts[i];
            }

            // Nowe tablice
            categoryTexts = newCategoryTexts;
            amountTexts = newAmountTexts;

            // Ustawiamy now¹ kategoriê i iloœæ
            categoryTexts[newCategoryCount - 1].text = categories[newCategoryCount - 1];
            amountTexts[newCategoryCount - 1].text = "0";
        }
    }
}
