using UnityEngine;

public class DNA
{
    public Shape bodyShape;             // creature shapes
    public Shape headShape;
    public Shape earShape;
    public float size;                  // creature size
    public float red;                   // creature color
    public float green;
    public float blue;
    public int earNumber;               // creature number of ears
    public float health = 100f;         // creature health
    public float timeToLive;            // creature life span

    public bool isHealthy => health > 50;

    public DNA()
    {
        bodyShape = GetRandomShape();
        headShape = GetRandomShape();
        earShape = GetRandomShape();
        size = Random.Range(0.5f, 2.0f);
        red = Random.Range(0.0f, 1.0f);
        green = Random.Range(0.0f, 1.0f);
        blue = Random.Range(0.0f, 1.0f);
        earNumber = Random.Range(0, 4);
        health = Random.Range(80.0f, 100.0f);
        timeToLive = Mathf.Lerp(5f, 20f, health / 100f);
    }

    public void Mutate(float mutationRate)
    {
        if (Random.Range(0.0f, 1.0f) < mutationRate)
        {
            size = Random.Range(1.0f, 5.0f);
            red = Random.Range(0.0f, 1.0f);
            green = Random.Range(0.0f, 1.0f);
            blue = Random.Range(0.0f, 1.0f);
            earNumber = Random.Range(0, 4);
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
