using UnityEngine;

public class CreatureGenerator : MonoBehaviour
{
    public GameObject eyePrefab;
    public GameObject spherePrefab;
    public GameObject cubePrefab;
    public GameObject capsulePrefab;

    private Vector3 initialPosition = Vector3.zero;

    private float scaleFactor;
    private Color creatureColor;
    private float bodySize;
    private float headSize;
    private float earSize;
    private float eyeSize;
    private int earNumber;

    public GameObject GenerateModel(Creature creature)
    {
        GameObject creatureModel = new GameObject("Creature");
        creatureModel.transform.position = initialPosition;

        GameObject prefab = GetPrefabByName(creature.dna.shape);

        InitializeVariables(creature);

        Vector3 headPosition = CreateCreatureModel(prefab, initialPosition, creatureModel);
        CreateEyes(eyePrefab, headPosition, creatureModel);

        return creatureModel;
    }

    private void InitializeVariables(Creature creature)
    {
        scaleFactor = creature.dna.size;
        bodySize = 1f * scaleFactor;
        headSize = 0.8f * scaleFactor;
        earSize = 0.2f * scaleFactor;
        eyeSize = 0.25f * scaleFactor;

        creatureColor = new Color(creature.dna.red, creature.dna.green, creature.dna.blue);

        earNumber = creature.dna.earNumber;
    }

    private Vector3 CreateCreatureModel(GameObject prefab, Vector3 initialPosition, GameObject creatureModel)
    {
        Vector3 basePosition = initialPosition;
        Vector3 headPosition = basePosition + new Vector3(0, (bodySize / 2 + headSize / 2) * 0.8f, 0);
        if (prefab == capsulePrefab) headPosition.y += headPosition.y / 2;
        Vector3 earPosition = headPosition + new Vector3(0, (headSize / 2 + earSize / 2) * 0.8f, 0);
        if (prefab == capsulePrefab) earPosition.y += earPosition.y / 4;

        CreateBodyPart(prefab, basePosition, bodySize, creatureColor, creatureModel);
        CreateBodyPart(prefab, headPosition, headSize, creatureColor, creatureModel);
        CreateEars(prefab, earPosition, creatureModel);

        return headPosition;
    }

    private void CreateEars(GameObject prefab, Vector3 position, GameObject creatureModel)
    {
        var earOffset = new Vector3(headSize / 3, 0, 0);

        switch (earNumber)
        {
            case 1:
                CreateBodyPart(prefab, position, earSize, creatureColor, creatureModel);
                break;
            case 2:
                CreateBodyPart(prefab, position + earOffset, earSize, creatureColor, creatureModel);
                CreateBodyPart(prefab, position - earOffset, earSize, creatureColor, creatureModel);
                break;
            case 3:
                CreateBodyPart(prefab, position, earSize, creatureColor, creatureModel);
                CreateBodyPart(prefab, position + earOffset, earSize, creatureColor, creatureModel);
                CreateBodyPart(prefab, position - earOffset, earSize, creatureColor, creatureModel);
                break;
            default:
                break;
        }
    }

    private void CreateBodyPart(GameObject prefab, Vector3 position, float size, Color color, GameObject creatureModel)
    {
        GameObject body = Instantiate(prefab, position, Quaternion.identity);
        body.transform.localScale = new Vector3(size, size, size);
        body.GetComponent<Renderer>().material.color = color;
        body.transform.SetParent(creatureModel.transform);
    }

    private void CreateEyes(GameObject eyePrefab, Vector3 headPosition, GameObject creatureModel)
    {
        // Left eye
        Vector3 leftEyePosition = headPosition + new Vector3(-headSize / 6, 0.1f * scaleFactor, headSize / 2 - 0.1f * scaleFactor);
        GameObject leftEye = Instantiate(eyePrefab, leftEyePosition, Quaternion.identity);
        leftEye.transform.localScale = new Vector3(eyeSize, eyeSize, eyeSize);
        leftEye.transform.SetParent(creatureModel.transform);

        // Right eye
        Vector3 rightEyePosition = headPosition + new Vector3(headSize / 6, 0.1f * scaleFactor, headSize / 2 - 0.1f * scaleFactor);
        GameObject rightEye = Instantiate(eyePrefab, rightEyePosition, Quaternion.identity);
        rightEye.transform.localScale = new Vector3(eyeSize, eyeSize, eyeSize);
        rightEye.transform.SetParent(creatureModel.transform);
    }

    private GameObject GetPrefabByName(Shape shapeName)
    {
        switch (shapeName)
        {
            case Shape.Cube: return cubePrefab;
            case Shape.Sphere: return spherePrefab;
            case Shape.Capsule: return capsulePrefab;
            default: return cubePrefab;
        }
    }
}
