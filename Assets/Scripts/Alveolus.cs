using System.Collections;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using Assets.Scripts;

public class Alveolus : MonoBehaviour
{
    // Shared model parameter
    private ModelParameter mParameter; 
    // Vector of current chemokine concentration.
    private Vector<double> mCurrentCheConcentration;

    // Parameters definable in Unity
    // Object used for generating the tile map
    public GameObject cell;
    // Size you want your tile in unity units
    public float cellTileWidth = 1.5F;
    public float cellTileHeight = 1.11F;
    // Tilemap width and height
    public int cellHorizontalCount = 13;
    public int cellVerticalCount = 9;

    // Cell parameters
    // Complete surface in micron (x-y-area)
    private float SurfaceLength { get { return mParameter.EpithelialCellWidth * mParameter.EpithelialCellsPerRow; } }
    // Thickness of cell layer (z-Dimension)
    private float CellLayerThickness { get { return SurfaceLength / mParameter.EpithelialCellsPerRow; } }

    // Arbitrary model parameter
    private const float k1 = 4e-4F; // Arbitrary parameter
    private const float k2 = 1e3F; // Arbitrary parameter
    private float Di { get { return mParameter.ChemokineDiffusionConstant; } }

    // 2D array to hold all cells, which makes it easier to reference adjacent tiles etc.
    public GameObject[,] cellMap;

    private float mTimeStep = 0F;

    public int MacrophageCount;

    // Use this for initialization
    void Start ()
    {
        if(!cell)
            throw new System.Exception("Cell not found!!!");
        GameController gc = GameObject.Find("GameController").GetComponent<GameController>();
        mParameter = gc.Parameter;

        // Overwrite parameter from unity to match screen
        mParameter.EpithelialCellsPerRow = cellHorizontalCount;
        mParameter.EpithelialCellsPerColumn = cellVerticalCount;
        
        // Initializing chemokine concentration to 0
        mCurrentCheConcentration = Vector<double>.Build.Dense(mParameter.EpithelialCellsPerRow * mParameter.EpithelialCellsPerColumn, 0);

        //Initialize our 2D Transform array with the width and height
        cellMap = new GameObject[cellHorizontalCount, cellVerticalCount];

        //Iterate over each future tile positions for x and y
        for (int y = 0; y < cellVerticalCount; y++)
        {
            for (int x = 0; x < cellHorizontalCount; x++)
            {
                //Instantiate tile prefab at the desired position as a Transform object
                var offset = y % 2 == 0 ? cellTileWidth / 2 : 0;
                var xCoord = x * cellTileWidth - gc.Width - offset;
                var yCoord = y * cellTileHeight - gc.Height;
                Transform tile = Instantiate(cell.transform, new Vector3(xCoord, yCoord, 0), Quaternion.identity) as Transform;
                //Set the tiles parent to the GameObject this script is attached to
                tile.parent = transform;
                //Set the 2D map array element to the current tile that we just created.
                cellMap[x, y] = tile.gameObject;
            }
        }        

        StartCoroutine(CalculateChemokine());
    }
	
	// Update is called once per frame
	void Update () {
    }

    /// <summary>
    /// Coroutine that calculates the chemokine concentration and updates the cells
    /// </summary>
    IEnumerator CalculateChemokine()
    {
        while (true)
        {
            mCurrentCheConcentration = SolveEquations(mCurrentCheConcentration);
            var m = VectorToMatrix(mCurrentCheConcentration, mParameter.EpithelialCellsPerColumn);
            for (int row = 0; row < mParameter.EpithelialCellsPerRow; row++)
            {
                for (int col = 0; col < mParameter.EpithelialCellsPerColumn; col++)
                {
                    GameObject go = cellMap[row, col];
                    go.GetComponent<Cell>().Chemokine = (float)m[row, col];
                }
            }
            //Debug.Log(m);
            // Time for bacteria doubling is 200 Minutes while the Chemokine is recalculated every 5 minues. 
            // So the ration is 0.025 which we can deduct from the doubling time.
            yield return new WaitForSeconds(mParameter.BacteriaDoublingTime * 0.025F);
        }
    }

    /// <summary>
    /// Solve the equations given by the model using RungaKutta 4th order ODE solver.
    /// </summary>
    /// <param name="initials">Vector of initial values y0</param>
    /// <returns>Initals for the next step. Current chemokine concentration</returns>
    private Vector<double> SolveEquations(Vector<double> initials)
    {
        // Integrating over max of 10 hours. Step is 5 minutes
        // That means a integration step is 0.083. The Simulation uses the numbers of cells in a row as number for iterations
        // https://github.com/mathnet/mathnet-numerics/blob/master/src/UnitTests/OdeSolvers/OdeSolverTest.cs
        double start = mTimeStep;
        double stop = mTimeStep += mParameter.SychronisationTime / 60F;
        var ret = MathNet.Numerics.OdeSolvers.RungeKutta.FourthOrder(initials, start, stop, mParameter.EpithelialCellsPerRow, SetEquations);

        // Remark: This is part of the original code. But the list is never used. Macrophage movement only considers the current CHE concentration
        //foreach (Vector<double> v in ret)
        //{
        //    mChemokine.Add(VectorToMatrix(v, mParameter.EpithelialCellsPerColumn));
        //}
        initials = ret[ret.Length - 1];
        return initials;
    }

