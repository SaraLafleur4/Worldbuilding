using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Rocks : MonoBehaviour
{
    public GameObject rockPrefab1;
    public GameObject rockPrefab2;
    public GameObject rockPrefab3;

    public int numberOfRocks = 10;

    public Vector2 planeAngle = new Vector2();
    public Vector2 planeSize = new Vector2(10, 10); // Size of the area where rocks will be generated
    public Vector2 planePos = new Vector2(250, 250); // Base position of the area

    public void GenerateRocks()
    {
        for (int i = 0; i < numberOfRocks; i++)
        {
            GenerateSingleRock();
        }
    }

    void GenerateSingleRock()
    {
        // List of rock prefabs
        List<GameObject> rockPrefabs = new List<GameObject> { rockPrefab1, rockPrefab2, rockPrefab3 };

        // Randomly select a rock prefab
        GameObject chosenRockPrefab = rockPrefabs[Random.Range(0, rockPrefabs.Count)];

        // Calculate a random position within the defined area
        var randomPosition = new Vector3(
            planeAngle.x + Random.Range(planePos.x, planePos.x + planeSize.x), // X coordinate
            0, // Initial height
            planeAngle.y + Random.Range(planePos.y, planePos.y + planeSize.y) // Z coordinate
        );

        // Clamp the position to ensure it remains within the defined area
        randomPosition.x = Mathf.Clamp(randomPosition.x, planeAngle.x, planeAngle.x + planeSize.x);
        randomPosition.z = Mathf.Clamp(randomPosition.z, planeAngle.y, planeAngle.y + planeSize.y);

        // Set the height based on the terrain
        randomPosition.y = Terrain.activeTerrain.SampleHeight(randomPosition);

        // Get the terrain normal at the generated position
        Vector3 terrainNormal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal(
            (randomPosition.x - Terrain.activeTerrain.transform.position.x) / Terrain.activeTerrain.terrainData.size.x,
            (randomPosition.z - Terrain.activeTerrain.transform.position.z) / Terrain.activeTerrain.terrainData.size.z
        );

        // Calculate the rock's rotation to align with the terrain normal
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, terrainNormal);

        // Instantiate the rock in the scene with the calculated rotation
        Instantiate(chosenRockPrefab, randomPosition, rotation);
    }

    void Update()
    {
        // Additional logic can be added here if needed on each frame
    }
}
