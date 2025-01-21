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
    public Vector2 planePos = new Vector2(10, 10);
    public Vector2 planeSize = new Vector2(500, 500);

    private List<AiTree> originalTrees = new List<AiTree>();
    public LSystem lSystem;
    private List<GameObject> parentTrees = new List<GameObject>();

    public void GenerateForest()
    {
        ClearForest();

        for (int i = 0; i < numberOfOriginalTrees; i++) {
            GenerateSingleOriginalTree(i + 1);
        }

        foreach (var tree in originalTrees)
        {
            // Parent the original tree's components under the new "OriginalTreeX" GameObject
            GameObject originalParent = new GameObject("OriginalTree" + (originalTrees.IndexOf(tree) + 1));
            foreach (var obj in tree.GetTreeMesh())
                obj.transform.SetParent(originalParent.transform);

            parentTrees.Add(originalParent);

            for (int i = 0; i < 20; i++)
                GenerateDuplicateTreeFromOriginal(tree.GetTreeMesh(), originalTrees.IndexOf(tree) + 1, i + 1);
        }
    }

    private void GenerateSingleOriginalTree(int treeNb)
    {
        lSystem.SetupLSystemForSingleTree();

        AiTree tree = gameObject.AddComponent<AiTree>();
        tree.SetMaterials(branchMaterial, leafMaterial);
        tree.Generate(
            planePos,
            planeSize,
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
            planePos.x + Random.Range(0, planeSize.x),
            0,
            planePos.y + Random.Range(0, planeSize.y)
        );
        if (Terrain.activeTerrain != null) newTreePosition.y = Terrain.activeTerrain.SampleHeight(newTreePosition);
        var newTreeRotation = new Vector3(
            0,
            Random.Range(0, 360),
            0
        );

        foreach (var obj in originalTreeMesh) {
            GameObject duplicatedObj = Instantiate(obj);
            duplicatedObj.transform.position = newTreePosition;
            duplicatedObj.transform.eulerAngles = newTreeRotation;
            newTreeComponents.Add(duplicatedObj);
        }

        GameObject duplicateParent = new GameObject("OriginalTree" + originalTreeNb + "-Duplicate" + duplicateTreeNb);
        foreach (var obj in newTreeComponents)
            obj.transform.SetParent(duplicateParent.transform); // Set the parent to the duplicate tree parent

        parentTrees.Add(duplicateParent);
    }

    private void ClearForest()
    {
        if (originalTrees != null)
        {
            foreach (var tree in originalTrees) tree.DestroyTree();
            originalTrees.Clear();
        }
        if (parentTrees != null) {
            foreach (var tree in parentTrees) Destroy(tree);
        }
    }
}
