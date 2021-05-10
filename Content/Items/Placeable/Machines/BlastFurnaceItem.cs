using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles;

namespace TerraScience.Content.Items.Placeable.Machines{
	public class BlastFurnaceItem : MachineItem<BlastFurnace>{
		public override string ItemName => "Blast Furnace";
		public override string ItemTooltip => "Doubles ore smelted";

		public override void SafeSetDefaults(){
			item.width = 20;
			item.height = 20;
			item.scale = 0.82f;
			item.rare = ItemRarityID.Green;
			item.value = Item.buyPrice(silver: 8, copper: 70);
		}
	}
}
