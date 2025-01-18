using UnityEngine;

public class Creature
{
    public DNA dna;
    public CreatureGenerator creatureGenerator;
    public FitnessLevel fitness;
    public GameObject model;

    public Creature(CreatureGenerator generator)
    {
        dna = new DNA();
        creatureGenerator = generator;
        fitness = EvaluateFitness();
        model = creatureGenerator.GenerateModel(this);
    }

    public Creature(DNA generatedDNA, CreatureGenerator generator)
    {
        dna = generatedDNA;
        creatureGenerator = generator;
        EvaluateFitness();
        model = creatureGenerator.GenerateModel(this);
    }

    public void DisplayCreature(Vector3 startingPosition, Vector2 spawningAreaAngle, Vector2 spawningAreaSize)
    {
        model.transform.position = startingPosition + GetRandomPosition(spawningAreaAngle, spawningAreaSize);
        AdjustToTerrain();
    }

    private Vector3 GetRandomPosition(Vector2 spawningAreaAngle, Vector2 spawningAreaSize)
    {
        var vec = new Vector3(
            spawningAreaAngle.x + Random.Range(0, spawningAreaSize.x),
            0,
            spawningAreaAngle.y + Random.Range(0, spawningAreaSize.y));

        vec.y = Terrain.activeTerrain.SampleHeight(vec);
        return vec;
    }

    private void AdjustToTerrain()
    {
        Vector3 position = model.transform.position;

        Vector3 terrainNormal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal(
            position.x / Terrain.activeTerrain.terrainData.size.x,
            position.z / Terrain.activeTerrain.terrainData.size.z);

        model.transform.rotation = Quaternion.FromToRotation(Vector3.up, terrainNormal);
    }

    // /!\ FITNESS EVALUATION SHOULD BE UPDATED LATER ON /!\
    private FitnessLevel EvaluateFitness()
    {
        if (dna.blue >= 0.6f && dna.bodyShape == Shape.Cube)
        {
            return FitnessLevel.Best;
        }
        else if (dna.blue >= 0.3f && dna.blue < 0.6f && dna.headShape == Shape.Sphere)
        {
            return FitnessLevel.Good;
        }
        else if (dna.blue >= 0.1f && dna.blue < 0.3f)
        {
            return FitnessLevel.NotBad;
        }
        else
        {
            return FitnessLevel.Poor;
        }
    }

    // private void EvaluateFitness()
    // {
    //     fitness = 0f;
    //     fitness += EvaluateColor();
    //     fitness += EvaluateScale();
    //     fitness += EvaluateTentaclesWidth();
    //     fitness += EvaluateTentaclesNumber();
    //     fitness += EvaluateCourbe();
    // }

    // private float EvaluateColor()
    // {
    //     int colorBits = Utils.BitToInt(genome[0], genome[1]);
    //     switch (colorBits)
    //     {
    //         case (3): return 1.75f;
    //         case (2): return 2.5f;
    //         case (1): return 2.25f;
    //         case (0): return 3.5f;
    //         default: return 0;
    //     }
    // }

    // private float EvaluateScale()
    // {
    //     int scaleBits = Utils.BitToInt(genome[6], genome[7]);
    //     switch (scaleBits)
    //     {
    //         case (3): return 1.5f;
    //         case (2): return 2.75f;
    //         case (1): return 3.25f;
    //         case (0): return 2.5f;
    //         default: return 0;
    //     }
    // }

    // private float EvaluateTentaclesWidth()
    // {
    //     int tentaclesWidthBits = Utils.BitToInt(genome[2], genome[3]);
    //     switch (tentaclesWidthBits)
    //     {
    //         case (3): return 1.5f;
    //         case (2): return 3f;
    //         case (1): return 2.5f;
    //         case (0): return 2f;
    //         default: return 0;
    //     }
    // }

    // private float EvaluateTentaclesNumber()
    // {
    //     int legsBits = Utils.BitToInt(genome[4], genome[5]);
    //     switch (legsBits)
    //     {
    //         case (3): return 1.75f;
    //         case (2): return 3f;
    //         case (1): return 3.5f;
    //         case (0): return 1.75f;
    //         default: return 0;
    //     }
    // }

    // private float EvaluateCourbe()
    // {
    //     int courbeBits = Utils.BitToInt(genome[8], genome[9]);
    //     switch (courbeBits)
    //     {
    //         case (3): return 3f;
    //         case (2): return 1.5f;
    //         case (1): return 2.5f;
    //         case (0): return 3.5f;
    //         default: return 0;
    //     }
    // }
}
