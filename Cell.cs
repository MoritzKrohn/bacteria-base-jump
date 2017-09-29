using System;
using System.Linq;

namespace GrowthModelLibrary
{
	public class Cell : GameObject
	{
		//global contador matBac
		//contador = contador + 1; %Count the iterations
		//%Parameters
		//x(x< 1e-6) = 0;
		//R           = cel* N; %Total length of surface, microm
		//dx          = R/N; %Thickness of each layer microm
		//NN = N * N; %Number of cells
		//X1 = vec2mat(x(1:NN), N)';
		//k1          = 4e-4; %Abitrary parameter
		//k2          = 1e3; %Abitrary parameter
		//Di          = 6e3* pars(3); %Abitrary parameter

		private double SurfaceLength { get { return mParameter.EpithelialCellWidth * mParameter.EpithelialCellsPerRow; } }
		private double CellLayerThickness { get { return SurfaceLength / mParameter.EpithelialCellsPerRow; } }
		private int NumberOfCells { get { return mParameter.EpithelialCellsPerRow * mParameter.EpithelialCellsPerRow; } }

		private double[,] CellMatrix;
		private const double k1 = 4e-4; // Arbitrary parameter
		private const double k2 = 1e2; // Arbitrary parameter
		private double Di { get { return mParameter.ChemokineDiffusionConstant; } }

		public Cell(ModelParameter parameter) : base(parameter)
		{
			CellMatrix = new double[mParameter.EpithelialCellsPerRow, mParameter.EpithelialCellsPerRow];
			Array.Clear(CellMatrix, 0, NumberOfCells);

			int count = 0;
			for (int row = 0; row < CellMatrix.GetLength(0); row++)
			{
				for (int col = 0; col < CellMatrix.GetLength(1); col++)
				{
					CellMatrix[row, col] = count++;
				}
			}
		}

		//		%%
		//%Spatial model
		// zero = zeros(1, N);
		// % 1. Fluxes in x-direction; zero fluxes near boundaries
		// FI = -Di.*[zero;X1(2:N,:) - X1(1:N-1,:); zero]./dx;
		// % Add flux gradient to rate of change
		// dI = (FI(2:N + 1,:) - FI(1:N,:))./ dx;
		// % 2. Fluxes in y-direction; zero fluxes near boundaries
		// FI = -Di.*[zero',X1(:,2:N) - X1(:,1:N-1),zero']./dx;
		// % Add flux gradient to rate of change
		// dI = -dI - (FI(:, 2:N + 1) - FI(:, 1:N))./ dx;
		//%%
		//dxX = k1.* matBac.*pars(4) - k2.* X1.^2 + dI;
		//dfx = dxX(:); %Me ge equations
		public override void Update()
		{
			int N = mParameter.EpithelialCellsPerRow;
			//1. Fluxes in x-direction; zero fluxes near boundaries
			double[,] diff = DiffMatrix(CellMatrix);
			double[] FI = new double[diff.Length + N];
			diff = Concat(diff, FI);
			diff = Multiply(diff, -Di);
			diff = Divide(diff,CellLayerThickness);
			var dI = DiffMatrix(diff);
			//dI = dI.Take(dI.Length - N).ToArray();
			//for (int i = 0; i < dI.Length; i++)
			//{
			//	dI[i] = dI[i] / CellLayerThickness;
			//}

			//2. Fluxes in y-direction; zero fluxes near boundaries
			diff = DiffMatrix(Transpose(CellMatrix));
			FI = new double[diff.Length + N];
			diff.CopyTo(FI, N);
		}

		private double[,] Concat(double[,] matrix, double[] vector)
		{
			double[,] res = new double[matrix.GetLength(0), matrix.GetLength(1) + 1];
			for (int row = 0; row < res.GetLength(0); row++)
			{
				for (int col = 0; col < res.GetLength(1); col++)
				{
					if (col == 0)
					{
						res[row, col] = vector[col];
					}
					else
					{
						res[row, col] = matrix[row, col - 1];
					}
				}
			}
			return res;
		}

		private double[,] DiffMatrix(double[,] matrix)
		{
			double[,] res = new double[matrix.GetLength(0), matrix.GetLength(1)];
			for (int row = 0; row < matrix.GetLength(0) - 1; row++)
			{
				for (int col = 0; col < matrix.GetLength(1); col++)
				{
					res[row, col] = matrix[row + 1, col] - matrix[row, col];
				}
			}
			return res;
		}

		private double[,] Multiply(double[,] matrix, double scalar)
		{
			double[,] res = new double[matrix.GetLength(0), matrix.GetLength(1)];
			for (int row = 0; row < matrix.GetLength(0); row++)
			{
				for (int col = 0; col < matrix.GetLength(1); col++)
				{
					res[row, col] = scalar * matrix[row, col];
				}
			}
			return res;
		}

		private double[,] Divide(double[,] matrix, double scalar)
		{
			double[,] res = new double[matrix.GetLength(0), matrix.GetLength(1)];
			for (int row = 0; row < matrix.GetLength(0); row++)
			{
				for (int col = 0; col < matrix.GetLength(1); col++)
				{
					res[row, col] = scalar * matrix[row, col];
				}
			}
			return res;
		}

		private double[,] Transpose(double[,] matrix)
		{
			var newArray = new double[matrix.GetLength(0), matrix.GetLength(1)];
			for (int row = 0; row < matrix.GetLength(0); row++)
			{
				for (int col = 0; col < matrix.GetLength(1); col++)
				{
					newArray[row, col] = matrix[col, row];
				}
			}
			return newArray;
		}

		public override double Radius
		{
			get
			{
				return 10;
			}
		}

		public override void Collision(GameObject otherObject)
		{
		}

	}
}
