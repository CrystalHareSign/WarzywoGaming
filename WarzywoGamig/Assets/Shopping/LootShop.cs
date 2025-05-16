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

    [Header("Tekst gracza – waluta")]
    public TMP_Text playerCurrencyText;

    [Header("Ustawienia interakcji")]
    public Transform player;
    public float interactionRange = 5f;

    [Header("Przycisk interakcji")]
    public Transform buttonTransform; // Obiekt w œwiecie, np. fizyczny przycisk

    private Dictionary<string, float> lootContents = new Dictionary<string, float>();  // S³ownik przechowuj¹cy kategorie i ich iloœci
    private Dictionary<string, float> unitPrices = new Dictionary<string, float>();    // S³ownik przechowuj¹cy ceny jednostkowe
    private List<string> categories = new List<string>();  // Lista przechowuj¹ca kategorie, które zosta³y znalezione
    private List<GameObject> objectsInTrigger = new List<GameObject>();

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

        // Automatyczne znajdowanie gracza tylko w scenie "Home"
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Home")
        {
            GameObject foundPlayer = GameObject.FindWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
            }
            else
            {
                Debug.LogWarning("Nie znaleziono gracza z tagiem 'Player' w scenie Home.");
            }
        }

        // Inicjalizacja wartoœci waluty gracza z GameManager
        playerCurrencyText.text = SaveManager.Instance.playerCurrency.ToString("0.##");

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (buttonTransform != null && player != null)
            {
                float distance = Vector3.Distance(player.position, buttonTransform.position);
                if (distance <= interactionRange)
                {
                    CollectLootValue();
                }
            }
        }
    }
    private void CollectLootValue()
    {
        // Sprawdzamy, czy w obszarze znajduje siê jakikolwiek przedmiot
        if (objectsInTrigger.Count == 0)
        {
            Debug.LogWarning("Brak przedmiotów do sprzeda¿y w obszarze.");
            return; // Jeœli nie ma przedmiotów w obszarze, przerywamy wykonanie metody
        }

        float totalValue = 0f;
        Debug.Log("CollectLootValue started. Calculating total value...");

        // Obliczanie ca³kowitej wartoœci
        foreach (var category in categories)
        {
            if (lootContents.ContainsKey(category))
            {
                float categoryAmount = lootContents[category];
                float categoryPrice = unitPrices[category];
                float value = categoryAmount * categoryPrice;

                // Debug: wypisz szczegó³y dla ka¿dej kategorii
                Debug.Log($"Category: {category}, Amount: {categoryAmount}, Price: {categoryPrice}, Value: {value}");

                totalValue += value;
            }
        }

        // Debug: wypisz ca³kowit¹ wartoœæ po obliczeniach
        Debug.Log($"Total value after summing up categories: {totalValue}");

        // Dodajemy totalValue do obecnej waluty gracza w GameManager
        SaveManager.Instance.AddCurrency(totalValue);

        // Wyœwietlamy zaktualizowan¹ walutê gracza
        playerCurrencyText.text = SaveManager.Instance.playerCurrency.ToString("0.##");

        // Resetowanie iloœci i wartoœci w UI
        for (int i = 0; i < categories.Count; i++)
        {
            amountTexts[i].text = "0";
            valueTexts[i].text = "0";
        }

        // Zniszczenie wszystkich obiektów w triggerze
        List<GameObject> objectsToDestroy = new List<GameObject>(objectsInTrigger);

        foreach (GameObject obj in objectsToDestroy)
        {
            if (obj != null)
            {
                Destroy(obj);
                Debug.Log($"Destroyed object: {obj.name}");

                // Usuwamy obiekt z listy po zniszczeniu
                objectsInTrigger.Remove(obj);
            }
            else
            {
                Debug.LogWarning("Attempted to destroy a null object.");
            }
        }

        // Resetowanie kategorii na UI
        for (int i = 0; i < categoryTexts.Length; i++)
        {
            categoryTexts[i].text = "- - -";
        }

        // Resetowanie listy kategorii
        categories.Clear();
        lootContents.Clear();

        Debug.Log("Loot sale complete. UI and data reset.");
    }

    private void OnTriggerEnter(Collider other)
    {
        TreasureValue treasure = other.GetComponent<TreasureValue>();

        if (treasure != null)
        {
            if (!objectsInTrigger.Contains(other.gameObject))
            {
                objectsInTrigger.Add(other.gameObject);
            }

            if (!lootContents.ContainsKey(treasure.category))
            {
                lootContents[treasure.category] = 0;
                categories.Add(treasure.category);

                if (!unitPrices.ContainsKey(treasure.category))
                {
                    unitPrices[treasure.category] = defaultPrice;
                }

                ResizeUI(categories.Count);

                int index = categories.IndexOf(treasure.category);
                if (index != -1 && index < categoryTexts.Length)
                {
                    categoryTexts[index].text = treasure.category;
                }
            }

            lootContents[treasure.category] += treasure.amount;
            UpdateAmount(treasure.category);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        TreasureValue treasure = other.GetComponent<TreasureValue>();

        if (objectsInTrigger.Contains(other.gameObject))
        {
            objectsInTrigger.Remove(other.gameObject);
        }

        if (treasure != null && lootContents.ContainsKey(treasure.category))
        {
            lootContents[treasure.category] -= treasure.amount;

            if (lootContents[treasure.category] <= 0)
            {
                lootContents[treasure.category] = 0;

                int index = categories.IndexOf(treasure.category);
                if (index != -1)
                {
                    categories.RemoveAt(index);
                    lootContents.Remove(treasure.category);

                    RemoveCategoryFromUI(index);
                }
            }
            else
            {
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