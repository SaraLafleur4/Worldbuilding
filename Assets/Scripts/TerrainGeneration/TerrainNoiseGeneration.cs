using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainNoiseGeneration : MonoBehaviour
{
    [SerializeField]
    private Vector3Int size = new Vector3Int(500, 3, 500);

    [SerializeField]
    private float scale;

    private Terrain terrain;
    void Start()
    {
        terrain = GetComponent<Terrain>();

        GenerateTerrain(terrain.terrainData);
    }

    public void GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = 513;
        terrainData.size = (Vector3)size;
        
        terrainData.SetHeights(0, 0, GenerateHeights(size));
    }

    float[,] GenerateHeights(Vector3Int size)
    {
        float[,] heights = new float[size.x, size.z];
        for (int x = 0; x <= size.x+12; ++x)
            for (int z = 0; z <= size.z+12; ++z)
                heights[x,z] = Mathf.PerlinNoise(
                    ((float)x / size.x) * scale,
                    ((float)z / size.z) * scale
                );

        return heights;
    }

    public void GenerateTerrainGUI() {
        Terrain t = GetComponent<Terrain>();
        GenerateTerrain(t.terrainData);
    }

    public void ResetHeightsTerrainGUI() {
        Terrain t = GetComponent<Terrain>();

        t.terrainData.size = new Vector3(size.x, 0, size.z);
    }
}
