using UnityEngine;

public class CreatureGenerator : MonoBehaviour
{
    public GameObject eyePrefab;
    public GameObject spherePrefab;
    public GameObject cubePrefab;
    public GameObject capsulePrefab;

    private Vector3 initialPosition = Vector3.zero;

    private float scaleFactor;
    private Color bodyColor;
    private Color headColor;
    private Color earColor;
    private GameObject bodyPrefab;
    private GameObject headPrefab;
    private GameObject earPrefab;
    private float bodySize;
    private float headSize;
    private float earSize;
    private float eyeSize;
    private int earNumber;

    // Method to generate the creature's model based on its DNA
    public GameObject GenerateModel(Creature creature)
    {
        GameObject creatureModel = new GameObject("Creature");
        creatureModel.transform.position = initialPosition;

        InitializeVariables(creature);
        CreateCreatureModel(initialPosition, creatureModel);

        return creatureModel;
    }

    // Method to initialize the variables based on the creature's DNA
    private void InitializeVariables(Creature creature)
    {
        bodyPrefab = GetPrefabByName(creature.dna.bodyShape);
        headPrefab = GetPrefabByName(creature.dna.headShape);
        earPrefab = GetPrefabByName(creature.dna.earShape);

        scaleFactor = creature.dna.size;
        bodySize = 1f * scaleFactor;
        headSize = 0.8f * scaleFactor;
        earSize = 0.2f * scaleFactor;
        eyeSize = 0.25f * scaleFactor;

        bodyColor = new Color(creature.dna.red, creature.dna.green, creature.dna.blue, 1f);
        headColor = new Color(creature.dna.red, creature.dna.green / 2, creature.dna.blue / 4, 1f);
        earColor = new Color(creature.dna.red / 2, creature.dna.green, creature.dna.blue, 1f);

        earNumber = creature.dna.earNumber;
    }

    // Method to build the creature model by placing body, head, ears, and eyes
    private void CreateCreatureModel(Vector3 initialPosition, GameObject creatureModel)
    {
        // Body
        Vector3 bodyPosition = initialPosition + new Vector3(0, bodySize / 2, 0);
        CreateBodyPart(bodyPrefab, bodyPosition, bodySize, bodyColor, creatureModel, true);

        // Head
        Vector3 headPosition = bodyPosition + new Vector3(0, (bodySize / 2 + headSize / 2) * 0.8f, 0);
        if (bodyPrefab == capsulePrefab) headPosition.y += headPosition.y / 2;
        CreateBodyPart(headPrefab, headPosition, headSize, headColor, creatureModel);

        // Ears
        Vector3 earPosition = headPosition + new Vector3(0, (headSize / 2 + earSize / 2) * 0.8f, 0);
        if (headPrefab == capsulePrefab)
        {
            earPosition.y += earPosition.y / 4;
            if (earPrefab == cubePrefab || earPrefab == spherePrefab)
            {
                earPosition.y += earPosition.y / 16;
            }
        }
        if (headPrefab == cubePrefab && earPrefab == spherePrefab) earPosition.y += earPosition.y / 16;
        CreateEars(earPrefab, earPosition, creatureModel);

        // Eyes
        CreateEyes(eyePrefab, headPosition, creatureModel);
    }

    // Method to create ears based on the number of ears and their positions
    private void CreateEars(GameObject prefab, Vector3 position, GameObject creatureModel)
    {
        var earOffset = new Vector3(headSize / 3, 0, 0);

        switch (earNumber)
        {
            case 1:
                CreateBodyPart(prefab, position, earSize, earColor, creatureModel);
                break;
            case 2:
                CreateBodyPart(prefab, position + earOffset, earSize, earColor, creatureModel);
                CreateBodyPart(prefab, position - earOffset, earSize, earColor, creatureModel);
                break;
            case 3:
                CreateBodyPart(prefab, position, earSize, earColor, creatureModel);
                CreateBodyPart(prefab, position + earOffset, earSize, earColor, creatureModel);
                CreateBodyPart(prefab, position - earOffset, earSize, earColor, creatureModel);
                break;
            default:
                break;
        }
    }

    // Method to instantiate a body part (body, head, ear, eye) and set its properties
    private GameObject CreateBodyPart(GameObject prefab, Vector3 position, float size, Color color, GameObject creatureModel, bool isBody = false)
    {
        GameObject body = Instantiate(prefab, position, Quaternion.identity);
        body.transform.localScale = new Vector3(size, size, size);
        body.GetComponent<Renderer>().material.color = color;
        body.transform.SetParent(creatureModel.transform);
        return body;
    }

    // Method to create the eyes of the creature
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

    // Method to get the prefab corresponding to a shape
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
