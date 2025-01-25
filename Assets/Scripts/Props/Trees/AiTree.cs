using System.Collections.Generic;
using UnityEngine;

public class AiTree : MonoBehaviour
{
    [Header("Branch Settings")]
    public Material branchMaterial;
    [Header("Branch Thickness Settings")]
    public float initialThickness = 0.5f;
    public float thicknessReduction = 0.4f;

    [Header("Leaf Settings")]
    public Texture2D leafTexture;
    public Material leafMaterial;
    public float leafScaleMin = 0.2f;
    public float leafScaleMax = 0.4f;
    [Header("Leaf Shape")]
    public float leafAspectRatio = 0.2f;
    public float leafRotationRange = 45f;

    private GameObject branches;
    private GameObject leaves;
    private List<GameObject> treeMesh; // branches + leaves

    // Retrieves the list of generated tree meshes
    public List<GameObject> GetTreeMesh() => treeMesh;

    // Helper struct to store position and rotation information
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

    // Destroys the original branch and leaf objects
    public void DestroyOriginalTreeMesh()
    {
        Destroy(branches);
        Destroy(leaves);
    }

    // Destroys all tree meshes
    public void DestroyTree()
    {
        foreach (var mesh in treeMesh) Destroy(mesh);
    }

    // Sets the materials for branches and leaves
    public void SetMaterials(Material branch, Material leaf)
    {
        branchMaterial = branch;
        leafMaterial = leaf;
    }

    // Generates a tree based on L-System parameters
    public void Generate(Vector2 pos, Vector2 size, string lSystemString, float lSystemLength, float lSystemAngle, int treeNb)
    {
        branches = new GameObject("Branches");
        leaves = new GameObject("Leaves");

        var randomPosition = new Vector3(
            size.x + Random.Range(0, pos.x),
            0,
            size.y + Random.Range(0, pos.y)
        );
        if (Terrain.activeTerrain != null) randomPosition.y = Terrain.activeTerrain.SampleHeight(randomPosition);

        // Draws the tree and combines its meshes
        DrawTreeAt(Vector3.zero, lSystemString, lSystemLength, lSystemAngle);
        CombineTreeMesh(treeNb);
        DestroyOriginalTreeMesh();
    }

    // Draws a tree based on the L-System string at a given position
    public void DrawTreeAt(Vector3 basePosition, string lSystemString, float lSystemLength, float lSystemAngle)
    {
        Stack<TransformInfo> transformStack = new Stack<TransformInfo>();
        Stack<float> thicknessStack = new Stack<float>();
        Vector3 position = basePosition;
        Quaternion rotation = Quaternion.identity;
        float currentThickness = initialThickness;

        foreach (char c in lSystemString)
        {
            switch (c)
            {
                case 'F': // Draws a branch
                    Vector3 start = position;
                    Vector3 end = start + (rotation * Vector3.up * lSystemLength);

                    GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    branch.transform.parent = branches.transform;
                    branch.transform.position = (start + end) / 2;
                    branch.transform.up = end - start;
                    branch.transform.localScale = new Vector3(currentThickness, Vector3.Distance(start, end) / 2, currentThickness);

                    if (branchMaterial != null) branch.GetComponent<Renderer>().material = branchMaterial;

                    CapsuleCollider branchCollider = branch.AddComponent<CapsuleCollider>();
                    branchCollider.height = Vector3.Distance(start, end);
                    branchCollider.radius = currentThickness / 2;
                    branchCollider.direction = 1;

                    position = end;
                    break;
                case '+': // Rotates clockwise on the Z-axis
                    rotation *= Quaternion.Euler(0, 0, lSystemAngle);
                    break;
                case '-': // Rotates counterclockwise on the Z-axis
                    rotation *= Quaternion.Euler(0, 0, -lSystemAngle);
                    break;
                case '&': // Rotates upward on the X-axis
                    rotation *= Quaternion.Euler(lSystemAngle, 0, 0);
                    break;
                case '^': // Rotates downward on the X-axis
                    rotation *= Quaternion.Euler(-lSystemAngle, 0, 0);
                    break;
                case '<': // Rotates clockwise on the Y-axis
                    rotation *= Quaternion.Euler(0, lSystemAngle, 0);
                    break;
                case '>': // Rotates counterclockwise on the Y-axis
                    rotation *= Quaternion.Euler(0, -lSystemAngle, 0);
                    break;
                case '[': // Saves the current transform and thickness
                    transformStack.Push(new TransformInfo(position, rotation));
                    thicknessStack.Push(currentThickness);
                    currentThickness *= thicknessReduction;
                    break;
                case ']': // Restores the saved transform and thickness
                    TransformInfo t = transformStack.Pop();
                    position = t.Position;
                    rotation = t.Rotation;
                    currentThickness = thicknessStack.Pop();

                    // Adds a leaf at the restored position
                    CreateLeaf(position, rotation);
                    break;
            }
        }
    }

    // Creates a leaf at the specified position and rotation
    private void CreateLeaf(Vector3 position, Quaternion rotation)
    {
        GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leaf.transform.parent = leaves.transform;

        leaf.transform.position = position;
        leaf.transform.rotation = rotation;

        float randomScaleX = Random.Range(leafScaleMin, leafScaleMax);
        float randomScaleY = randomScaleX * leafAspectRatio;
        leaf.transform.localScale = new Vector3(randomScaleX, randomScaleY, 1f);

        leaf.transform.Rotate(Random.Range(-leafRotationRange, leafRotationRange), Random.Range(-leafRotationRange, leafRotationRange), 0);

        if (leafMaterial != null) leaf.GetComponent<Renderer>().material = leafMaterial;
    }

    // Combines branch and leaf meshes into single meshes for performance optimization
    private void CombineTreeMesh(int treeNb)
    {
        string newBranchesName = "Tree" + treeNb + "-Branches";
        List<GameObject> branchObjects = CombineMeshes(branches, branchMaterial, newBranchesName, null);
        string newLeavesName = "Tree" + treeNb + "-Leaves";
        List<GameObject> leavesObjects = CombineMeshes(leaves, leafMaterial, newLeavesName, null);

        treeMesh = branchObjects;
        foreach (var obj in leavesObjects)
            treeMesh.Add(obj);

        // Adds a collider around the entire tree
        GameObject treeColliderObject = new GameObject("TreeCollider");
        treeColliderObject.transform.position = branches.transform.position;

        CapsuleCollider treeCollider = treeColliderObject.AddComponent<CapsuleCollider>();
        treeCollider.center = Vector3.zero;
        treeCollider.height = 10f;
        treeCollider.radius = 2f;
    }

    // Combines child meshes into a single mesh for optimization
    private List<GameObject> CombineMeshes(GameObject parent, Material material, string newMeshName, Transform newParent)
    {
        if (parent.transform.childCount == 0) return null;

        MeshCombiner meshCombiner = gameObject.GetComponent<MeshCombiner>() ?? gameObject.AddComponent<MeshCombiner>();
        return meshCombiner.CombineMeshes(parent.transform, material, newMeshName, newParent);
    }
}
