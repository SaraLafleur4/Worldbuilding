using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Tree : MonoBehaviour
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

    public void DestroyTree()
    {
        Destroy(branches);
        Destroy(leaves);
    }

    public void SetMaterials(Material branch, Material leaf)
    {
        branchMaterial = branch;
        leafMaterial = leaf;
    }

    public void Generate(Vector2 size, string lSystemString, float lSystemLength, float lSystemAngle)
    {
        branches = new GameObject("Branches");
        leaves = new GameObject("Leaves");

        var randomPosition = new Vector3(
            Random.Range(0, size.x),
            0,
            Random.Range(0, size.y)
        );
        if (Terrain.activeTerrain != null) randomPosition.y = Terrain.activeTerrain.SampleHeight(randomPosition);

        CombineTreeMesh();

        DrawTreeAt(randomPosition, lSystemString, lSystemLength, lSystemAngle);
    }

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
                case 'F':
                    Vector3 start = position;
                    Vector3 end = start + (rotation * Vector3.up * lSystemLength);

                    GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    branch.transform.parent = branches.transform;
                    branch.transform.position = (start + end) / 2;
                    branch.transform.up = end - start;
                    branch.transform.localScale = new Vector3(currentThickness, Vector3.Distance(start, end) / 2, currentThickness);

                    if (branchMaterial != null) branch.GetComponent<Renderer>().material = branchMaterial;

                    position = end;
                    break;
                case '+':
                    rotation *= Quaternion.Euler(0, 0, lSystemAngle);
                    break;
                case '-':
                    rotation *= Quaternion.Euler(0, 0, -lSystemAngle);
                    break;
                case '&':
                    rotation *= Quaternion.Euler(lSystemAngle, 0, 0);
                    break;
                case '^':
                    rotation *= Quaternion.Euler(-lSystemAngle, 0, 0);
                    break;
                case '<':
                    rotation *= Quaternion.Euler(0, lSystemAngle, 0);
                    break;
                case '>':
                    rotation *= Quaternion.Euler(0, -lSystemAngle, 0);
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

    public void CombineTreeMesh()
    {
        CombineMeshes(branches, branchMaterial);
        CombineMeshes(leaves, leafMaterial);
    }

    private void CombineMeshes(GameObject parent, Material material)
    {
        if (parent.transform.childCount == 0) return;

        MeshCombiner meshCombiner = gameObject.GetComponent<MeshCombiner>();
        if (meshCombiner == null) gameObject.AddComponent<MeshCombiner>();

        meshCombiner.CombineMeshes(parent.transform, material);
    }
}
