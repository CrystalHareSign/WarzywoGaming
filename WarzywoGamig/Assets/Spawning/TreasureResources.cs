using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ResourceCategory
{
    public string name;
    public bool isActive;
    public int minResourceCount = 1;
    public int maxResourceCount = 10;
    public int resourceCount;
}

public class TreasureResources : MonoBehaviour
{
    // Public list to store the categories of the resources and their counts
    public List<ResourceCategory> resourceCategories;

    void Start()
    {
        // Initialize the resource count for each category with a random value within the specified range
        foreach (var category in resourceCategories)
        {
            if (category.isActive)
            {
                category.resourceCount = Random.Range(category.minResourceCount, category.maxResourceCount + 1);
                //Debug.Log($"Initialized {category.resourceCount} resources for category: {category.name}");
            }
        }
    }

    // Method to set the resource categories (if needed)
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