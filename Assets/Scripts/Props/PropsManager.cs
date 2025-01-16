using UnityEngine;

public class PropsManager : MonoBehaviour
{
    public Forest forest;
    public Rocks rocks;

    public void Generate()
    {
        forest.GenerateForest();
        rocks.GenerateRocks();
    }
}
