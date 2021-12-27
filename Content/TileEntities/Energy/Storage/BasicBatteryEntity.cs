using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerraScience.Content.Tiles.Energy;
using TerraScience.Content.Tiles.Multitiles.EnergyMachines.Storage;
using TerraScience.Systems.Energy;
using TerraScience.Utilities;

namespace TerraScience.Content.TileEntities.Energy.Storage{
	public class BasicBatteryEntity : Battery{
		public override TerraFlux ImportRate => ModContent.GetInstance<TFWireTile>().ExportRate * 0.75f;

		public override TerraFlux ExportRate => ModContent.GetInstance<TFWireTile>().ImportRate;

		public override TerraFlux FluxCap => new TerraFlux(50000f);

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
