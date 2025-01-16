using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PopulationManager : MonoBehaviour
{
    public GameObject cubePrefab;
    public GameObject spherePrefab;
    public GameObject capsulePrefab;

    public int initialPopSize = 10;
    public int maxPopSize = 300;

    public List<GameObject> population;

    public float mutationRate = 0.01f;
    public int generation = 0;

    public Vector2 spawningAreaAngle = new Vector2();
    public Vector2 spawningAreaSize = new Vector2(10f, 10f);
    public Vector2 breedingDisplacement = new Vector2(10f, 10f);

    public bool DEBUG = false;

    private Coroutine breedingCoroutine;

    public void StartBreeding()
    {
        breedingCoroutine = StartCoroutine(BreedingCycle());

        if (DEBUG) Debug.Log("Breeding Started!");
    }

    private IEnumerator BreedingCycle()
    {
        while (true)
        {
            BreedNewGeneration();
            yield return new WaitForSeconds(3f);
        }
    }

    public void StopBreeding()
    {
        if (breedingCoroutine != null)
        {
            StopCoroutine(breedingCoroutine);
            breedingCoroutine = null;

            if (DEBUG) Debug.Log($"Breeding Stopped after {generation} generations!");
        }
    }

    public void Initialize()
    {
        population = new List<GameObject>();
        InitializePopulation();

        StartBreeding();
    }

    private void Update()
    {
        for (int i = population.Count - 1; i >= 0; i--)
        {
            DNA dna = population[i].GetComponent<DNA>();
            dna.timeToLive -= Time.deltaTime;

            if (dna.timeToLive <= 0)
            {
                Destroy(population[i]);
                population.RemoveAt(i);
            }
        }

        if (population.Count >= maxPopSize) StopBreeding();
    }

    private void InitializePopulation()
    {
        for (int i = 0; i < initialPopSize; i++)
        {
            GameObject prefab = GetRandomPrefab();
            Vector3 position = GetRandomPosition();
            GameObject creature = Instantiate(prefab, position, Quaternion.identity, gameObject.transform);

            AdjustToTerrain(creature);

            DNA dna = creature.GetComponent<DNA>();
            dna.shape = GetNameByPrefab(prefab);
            dna.size = Random.Range(0.0f, 1.0f);
            dna.red = Random.Range(0.0f, 1.0f);
            dna.green = Random.Range(0.0f, 1.0f);
            dna.blue = Random.Range(0.0f, 1.0f);
            dna.health = (uint)Random.Range(1.0f, 10.0f); // TODO: use health for something
            dna.timeToLive = Random.Range(5.0f, 30.0f);

            EvaluateFitness(creature);

            population.Add(creature);
        }
    }

    public void BreedNewGeneration()
    {
        if (DEBUG) Debug.Log($"Breed! - current population size = {population.Count}");

        if (population.Count == 0)
        {
            if (DEBUG) Debug.LogError("Population is empty. Cannot breed new population.");
            StopBreeding();
            return;
        }

        List<GameObject> possibleParents = GetPossibleParentsList();
        if (DEBUG) Debug.Log($"{possibleParents.Count} possible parents.");

        for (int i = 0; i < possibleParents.Count; i += 2)
        {
            GameObject parent1 = SelectParent(possibleParents);
            GameObject parent2 = SelectParent(possibleParents);

            if (parent1 != null && parent2 != null)
            {
                population.Add(Breed(parent1, parent2));
                population.Add(Breed(parent1, parent2));
            }
            else
            {
                if (DEBUG) Debug.LogError("Parent selection failed. Null parent returned.");
            }
        }

        generation++;
    }

    public List<GameObject> GetPossibleParentsList()
    {
        List<GameObject> possibleParents = new List<GameObject>();

        foreach (var creature in population)
        {
            float timeToLive = creature.GetComponent<DNA>().timeToLive; // TODO: should depend on health

            if (timeToLive > 15) possibleParents.Add(creature);
        }

        return possibleParents;
    }

    // Selects a parent based on weighted random selection, favoring creatures with higher fitness
    // /!\ PARENT SELECTION SHOULD BEE UPDATED LATER ON /!\
    GameObject SelectParent(List<GameObject> possibleParents)
    {
        List<int> weights = new List<int>();

        foreach (var creature in possibleParents)
        {
            FitnessLevel fitness = creature.GetComponent<DNA>().fitnessLevel;

            switch (fitness)
            {
                case FitnessLevel.Best:
                    weights.Add(4);
                    break;
                case FitnessLevel.Good:
                    weights.Add(3);
                    break;
                case FitnessLevel.NotBad:
                    weights.Add(2);
                    break;
                case FitnessLevel.Poor:
                    weights.Add(1);
                    break;
            }
        }

        int totalWeight = weights.Sum();

        if (totalWeight == 0)
        {
            if (DEBUG) Debug.LogError("Total weight is zero. Check if all creatures have been assigned a valid fitness level.");
            return possibleParents[0];
        }

        int randomPick = Random.Range(0, totalWeight);
        int currentSum = 0;

        for (int i = 0; i < possibleParents.Count; i++)
        {
            currentSum += weights[i];

            if (randomPick < currentSum)
            {
                return possibleParents[i];
            }
        }

        if (DEBUG) Debug.LogWarning("Fallback reached in SelectParent method. Returning the last creature.");
        return possibleParents[possibleParents.Count - 1];
    }

    private GameObject Breed(GameObject parent1, GameObject parent2)
    {
        DNA dna1 = parent1.GetComponent<DNA>();
        DNA dna2 = parent2.GetComponent<DNA>();

        Shape newCreatureShape = Random.Range(0, 10) < 5 ? dna1.shape : dna2.shape;
        GameObject prefab = GetPrefabByName(newCreatureShape);
        Vector3 position = GetNewCreaturePosition(parent1.transform.localPosition, parent2.transform.localPosition);

        GameObject offspring = Instantiate(prefab, position, Quaternion.identity, gameObject.transform);

        AdjustToTerrain(offspring);

        // Crossover: randomly choose genes from either parent
        DNA offspringDNA = offspring.GetComponent<DNA>();
        offspringDNA.shape = newCreatureShape;
        offspringDNA.size = Random.Range(0, 10) < 5 ? dna1.size : dna2.size;
        offspringDNA.red = Random.Range(0, 10) < 5 ? dna1.red : dna2.red;
        offspringDNA.green = Random.Range(0, 10) < 5 ? dna1.green : dna2.green;
        offspringDNA.blue = Random.Range(0, 10) < 5 ? dna1.blue : dna2.blue;
        // NO Crossover: random health and life span
        offspringDNA.health = (uint)Random.Range(1.0f, 10.0f); // TODO: use health for something
        offspringDNA.timeToLive = Random.Range(5.0f, 30.0f);

        if (Random.Range(0.0f, 1.0f) < mutationRate)
        {
            offspringDNA.size = Random.Range(0.0f, 5.0f);
            offspringDNA.red = Random.Range(0.0f, 1.0f);
            offspringDNA.green = Random.Range(0.0f, 1.0f);
            offspringDNA.blue = Random.Range(0.0f, 1.0f);
        }

        EvaluateFitness(offspring);

        return offspring;
    }

    // Evaluates fitness of an creature based on gene expression
    // /!\ FITNESS EVALUATION SHOULD BE UPDATED LATER ON /!\
    private void EvaluateFitness(GameObject creature)
    {
        DNA dna = creature.GetComponent<DNA>();

        if (dna.blue >= 0.6f && dna.shape == Shape.Cube)
        {
            dna.fitnessLevel = FitnessLevel.Best;
        }
        else if (dna.blue >= 0.3f && dna.blue < 0.6f)
        {
            dna.fitnessLevel = FitnessLevel.Good;
        }
        else if (dna.blue >= 0.1f && dna.blue < 0.3f && dna.shape == Shape.Sphere)
        {
            dna.fitnessLevel = FitnessLevel.NotBad;
        }
        else
        {
            dna.fitnessLevel = FitnessLevel.Poor;
        }
    }

    private void AdjustToTerrain(GameObject creature)
    {
        Vector3 position = creature.transform.position;

        Vector3 terrainNormal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal(
            position.x / Terrain.activeTerrain.terrainData.size.x,
            position.z / Terrain.activeTerrain.terrainData.size.z);

        creature.transform.rotation = Quaternion.FromToRotation(Vector3.up, terrainNormal);

        Renderer renderer = creature.GetComponent<Renderer>();
        if (renderer != null)
        {
            float creatureHeight = renderer.bounds.size.y;
            position.y += creatureHeight / 2.0f; // because the creatures anchor point is in its center
            position.y += 0.1f; // fine tuning
        }

        creature.transform.position = position;
    }

    ///// FOR INITIAL POPULATION
    private Vector3 GetRandomPosition()
    {
        var vec = new Vector3(
            spawningAreaAngle.x + Random.Range(0, spawningAreaSize.x),
            0,
            spawningAreaAngle.y + Random.Range(0, spawningAreaSize.y));

        vec.y = Terrain.activeTerrain.SampleHeight(vec);
        return vec;
    }

    private GameObject GetRandomPrefab()
    {
        int value = Random.Range(1, 12);

        if (value < 5)
            return cubePrefab;
        else if (value > 4 && value < 9)
            return spherePrefab;
        else // if value > 8
            return capsulePrefab;
    }

    private Shape GetNameByPrefab(GameObject prefab)
    {
        if (prefab == cubePrefab)
            return Shape.Cube;
        else if (prefab == spherePrefab)
            return Shape.Sphere;
        else // if prefab == capsulePrefab
            return Shape.Capsule;
    }

    ///// FOR BREEDING
    private Vector3 GetNewCreaturePosition(Vector3 parent1Position, Vector3 parent2Position)
    {
        Vector3 vec = (parent1Position + parent2Position) / 2 +
            new Vector3(
                Random.Range(-breedingDisplacement.x / 2f, breedingDisplacement.x / 2f),
                0f,
                Random.Range(-breedingDisplacement.y / 2f, breedingDisplacement.y / 2f));

        vec.x = Mathf.Clamp(vec.x, spawningAreaAngle.x, spawningAreaAngle.x + spawningAreaSize.x);
        vec.y = Terrain.activeTerrain.SampleHeight(vec);
        vec.z = Mathf.Clamp(vec.z, spawningAreaAngle.y, spawningAreaAngle.y + spawningAreaSize.y);

        return vec;
    }

    private GameObject GetPrefabByName(Shape shapeName)
    {
        if (shapeName == Shape.Cube)
            return cubePrefab;
        else if (shapeName == Shape.Sphere)
            return spherePrefab;
        else // if shapeName == Shape.Capsule
            return capsulePrefab;
    }
}
