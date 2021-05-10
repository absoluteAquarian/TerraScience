using TerraScience.Content.TileEntities.Energy.Generators;
using TerraScience.Systems.Energy;

namespace TerraScience.Content.TileEntities.Energy.Storage{
	public abstract class Battery : GeneratorEntity{
		/// <summary>
		/// The max Terra Flux that can be imported from a connected network per tick.
		/// </summary>
		public abstract TerraFlux ImportRate{ get; }

		public override TerraFlux FluxUsage => new TerraFlux(0f);
	}
}
