using UnityEngine;

public class PropsManager : MonoBehaviour
{
    public Forest forest;
    public Rocks rocks;

    // Method triggered bu the "Propspulate" button
    // Generates all props (Trees and Rocks)
    // All props are cleared before a new generation
    public void Generate()
    {
        forest.GenerateForest();
        rocks.GenerateRocks();
    }
}
