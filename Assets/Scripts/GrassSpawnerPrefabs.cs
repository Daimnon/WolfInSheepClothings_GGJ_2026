using System.Collections.Generic;
using UnityEngine;

public class GrassSpawnerPrefabs : MonoBehaviour
{
    public GameObject[] grassPrefabs; // Assign multiple grass prefabs here
    public Transform parent;
    public int spawnCount = 100;
    public float areaSize = 10f;

    [Header("Collision & Variation")]
    public float avoidanceRadius = 0.5f;
    public LayerMask obstaclesLayer;
    public float minScale = 0.8f;
    public float maxScale = 1.2f;

    void Start()
    {
        if (grassPrefabs == null || grassPrefabs.Length == 0)
        {
            Debug.LogError("Please assign at least one prefab to the GrassSpawner!");
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = transform.position + new Vector3(
                Random.Range(-areaSize, areaSize), 
                0, 
                Random.Range(-areaSize, areaSize)
            );

            // Check if space is clear
            if (!Physics.CheckSphere(pos, avoidanceRadius, obstaclesLayer))
            {
                // 1. Pick a random prefab
                GameObject randomPrefab = grassPrefabs[Random.Range(0, grassPrefabs.Length)];
                
                // 2. Random rotation (360 degrees on Y axis)
                Quaternion randomRot = Quaternion.Euler(0, Random.Range(0, 360), 0);

                // 3. Spawn
                GameObject blade = Instantiate(randomPrefab, pos, randomRot, parent);
                blade.name = "GrassBlade_" + i;

                // 4. Random scale
                blade.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);
            }
        }
    }
}