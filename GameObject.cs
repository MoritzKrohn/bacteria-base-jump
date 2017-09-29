using System;
namespace GrowthModelLibrary
{
	public abstract class GameObject
	{
		public Dimension Dimension;
		protected Position mPosition = new Position();
		protected ModelParameter mParameter;
		protected static Random mRandom = new Random();
		public abstract double Radius { get; }
		public bool isDead = false;

		public int X
		{
			get { return this.mPosition.X; }
			set
			{
				if (value > Dimension.Xmax)
				{
					mPosition.X = Dimension.Xmax;
				}
				if (value < Dimension.Xmin)
				{
					mPosition.X = Dimension.Xmin;
				}
				else
				{
					mPosition.X = value;
				}
			}
		}
		public int Y
		{
			get { return this.mPosition.Y; }
			set
			{
				if (value > Dimension.Ymax)
				{
					mPosition.Y = Dimension.Ymax;
				}
				else if (value < Dimension.Ymin)
				{
					mPosition.Y = Dimension.Ymin;
				}
				else
				{
					mPosition.Y = value;
				}
			}
		}

		public GameObject(ModelParameter parameter)
		{
			mParameter = parameter;
			int dimens = parameter.CellDimension;
			this.Dimension = new Dimension { Xmin = 0, Xmax = dimens, Ymin = 0, Ymax = dimens };
			this.X = (int)(mRandom.NextDouble() * (double)(dimens));
			this.Y = (int)(mRandom.NextDouble() * (double)(dimens));
		}

		public abstract void Update();
		public abstract void Collision(GameObject otherObject);
	}
}
