using UnityEngine;

public class DNA
{
    public Shape shape;                 // creature shape
    public float size;                  // creature size
    public float red;                   // creature color
    public float green;
    public float blue;
    public float health;                // creature health
    public float timeToLive;            // creature life span

    public bool isHealthy() => health > 0; // TODO: use health for something

    public DNA()
    {
        shape = GetRandomShape();
        size = Random.Range(0.0f, 1.0f);
        red = Random.Range(0.0f, 1.0f);
        green = Random.Range(0.0f, 1.0f);
        blue = Random.Range(0.0f, 1.0f);
        health = (uint)Random.Range(1.0f, 10.0f); // TODO: use health for something
        timeToLive = Random.Range(5.0f, 30.0f);
    }

    public void Mutate(float mutationRate)
    {
        if (Random.Range(0.0f, 1.0f) < mutationRate)
        {
            size = Random.Range(0.0f, 5.0f);
            red = Random.Range(0.0f, 1.0f);
            green = Random.Range(0.0f, 1.0f);
            blue = Random.Range(0.0f, 1.0f);
        }
    }

    private Shape GetRandomShape()
    {
        int value = Random.Range(1, 12);

        if (value < 5)
            return Shape.Cube;
        else if (value > 4 && value < 9)
            return Shape.Sphere;
        else // if value > 8
            return Shape.Capsule;
    }
}
