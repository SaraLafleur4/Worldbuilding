using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RocksGenerator : MonoBehaviour
{
    public GameObject rockPrefab1;
    public GameObject rockPrefab2;
    public GameObject rockPrefab3;

    public int numberOfRocks = 10;

    public Vector2 planeSize = new Vector2(10, 10); // Tama�o del �rea donde se generar�n las rocas
    public Vector2 planePos = new Vector2(250, 250); // Posici�n base del �rea


    public void GenerateRocks()
    {


        for (int i = 0; i < numberOfRocks; i++)
        {


            GenerateSingleRock();
        }


    }

    void GenerateSingleRock()
    {
        // Lista de prefabs de rocas
        List<GameObject> rockPrefabs = new List<GameObject> { rockPrefab1, rockPrefab2, rockPrefab3 };

        // Elegir un prefab de roca aleatoriamente
        GameObject chosenRockPrefab = rockPrefabs[Random.Range(0, rockPrefabs.Count)];

        // Calcular una posici�n aleatoria dentro del �rea definida
        var randomPosition = new Vector3(
            Random.Range(planePos.x, planePos.x + planeSize.x), // Coordenada X
            0,                                                 // Altura inicial
            Random.Range(planePos.y, planePos.y + planeSize.y)  // Coordenada Z
        );

        // Ajustar la altura si hay un terreno activo
        if (Terrain.activeTerrain != null)
        {
            randomPosition.y = Terrain.activeTerrain.SampleHeight(randomPosition);
        }

        // Generar la roca en la escena
        Instantiate(chosenRockPrefab, randomPosition, Quaternion.identity);
    }

    void Update()
    {
        // Aqu� puedes a�adir l�gica adicional si se necesita en cada frame
    }
}
