using UnityEngine;

public class HarpoonController : MonoBehaviour
{
    public GameObject harpoonPrefab; // Prefab Harpoon
    public Transform firePoint; // Punkt, do którego przypisywany bêdzie Harpoon

    void Start()
    {
        if (harpoonPrefab == null)
        {
            Debug.LogError("Nie przypisano prefabrykatu Harpoon.");
            return;
        }

        if (firePoint == null)
        {
            Debug.LogError("Nie przypisano FirePoint.");
            return;
        }

        Debug.Log("Instantiating harpoon at FirePoint.");
        Instantiate(harpoonPrefab, firePoint.position, firePoint.rotation, firePoint);
    }
}