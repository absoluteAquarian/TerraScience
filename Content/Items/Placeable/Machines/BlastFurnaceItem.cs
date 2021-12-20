using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines{
	public class BlastFurnaceItem : MachineItem<BlastFurnace>{
		public override string ItemName => "Blast Furnace";
		public override string ItemTooltip => "Doubles gains from smelted ores";

		internal override ScienceWorkbenchItemRegistry GetRegistry()
			=> new ScienceWorkbenchItemRegistry(tick => MachineTile.GetExampleTexturePath("closed"),
				tick => MachineTile.GetExampleTexturePath("open"),
				ItemTooltip);

		public override void SafeSetDefaults(){
			item.width = 20;
			item.height = 20;
			item.scale = 0.82f;
			item.rare = ItemRarityID.Green;
			item.value = Item.buyPrice(silver: 8, copper: 70);
		}
	}
}
