using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Multitiles;

namespace TerraScience.Content.Items.Placeable.Machines{
	public class ReinforcedFurnaceItem : MachineItem<ReinforcedFurnace>{
		public override string ItemName => "Reinforced Furnace";
		public override string ItemTooltip => "Burns wooden items into Coal";

		public override void SafeSetDefaults(){
			Item.width = 40;
			Item.height = 46;
			Item.scale = 0.62f;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(silver: 8, copper: 70);
		}
	}
}
