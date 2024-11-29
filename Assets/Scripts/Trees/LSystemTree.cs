using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSystemTree : MonoBehaviour
{
    [Header("L-System Settings")]
    public string axiom = "F"; // Starting point of the system
    public int iterations = 5; // Number of iterations of the system
    public float angle = 25.0f; // Branching angle
    public float length = 1.0f; // Initial branch length

    [Header("Branch Settings")]
    public Material branchMaterial; // Material for the branches


    [Header("Leaf Settings")]
    public Texture2D leafTexture; // Texture for the leaves
    public float leafScaleMin = 0.8f; // Minimum leaf scale
    public float leafScaleMax = 1.2f; // Maximum leaf scale

    [Header("Leaf Shape")]
    public float leafAspectRatio = 0.6f; // Height-to-width ratio for the leaves
    public float leafRotationRange = 30f; // Random variation in leaf rotation

    [Header("L-System Rules")]
    public List<Rule> rules = new List<Rule>(); // List of rules configurable from the Inspector

    [Header("Branch Thickness Settings")]
    public float initialThickness = 0.3f; // Initial thickness of the trunk
    public float thicknessReduction = 0.7f; // Thickness reduction factor per level


    private string currentString;

    void Start()
    {
        // Generate the system based on the axiom
        currentString = GenerateLSystem(axiom, iterations);

        // Draw the tree
        DrawTree();
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
                // Replace the character according to the defined rules
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
                    next += c.ToString(); // If no rule exists, keep the original character
                }
            }
            result = next;
        }
        return result;
    }

    void DrawTree()
    {
        Stack<TransformInfo> transformStack = new Stack<TransformInfo>();
        Stack<float> thicknessStack = new Stack<float>(); // Stack to track the current thickness
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;
        float currentThickness = initialThickness; // Start with the initial thickness

        foreach (char c in currentString)
        {
            if (c == 'F')
            {
                // Calculate the start and end points of the branch
                Vector3 start = position;
                Vector3 end = start + (rotation * Vector3.up * length);

                // Create a branch as a cylinder
                GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                branch.transform.position = (start + end) / 2; // Position at the center of the line
                branch.transform.up = end - start; // Orientation towards the endpoint
                branch.transform.localScale = new Vector3(currentThickness, Vector3.Distance(start, end) / 2, currentThickness); // Scale proportional to the branch length

                // Assign the brown material to the branch
                if (branchMaterial != null)
                {
                    branch.GetComponent<Renderer>().material = branchMaterial;
                }

                // Update the position for the next branch
                position = end;
            }
            else if (c == '+')
            {
                rotation *= Quaternion.Euler(0, 0, angle); // Positive rotation in Z
            }
            else if (c == '-')
            {
                rotation *= Quaternion.Euler(0, 0, -angle); // Negative rotation in Z
            }
            else if (c == '&')
            {
                rotation *= Quaternion.Euler(angle, 0, 0); // Positive rotation in X
            }
            else if (c == '^')
            {
                rotation *= Quaternion.Euler(-angle, 0, 0); // Negative rotation in X
            }
            else if (c == '<')
            {
                rotation *= Quaternion.Euler(0, angle, 0); // Positive rotation in Y
            }
            else if (c == '>')
            {
                rotation *= Quaternion.Euler(0, -angle, 0); // Negative rotation in Y
            }
            else if (c == '[')
            {
                // Save the current position, rotation, and thickness
                transformStack.Push(new TransformInfo(position, rotation));
                thicknessStack.Push(currentThickness);

                // Reduce the thickness for child branches
                currentThickness *= thicknessReduction;
            }
            else if (c == ']')
            {
                // Restore the saved position, rotation, and thickness
                TransformInfo t = transformStack.Pop();
                position = t.Position;
                rotation = t.Rotation;
                currentThickness = thicknessStack.Pop();

                // Create a leaf at the branching point
                CreateLeaf(position, rotation);
            }
        }
    }


    void CreateLeaf(Vector3 position, Quaternion rotation)
    {
        // Create a sphere for the leaf, which will be scaled to make it oval
        GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        // Adjust the position and orientation of the leaf
        leaf.transform.position = position; // At the end position of the branch
        leaf.transform.rotation = rotation; // With the same orientation as the branch

        // Apply scaling with manual aspect ratio
        float randomScaleX = Random.Range(leafScaleMin, leafScaleMax);
        float randomScaleY = randomScaleX * leafAspectRatio; // Use the parameter configured in the Inspector
        leaf.transform.localScale = new Vector3(randomScaleX, randomScaleY, 1f); // Scale in X and Y for an oval shape

        // Apply random rotation
        leaf.transform.Rotate(Random.Range(-leafRotationRange, leafRotationRange), Random.Range(-leafRotationRange, leafRotationRange), 0);

        // Change the material to apply the leaf texture
        if (leafTexture != null)
        {
            Material leafMaterial = new Material(Shader.Find("Standard"));
            leafMaterial.mainTexture = leafTexture;
            leafMaterial.color = Color.green; // Adjust a green tint if there is no texture
            leaf.GetComponent<Renderer>().material = leafMaterial;
        }
        else
        {
            // If there is no texture, use a default green color
            leaf.GetComponent<Renderer>().material.color = Color.green;
        }
    }

    [System.Serializable]
    public class Rule
    {
        public char symbol; // Rule symbol (e.g., 'F')
        public string replacement; // Rule replacement (e.g., "F[+F]F[-F]F")
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
