using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles;

namespace TerraScience.Content.Items.Placeable.Machines{
	public class BlastFurnaceItem : MachineItem<BlastFurnace>{
		public override string ItemName => "Blast Furnace";
		public override string ItemTooltip => "Doubles ore smelted";

		public override void SafeSetDefaults(){
			Item.width = 20;
			Item.height = 20;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(silver: 8, copper: 70);
		}
	}
}
