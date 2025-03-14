using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ResourceCategory
{
    public string name;
    public bool isActive;
    public int resourceCount;

    // Przywrócone pola dla min i max
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
}
