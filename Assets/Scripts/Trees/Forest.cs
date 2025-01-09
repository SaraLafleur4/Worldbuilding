using System.Collections.Generic;
using UnityEngine;

public class Forest : MonoBehaviour
{
    [Header("Branch Settings")]
    public Material branchMaterial;

    [Header("Leaf Settings")]
    public Texture2D leafTexture;
    public Material leafMaterial;

    [Header("Forest Settings")]
    public int numberOfOriginalTrees = 10;
    public Vector2 planePos = new Vector2(250, 250);
    public Vector2 planeSize = new Vector2(10, 10);

    private List<AiTree> originalTrees = new List<AiTree>();
    public LSystem lSystem;
    private List<List<GameObject>> allTrees = new List<List<GameObject>>();

    public void GenerateForest()
    {
        // Clear existing forest ...
        ClearForest();

        // ... before generating a new one

        // First use the LSystem algorithm to generate the original AiTrees
        for (uint i = 0; i < numberOfOriginalTrees; i++)
        {
            GenerateSingleOriginalTree(i + 1); // treeNb should start at 1, not 0
        }

        // Then duplicate each original tree x10 to create the forest
        foreach (var tree in originalTrees)
        {
            // Add the original tree to the tree list
            allTrees.Add(tree.GetTreeMesh());

            for (uint i = 0; i < 20; i++)
            {
                List<GameObject> newTreeComponents = new List<GameObject>();

                var newTreePosition = new Vector3(
                    Random.Range(0, planeSize.x + planePos.y),
                    0,
                    Random.Range(0, planeSize.y + planePos.x)
                );
                var newTreeRotation = new Vector3(
                    0,
                    Random.Range(0, 360),
                    0
                );

                // Duplicate the branches and leaves
                // Set them to the new position and new rotation
                // Add them to the tree components list
                foreach (var obj in tree.GetTreeMesh())
                {
                    GameObject duplicatedObj = Instantiate(obj);
                    duplicatedObj.transform.position = newTreePosition;
                    duplicatedObj.transform.eulerAngles = newTreeRotation;
                    newTreeComponents.Add(duplicatedObj);
                }

                // Add the new tree to the tree list
                allTrees.Add(newTreeComponents);
            }
        }
    }

    void GenerateSingleOriginalTree(uint treeNb)
    {
        // L-System settings
        lSystem.SetupLSystemForSingleTree();

        // Tree generation
        AiTree tree = gameObject.AddComponent<AiTree>();
        tree.SetMaterials(branchMaterial, leafMaterial);
        tree.Generate(new Vector2(planeSize.x + planePos.y, planeSize.y + planePos.x),
            lSystem.getCurrentString(),
            lSystem.length,
            lSystem.angle,
            treeNb
        );
        originalTrees.Add(tree);
    }

    void ClearForest()
    {
        // Destroy original AiTrees
        if (originalTrees != null)
        {
            foreach (var tree in originalTrees)
            {
                tree.DestroyTree();
            }
            originalTrees.Clear();
        }
        // Destroy mesh for all trees
        if (allTrees != null)
        {
            foreach (var tree in allTrees)
            {
                foreach (var obj in tree)
                {
                    Destroy(obj);
                }
            }
            allTrees.Clear();
        }
    }
}
