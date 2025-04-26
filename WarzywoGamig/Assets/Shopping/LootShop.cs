using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LootShop : MonoBehaviour
{
    [Header("Teksty kategorii (przypisane r�cznie na pocz�tku)")]
    public TMP_Text[] categoryTexts;  // Lista tekst�w dla kategorii

    [Header("Teksty ilo�ci (przypisane r�cznie na pocz�tku)")]
    public TMP_Text[] amountTexts;    // Lista tekst�w dla ilo�ci

    private Dictionary<string, float> lootContents = new Dictionary<string, float>();  // S�ownik przechowuj�cy kategorie i ich ilo�ci
    private List<string> categories = new List<string>();  // Lista przechowuj�ca kategorie, kt�re zosta�y znalezione

    private void Start()
    {
        // Pocz�tkowo wszystkie ilo�ci ustawiamy na 0
        foreach (var amountText in amountTexts)
        {
            amountText.text = "0";
            
        }
        // Pocz�tkowo wszystkie ilo�ci ustawiamy na 0
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
            Debug.Log($"Znaleziono TreasureValue z kategori�: {treasure.category}");

            // Je�li kategoria jeszcze nie istnieje, dodajemy j� do s�ownika i kategorii
            if (!lootContents.ContainsKey(treasure.category))
            {
                lootContents[treasure.category] = 0;
                categories.Add(treasure.category); // Dodajemy kategori� do listy

                // Sprawdzamy, czy mamy wystarczaj�co du�o miejsca na UI
                ResizeUI(categories.Count);

                // Ustawiamy nazw� kategorii w odpowiednim miejscu w UI
                int index = categories.IndexOf(treasure.category);
                if (index != -1 && index < categoryTexts.Length)
                {
                    categoryTexts[index].text = treasure.category; // Przypisanie nazwy kategorii
                }
            }

            // Aktualizacja ilo�ci tej kategorii
            lootContents[treasure.category] += treasure.amount;
            UpdateAmount(treasure.category);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        TreasureValue treasure = other.GetComponent<TreasureValue>();

        if (treasure != null && lootContents.ContainsKey(treasure.category))
        {
            // Odejmujemy ilo�� od danej kategorii
            lootContents[treasure.category] -= treasure.amount;

            // Je�li ilo�� spadnie poni�ej zera, ustawiamy j� na 0
            if (lootContents[treasure.category] <= 0)
            {
                lootContents[treasure.category] = 0;

                // Usuwamy kategori� z listy i UI
                int index = categories.IndexOf(treasure.category);
                if (index != -1)
                {
                    categories.RemoveAt(index); // Usuwamy kategori� z listy
                    lootContents.Remove(treasure.category); // Usuwamy kategori� ze s�ownika

                    // Aktualizujemy UI
                    RemoveCategoryFromUI(index);
                }
            }
            else
            {
                // Aktualizujemy ilo�� w interfejsie u�ytkownika
                UpdateAmount(treasure.category);
            }
        }
    }

    private void RemoveCategoryFromUI(int index)
    {
        // Przesuwamy pozosta�e elementy w d� w UI
        for (int i = index; i < categories.Count; i++)
        {
            categoryTexts[i].text = categories[i];
            amountTexts[i].text = lootContents[categories[i]].ToString("0.#");
        }

        // Czyszczymy ostatni element UI, poniewa� kategoria zosta�a usuni�ta
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
        // Powi�kszamy listy UI w miar� potrzeby
        if (newCategoryCount > categoryTexts.Length)
        {
            // Tworzymy nowe miejsce w UI, zwi�kszamy tablice
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

            // Ustawiamy now� kategori� i ilo��
            categoryTexts[newCategoryCount - 1].text = categories[newCategoryCount - 1];
            amountTexts[newCategoryCount - 1].text = "0";
        }
    }
}
