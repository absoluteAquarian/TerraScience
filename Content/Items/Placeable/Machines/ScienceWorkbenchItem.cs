using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles;

namespace TerraScience.Content.Items.Placeable.Machines{
	public class ScienceWorkbenchItem : MachineItem<ScienceWorkbench>{
		public override string ItemName => "Science Workbench";
		public override string ItemTooltip => "Create machinery at this crafting station" +
			"\nRight click when placed to open a crafting interface";

		public override void SafeSetDefaults(){
			item.width = 24;
			item.height = 24;
			item.scale = 0.82f;
			item.rare = ItemRarityID.Blue;
			item.value = Item.buyPrice(silver: 4, copper: 30);
		}
	}
}
