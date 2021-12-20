using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.UI;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable.Machines{
	public class ReinforcedFurnaceItem : MachineItem<ReinforcedFurnace>{
		public override string ItemName => "Reinforced Furnace";
		public override string ItemTooltip => "Burns wood into Coal";

		internal override ScienceWorkbenchItemRegistry GetRegistry()
			=> new ScienceWorkbenchItemRegistry(tick => MachineTile.GetExampleTexturePath("closed"),
				tick => MachineTile.GetExampleTexturePath("openactive"),
				ItemTooltip);

		public override void SafeSetDefaults(){
			item.width = 40;
			item.height = 46;
			item.scale = 0.62f;
			item.rare = ItemRarityID.Blue;
			item.value = Item.buyPrice(silver: 8, copper: 70);
		}
	}
}
