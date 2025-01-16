using UnityEngine;

public class CreatureGenerator : MonoBehaviour
{
    public GameObject eyePrefab;
    public GameObject spherePrefab;
    public GameObject cubePrefab;
    public GameObject capsulePrefab;

    private Vector3 initialPosition = Vector3.zero;

    public GameObject GenerateModel(Creature creature)
    {
        GameObject creatureModel = new GameObject("Creature");
        creatureModel.transform.position = initialPosition;

        GameObject prefab = GetPrefabByName(creature.dna.shape);

        Vector3 headPosition = CreateBody(prefab, initialPosition, creatureModel, creature);
        CreateEyes(eyePrefab, headPosition, creatureModel, creature);

        return creatureModel;
    }

    private Vector3 CreateBody(GameObject prefab, Vector3 initialPosition, GameObject creatureModel, Creature creature)
    {
        float scaleFactor = creature.scaleFactor;
        Color creatureColor = creature.color;

        // Tailles des sphères du corps basées sur le scaleFactor contenu dans le genome
        float baseSize = 1f * scaleFactor;
        float largeSize = 0.8f * scaleFactor;
        // float smallerSize = 0.7f * scaleFactor;
        // float topSize = 0.3f * scaleFactor;

        // Positionement de chaque sphère
        Vector3 basePosition = initialPosition;
        Vector3 largePosition = basePosition + new Vector3(0, (baseSize / 2 + largeSize / 2) * 0.8f, 0);
        // Vector3 smallerPosition = largePosition + new Vector3(0.2f, (largeSize / 2 + smallerSize / 2) * 0.8f, 0);
        // Vector3 topPosition = smallerPosition + new Vector3(0.2f, (smallerSize / 2 + topSize / 2) * 0.8f, 0);

        // Création des sphères du corps
        CreateSphere(prefab, basePosition, baseSize, creatureColor, creatureModel);
        CreateSphere(prefab, largePosition, largeSize, creatureColor, creatureModel);
        // CreateSphere(prefab, smallerPosition, smallerSize, creatureColor, creatureModel);
        // CreateSphere(prefab, topPosition, topSize, creatureColor, creatureModel);

        return largePosition;
    }

    private void CreateSphere(GameObject prefab, Vector3 position, float size, Color color, GameObject creatureModel)
    {
        GameObject bodySphere = Instantiate(prefab, position, Quaternion.identity);
        bodySphere.transform.localScale = new Vector3(size, size, size);
        bodySphere.GetComponent<Renderer>().material.color = color;
        bodySphere.transform.SetParent(creatureModel.transform);
    }

    private void CreateEyes(GameObject eyePrefab, Vector3 headPosition, GameObject creatureModel, Creature creature)
    {
        float scaleFactor = creature.scaleFactor;
        float largeSize = 1.2f * scaleFactor;
        float size = 0.3f * scaleFactor;

        // Left eye
        Vector3 leftEyePosition = headPosition + new Vector3(-largeSize / 6, 0.1f * scaleFactor, largeSize / 2 - 0.1f * scaleFactor);
        GameObject leftEye = Instantiate(eyePrefab, leftEyePosition, Quaternion.identity);
        leftEye.transform.localScale = new Vector3(size, size, size);
        leftEye.transform.SetParent(creatureModel.transform);

        // Right eye
        Vector3 rightEyePosition = headPosition + new Vector3(largeSize / 6, 0.1f * scaleFactor, largeSize / 2 - 0.1f * scaleFactor);
        GameObject rightEye = Instantiate(eyePrefab, rightEyePosition, Quaternion.identity);
        rightEye.transform.localScale = new Vector3(size, size, size);
        rightEye.transform.SetParent(creatureModel.transform);
    }

    private GameObject GetPrefabByName(Shape shapeName)
    {
        if (shapeName == Shape.Cube)
            return cubePrefab;
        else if (shapeName == Shape.Sphere)
            return spherePrefab;
        else // if shapeName == Shape.Capsule
            return capsulePrefab;
    }
}
