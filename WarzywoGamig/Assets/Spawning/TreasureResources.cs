using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ResourceCategory
{
    public string name;
    public bool isActive;
    public int resourceCount;

    // Przywr�cone pola dla min i max
    public int minResourceCount = 1;
    public int maxResourceCount = 10;
}


public class TreasureResources : MonoBehaviour
{
    // Public list to store the categories of the resources and their counts
    public List<ResourceCategory> resourceCategories;

    // Method to set the resource categories (received from TreasureDefiner)
    public void SetResourceCategories(List<ResourceCategory> categories)
    {
        resourceCategories = categories;
    }

    // Method to get the resource categories
    public List<ResourceCategory> GetResourceCategories()
    {
        return resourceCategories;
    }

    // Method to get the names of active resource categories
    public List<string> GetActiveResourceCategories()
    {
        List<string> activeCategories = new List<string>();
        foreach (var category in resourceCategories)
        {
            if (category.isActive)
            {
                activeCategories.Add(category.name);
            }
        }
        return activeCategories;
    }
    public void UpdateResourceCategoryCount(string categoryName, int newCount)
    {
        foreach (var category in resourceCategories)
        {
            if (category.name == categoryName)
            {
                category.resourceCount = newCount;

                // Tutaj mo�esz doda� dowolne dodatkowe efekty wizualne lub logik�
                // np. aktualizacja UI w �wiecie gry
                //Debug.Log($"Zaktualizowano {categoryName} do {newCount} zasob�w.");
                return;
            }
        }

        Debug.LogWarning($"Nie znaleziono kategorii {categoryName} do aktualizacji.");
    }
}
