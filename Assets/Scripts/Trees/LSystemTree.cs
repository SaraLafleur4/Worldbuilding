using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSystemTree : MonoBehaviour
{
    [Header("Forest Settings")]
    public int numberOfTrees = 5; // Número de árboles a generar
    public Vector2 planePos;
    public Vector2 planeSize = new Vector2(10, 10); // Tamaño del área de generación (X y Z)

    [Header("L-System Settings")]
    public string axiom = "F"; // Starting point of the system
    public int iterations = 3; // Number of iterations of the system
    public float angle = 25.0f; // Branching angle
    public float length = 1.0f; // Initial branch length

    [Header("Branch Settings")]
    public Material branchMaterial; // Material for the branches

    [Header("Leaf Settings")]
    public Texture2D leafTexture; // Texture for the leaves
    public Material leafMaterial; // Material for the leaves
    public float leafScaleMin = 0.2f; // Minimum leaf scale
    public float leafScaleMax = 0.4f; // Maximum leaf scale

    [Header("Leaf Shape")]
    public float leafAspectRatio = 0.2f; // Height-to-width ratio for the leaves
    public float leafRotationRange = 45f; // Random variation in leaf rotation

    [Header("L-System Rules")]
    public List<Rule> rules = new List<Rule>(); // List of rules configurable from the Inspector

    [Header("Branch Thickness Settings")]
    public float initialThickness = 0.5f; // Initial thickness of the trunk
    public float thicknessReduction = 0.4f; // Thickness reduction factor per level

    private string currentString;
    private GameObject branchParent; // Parent object for all branches
    private GameObject leafParent; // Parent object for all leaves

    public void GenerateForest() {
        if (branchParent != null) Destroy(branchParent);
        if (leafParent != null) Destroy(leafParent);

        branchParent = new GameObject("Branches");
        leafParent = new GameObject("Leaves");

        for (int i = 0; i < numberOfTrees; i++) {
            var randomPosition = new Vector3(
                Random.Range(0, planeSize.x + planePos.y),
                0,
                Random.Range(0, planeSize.y + planePos.x)
            );

            if (Terrain.activeTerrain != null) randomPosition.y = Terrain.activeTerrain.SampleHeight(randomPosition);

            GenerateSingleTree(randomPosition);
        }

        CombineMeshes(branchParent, branchMaterial);
        CombineMeshes(leafParent, leafMaterial);
    }

    void GenerateSingleTree(Vector3 position) {
        iterations = Random.Range(1, 5);
        angle = Random.Range(10.0f, 35.0f);

        List<Rule[]> possibleRules = new List<Rule[]> {
            new Rule[] { new Rule { symbol = 'F', replacement = "F[+F]F[-F]F" } },
            new Rule[] { new Rule { symbol = 'F', replacement = "FF+[+F-F-F]-[-F+F+F]" } },
            new Rule[] { new Rule { symbol = 'F', replacement = "F[+F&F][-F^F]" } }
        };

        // Elegir una regla aleatoriamente
        Rule[] chosenRules = possibleRules[Random.Range(0, possibleRules.Count)];
        rules = new List<Rule>(chosenRules);

        // Generar el sistema basado en el axiom y las reglas seleccionadas
        currentString = GenerateLSystem(axiom, iterations);

        // Dibujar el árbol
        DrawTreeAt(position);
    }


    string GenerateLSystem(string axiom, int iterations)
    {
        string result = axiom;
        for (int i = 0; i < iterations; i++)
        {
            string next = "";
            foreach (char c in result)
            {
                bool replaced = false;
                // Reemplazar el carácter según las reglas definidas
                foreach (var rule in rules)
                {
                    if (c == rule.symbol)
                    {
                        next += rule.replacement;
                        replaced = true;
                        break;
                    }
                }
                if (!replaced)
                {
                    next += c.ToString(); // Si no hay regla, mantener el carácter original
                }
            }
            result = next;
        }
        return result;
    }

    void DrawTreeAt(Vector3 basePosition)
    {
        Stack<TransformInfo> transformStack = new Stack<TransformInfo>();
        Stack<float> thicknessStack = new Stack<float>();
        Vector3 position = basePosition; // Empezar en la posición base
        Quaternion rotation = Quaternion.identity;
        float currentThickness = initialThickness;

        foreach (char c in currentString) {
            switch (c) {
            case 'F':
                Vector3 start = position;
                Vector3 end = start + (rotation * Vector3.up * length);

                GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                branch.transform.parent = branchParent.transform;
                branch.transform.position = (start + end) / 2;
                branch.transform.up = end - start;
                branch.transform.localScale = new Vector3(currentThickness, Vector3.Distance(start, end) / 2, currentThickness);

                if (branchMaterial != null)
                    branch.GetComponent<Renderer>().material = branchMaterial;

                position = end;
            break;
            case '+':
                rotation *= Quaternion.Euler(0, 0, angle);
            break;
            case '-':
                rotation *= Quaternion.Euler(0, 0, -angle);
            break;
            case '&':
                rotation *= Quaternion.Euler(angle, 0, 0);
            break;
            case '^':
                rotation *= Quaternion.Euler(-angle, 0, 0);
            break;
            case '<':
                rotation *= Quaternion.Euler(0, angle, 0);
            break;
            case '>':
                rotation *= Quaternion.Euler(0, -angle, 0);
            break;
            case '[':
                transformStack.Push(new TransformInfo(position, rotation));
                thicknessStack.Push(currentThickness);
                currentThickness *= thicknessReduction;
            break;
            case ']':
                TransformInfo t = transformStack.Pop();
                position = t.Position;
                rotation = t.Rotation;
                currentThickness = thicknessStack.Pop();

                CreateLeaf(position, rotation);
            break;
            }
        }
    }


    void CreateLeaf(Vector3 position, Quaternion rotation) {
        // Crear una esfera para la hoja
        GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leaf.transform.parent = leafParent.transform; // Asignar al objeto padre de las hojas

        leaf.transform.position = position;
        leaf.transform.rotation = rotation;

        // Aplicar escalado con aspecto aleatorio
        float randomScaleX = Random.Range(leafScaleMin, leafScaleMax);
        float randomScaleY = randomScaleX * leafAspectRatio; // Usar el parámetro configurado en el Inspector
        leaf.transform.localScale = new Vector3(randomScaleX, randomScaleY, 1f); // Escalar en X y Y para forma ovalada

        leaf.transform.Rotate(Random.Range(-leafRotationRange, leafRotationRange), Random.Range(-leafRotationRange, leafRotationRange), 0);

        if (leafMaterial != null)
            leaf.GetComponent<Renderer>().material = leafMaterial;
        else if (leafTexture != null) {
            Material material = new Material(Shader.Find("Standard"));
            material.mainTexture = leafTexture;
            material.color = Color.green; // Ajustar un tinte verde si no hay textura
            leaf.GetComponent<Renderer>().material = material;
        } else
            leaf.GetComponent<Renderer>().material.color = Color.green;
    }

    void CombineMeshes(GameObject parent, Material material) {
        if (parent.transform.childCount == 0) return;

        MeshCombiner meshCombiner = gameObject.GetComponent<MeshCombiner>();
        if (meshCombiner == null) gameObject.AddComponent<MeshCombiner>();

        meshCombiner.CombineMeshes(parent.transform, material);
    }

    [System.Serializable]
    public struct Rule
    {
        public char symbol; // Símbolo de la regla (ejemplo: 'F')
        public string replacement; // Reemplazo de la regla (ejemplo: "F[+F]F[-F]F")
    }

    private struct TransformInfo
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public TransformInfo(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }
}
