using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab;
    public int startAmount;
}

public class RoundManager : MonoBehaviour
{
    [Header("RUNDY")]
    public int currentRound = 1;
    public float timeBetweenRounds = 5f;

    [Header("POTWORY")]
    public int remainingEnemiesThreshold = 2;
    public float spawnRateMultiplier = 1.2f;
    public float healthMultiplierPerRound = 1.2f;

    [Header("LISTA WROGÓW")]
    public List<EnemySpawnData> enemyTypes;

    [Header("REFERENCJE")]
    public Transform targetArea; // ✅ Teraz targetArea jest dostępne
    private MonsterSpawner monsterSpawner;

    [Header("UI")]
    public TextMeshProUGUI RoundNumberText;

    void Start()
    {
        monsterSpawner = FindFirstObjectByType<MonsterSpawner>();

        if (monsterSpawner == null)
        {
            Debug.LogError("[RoundManager] Nie znaleziono MonsterSpawner! Upewnij się, że jest w scenie.");
            return;
        }

        if (targetArea == null)
        {
            Debug.LogError("[RoundManager] Brak przypisanego targetArea! Upewnij się, że obiekt jest przypisany w Inspectorze.");
            return;
        }

        // ✅ Przekazujemy TargetArea do MonsterSpawner, zanim zacznie spawnować wrogów
        monsterSpawner.SetTarget(targetArea);

        Debug.Log("[RoundManager] Uruchamiam pierwszą falę wrogów...");
        SpawnInitialEnemies();

        StartCoroutine(RoundLoop());
        UpdateRoundUI();
    }
    void SpawnInitialEnemies()
    {
        if (enemyTypes == null || enemyTypes.Count == 0)
        {
            Debug.LogWarning("[RoundManager] Brak wrogów w RoundManager! Dodaj wrogów w Inspectorze.");
            return;
        }

        Debug.Log($"[RoundManager] Spawnuję pierwszą falę wrogów ({enemyTypes.Count} typów)");

        foreach (var enemy in enemyTypes)
        {
            if (enemy.enemyPrefab == null)
            {
                Debug.LogError("[RoundManager] enemyPrefab jest NULL! Sprawdź przypisania w Inspectorze.");
                continue;
            }

            monsterSpawner.SpawnEnemyGroup(enemy.enemyPrefab, enemy.startAmount);
            Debug.Log($"[RoundManager] Zespawnowano {enemy.startAmount}x {enemy.enemyPrefab.name}");
        }

        Debug.Log("[RoundManager] Pierwsza fala wrogów zespawnowana!");
    }

    IEnumerator RoundLoop()
    {
        while (true)
        {
            yield return StartCoroutine(WaitForEnemiesToDropBelowThreshold());
            yield return new WaitForSeconds(timeBetweenRounds);
            StartNewRound();
        }
    }

    IEnumerator WaitForEnemiesToDropBelowThreshold()
    {
        Debug.Log($"[RoundManager] 🕒 Czekam, aż liczba wrogów spadnie poniżej {remainingEnemiesThreshold}...");

        // ✅ Nowa poprawka: Czekamy krótką chwilę na aktualizację liczby wrogów
        yield return new WaitForSeconds(0.5f);

        // ✅ Jeśli na mapie jest mniej wrogów niż threshold, czekamy na ich spawn
        while (GameObject.FindGameObjectsWithTag("Enemy").Length < remainingEnemiesThreshold)
        {
            Debug.Log("[RoundManager] ⚠️ Na mapie jest za mało wrogów, czekam na spawn...");
            yield return new WaitForSeconds(1f);
        }

        Debug.Log($"[RoundManager] ✅ Na mapie jest wystarczająco wrogów ({GameObject.FindGameObjectsWithTag("Enemy").Length}), czekam na ich eliminację...");

        // ✅ Dodajemy krótką pauzę, żeby system poprawnie liczył liczbę wrogów
        yield return new WaitForSeconds(0.5f);

        // ✅ Dopiero teraz czekamy na ich eliminację
        while (GameObject.FindGameObjectsWithTag("Enemy").Length > remainingEnemiesThreshold)
        {
            yield return null;
        }

        Debug.Log($"[RoundManager] ✅ Liczba wrogów spadła poniżej {remainingEnemiesThreshold}, startuję nową rundę.");
    }

    void StartNewRound()
    {
        currentRound++;
        Debug.Log($"[RoundManager] Runda {currentRound} rozpoczęta!");

        IncreaseEnemyHealth();

        foreach (var enemy in enemyTypes)
        {
            int newAmount = Mathf.RoundToInt(enemy.startAmount * Mathf.Pow(spawnRateMultiplier, currentRound - 1));
            monsterSpawner.SpawnEnemyGroup(enemy.enemyPrefab, newAmount);
            Debug.Log($"[RoundManager] Nowa fala: {newAmount}x {enemy.enemyPrefab.name}");
        }

        UpdateRoundUI();
    }

    void IncreaseEnemyHealth()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.IncreaseHealth(healthMultiplierPerRound); // ✅ Teraz działa tylko na nowych wrogach
            }
        }
    }

    void UpdateRoundUI()
    {
        if (RoundNumberText != null)
        {
            RoundNumberText.text = "Runda: " + currentRound;
        }
    }
}
