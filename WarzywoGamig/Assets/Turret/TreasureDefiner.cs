using UnityEngine;
using System.Collections.Generic;

public class TreasureDefiner : MonoBehaviour
{
    public TreasureResources treasureResources;  // Reference to the TreasureResources script
    public List<ResourceCategory> predefinedCategories;  // Categories defined by the designer

    void Start()
    {
        InitializeResources();
    }

    // Initialize the resources with random values within the specified range
    private void InitializeResources()
    {
        if (predefinedCategories == null || predefinedCategories.Count == 0)
        {
            Debug.LogWarning("No predefined categories found in TreasureDefiner.");
            return;
        }

        List<ResourceCategory> initializedCategories = new List<ResourceCategory>();

        foreach (var category in predefinedCategories)
        {
            // Ensure minResourceCount is not greater than maxResourceCount
            int minCount = Mathf.Min(category.minResourceCount, category.maxResourceCount);
            int maxCount = Mathf.Max(category.minResourceCount, category.maxResourceCount);

            category.resourceCount = Random.Range(minCount, maxCount + 1);

            initializedCategories.Add(category);
        }

        // Pass the initialized categories to the TreasureResources script
        if (treasureResources != null)
        {
            treasureResources.SetResourceCategories(initializedCategories);
        }
        else
        {
            Debug.LogError("TreasureResources reference is missing in TreasureDefiner.");
        }
    }
}
