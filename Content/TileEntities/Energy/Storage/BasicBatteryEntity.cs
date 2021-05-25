using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Storage;
using TerraScience.Systems.Energy;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities.Energy.Storage{
	public class BasicBatteryEntity : Battery{
		public override TerraFlux ImportRate => new TerraFlux(800f / 60f);

		public override TerraFlux ExportRate => new TerraFlux(2000f / 60f);

		public override TerraFlux FluxCap => new TerraFlux(50000f);

		public override TerraFlux GetPowerGeneration(int ticks) => ExportRate;

		public override int MachineTile => ModContent.TileType<BasicBattery>();

		public override void PreUpdateReaction(){
			if((float)StoredFlux > 0)
				Lighting.AddLight(TileUtils.TileEntityCenter(this, MachineTile), Color.Green.ToVector3() * 0.3f);
		}

		public override bool UpdateReaction() => false;

		public override void ReactionComplete(){ }

		public override int SlotsCount => 0;
	}
}
