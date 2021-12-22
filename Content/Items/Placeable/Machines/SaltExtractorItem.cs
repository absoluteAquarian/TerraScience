using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines{
	public class SaltExtractorItem : MachineItem<SaltExtractor>{
		public override string ItemName => "Salt Extractor";
		public override string ItemTooltip => "Processes water-based liquids and extracts the salt from them";

		internal override ScienceWorkbenchItemRegistry GetRegistry()
			=> new ScienceWorkbenchItemRegistry(tick => MachineTile.GetExampleTexturePath("on"),
				tick => MachineTile.GetExampleTexturePath("off"),
				ItemTooltip,
				consumeTFLine: null,
				produceTFLine: null);

		public override void SafeSetDefaults(){
			item.width = 34;
			item.height = 20;
			item.scale = 0.82f;
			item.rare = ItemRarityID.Green;
			item.value = Item.buyPrice(silver: 20, copper: 15);
		}
	}
}
