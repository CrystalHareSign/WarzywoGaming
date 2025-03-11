using UnityEngine;
using System.Collections.Generic;

public class TreasureResources : MonoBehaviour
{
    // Public variable to store the number of resources in the treasure
    public int resourceCount;

    // Public variables to define the range of resources
    public int minResourceCount = 1;
    public int maxResourceCount = 10;

    // Public list to store the categories of the resources
    public List<string> resourceCategories;

    void Start()
    {
        // Initialize the resource count with a random value within the specified range
        resourceCount = Random.Range(minResourceCount, maxResourceCount + 1);

        // Initialize the list of categories (example categories)
        resourceCategories = new List<string> { "Gold"};
        //{ "Gold", "Silver", "Gems" };

        // Optionally, you can assign categories dynamically or based on certain conditions
        // Example: Assign a random category to the treasure
        string randomCategory = resourceCategories[Random.Range(0, resourceCategories.Count)];
        Debug.Log($"Treasure initialized with {resourceCount} resources of category: {randomCategory}");
    }

    // Method to set the resource count (if needed)
    public void SetResourceCount(int count)
    {
        resourceCount = count;
    }

    // Method to get the resource count
    public int GetResourceCount()
    {
        return resourceCount;
    }

    // Method to set the resource categories (if needed)
    public void SetResourceCategories(List<string> categories)
    {
        resourceCategories = categories;
    }

    // Method to get the resource categories
    public List<string> GetResourceCategories()
    {
        return resourceCategories;
    }
}