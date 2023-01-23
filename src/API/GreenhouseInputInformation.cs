using SerousEnergyLib.API;
using SerousEnergyLib.API.Fluid;
using System;
using TerraScience.Content.Tiles.Machines;

namespace TerraScience.API {
	/// <summary>
	/// A structure representing input requirements for growing a plant in a <see cref="Greenhouse"/>
	/// </summary>
	public readonly struct GreenhouseInputInformation {
		public readonly int soil;
		public readonly int modifier;
		public readonly int plant;
		public readonly GreenhouseRecipeOutput[] possibleOutputs;
		public readonly Ticks growthTime;
		public readonly int requiredFluid;
		public readonly double requiredFluidQuantity;

		public GreenhouseInputInformation(int soil, int modifier, int plant, Ticks growthTime, int requiredFluid, double requiredFluidQuantity, params GreenhouseRecipeOutput[] possibleOutputs) {
			if (growthTime <= 0)
				throw new ArgumentException("Growth duration must be at least one tick", nameof(growthTime));

			if (requiredFluid > FluidTypeID.None && requiredFluidQuantity <= 0)
				throw new ArgumentException("Required fluid quantity must be greater than zero when the required fluid is not FluidTypeID.None", nameof(requiredFluidQuantity));

			if (possibleOutputs is not { Length: > 0 })
				throw new ArgumentException("Recipe output list was invalid", nameof(possibleOutputs));

			this.soil = soil;
			this.modifier = modifier;
			this.plant = plant;
			this.possibleOutputs = possibleOutputs;
			this.growthTime = growthTime;
			this.requiredFluid = requiredFluid;
			this.requiredFluidQuantity = requiredFluidQuantity;
		}
	}
}
