using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainNoiseGeneration : MonoBehaviour
{
    [SerializeField]
    private Vector3Int size = new Vector3Int(500, 3, 500);

    [SerializeField]
    private float scale;

    public Vector2 seed = new Vector2(0f, 0f);

    private Terrain terrain;

    // Initializes the terrain and generates the terrain heights on start
    void Start()
    {
        terrain = GetComponent<Terrain>();
        GenerateTerrain(terrain.terrainData);
    }

    // Sets the terrain size and generates the heights based on the specified size and noise
    public void GenerateTerrain(TerrainData terrainData)
    {
        terrainData.size = (Vector3)size;
        terrainData.SetHeights(0, 0, GenerateHeights(size));
    }

    // Generates a 2D array of height values using Perlin noise based on terrain size and scale
    float[,] GenerateHeights(Vector3Int size)
    {
        float[,] heights = new float[size.x, size.z];
        for (int x = 0; x < size.x; ++x)
            for (int z = 0; z < size.z; ++z)
                heights[x, z] = Mathf.PerlinNoise(
                    ((float)x / size.x) * scale + seed.x,
                    ((float)z / size.z) * scale + seed.y
                );

        return heights;
    }

    // Regenerates the terrain when called from the editor GUI
    public void GenerateTerrainGUI()
    {
        Terrain t = GetComponent<Terrain>();
        GenerateTerrain(t.terrainData);
    }

    // Resets the terrain heights to zero, effectively flattening the terrain
    public void ResetHeightsTerrainGUI()
    {
        Terrain t = GetComponent<Terrain>();
        t.terrainData.size = new Vector3(size.x, 0, size.z);
    }
}
