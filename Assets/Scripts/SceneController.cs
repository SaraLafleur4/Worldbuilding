using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    // UI buttons to trigger actions
    public Button generateDecor;
    public Button generateCreatures;

    // For creature population management
    public PopulationManager populationManager;

    // Start is called before the first frame update
    void Start()
    {
        generateDecor.onClick.AddListener(delegate { GenerateDecor(); });
        generateCreatures.onClick.AddListener(delegate { GenerateCreatures(); });
    }

    public void GenerateDecor()
    {
        Debug.Log("Generate Decor clicked!");
        return;
    }

    public void GenerateCreatures()
    {
        Debug.Log("Generate Creatures clicked!");
        populationManager.Initialize();
        return;
    }
}
