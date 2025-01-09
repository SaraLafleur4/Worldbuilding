using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forest : MonoBehaviour
{
    [Header("Branch Settings")]
    public Material branchMaterial;
    [Header("Leaf Settings")]
    public Texture2D leafTexture;
    public Material leafMaterial;

    public int numberOfTrees = 10;
    public Vector2 planePos = new Vector2(250, 250);
    public Vector2 planeSize = new Vector2(10, 10);

    private List<Tree> trees = new List<Tree>();
    public LSystem lSystem;

    public void GenerateForest()
    {
        // Clear existing forest ...
        if (trees != null)
        {
            foreach (var tree in trees)
            {
                tree.DestroyTree();
            }
            trees.Clear();
        }
        // ... before generating a new one
        for (int i = 0; i < numberOfTrees; i++)
        {
            GenerateSingleTree();
        }
    }

    void GenerateSingleTree()
    {
        // L-System settings
        lSystem.SetupLSystemForSingleTree();

        // Tree generation
        Tree tree = gameObject.AddComponent<Tree>();
        tree.SetMaterials(branchMaterial, leafMaterial);
        tree.Generate(new Vector2(planeSize.x + planePos.y, planeSize.y + planePos.x),
            lSystem.getCurrentString(),
            lSystem.length,
            lSystem.angle
        );
        trees.Add(tree);
    }
}
