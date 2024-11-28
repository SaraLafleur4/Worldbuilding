using System.Collections.Generic;

class Matrix
{
    // Holds the matrix data as a list of lists of double values
    private List<List<double>> data;

    // Constructor that initializes a matrix with the given number of rows and columns
    public Matrix(int rows, int cols)
    {
        // Create the matrix with the specified number of rows
        data = new List<List<double>>(rows);

        // Initialize each row with the specified number of columns
        for (int i = 0; i < rows; i++)
        {
            data.Add(new List<double>(new double[cols])); // Initialize each row with default values (0.0)
        }
    }

    // Indexer to access and modify matrix elements using row and column indices
    public double this[int row, int col]
    {
        get => data[row][col]; // Get the value at the specified row and column
        set => data[row][col] = value; // Set the value at the specified row and column
    }
}
