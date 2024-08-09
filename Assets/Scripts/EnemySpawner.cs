using UnityEngine;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    public GameObject turtleEnemyPrefab;
    public GameObject mageEnemyPrefab;
    public float spawnRate = 5f;
    public int totalGroups = 20;
    public int turtlesPerGroup = 9;
    public int magesPerGroup = 6;

    private float nextSpawnTime = 0f;
    private int groupsSpawned = 0;

    private Transform[] turtleSpawnPoints;
    private Transform[] mageSpawnPoints;

    void Start()
    {
        turtleSpawnPoints = GameObject.FindGameObjectsWithTag("TurtleSpawnPoint").Select(go => go.transform).ToArray();
        mageSpawnPoints = GameObject.FindGameObjectsWithTag("MageSpawnPoint").Select(go => go.transform).ToArray();
    }

    void Update()
    {
        if (Time.time > nextSpawnTime && groupsSpawned < totalGroups)
        {
            nextSpawnTime = Time.time + spawnRate;
            SpawnGroup();
            groupsSpawned++;
        }
    }

    void SpawnGroup()
    {
        // Spawn turtles
        for (int i = 0; i < turtlesPerGroup; i++)
        {
            Vector3 spawnPosition = GetValidSpawnPosition(turtleSpawnPoints);
            if (spawnPosition != Vector3.zero)
            {
                spawnPosition.y = 0.1f;
                Instantiate(turtleEnemyPrefab, spawnPosition, Quaternion.identity);
            }
        }

        // Spawn mages
        for (int i = 0; i < magesPerGroup; i++)
        {
            Vector3 spawnPosition = GetValidSpawnPosition(mageSpawnPoints);
            if (spawnPosition != Vector3.zero)
            {
                spawnPosition.y = 0.1f;
                Instantiate(mageEnemyPrefab, spawnPosition, Quaternion.identity);
            }
        }
    }

    Vector3 GetValidSpawnPosition(Transform[] spawnPoints)
    {
        int attempts = 10; // Limit the number of attempts to find a valid position
        for (int i = 0; i < attempts; i++)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Length);
            Vector3 spawnPosition = spawnPoints[spawnIndex].position;

            if (IsPositionValidForSpawn(spawnPosition))
            {
                return spawnPosition;
            }
        }
        return Vector3.zero; // Return zero vector if no valid position found
    }

    bool IsPositionValidForSpawn(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 50, Vector3.down, out hit))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                return false;
            }
        }
        return true;
    }
}
