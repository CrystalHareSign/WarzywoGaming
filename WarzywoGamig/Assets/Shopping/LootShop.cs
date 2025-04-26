using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LootShop : MonoBehaviour
{
    [Header("Teksty kategorii (przypisane rêcznie na pocz¹tku)")]
    public TMP_Text[] categoryTexts;  // Lista tekstów dla kategorii

    [Header("Teksty iloœci (przypisane rêcznie na pocz¹tku)")]
    public TMP_Text[] amountTexts;    // Lista tekstów dla iloœci

    [Header("Teksty wartoœci (przypisane rêcznie na pocz¹tku)")]
    public TMP_Text[] valueTexts;     // Lista tekstów dla wartoœci ca³kowitej (nowa)

    [Header("Kategorie i ceny (przypisane w Inspektorze)")]
    [SerializeField] private List<CategoryPricePair> categoryPricePairs = new List<CategoryPricePair>(); // Lista par kategoria-cena

    private Dictionary<string, float> lootContents = new Dictionary<string, float>();  // S³ownik przechowuj¹cy kategorie i ich iloœci
    private Dictionary<string, float> unitPrices = new Dictionary<string, float>();    // S³ownik przechowuj¹cy ceny jednostkowe
    private List<string> categories = new List<string>();  // Lista przechowuj¹ca kategorie, które zosta³y znalezione

    private float defaultPrice = 1.0f; // Domyœlna cena jednostkowa

    private void Start()
    {
        // Inicjalizacja cen jednostkowych z listy par
        foreach (var pair in categoryPricePairs)
        {
            unitPrices[pair.category] = pair.price;
        }

        // Pocz¹tkowo wszystkie iloœci ustawiamy na 0
        foreach (var amountText in amountTexts)
        {
            amountText.text = "0";
        }

        // Pocz¹tkowo wszystkie kategorie ustawiamy na "- - -"
        foreach (var categoryText in categoryTexts)
        {
            categoryText.text = "- - -";
        }

        // Pocz¹tkowo wszystkie wartoœci ustawiamy na "0"
        foreach (var valueText in valueTexts)
        {
            valueText.text = "0";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TreasureValue treasure = other.GetComponent<TreasureValue>();

        if (treasure != null)
        {
            //Debug.Log($"Znaleziono TreasureValue z kategori¹: {treasure.category}");

            // Jeœli kategoria jeszcze nie istnieje, dodajemy j¹ do s³ownika i kategorii
            if (!lootContents.ContainsKey(treasure.category))
            {
                lootContents[treasure.category] = 0;
                categories.Add(treasure.category); // Dodajemy kategoriê do listy

                // Jeœli kategoria nie ma przypisanej ceny, ustawiamy domyœln¹ cenê
                if (!unitPrices.ContainsKey(treasure.category))
                {
                    unitPrices[treasure.category] = defaultPrice;
                }

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

    private void UpdateAmount(string category)
    {
        int index = categories.IndexOf(category);
        if (index != -1)
        {
            // Aktualizujemy iloœæ
            amountTexts[index].text = lootContents[category].ToString("0.#");

            // Obliczamy wartoœæ ca³kowit¹ (iloœæ * cena jednostkowa)
            float totalValue = lootContents[category] * unitPrices[category];
            valueTexts[index].text = totalValue.ToString("0.##");
        }
    }

    private void ResizeUI(int newCategoryCount)
    {
        if (newCategoryCount > categoryTexts.Length)
        {
            TMP_Text[] newCategoryTexts = new TMP_Text[newCategoryCount];
            TMP_Text[] newAmountTexts = new TMP_Text[newCategoryCount];
            TMP_Text[] newValueTexts = new TMP_Text[newCategoryCount]; // Rozszerzamy listê tekstów wartoœci

            for (int i = 0; i < categoryTexts.Length; i++)
            {
                newCategoryTexts[i] = categoryTexts[i];
                newAmountTexts[i] = amountTexts[i];
                newValueTexts[i] = valueTexts[i];
            }

            for (int i = categoryTexts.Length; i < newCategoryCount; i++)
            {
                // Tworzymy nowe TMP_Text dla kategorii
                GameObject categoryTextObj = new GameObject($"CategoryText_{i}");
                categoryTextObj.transform.SetParent(this.transform); // Dodajemy do kanwy
                TMP_Text categoryText = categoryTextObj.AddComponent<TMP_Text>();
                categoryText.fontSize = 36;
                categoryText.alignment = TextAlignmentOptions.Center;
                categoryText.color = Color.black;
                categoryText.text = "- - -";
                newCategoryTexts[i] = categoryText;

                // Tworzymy nowe TMP_Text dla iloœci
                GameObject amountTextObj = new GameObject($"AmountText_{i}");
                amountTextObj.transform.SetParent(this.transform);
                TMP_Text amountText = amountTextObj.AddComponent<TMP_Text>();
                amountText.fontSize = 36;
                amountText.alignment = TextAlignmentOptions.Center;
                amountText.color = Color.black;
                amountText.text = "0";
                newAmountTexts[i] = amountText;

                // Tworzymy nowe TMP_Text dla wartoœci
                GameObject valueTextObj = new GameObject($"ValueText_{i}");
                valueTextObj.transform.SetParent(this.transform);
                TMP_Text valueText = valueTextObj.AddComponent<TMP_Text>();
                valueText.fontSize = 36;
                valueText.alignment = TextAlignmentOptions.Center;
                valueText.color = Color.black;
                valueText.text = "0";
                newValueTexts[i] = valueText;
            }

            categoryTexts = newCategoryTexts;
            amountTexts = newAmountTexts;
            valueTexts = newValueTexts; // Aktualizujemy tablicê tekstów wartoœci
        }
    }

    private void RemoveCategoryFromUI(int index)
    {
        // Przesuwamy pozosta³e elementy w dó³ w UI
        for (int i = index; i < categories.Count; i++)
        {
            categoryTexts[i].text = categories[i];
            amountTexts[i].text = lootContents[categories[i]].ToString("0.#");
            valueTexts[i].text = (lootContents[categories[i]] * unitPrices[categories[i]]).ToString("0.##");
        }

        // Czyszczymy ostatni element UI, poniewa¿ kategoria zosta³a usuniêta
        if (categories.Count < categoryTexts.Length)
        {
            categoryTexts[categories.Count].text = "- - -";
            amountTexts[categories.Count].text = "0";
            valueTexts[categories.Count].text = "0";
        }
    }

    [System.Serializable]
    public class CategoryPricePair
    {
        public string category; // Nazwa kategorii
        public float price;     // Cena jednostkowa
    }
}