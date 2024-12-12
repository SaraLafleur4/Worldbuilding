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

    public void StartBreeding() {
        breedingCoroutine = StartCoroutine(BreedingCycle());

        if (DEBUG) Debug.Log("Breeding Started!");
    }

    private IEnumerator BreedingCycle() {
        while (true) {
            BreedNewGeneration();
            yield return new WaitForSeconds(3f);
        }
    }

    public void StopBreeding() {
        if (breedingCoroutine != null)
        {
            StopCoroutine(breedingCoroutine);
            breedingCoroutine = null;

            if (DEBUG) Debug.Log($"Breeding Stopped after {generation} generations!");
        }
    }

    public void Initialize() {
        population = new List<GameObject>();
        InitializePopulation();

        StartBreeding();
    }

    // Updates for each frame
    private void Update()
    {
        // Update population by removing dead creatures
        for (int i = population.Count - 1; i >= 0; i--)
        {
            DNA dna = population[i].GetComponent<DNA>();
            dna.timeToLive -= Time.deltaTime;

            if (dna.timeToLive <= 0) {
                Destroy(population[i]);
                population.RemoveAt(i);
            }
        }

        if (population.Count >= maxPopSize) StopBreeding();
    }

    // Creates the initial population of creatures with random genes
    private void InitializePopulation()
    {
        for (int i = 0; i < initialPopSize; i++)
        {
            // Instantiate the creature with prefab and position
            GameObject prefab = GetRandomPrefab();
            Vector3 position = GetRandomPosition();
            GameObject creature = Instantiate(prefab, position, Quaternion.identity, gameObject.transform);

            // Randomize the creature's genes
            DNA dna = creature.GetComponent<DNA>();
            dna.shape = GetNameByPrefab(prefab);
            dna.size = Random.Range(0.0f, 1.0f);
            dna.red = Random.Range(0.0f, 1.0f);
            dna.green = Random.Range(0.0f, 1.0f);
            dna.blue = Random.Range(0.0f, 1.0f);
            dna.health = (uint)Random.Range(1.0f, 10.0f); // TODO: use health for something
            dna.timeToLive = Random.Range(5.0f, 30.0f);

            // Evaluate the fitness of the newly created creature
            EvaluateFitness(creature);

            // Add the creature to the population list
            population.Add(creature);
        }
    }

    // Breeds a new population from the current population
    public void BreedNewGeneration()
    {
        if (DEBUG) Debug.Log($"Breed! - current population size = {population.Count}");

        // Check if population is non-empty before breeding
        if (population.Count == 0)
        {
            if (DEBUG) Debug.LogError("Population is empty. Cannot breed new population.");
            StopBreeding();
            return;
        }

        // Reset possible parents' list
        List<GameObject> possibleParents = GetPossibleParentsList();
        if (DEBUG) Debug.Log($"{possibleParents.Count} possible parents.");

        // Generate a new population by breeding pairs of creatures
        for (int i = 0; i < possibleParents.Count; i += 2) {
            // It is apparently possible to auto breed yourself in this universe
            GameObject parent1 = SelectParent(possibleParents);
            GameObject parent2 = SelectParent(possibleParents);

            // Check for valid parent selection before breeding
            if (parent1 != null && parent2 != null) {
                population.Add(Breed(parent1, parent2));
                population.Add(Breed(parent1, parent2));
            } else
                if (DEBUG) Debug.LogError("Parent selection failed. Null parent returned.");
        }

        generation++;
    }

    public List<GameObject> GetPossibleParentsList() {
        List<GameObject> possibleParents = new List<GameObject>();

        foreach (var creature in population) {
            float timeToLive = creature.GetComponent<DNA>().timeToLive; // TODO: should depend on health

            if (timeToLive > 15) possibleParents.Add(creature);
        }

        return possibleParents;
    }

    // Selects a parent based on weighted random selection, favoring creatures with higher fitness
    // /!\ PARENT SELECTION SHOULD BEE UPDATED LATER ON /!\
    GameObject SelectParent(List<GameObject> possibleParents)
    {
        // List to hold fitness weights for selection
        List<int> weights = new List<int>();

        // Assign weights based on fitness levels
        foreach (var creature in possibleParents)
        {
            FitnessLevel fitness = creature.GetComponent<DNA>().fitnessLevel;

            switch (fitness)
            {
                case FitnessLevel.Best:
                    weights.Add(4);  // Best creatures have the highest weight
                    break;
                case FitnessLevel.Good:
                    weights.Add(3);
                    break;
                case FitnessLevel.NotBad:
                    weights.Add(2);
                    break;
                case FitnessLevel.Poor:
                    weights.Add(1);  // Poor creatures have the lowest weight
                    break;
            }
        }

        // Calculate total weight for random selection
        int totalWeight = weights.Sum();

        // Error check: if total weight is 0, return the first creature
        if (totalWeight == 0)
        {
            if (DEBUG) Debug.LogError("Total weight is zero. Check if all creatures have been assigned a valid fitness level.");
            return possibleParents[0];
        }

        // Select a random value between 0 and totalWeight
        int randomPick = Random.Range(0, totalWeight);

        // Iterate over the population to find the parent based on cumulative weight
        int currentSum = 0;
        for (int i = 0; i < possibleParents.Count; i++)
        {
            currentSum += weights[i];

            // Return the selected parent when the random value falls within the current sum
            if (randomPick < currentSum)
            {
                return possibleParents[i];
            }
        }

        // Fallback in case of an issue (this should not be reached)
        if (DEBUG) Debug.LogWarning("Fallback reached in SelectParent method. Returning the last creature.");
        return possibleParents[possibleParents.Count - 1];
    }


    // Breeds two creatures to create offspring with a mix of parent genes
    private GameObject Breed(GameObject parent1, GameObject parent2)
    {
        // Get the DNA of both parents
        DNA dna1 = parent1.GetComponent<DNA>();
        DNA dna2 = parent2.GetComponent<DNA>();

        // Create a new offspring
        Shape newCreatureShape = Random.Range(0, 10) < 5 ? dna1.shape : dna2.shape;
        GameObject prefab = GetPrefabByName(newCreatureShape);
        Vector3 position = GetNewCreaturePosition(parent1.transform.localPosition, parent2.transform.localPosition);

        GameObject offspring = Instantiate(prefab, position, Quaternion.identity, gameObject.transform);

        DNA offspringDNA = offspring.GetComponent<DNA>();

        // Crossover: randomly choose genes from either parent
        offspringDNA.shape = newCreatureShape;
        offspringDNA.size = Random.Range(0, 10) < 5 ? dna1.size : dna2.size;
        offspringDNA.red = Random.Range(0, 10) < 5 ? dna1.red : dna2.red;
        offspringDNA.green = Random.Range(0, 10) < 5 ? dna1.green : dna2.green;
        offspringDNA.blue = Random.Range(0, 10) < 5 ? dna1.blue : dna2.blue;
        // NO Crossover: random health and life span
        offspringDNA.health = (uint)Random.Range(1.0f, 10.0f); // TODO: use health for something
        offspringDNA.timeToLive = Random.Range(5.0f, 30.0f);

        // Apply random mutation based on the mutation rate
        if (Random.Range(0.0f, 1.0f) < mutationRate)
        {
            offspringDNA.size = Random.Range(0.0f, 5.0f);

            offspringDNA.red = Random.Range(0.0f, 1.0f);
            offspringDNA.green = Random.Range(0.0f, 1.0f);
            offspringDNA.blue = Random.Range(0.0f, 1.0f);
        }

        // Evaluate fitness of the new offspring
        EvaluateFitness(offspring);

        return offspring;
    }

    // Evaluates fitness of an creature based on gene expression
    // /!\ FITNESS EVALUATION SHOULD BE UPDATED LATER ON /!\
    private void EvaluateFitness(GameObject creature)
    {
        DNA dna = creature.GetComponent<DNA>();

        // Simple fitness function based on a single gene
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

    ///// FOR INITIAL POPULATION
    private Vector3 GetRandomPosition() {
        var vec = new Vector3(
            spawningAreaAngle.x + Random.Range(0, spawningAreaSize.x),
            0,
            spawningAreaAngle.y + Random.Range(0, spawningAreaSize.y));

        vec.y = Terrain.activeTerrain.SampleHeight(vec);
        return vec;
    }

    // Gets random prefab
    private GameObject GetRandomPrefab() {
        int value = Random.Range(1, 12);

        if (value < 5)
            return cubePrefab;
        else if (value > 4 && value < 9)
            return spherePrefab;
        else // if value > 8
            return capsulePrefab;
    }

    private Shape GetNameByPrefab(GameObject prefab) {
        if (prefab == cubePrefab)
            return Shape.Cube;
        else if (prefab == spherePrefab)
            return Shape.Sphere;
        else // if prefab == capsulePrefab
            return Shape.Capsule;
    }

    ///// FOR BREEDING
    private Vector3 GetNewCreaturePosition(Vector3 parent1Position, Vector3 parent2Position) {
        Vector3 vec = (parent1Position + parent2Position) / 2 +
            new Vector3(
                Random.Range(-breedingDisplacement.x/2f, breedingDisplacement.x/2f),
                0f,
                Random.Range(-breedingDisplacement.y/2f, breedingDisplacement.y/2f));

        vec.x = Mathf.Clamp(vec.x, spawningAreaAngle.x, spawningAreaAngle.x + spawningAreaSize.x);
        vec.y = Terrain.activeTerrain.SampleHeight(vec);
        vec.z = Mathf.Clamp(vec.z, spawningAreaAngle.y, spawningAreaAngle.y + spawningAreaSize.y);

        return vec;
    }

    private GameObject GetPrefabByName(Shape shapeName) {
        if (shapeName == Shape.Cube)
            return cubePrefab;
        else if (shapeName == Shape.Sphere)
            return spherePrefab;
        else // if shapeName == Shape.Capsule {
            return capsulePrefab;
    }
}
