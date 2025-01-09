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

    public Vector2 planeAngle = new Vector2();
    public Vector2 planeSize = new Vector2(10, 10); // Tamaño del área donde se generarán las rocas
    public Vector2 planePos = new Vector2(250, 250); // Posición base del área


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

        // Calcular una posición aleatoria dentro del área definida
        var randomPosition = new Vector3(
            planeAngle.x + Random.Range(planePos.x, planePos.x + planeSize.x), // Coordenada X
            0,                                                 // Altura inicial
            planeAngle.y + Random.Range(planePos.y, planePos.y + planeSize.y)  // Coordenada Z



        
        );

        randomPosition.x = Mathf.Clamp(randomPosition.x, planeAngle.x, planeAngle.x + planeSize.x);
        randomPosition.y = Terrain.activeTerrain.SampleHeight(randomPosition);
        randomPosition.z = Mathf.Clamp(randomPosition.z, planeAngle.y, planeAngle.y + planeSize.y);



        randomPosition.y = Terrain.activeTerrain.SampleHeight(randomPosition);

        


        // Generar la roca en la escena
        Instantiate(chosenRockPrefab, randomPosition, Quaternion.identity);
    }

    void Update()
    {
        // Aquí puedes añadir lógica adicional si se necesita en cada frame
    }
}
