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
    private List<GameObject> parentTrees = new List<GameObject>();

    public void GenerateForest()
    {
        // Clear existing forest
        ClearForest();

        // Generate the original trees with the LSystem algorithm
        for (int i = 0; i < numberOfOriginalTrees; i++)
        {
            GenerateSingleOriginalTree(i + 1); // treeNb should start at 1, not 0
        }

        foreach (var tree in originalTrees)
        {
            // Parent the original tree's components under the new "OriginalTreeX" GameObject
            GameObject originalParent = new GameObject("OriginalTree" + (originalTrees.IndexOf(tree) + 1));
            foreach (var obj in tree.GetTreeMesh())
            {
                obj.transform.SetParent(originalParent.transform);
            }

            // Add the original tree to the tree list
            parentTrees.Add(originalParent);

            // Duplicate the original tree 20 times
            for (int i = 0; i < 20; i++)
            {
                GenerateDuplicateTreeFromOriginal(tree.GetTreeMesh(), originalTrees.IndexOf(tree) + 1, i + 1);
            }
        }
    }

    private void GenerateSingleOriginalTree(int treeNb)
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

    private void GenerateDuplicateTreeFromOriginal(List<GameObject> originalTreeMesh, int originalTreeNb, int duplicateTreeNb)
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
        foreach (var obj in originalTreeMesh)
        {
            GameObject duplicatedObj = Instantiate(obj);
            duplicatedObj.transform.position = newTreePosition;
            duplicatedObj.transform.eulerAngles = newTreeRotation;
            newTreeComponents.Add(duplicatedObj);
        }

        // Parent the duplicated tree under the new "OriginalTreeX-DuplicateY" GameObject
        GameObject duplicateParent = new GameObject("OriginalTree" + originalTreeNb + "-Duplicate" + duplicateTreeNb);
        foreach (var obj in newTreeComponents)
        {
            obj.transform.SetParent(duplicateParent.transform); // Set the parent to the duplicate tree parent
        }

        // Add the new tree to the tree list
        parentTrees.Add(duplicateParent);
    }

    private void ClearForest()
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
        // Destroy GameObjects for all trees
        if (parentTrees != null)
        {
            foreach (var tree in parentTrees)
            {
                Destroy(tree);
            }
        }
    }
}
