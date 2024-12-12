using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainNoiseGeneration))]
public class TerrainGenEditor : Editor
{
    public override void OnInspectorGUI() {
        TerrainNoiseGeneration component = (TerrainNoiseGeneration)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Regenerate")) {
            component.GenerateTerrainGUI();
        }
        if (GUILayout.Button("Reset Heights")) {
            component.ResetHeightsTerrainGUI();
        }
    }
}
