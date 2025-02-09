using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject prefabToSpawn; // Prefab do spawnowania
    public GameObject[] spawnPoints; // Lista GameObject�w do spawnowania
    public float spawnInterval = 2f; // Cz�stotliwo�� spawnowania w sekundach

    private float timer; // Timer do �ledzenia czasu

    void Start()
    {
        timer = spawnInterval; // Inicjalizacja timera warto�ci� interwa�u
    }

    void Update()
    {
        // Odliczanie czasu
        timer -= Time.deltaTime;

        // Sprawdzenie, czy up�yn�� czas spawnowania
        if (timer <= 0f)
        {
            // Spawnowanie obiektu w losowo wybranym miejscu
            SpawnRandomObject();

            // Resetowanie timera
            timer = spawnInterval;
        }
    }

    void SpawnRandomObject()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned.");
            return;
        }

        // Wybierz losowy punkt spawnowania
        int randomIndex = Random.Range(0, spawnPoints.Length);
        GameObject spawnPoint = spawnPoints[randomIndex];

        // Spawnowanie obiektu w losowo wybranym miejscu
        Instantiate(prefabToSpawn, spawnPoint.transform.position, spawnPoint.transform.rotation);
    }
}