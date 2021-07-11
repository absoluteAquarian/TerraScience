using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.Items.Energy;
using TerraScience.Systems;
using TerraScience.Systems.Energy;

namespace TerraScience.Content.Tiles.Energy{
	public class AdvancedTFWireTile : TFWireTile{
		public override TerraFlux Capacity => base.Capacity * 2.5f;

		public override TerraFlux ImportRate => base.ImportRate * 2f;
		public override TerraFlux ExportRate => base.ExportRate * 2f;

		public override void SafeSetDefaults(){
			AddMapEntry(Color.Yellow);
			drop = ModContent.ItemType<AdvancedTFWireItem>();
		}
	}
}
