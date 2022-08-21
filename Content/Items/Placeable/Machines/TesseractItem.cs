using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines{
	public class TesseractItem : MachineItem<Tesseract>{
		public override string ItemName => "Tesseract";

		public override string ItemTooltip => "Uses quantum instability to connect two places" +
			"\nUseful for long-distance Item, fluid and power transportation";

		internal override ScienceWorkbenchItemRegistry GetRegistry()
			=> new ScienceWorkbenchItemRegistry(
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("tile")),
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("active")),
				"Connects two locations for transportation purposes",
				null,
				null);

		public override void SafeSetDefaults(){
			Item.width = 24;
			Item.height = 24;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(silver: 30, copper: 75);
		}
	}
}
