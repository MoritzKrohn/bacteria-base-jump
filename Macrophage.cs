using System;
using MathNet.Numerics.LinearAlgebra;

namespace GrowthModelLibrary
{
	public class Macrophage : GameObject
	{
		private int mEatenBacteria = 0;
		// Biological parameters for macrophages
		int macsiz = 10; // Macrophage size in micras (21 microm of diameter, from Wikipedia)
		int stepm;
		double clebac;
		double sensit;
		Matrix<double> indMac;

		public override double Radius
		{
			get
			{
				return 25;
			}
		}

		public Macrophage(ModelParameter parameter) : base(parameter)
		{
			clebac = parameter.PhagocytosysRate; // Phagocytosys rate (min-1, calculated in Excel parameters calculation.xlsx (sheet: Phagocytosys rate) [[PMC266186]])
			sensit = parameter.SensitivityToFeelCytokineGradient; // Sensitivity to feel the cytokine gradient [[buscar]].
		}
		public override void Update()
		{
			var angl = mRandom.NextDouble() * 2 * Math.PI; // Random direction
			X += (int)(Math.Cos(angl) * mParameter.MacrophageMovement);
			Y += (int)(Math.Sin(angl) * mParameter.MacrophageMovement); // Step into the direction defined
		}

		public override void Collision(GameObject otherObject)
		{
			if (otherObject.GetType() == typeof(Bacteria))
			{
				mEatenBacteria++;
				otherObject.isDead = true;
				Console.WriteLine("Bacteria eaten: {0}", mEatenBacteria);
				if (mEatenBacteria >= mParameter.MaximumBacteriaPerMacrophage)
				{
					this.isDead = true;
				}
			}
		}
	}

}
