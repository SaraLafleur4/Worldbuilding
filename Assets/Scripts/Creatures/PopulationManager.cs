using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PopulationManager : MonoBehaviour
{
    public int initialPopSize = 100;
    public int maxPopSize = 3000;

    public List<Creature> population;

    public float mutationRate = 0.01f;
    public int generation = 0;

    private Vector3 startingPosition = Vector3.zero;
    private Vector2 spawningAreaAngle = new Vector2(10f, 10f);
    private Vector2 spawningAreaSize = new Vector2(490f, 490f);

    private Coroutine breedingCoroutine;
    public CreatureGenerator creatureGenerator;

    private bool isInitialized = false;

    public bool DEBUG = false;

    // Starts the breeding cycle by beginning a coroutine
    private void StartBreeding()
    {
        breedingCoroutine = StartCoroutine(BreedingCycle());
        Debug.Log("Breeding Started!");
    }

    // Continuously breeds a new generation at fixed intervals
    private IEnumerator BreedingCycle()
    {
        while (true)
        {
            BreedNewGeneration();
            yield return new WaitForSeconds(3f);
        }
    }

    // Stops the breeding cycle coroutine and logs the result
    private void StopBreeding()
    {
        if (breedingCoroutine != null)
        {
            StopCoroutine(breedingCoroutine);
            breedingCoroutine = null;
            Debug.Log($"Breeding Stopped after {generation} generations!");
        }
    }

    // Initializes the population and starts the breeding process
    public void Initialize()
    {
        population = new List<Creature>();
        InitializePopulation();
        isInitialized = true;
        StartBreeding();
    }

    // Updates the population every frame, handling creature lifespans and stopping breeding if max population is reached
    private void Update()
    {
        if (!isInitialized) return;

        for (int i = population.Count - 1; i >= 0; i--)
        {
            DNA dna = population[i].dna;
            dna.timeToLive -= Time.deltaTime;

            if (dna.timeToLive <= 0)
            {
                Destroy(population[i].model);
                population.RemoveAt(i);

                UpdateCreatureHealthAndOpacity(population[i]);
            }
        }

        if (population.Count >= maxPopSize) StopBreeding();
    }

    // Initializes the population with a specified number of creatures at random positions
    private void InitializePopulation()
    {
        for (int i = 0; i < initialPopSize; i++)
        {
            Creature creature = new Creature(creatureGenerator);
            creature.DisplayCreature(startingPosition, spawningAreaAngle, spawningAreaSize);
            population.Add(creature);
        }
    }

    // Breeds a new generation of creatures by selecting parents and generating offspring
    private void BreedNewGeneration()
    {
        if (DEBUG) Debug.Log($"Breed! - current population size = {population.Count}");

        if (population.Count == 0)
        {
            if (DEBUG) Debug.LogError("Population is empty. Cannot breed new population.");
            StopBreeding();
            return;
        }

        List<Creature> possibleParents = GetPossibleParentsList();
        if (DEBUG) Debug.Log($"{possibleParents.Count} possible parents.");

        for (int i = 0; i < possibleParents.Count; i += 2)
        {
            Creature parent1 = SelectParent(possibleParents);
            Creature parent2 = SelectParent(possibleParents);

            if (parent1 != null && parent2 != null)
            {
                Creature offspring1 = Breed(parent1.dna, parent2.dna);
                offspring1.DisplayCreature(startingPosition, spawningAreaAngle, spawningAreaSize);
                population.Add(offspring1);

                Creature offspring2 = Breed(parent1.dna, parent2.dna);
                offspring2.DisplayCreature(startingPosition, spawningAreaAngle, spawningAreaSize);
                population.Add(offspring2);
            }
            else
            {
                if (DEBUG) Debug.LogError("Parent selection failed. Null parent returned.");
            }
        }

        generation++;
    }

    // Returns a list of creatures eligible to be parents, based on their remaining lifespan
    private List<Creature> GetPossibleParentsList()
    {
        List<Creature> possibleParents = new List<Creature>();

        foreach (var creature in population)
        {
            if (creature.dna.timeToLive > 15) possibleParents.Add(creature); // timeToLive depends on health
        }

        return possibleParents;
    }

    // Selects a parent based on a weighted fitness system
    private Creature SelectParent(List<Creature> possibleParents)
    {
        List<int> weights = new List<int>();

        foreach (var creature in possibleParents)
        {
            FitnessLevel fitness = creature.fitness;

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

    // Combines the DNA of two parents, applies mutation, and creates a new creature
    private Creature Breed(DNA dna1, DNA dna2)
    {
        DNA offspringDNA = CombineDNA(dna1, dna2);
        offspringDNA.Mutate(mutationRate);

        return new Creature(offspringDNA, creatureGenerator);
    }

    // Combines DNA from two parents, choosing genes randomly for the offspring
    private DNA CombineDNA(DNA parent1, DNA parent2)
    {
        DNA offspringDNA = new DNA();
        // Crossover: randomly choose genes from either parent
        offspringDNA.bodyShape = Random.Range(0, 10) < 5 ? parent1.bodyShape : parent2.bodyShape;
        offspringDNA.headShape = Random.Range(0, 10) < 5 ? parent1.headShape : parent2.headShape;
        offspringDNA.earShape = Random.Range(0, 10) < 5 ? parent1.earShape : parent2.earShape;
        offspringDNA.size = Random.Range(0, 10) < 5 ? parent1.size : parent2.size;
        offspringDNA.red = Random.Range(0, 10) < 5 ? parent1.red : parent2.red;
        offspringDNA.green = Random.Range(0, 10) < 5 ? parent1.green : parent2.green;
        offspringDNA.blue = Random.Range(0, 10) < 5 ? parent1.blue : parent2.blue;
        offspringDNA.earNumber = Random.Range(0, 10) < 5 ? parent1.earNumber : parent2.earNumber;
        // NO Crossover: random health and life span
        offspringDNA.health = Random.Range(80.0f, 100.0f);
        offspringDNA.timeToLive = Mathf.Lerp(5f, 20f, offspringDNA.health / 100f); // proportional to health
        return offspringDNA;
    }

    // Updates the health and opacity of a creature based on its health status
    private void UpdateCreatureHealthAndOpacity(Creature creature)
    {
        creature.dna.timeToLive -= (!creature.dna.isHealthy) ? 2 : 0;

        // THIS PART DOES NOT WORK (opacity editing)
        var renderer = creature.model.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material creatureMaterial = creature.creatureMaterial;
            Color color = creatureMaterial.color;
            color.a = creature.dna.health / 100f;
            creatureMaterial.color = color;
        }
    }
}
