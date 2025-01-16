using System.Collections.Generic;
using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    public List<GameObject> CombineMeshes(Transform parentTransform, Material meshMaterial, string newMeshName, Transform newParent = null)
    {
        List<GameObject> newObjectList = new List<GameObject>();

        MeshFilter[] meshFilters = parentTransform.GetComponentsInChildren<MeshFilter>();
        List<CombineInstance> combine = new List<CombineInstance>();
        int vertexCount = 0;
        List<GameObject> objectsToDelete = new List<GameObject>();

        for (int i = 0; i < meshFilters.Length; i++)
        {
            Mesh mesh = meshFilters[i].sharedMesh;
            int meshVertexCount = mesh.vertexCount;

            if (vertexCount + meshVertexCount > 65535)
            {
                GameObject newObject = CreateCombinedMesh(newParent, combine, meshMaterial, newMeshName);
                newObjectList.Add(newObject);
                combine.Clear();
                vertexCount = 0;
            }

            CombineInstance instance = new CombineInstance();
            instance.mesh = mesh;
            instance.transform = meshFilters[i].transform.localToWorldMatrix;
            combine.Add(instance);

            vertexCount += meshVertexCount;
            objectsToDelete.Add(meshFilters[i].gameObject);
            meshFilters[i].gameObject.SetActive(false);
        }

        if (combine.Count > 0)
        {
            GameObject newObject = CreateCombinedMesh(newParent, combine, meshMaterial, newMeshName);
            newObjectList.Add(newObject);
        }

        DeleteOriginalMeshes(objectsToDelete);

        return newObjectList;
    }

    private GameObject CreateCombinedMesh(Transform newParent, List<CombineInstance> combine, Material meshMaterial, string newMeshName)
    {
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine.ToArray(), true, true);

        GameObject combinedObject = new GameObject(newMeshName);
        combinedObject.transform.parent = newParent; // Place under new parent if provided
        MeshFilter meshFilter = combinedObject.AddComponent<MeshFilter>();
        meshFilter.mesh = combinedMesh;
        MeshRenderer meshRenderer = combinedObject.AddComponent<MeshRenderer>();
        meshRenderer.material = meshMaterial;

        return combinedObject;
    }

    private void DeleteOriginalMeshes(List<GameObject> objectsToDelete)
    {
        foreach (var obj in objectsToDelete)
        {
            Destroy(obj);
        }
    }
}
