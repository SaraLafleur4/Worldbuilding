using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LSystem : MonoBehaviour
{
    [Header("L-System Settings")]
    public string axiom = "F";
    public int iterations = 3;
    public float angle = 25.0f;
    public float length = 1.0f;

    [Header("L-System Rules")]
    public List<Rule> rules = new List<Rule>();

    [System.Serializable]
    public struct Rule
    {
        public char symbol; // ex: F
        public string replacement; // ex: F[+F]F[-F]F
    }

    private string currentString;
    // Returns the current L-System string
    public string getCurrentString() => currentString;

    // Configures the L-System with random parameters for generating a single tree
    public void SetupLSystemForSingleTree()
    {
        iterations = Random.Range(1, 5);

        angle = Random.Range(10.0f, 35.0f);

        List<Rule[]> possibleRules = new List<Rule[]> {
            new Rule[] { new Rule { symbol = 'F', replacement = "F[+F]F[-F]F" } },
            new Rule[] { new Rule { symbol = 'F', replacement = "FF+[+F-F-F]-[-F+F+F]" } },
            new Rule[] { new Rule { symbol = 'F', replacement = "F[+F&F][-F^F]" } }
        };

        Rule[] chosenRules = possibleRules[Random.Range(0, possibleRules.Count)];
        rules = new List<Rule>(chosenRules);

        currentString = GenerateLSystem(axiom, iterations);
    }

    // Generates the final L-System string based on the axiom, rules, and number of iterations
    public string GenerateLSystem(string axiom, int iterations)
    {
        string result = axiom;

        for (int i = 0; i < iterations; i++)
        {
            string next = "";
            foreach (char c in result)
            {
                bool replaced = false;
                foreach (var rule in rules)
                {
                    if (c == rule.symbol)
                    {
                        next += rule.replacement;
                        replaced = true;
                        break;
                    }
                }
                if (!replaced) next += c.ToString();
            }
            result = next;
        }
        return result;
    }
}
