using System.Collections.Generic;
using UnityEngine;

public class Rocks : MonoBehaviour
{
    public GameObject rockPrefab1;
    public GameObject rockPrefab2;
    public GameObject rockPrefab3;

    public int numberOfRocks = 10;

    public List<GameObject> rocks;

    public Vector2 planeAngle = new Vector2();
    public Vector2 planeSize = new Vector2(500, 500);
    public Vector2 planePos = new Vector2(10, 10);

    public void GenerateRocks()
    {
        ClearRocks();

        for (int i = 0; i < numberOfRocks; i++)
        {
            GenerateSingleRock();
        }
    }

    void GenerateSingleRock()
    {
        // List of rock prefabs
        List<GameObject> rockPrefabs = new List<GameObject> { rockPrefab1, rockPrefab2, rockPrefab3 };
        GameObject chosenRockPrefab = rockPrefabs[Random.Range(0, rockPrefabs.Count)];

        // Calculate a random position within the defined area
        var randomPosition = new Vector3(
            planePos.x + Random.Range(0, planeSize.x),
            0, // Initial height
            planePos.y + Random.Range(0, planeSize.y)
        );
        if (Terrain.activeTerrain != null) randomPosition.y = Terrain.activeTerrain.SampleHeight(randomPosition);

        // Adjust rotation to terrain
        Vector3 terrainNormal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal(
            (randomPosition.x - Terrain.activeTerrain.transform.position.x) / Terrain.activeTerrain.terrainData.size.x,
            (randomPosition.z - Terrain.activeTerrain.transform.position.z) / Terrain.activeTerrain.terrainData.size.z
        );
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, terrainNormal);

        // Create new rock
        GameObject rock = Instantiate(chosenRockPrefab, randomPosition, rotation);
        if (rock.GetComponent<Collider>() == null)
        {
            MeshCollider meshCollider = rock.AddComponent<MeshCollider>();
            meshCollider.convex = true; // Ensure convex for physics interactions
        }
        rocks.Add(rock);
    }

    private void ClearRocks()
    {
        if (rocks != null)
        {
            foreach (var rock in rocks) Destroy(rock);
            rocks.Clear();
        }
    }
}
