using UnityEngine;

public class DNA : MonoBehaviour
{
    // Creature genes
    public Shape shape;           // creature shape
    public float size;                  // creature size
    public float red;                   // creature color
    public float green;
    public float blue;
    public float health;                // creature health
    public float timeToLive;            // creature life span
    public FitnessLevel fitnessLevel;   // creature fitness

    // Cached reference to the MeshRenderer component
    private MeshRenderer meshRenderer;

    // Creature initialization
    public void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        SetSize();
        SetColor();
        DieNaturally();
    }

    // Sets creature size
    public void SetSize()
    {
        meshRenderer.transform.localScale += new Vector3(size, size, size);
    }

    // Sets creature color
    public void SetColor()
    {
        meshRenderer.material.color = new Color(red, green, blue);
    }

    // Checks if creature is healthy
    public bool isHealthy() => health > 0;

    // Destroys creature at the end of its life span
    public void DieNaturally()
    {
        Destroy(gameObject, timeToLive);
    }

    // Destroys creature
    public void Die()
    {
        Destroy(gameObject);
    }
}

// Enum representing different shapes
public enum Shape
{
    Cube,
    Sphere,
    Capsule
}

// Enum representing different fitness levels
public enum FitnessLevel
{
    Best,
    Good,
    NotBad,
    Poor
}
