using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines{
	public class TesseractItem : MachineItem<Tesseract>{
		public override string Texture => "TerraScience/Content/Items/Placeable/Machines/TemporaryMachineSprite";

		public override string ItemName => "Tesseract";

		public override string ItemTooltip => "Uses quantum instability to connect two places" +
			"\nUseful for long-distance item, fluid and power transportation";

		internal override ScienceWorkbenchItemRegistry GetRegistry()
			=> new ScienceWorkbenchItemRegistry(
				tick => new RegistryAnimation(MachineTile.GetExampleTexturePath("tile")),
				null,
				"Connects two locations for transportation purposes",
				null,
				null);

		public override void SafeSetDefaults(){
			item.width = 24;
			item.height = 24;
			item.scale = 0.82f;
			item.rare = ItemRarityID.Green;
			item.value = Item.buyPrice(silver: 30, copper: 75);
		}
	}
}
