using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe qui combine plusieurs meshes en un seul
/// </summary>
public class MeshCombiner : MonoBehaviour
{
    /// <summary>
    /// Combine tous les MeshFilters enfants en un seul ou plusieurs meshes (en fonction de la limite de vertex par mesh)
    /// </summary>
    /// <param name="parentTransform">Transform parent qui contient les MeshFilters à combiner</param>
    /// <param name="meshMaterial"> Matériau à appliquer aux meshes</param>
    public void CombineMeshes(Transform parentTransform, Material meshMaterial)
    {
        MeshFilter[] meshFilters = parentTransform.GetComponentsInChildren<MeshFilter>();
        List<CombineInstance> combine = new List<CombineInstance>();
        int vertexCount = 0;
        List<Mesh> combinedMeshes = new List<Mesh>();

        // Parcourt tous les MeshFilters pour les combiner
        for (int i = 0; i < meshFilters.Length; i++)
        {
            Mesh mesh = meshFilters[i].sharedMesh;
            int meshVertexCount = mesh.vertexCount;

            // Si ajouter le  mesh dépasse la limite, on crée un mesh combiné
            if (vertexCount + meshVertexCount > 65535)
            {
                CreateCombinedMesh(parentTransform, combine, meshMaterial, combinedMeshes);
                combine.Clear();
                vertexCount = 0;
            }

            CombineInstance instance = new CombineInstance();
            instance.mesh = mesh;
            instance.transform = meshFilters[i].transform.localToWorldMatrix;
            combine.Add(instance);

            vertexCount += meshVertexCount;
            meshFilters[i].gameObject.SetActive(false);  // Désactive les voxels originaux
        }

        // Combine le dernier groupe de meshes s'il en reste
        if (combine.Count > 0)
        {
            CreateCombinedMesh(parentTransform, combine, meshMaterial, combinedMeshes);
        }
    }

    /// <summary>
    /// Crée un mesh combiné à partir des MeshFilters donnés et l'ajoute comme enfant du parent
    /// </summary>
    /// <param name="parentTransform">Transform parent pour le mesh combiné</param>
    /// <param name="combine">Liste des CombineInstance représentant les meshes à combiner</param>
    /// <param name="meshMaterial"> Matériau à appliquer au mesh combiné</param>
    /// <param name="combinedMeshes">Liste des meshes combinés</param>
    private void CreateCombinedMesh(Transform parentTransform, List<CombineInstance> combine, Material meshMaterial, List<Mesh> combinedMeshes)
    {
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine.ToArray(), true, true);  
        combinedMeshes.Add(combinedMesh); 

        // Crée un nouvel objet pour le mesh combiné
        GameObject combinedObject = new GameObject("CombinedMesh");
        combinedObject.transform.parent = parentTransform;
        MeshFilter meshFilter = combinedObject.AddComponent<MeshFilter>();
        meshFilter.mesh = combinedMesh;
        MeshRenderer meshRenderer = combinedObject.AddComponent<MeshRenderer>();
        meshRenderer.material = meshMaterial;
    }
}
