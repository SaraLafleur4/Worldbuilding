using System;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork
{
    // Number of input, hidden, and output nodes
    private int inputNodesNb, hiddenNodesNb, outputNodesNb; // /!\ these are the nb of COLUMNS per each node category

    // Activation values for input, hidden, and output nodes
    private List<double> inputs, hiddens, outputs;

    // Weight matrices for connections between layers
    private Matrix inputWeight, outputWeight;

    // Change matrices for storing momentum updates
    private Matrix inputWeightChange, outputWeightChange;

    // Number of iterations for training
    public int iterations = 2000;

    // Generate a random number in the range [a, b]
    private static double Rand(double a, double b)
    {
        System.Random rand = new System.Random();
        return (b - a) * rand.NextDouble() + a;
    }

    // Activation function: Sigmoid (using hyperbolic tangent)
    private static double Sigmoid(double x) => Math.Tanh(x);

    // Derivative of the Sigmoid function (based on the output value)
    private static double DSigmoid(double y) => 1.0 - y * y;

    // Initialize the neural network with the specified number of nodes
    public NeuralNetwork(int inputNodeCount, int hiddenNodeCount, int outputNodeCount) // /!\ these should be the nb of COLUMNS per each node category
    {
        // Set up the number of nodes for each layer
        inputNodesNb = inputNodeCount + 1; // Include bias node in the input layer
        hiddenNodesNb = hiddenNodeCount;
        outputNodesNb = outputNodeCount;

        // Initialize the activation lists for each layer
        inputs = new List<double>();
        hiddens = new List<double>();
        outputs = new List<double>();

        // Create the weight matrices
        inputWeight = new Matrix(inputNodesNb, hiddenNodesNb);
        outputWeight = new Matrix(hiddenNodesNb, outputNodesNb);
        // And initialize them with random values
        for (int i = 0; i < inputNodesNb; i++)
        {
            for (int j = 0; j < hiddenNodesNb; j++)
            {
                inputWeight[i, j] = Rand(-2.0, 2.0);
            }
        }
        for (int j = 0; j < hiddenNodesNb; j++)
        {
            for (int k = 0; k < outputNodesNb; k++)
            {
                outputWeight[j, k] = Rand(-2.0, 2.0);
            }
        }

        // Create and initialize the weight change matrices for momentum
        inputWeightChange = new Matrix(inputNodesNb, hiddenNodesNb);
        outputWeightChange = new Matrix(hiddenNodesNb, outputNodesNb);
    }

    // Compute the outputs of the network given a set of inputs
    public List<double> ComputeOutputs(List<double> currentInputs)
    {
        // Validate the number of inputs
        if (currentInputs.Count != inputNodesNb - 1) // Exclude bias node
        {
            throw new ArgumentException($"Wrong number of inputs, inputs.Count = {currentInputs.Count}, should be {inputNodesNb - 1}.");
        }

        // Clear previous activations
        ClearAllNodeLists();

        // Assign input values (excluding the bias node for now)
        for (int i = 0; i < inputNodesNb - 1; i++)
        {
            inputs.Add(currentInputs[i]);
        }

        // Add the bias node to the input activations
        inputs.Add(1.0);

        // Compute activations for the hidden layer
        for (int j = 0; j < hiddenNodesNb; j++)
        {
            double sum = 0.0;

            for (int i = 0; i < inputNodesNb; i++)
            {
                sum += inputs[i] * inputWeight[i, j];
            }

            hiddens.Add(Sigmoid(sum)); // Apply activation function
        }

        // Compute activations for the output layer
        for (int k = 0; k < outputNodesNb; k++)
        {
            double sum = 0.0;

            for (int j = 0; j < hiddenNodesNb; j++)
            {
                sum += hiddens[j] * outputWeight[j, k];
            }

            outputs.Add(Sigmoid(sum)); // Apply activation function
        }

        // Return a copy of the output activations
        return new List<double>(outputs);
    }

    // Perform backpropagation to adjust weights based on the error
    public double BackPropagate(List<double> desiredOutputs, double learningRate, double momentum)
    {
        // Validate the number of desired outputs
        if (desiredOutputs.Count != outputNodesNb)
        {
            throw new ArgumentException($"Wrong number of desiredOutputs, desiredOutputs = {desiredOutputs.Count}, should be {outputNodesNb}");
        }

        double error = 0.0;

        // Calculate error terms for the output layer
        List<double> outputDeltas = new List<double>();
        for (int k = 0; k < outputNodesNb; k++)
        {
            error += 0.5 * Math.Pow(desiredOutputs[k] - outputs[k], 2); // Compute total error

            double errorOutput = desiredOutputs[k] - outputs[k];
            outputDeltas.Add(DSigmoid(outputs[k]) * errorOutput); // Compute delta
        }

        // Calculate error terms for the hidden layer
        List<double> hiddenDeltas = new List<double>();
        for (int j = 0; j < hiddenNodesNb; j++)
        {
            double errorHidden = 0.0;

            for (int k = 0; k < outputNodesNb; k++)
            {
                errorHidden += outputDeltas[k] * outputWeight[j, k];
            }

            hiddenDeltas.Add(DSigmoid(hiddens[j]) * errorHidden); // Compute delta
        }

        // Update the output weights
        for (int j = 0; j < hiddenNodesNb; j++)
        {
            for (int k = 0; k < outputNodesNb; k++)
            {
                double change = outputDeltas[k] * hiddens[j];
                outputWeight[j, k] += learningRate * change + momentum * outputWeightChange[j, k];
                outputWeightChange[j, k] = change; // Store weight change for momentum
            }
        }

        // Update the input weights
        for (int i = 0; i < inputNodesNb; i++)
        {
            for (int j = 0; j < hiddenNodesNb; j++)
            {
                double change = hiddenDeltas[j] * inputs[i];
                inputWeight[i, j] += learningRate * change + momentum * inputWeightChange[i, j];
                inputWeightChange[i, j] = change; // Store weight change for momentum
            }
        }

        return error; // Return total error
    }

    // Train the network with a given set of patterns
    public void TrainNetwork(List<(List<double>, List<double>)> pattern, double learningRate = 0.5, double momentum = 0.1)
    {
        for (int i = 0; i < iterations; i++)
        {
            double error = 0.0;

            foreach (var p in pattern)
            {
                List<double> inputs = p.Item1;
                List<double> desiredOutputs = p.Item2;

                ComputeOutputs(inputs); // Forward pass
                error += BackPropagate(desiredOutputs, learningRate, momentum); // Backward pass
            }

            if (i % 100 == 0)
            {
                Debug.Log($"Error: {error}"); // Log error periodically
            }
        }
    }

    // Test the network on a given set of patterns
    public List<List<double>> TestNetwork(List<(List<double>, List<double>)> pattern)
    {
        List<List<double>> outputs = new List<List<double>>(); // just for log purposes
        List<string> text = new List<string>(); // just for log purposes

        foreach (var p in pattern) // for each line
        {
            List<double> input = p.Item1;
            List<double> output = ComputeOutputs(input);
            outputs.Add(output);

            List<double> desiredOutput = p.Item2; // just for log purposes
            text.Add($"input = {string.Join(" / ", input)} - desired output = {string.Join(" / ", desiredOutput)} --> actual output = {string.Join(" / ", output)}"); // just for log purposes
        }

        for (int i = text.Count; i > 0; i--) // just for log purposes
        {
            Debug.Log(text[i - 1]); // Log results
        }

        return outputs;
    }

    // Clear all node activation lists
    private void ClearAllNodeLists()
    {
        inputs.Clear();
        hiddens.Clear();
        outputs.Clear();
    }
}
