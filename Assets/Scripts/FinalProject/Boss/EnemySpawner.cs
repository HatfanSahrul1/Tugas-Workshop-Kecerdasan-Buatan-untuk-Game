using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private GameObject enemy1Prefab;
    [SerializeField] private GameObject enemy2Prefab;
    [SerializeField] private int maxEnemies = 5;
    
    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnRadius = 3f;
    
    [Header("Probability Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float enemy1Probability = 0.5f; // 0.5 = 50% chance for enemy1, 50% for enemy2
    
    [Header("Debug")]
    [SerializeField] private bool showSpawnGizmos = true;
    
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    
    public void SpawnEnemies()
    {
        // Clear any existing enemies first (optional)
        ClearExistingEnemies();
        
        Debug.Log($"Spawning {maxEnemies} enemies...");
        
        for (int i = 0; i < maxEnemies; i++)
        {
            SpawnSingleEnemy();
        }
    }
    
    void SpawnSingleEnemy()
    {
        // Choose enemy type based on probability
        GameObject enemyPrefab = ChooseEnemyType();
        
        if (enemyPrefab == null)
        {
            Debug.LogWarning("No enemy prefab assigned!");
            return;
        }
        
        // Choose spawn position
        Vector2 spawnPosition = GetSpawnPosition();
        
        // Spawn the enemy
        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        spawnedEnemies.Add(spawnedEnemy);
        
        
        Debug.Log($"Spawned {enemyPrefab.name} at position {spawnPosition}");
    }
    
    GameObject ChooseEnemyType()
    {
        // Generate random number between 0 and 1
        float randomValue = Random.Range(0f, 1f);
        
        // Return enemy based on probability
        if (randomValue <= enemy1Probability)
        {
            return enemy1Prefab;
        }
        else
        {
            return enemy2Prefab;
        }
    }
    
    Vector2 GetSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // Choose random spawn point from predefined points
            Transform chosenSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            
            // Add some randomness around the spawn point
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            return new Vector2(chosenSpawnPoint.position.x, chosenSpawnPoint.position.y) + randomOffset;
        }
        else
        {
            // If no spawn points defined, spawn around this object
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            return new Vector2(transform.position.x, transform.position.y) + randomOffset;
        }
    }
    
    public void ClearExistingEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        spawnedEnemies.Clear();
        Debug.Log("Cleared all existing enemies");
    }
    
    // Public methods for runtime adjustments
    public void SetMaxEnemies(int newMax)
    {
        maxEnemies = Mathf.Max(0, newMax);
    }
    
    public void SetEnemy1Probability(float probability)
    {
        enemy1Probability = Mathf.Clamp01(probability);
    }
    
    public int GetActiveEnemyCount()
    {
        // Remove null references (destroyed enemies)
        spawnedEnemies.RemoveAll(enemy => enemy == null);
        return spawnedEnemies.Count;
    }
    
    // Gizmos for visualization in Scene view
    private void OnDrawGizmos()
    {
        if (!showSpawnGizmos) return;
        
        // Draw spawn points
        if (spawnPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, spawnRadius);
                    Gizmos.DrawWireCube(spawnPoint.position, Vector3.one * 0.5f);
                }
            }
        }
        else
        {
            // Draw spawn area around this object
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }
}