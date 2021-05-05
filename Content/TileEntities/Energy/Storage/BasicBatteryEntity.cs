using Terraria.ModLoader;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Storage;
using TerraScience.Systems.Energy;

namespace TerraScience.Content.TileEntities.Energy.Storage{
	public class BasicBatteryEntity : Battery{
		public override TerraFlux ImportRate => new TerraFlux(500f / 60f);

		public override TerraFlux ExportRate => new TerraFlux(2000f / 60f);

		public override TerraFlux FluxCap => new TerraFlux(200000f);

		public override TerraFlux GetPowerGeneration(int ticks) => ExportRate;

		public override int MachineTile => ModContent.TileType<BasicBattery>();

		public override void ReactionComplete(){ }

		public override bool UpdateReaction() => false;

		public override int SlotsCount => 0;
	}
}
