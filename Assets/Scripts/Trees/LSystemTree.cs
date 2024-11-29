using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSystemTree : MonoBehaviour
{
    [Header("L-System Settings")]
    public string axiom = "F"; // Punto de inicio del sistema
    public int iterations = 5; // Número de iteraciones del sistema
    public float angle = 25.0f; // Ángulo de bifurcación
    public float length = 1.0f; // Longitud inicial de las ramas

    [Header("Branch Settings")]
    public Material branchMaterial; // Material para las ramas


    [Header("Leaf Settings")]
    public Texture2D leafTexture; // Textura para las hojas
    public float leafScaleMin = 0.8f; // Escala mínima de la hoja
    public float leafScaleMax = 1.2f; // Escala máxima de la hoja

    [Header("Leaf Shape")]
    public float leafAspectRatio = 0.6f; // Relación altura/ancho para las hojas
    public float leafRotationRange = 30f; // Variación aleatoria en la rotación de las hojas

    [Header("L-System Rules")]
    public List<Rule> rules = new List<Rule>(); // Lista de reglas configurables desde el Inspector

    [Header("Branch Thickness Settings")]
    public float initialThickness = 0.3f; // Grosor inicial del tronco
    public float thicknessReduction = 0.7f; // Factor de reducción del grosor por nivel


    private string currentString;

    void Start()
    {
        // Generar el sistema basado en el axiom
        currentString = GenerateLSystem(axiom, iterations);

        // Dibujar el árbol
        DrawTree();
    }

    string GenerateLSystem(string axiom, int iterations)
    {
        string result = axiom;
        for (int i = 0; i < iterations; i++)
        {
            string next = "";
            foreach (char c in result)
            {
                bool replaced = false;
                // Reemplaza el carácter según las reglas definidas
                foreach (var rule in rules)
                {
                    if (c == rule.symbol)
                    {
                        next += rule.replacement;
                        replaced = true;
                        break;
                    }
                }
                if (!replaced)
                {
                    next += c.ToString(); // Si no hay regla, conserva el carácter original
                }
            }
            result = next;
        }
        return result;
    }

    void DrawTree()
    {
        Stack<TransformInfo> transformStack = new Stack<TransformInfo>();
        Stack<float> thicknessStack = new Stack<float>(); // Pila para rastrear el grosor actual
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;
        float currentThickness = initialThickness; // Comienza con el grosor inicial

        foreach (char c in currentString)
        {
            if (c == 'F')
            {
                // Calcula el punto inicial y final de la rama
                Vector3 start = position;
                Vector3 end = start + (rotation * Vector3.up * length);

                // Crea una rama como cilindro
                GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                branch.transform.position = (start + end) / 2; // Posición en el centro de la línea
                branch.transform.up = end - start; // Orientación hacia el punto final
                branch.transform.localScale = new Vector3(currentThickness, Vector3.Distance(start, end) / 2, currentThickness); // Escala proporcional a la longitud de la rama

                // Asignar el material marrón a la rama
                if (branchMaterial != null)
                {
                    branch.GetComponent<Renderer>().material = branchMaterial;
                }

                // Actualiza la posición para la siguiente rama
                position = end;
            }
            else if (c == '+')
            {
                rotation *= Quaternion.Euler(0, 0, angle); // Rotación positiva en Z
            }
            else if (c == '-')
            {
                rotation *= Quaternion.Euler(0, 0, -angle); // Rotación negativa en Z
            }
            else if (c == '&')
            {
                rotation *= Quaternion.Euler(angle, 0, 0); // Rotación positiva en X
            }
            else if (c == '^')
            {
                rotation *= Quaternion.Euler(-angle, 0, 0); // Rotación negativa en X
            }
            else if (c == '<')
            {
                rotation *= Quaternion.Euler(0, angle, 0); // Rotación positiva en Y
            }
            else if (c == '>')
            {
                rotation *= Quaternion.Euler(0, -angle, 0); // Rotación negativa en Y
            }
            else if (c == '[')
            {
                // Guarda la posición, rotación y grosor actuales
                transformStack.Push(new TransformInfo(position, rotation));
                thicknessStack.Push(currentThickness);

                // Reduce el grosor para las ramas hijas
                currentThickness *= thicknessReduction;
            }
            else if (c == ']')
            {
                // Restaura la posición, rotación y grosor guardados
                TransformInfo t = transformStack.Pop();
                position = t.Position;
                rotation = t.Rotation;
                currentThickness = thicknessStack.Pop();

                // Crea una hoja en la bifurcación
                CreateLeaf(position, rotation);
            }
        }
    }


    void CreateLeaf(Vector3 position, Quaternion rotation)
    {
        // Crea una esfera para la hoja, que se escalará para hacerla ovalada
        GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        // Ajusta la posición y orientación de la hoja
        leaf.transform.position = position; // En la posición final de la rama
        leaf.transform.rotation = rotation; // Con la misma orientación de la rama

        // Aplica escalado con relación de aspecto manual
        float randomScaleX = Random.Range(leafScaleMin, leafScaleMax);
        float randomScaleY = randomScaleX * leafAspectRatio; // Usa el parámetro configurado en el Inspector
        leaf.transform.localScale = new Vector3(randomScaleX, randomScaleY, 1f); // Escala en X y Y para obtener una forma ovalada

        // Aplica rotación aleatoria
        leaf.transform.Rotate(Random.Range(-leafRotationRange, leafRotationRange), Random.Range(-leafRotationRange, leafRotationRange), 0);

        // Cambia el material para aplicar la textura de la hoja
        if (leafTexture != null)
        {
            Material leafMaterial = new Material(Shader.Find("Standard"));
            leafMaterial.mainTexture = leafTexture;
            leafMaterial.color = Color.green; // Ajusta un tinte verde si no hay textura
            leaf.GetComponent<Renderer>().material = leafMaterial;
        }
        else
        {
            // Si no hay textura, usa un color verde por defecto
            leaf.GetComponent<Renderer>().material.color = Color.green;
        }
    }

    [System.Serializable]
    public class Rule
    {
        public char symbol; // Símbolo de la regla (por ejemplo 'F')
        public string replacement; // Reemplazo de la regla (por ejemplo "F[+F]F[-F]F")
    }

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
}
