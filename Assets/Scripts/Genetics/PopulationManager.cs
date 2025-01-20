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
    private Vector2 breedingDisplacement = new Vector2(50f, 50f);

    private Coroutine breedingCoroutine;
    public CreatureGenerator creatureGenerator;

    private bool isInitialized = false;

    public bool DEBUG = false;

    private void StartBreeding()
    {
        breedingCoroutine = StartCoroutine(BreedingCycle());
        Debug.Log("Breeding Started!");
    }

    private IEnumerator BreedingCycle()
    {
        while (true)
        {
            BreedNewGeneration();
            yield return new WaitForSeconds(3f);
        }
    }

    private void StopBreeding()
    {
        if (breedingCoroutine != null)
        {
            StopCoroutine(breedingCoroutine);
            breedingCoroutine = null;
            Debug.Log($"Breeding Stopped after {generation} generations!");
        }
    }

    public void Initialize()
    {
        population = new List<Creature>();
        InitializePopulation();
        isInitialized = true;
        StartBreeding();
    }

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
            }
        }

        if (population.Count >= maxPopSize) StopBreeding();
    }

    private void InitializePopulation()
    {
        for (int i = 0; i < initialPopSize; i++)
        {
            Creature creature = new Creature(creatureGenerator);
            creature.DisplayCreature(startingPosition, spawningAreaAngle, spawningAreaSize);
            population.Add(creature);
        }
    }

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

    private List<Creature> GetPossibleParentsList()
    {
        List<Creature> possibleParents = new List<Creature>();

        foreach (var creature in population)
        {
            float timeToLive = creature.dna.timeToLive; // TODO: should depend on health

            if (timeToLive > 15) possibleParents.Add(creature);
        }

        return possibleParents;
    }

    // /!\ PARENT SELECTION SHOULD BEE UPDATED LATER ON /!\
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

    private Creature Breed(DNA dna1, DNA dna2)
    {
        DNA offspringDNA = CombineDNA(dna1, dna2);
        offspringDNA.Mutate(mutationRate);

        return new Creature(offspringDNA, creatureGenerator);
    }

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
        offspringDNA.health = (uint)Random.Range(1.0f, 10.0f); // TODO: use health for something
        offspringDNA.timeToLive = Random.Range(5.0f, 30.0f);
        return offspringDNA;
    }
}