    ///<summary> 
    /// Equatiosn for the spatial cell model (MATLAB code)
    ///</summary>
    ///<remarks>
    ///  zero = zeros(1, N);
    ///  % 1. Fluxes in x-direction; zero fluxes near boundaries
    ///  FI = -Di.*[zero;X1(2:N,:) - X1(1:N-1,:); zero]./dx;
    ///  % Add flux gradient to rate of change
    ///  dI = (FI(2:N + 1,:) - FI(1:N,:))./ dx;
    ///  % 2. Fluxes in y-direction; zero fluxes near boundaries
    ///  FI = -Di.*[zero',X1(:,2:N) - X1(:,1:N-1),zero']./dx;
    ///  % Add flux gradient to rate of change
    ///  dI = -dI - (FI(:, 2:N + 1) - FI(:, 1:N))./ dx;
    /// %%
    /// dxX = k1.* matBac.*pars(4) - k2.* X1.^2 + dI;
    /// dfx = dxX(:); %Merge equations
    ///</remarks>
    ///<param name="initials">Vector of initial values y0</param>
    ///<param name="tspan">Not used at the moment</param>
    ///<returns>
    ///Solved values for each time step
    ///</returns>
    private Vector<double> SetEquations(double tspan, Vector<double> initials)
    {
        initials.Map(x => x < 1e-6 ? 0 : x);
        int rows = mParameter.EpithelialCellsPerRow;
        int cols = mParameter.EpithelialCellsPerColumn;
        var X1 = VectorToMatrix(initials, cols).Transpose();
        rows = mParameter.EpithelialCellsPerColumn;
        cols = mParameter.EpithelialCellsPerRow;

        //1. Fluxes in x-direction; zero fluxes near boundaries
        var zero = Matrix<double>.Build.Dense(1, cols, 0);
        var diff = X1.SubMatrix(1, rows - 1, 0, X1.ColumnCount) - X1.SubMatrix(0, rows - 1, 0, X1.ColumnCount);
        var FI = zero.Stack(diff);
        FI = -Di * FI.Stack(zero) / CellLayerThickness;
        // Add flux gradient to rate of change
        var dI = (FI.SubMatrix(1, rows, 0, FI.ColumnCount) - FI.SubMatrix(0, rows, 0, FI.ColumnCount)) / CellLayerThickness;
        //2. Fluxes in y-direction; zero fluxes near boundaries
        zero = Matrix<double>.Build.Dense(rows, 1, 0);
        diff = X1.SubMatrix(0, X1.RowCount, 1, cols - 1) - X1.SubMatrix(0, X1.RowCount, 0, cols - 1);
        FI = zero.Append(diff);
        FI = -Di * FI.Append(zero) / CellLayerThickness;
        // Add flux gradient to rate of change
        dI = -dI - (FI.SubMatrix(0, FI.RowCount, 1, cols) - FI.SubMatrix(0, FI.RowCount, 0, cols)) / CellLayerThickness;
        var dxX = k1 * BactCount.Transpose() * mParameter.AntigenPerBacteria - k2 * X1.PointwiseMultiply(X1) + dI;
        
        // Merge equations into a vector
        var ret = Vector<double>.Build.Dense(rows * cols);
        for(int i = 0; i < dxX.ColumnCount; i++)
        {
            ret.SetSubVector(i * rows, rows, dxX.Column(i));
        }

        return ret;
    }

    /// <summary>
    /// Convert a vector to a matrix given the numbers of colums. 
    /// This is equivalent to MATLABs vec2mat. The remainder will be padded with zeros.
    /// </summary>
    /// <param name="vector">Vector of values</param>
    /// <param name="columns">Number of columns in the matrix</param>
    /// <returns></returns>
    private Matrix<double> VectorToMatrix(Vector<double> vector, int columns)
    {
        var rem = vector.Count % columns;
        var rows = Mathf.FloorToInt(vector.Count / columns);
        // If the last row is incomplete we will pad it with zeros
        rows += rem > 0 ? 1 : 0;
        Matrix<double> ret = Matrix<double>.Build.Dense(rows, columns);
        int count = 0;
        for(int row = 0; row < ret.RowCount; row++)
        {
            for(int col = 0; col < ret.ColumnCount; col++)
            {
                ret[row, col] = count < vector.Count ? vector[count] : 0;
                count++;
            }
        }
        return ret;
    }

    // Matrix that holds the number of bacteria on every cell.
    private Matrix<double> BactCount
    {
        get
        {
            Matrix <double> ret = Matrix<double>.Build.Dense(mParameter.EpithelialCellsPerRow, mParameter.EpithelialCellsPerColumn);
            for (int row = 0; row < ret.RowCount; row++)
            {
                for (int col = 0; col < ret.ColumnCount; col++)
                {
                    Cell curCell = cellMap[row, col].GetComponent<Cell>();
                    ret[row, col] = curCell.BacteriaOnCell;
                }
            }
            return ret;
        }
    }
}
