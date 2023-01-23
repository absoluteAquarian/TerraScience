using System;
using TerraScience.Content.Tiles.Machines;

namespace TerraScience.API {
	/// <summary>
	/// A structure representing a recipe output for <see cref="Greenhouse"/>
	/// </summary>
	public readonly struct GreenhouseRecipeOutput {
		public readonly int type;
		public readonly int stackMin, stackMax;
		public readonly double chance;

		public GreenhouseRecipeOutput(int type, int stack, double chance = 1.0) {
			if (stack < 1)
				throw new ArgumentOutOfRangeException(nameof(stackMin), "Stack must be greater than zero");

			this.type = type;
			stackMin = stack;
			stackMax = stack;
			this.chance = chance;
		}

		public GreenhouseRecipeOutput(int type, int stackMin, int stackMax, double chance = 1.0) {
			if (stackMin < 1)
				throw new ArgumentOutOfRangeException(nameof(stackMin), "Minimum stack must be greater than zero");

			if (stackMax < stackMin)
				throw new ArgumentOutOfRangeException(nameof(stackMax), "Maximum stack must be greater than or equal to minimum stack");

			this.type = type;
			this.stackMin = stackMin;
			this.stackMax = stackMax;
			this.chance = chance;
		}
	}
}
