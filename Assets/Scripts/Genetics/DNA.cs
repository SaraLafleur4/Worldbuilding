using UnityEngine;

public class DNA : MonoBehaviour
{
    // /!\ DEFINE GENES HERE /!\
    public float one;
    public float two;
    public float three;

    // Enum representing the fitness level of the creature
    public FitnessLevel fitnessLevel;

    // Cached reference to the MeshRenderer component
    private MeshRenderer meshRenderer;

    // DEFINE METHODS HERE
}

// Enum representing different fitness levels for creatures
public enum FitnessLevel
{
    Best,
    Good,
    NotBad,
    Poor
}
