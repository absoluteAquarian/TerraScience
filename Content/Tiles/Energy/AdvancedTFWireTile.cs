using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using TerraScience.Content.Items.Energy;
using TerraScience.Systems.Energy;

namespace TerraScience.Content.Tiles.Energy {
	public class AdvancedTFWireTile : TFWireTile{
		public override TerraFlux Capacity => base.Capacity * 2.5f;

		public override TerraFlux ImportRate => base.ImportRate * 2f;
		public override TerraFlux ExportRate => base.ExportRate * 2f;

		public override void SafeSetDefaults(){
			AddMapEntry(Color.Yellow);
			ItemDrop = ModContent.ItemType<AdvancedTFWireItem>();
		}
	}
}
