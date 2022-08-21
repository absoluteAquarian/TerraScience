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
			=> new ScienceWorkbenchItemRegistry(
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("off")),
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("on")),
				ItemTooltip,
				consumeTFLine: null,
				produceTFLine: null);

		public override void SafeSetDefaults(){
			Item.width = 34;
			Item.height = 20;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(silver: 20, copper: 15);
		}
	}
}
