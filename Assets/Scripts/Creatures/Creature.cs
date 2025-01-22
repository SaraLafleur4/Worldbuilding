using Palmmedia.ReportGenerator.Core.Parser.Analysis;
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

    private float EvaluateFitnessTotal()
    {
        float totalFitness = 0f;
        totalFitness += EvaluateColor();
        totalFitness += EvaluateSize();
        return totalFitness;
    }

    private float EvaluateColor()
    {
        float colorFitness = 0f;
        colorFitness += EvaluateRed();
        colorFitness += EvaluateGreen();
        colorFitness += EvaluateBlue();
        return colorFitness;
    }

    private float EvaluateRed()
    {
        switch (dna.red)
        {
            case < 0.2f: return 2f;
            case < 0.3f: return 1.5f;
            case < 0.4f: return 1f;
            default: return 0;
        }
    }

    private float EvaluateGreen()
    {
        switch (dna.green)
        {
            case > 0.8f: return 2f;
            case > 0.7f: return 1.5f;
            case > 0.6f: return 1f;
            default: return 0;
        }
    }

    private float EvaluateBlue()
    {
        switch (dna.blue)
        {
            case > 0.9f: return 2f;
            case > 0.8f: return 1.5f;
            case > 0.7f: return 1f;
            default: return 0;
        }
    }

    private float EvaluateSize()
    {
        switch (dna.size)
        {
            case > 0.8f: return 0;
            case < 0.2f: return 0;
            case > 0.6f: return 1f;
            case < 0.4f: return 1f;
            default: return 2f;
        }
    }

    private float EvaluateShape()
    {
        float shapeFitness = 0f;
        if (dna.bodyShape == dna.headShape) shapeFitness += 2f;
        if (dna.bodyShape == dna.earShape) shapeFitness += 1f;
        return shapeFitness;
    }
}
