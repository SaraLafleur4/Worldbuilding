using UnityEngine;

public class Creature
{
    public DNA dna;
    public GameObject model;
    private CreatureGenerator creatureGenerator;
    public FitnessLevel fitness;
    public Material creatureMaterial;

    // Constructor for creating a new creature with random DNA
    public Creature(CreatureGenerator generator)
    {
        dna = new DNA();
        creatureGenerator = generator;
        fitness = EvaluateFitness();
        model = creatureGenerator.GenerateModel(this);

        var movement = model.AddComponent<CreatureMovement>();
        movement.Initialize(this);

        AddCollider();

        creatureMaterial = model.GetComponentInChildren<Renderer>().material;
    }

    // Constructor for creating a creature with provided DNA
    public Creature(DNA generatedDNA, CreatureGenerator generator)
    {
        dna = generatedDNA;
        creatureGenerator = generator;
        EvaluateFitness();
        model = creatureGenerator.GenerateModel(this);

        var movement = model.AddComponent<CreatureMovement>();
        movement.Initialize(this);

        AddCollider();

        creatureMaterial = model.GetComponentInChildren<Renderer>().material;
    }

    // Method to add a collider and rigidbody for physics interactions
    private void AddCollider()
    {
        SphereCollider collider = model.AddComponent<SphereCollider>();
        collider.radius = Mathf.Lerp(0.5f, 2.5f, Mathf.InverseLerp(1f, 5f, dna.size)); // Adjust radius based on size
        collider.isTrigger = false; // Ensure it reacts to collisions

        Rigidbody rb = model.AddComponent<Rigidbody>();
        rb.mass = Mathf.Lerp(1f, 5f, Mathf.InverseLerp(1f, 5f, dna.size)); // Scale mass with size
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Prevent tipping over
    }

    // Method to display the creature at a specific position
    public void DisplayCreature(Vector3 startingPosition, Vector2 spawningAreaAngle, Vector2 spawningAreaSize)
    {
        model.transform.position = startingPosition + GetRandomPosition(spawningAreaAngle, spawningAreaSize);
        AdjustToTerrain();
    }

    // Method to generate a random position within a defined spawning area
    private Vector3 GetRandomPosition(Vector2 spawningAreaAngle, Vector2 spawningAreaSize)
    {
        var vec = new Vector3(
            spawningAreaAngle.x + Random.Range(0, spawningAreaSize.x),
            0,
            spawningAreaAngle.y + Random.Range(0, spawningAreaSize.y));

        vec.y = Terrain.activeTerrain.SampleHeight(vec);
        return vec;
    }

    // Adjust the creature's rotation to align with the terrain normal
    private void AdjustToTerrain()
    {
        Vector3 position = model.transform.position;

        Vector3 terrainNormal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal(
            position.x / Terrain.activeTerrain.terrainData.size.x,
            position.z / Terrain.activeTerrain.terrainData.size.z);

        model.transform.rotation = Quaternion.FromToRotation(Vector3.up, terrainNormal);
    }

    // Evaluate the creature's fitness based on various traits
    private FitnessLevel EvaluateFitness()
    {
        float totalFitness = EvaluateFitnessTotal();
        switch (totalFitness)
        {
            case > 10: return FitnessLevel.Best;
            case > 9: return FitnessLevel.Good;
            case > 7: return FitnessLevel.NotBad;
            default: return FitnessLevel.Poor;
        }
    }

    // Method to calculate the total fitness score based on individual traits
    private float EvaluateFitnessTotal()
    {
        float totalFitness = 0f;
        totalFitness += EvaluateColor();
        totalFitness += EvaluateSize();
        totalFitness += EvaluateShape();
        return totalFitness;
    }

    // Method to evaluate color fitness based on red, green, and blue components
    private float EvaluateColor()
    {
        float colorFitness = 0f;
        colorFitness += EvaluateRed();
        colorFitness += EvaluateGreen();
        colorFitness += EvaluateBlue();
        return colorFitness;
    }

    // Method to evaluate fitness based on the red component of color
    private float EvaluateRed()
    {
        switch (dna.red)
        {
            case < 0.2f: return 2f; // Low value of red gets the best fitness
            case < 0.3f: return 1.5f;
            case < 0.4f: return 1f;
            default: return 0;
        }
    }

    // Method to evaluate fitness based on the green component of color
    private float EvaluateGreen()
    {
        switch (dna.green)
        {
            case > 0.8f: return 2f; // High value of green gets the best fitness
            case > 0.7f: return 1.5f;
            case > 0.6f: return 1f;
            default: return 0;
        }
    }

    // Method to evaluate fitness based on the blue component of color
    private float EvaluateBlue()
    {
        switch (dna.blue)
        {
            case > 0.9f: return 2f; // High value of blue gets the best fitness
            case > 0.8f: return 1.5f;
            case > 0.7f: return 1f;
            default: return 0;
        }
    }

    // Method to evaluate fitness based on the size of the creature
    private float EvaluateSize()
    {
        switch (dna.size)
        {
            case > 0.8f: return 0;  // No fitness for large size
            case < 0.2f: return 0;  // No fitness for very small size
            case > 0.6f: return 1f;
            case < 0.4f: return 1f;
            default: return 2f;     // Medium size gets the best fitness
        }
    }

    // Method to evaluate fitness based on the shape of the body, head, and ears
    private float EvaluateShape()
    {
        float shapeFitness = 0f;
        if (dna.bodyShape == dna.headShape)
        {
            shapeFitness += 2f;
            if (dna.bodyShape == dna.earShape) shapeFitness += 1f; // Same shape for all body parts gets best fitness
        }
        return shapeFitness;
    }
}
