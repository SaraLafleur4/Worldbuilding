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
    public int numberOfTrees = 10;
    public Vector2 planePos = new Vector2(250, 250);
    public Vector2 planeSize = new Vector2(10, 10);

    private List<AiTree> trees = new List<AiTree>();
    public LSystem lSystem;

    public void GenerateForest()
    {
        // Clear existing forest ...
        if (trees != null)
        {
            foreach (var tree in trees)
            {
                tree.DestroyTree(); // TODO: fix
            }
            trees.Clear();
        }
        // ... before generating a new one
        for (uint i = 0; i < numberOfTrees; i++)
        {
            GenerateSingleTree(i + 1); // treeNb should start at 1, not 0
        }
    }

    void GenerateSingleTree(uint treeNb)
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
        trees.Add(tree);
    }
}
