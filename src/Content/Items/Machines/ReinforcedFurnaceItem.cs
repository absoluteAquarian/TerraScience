using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Machines;

namespace TerraScience.Content.Items.Machines {
	public class ReinforcedFurnaceItem : BaseMachinePlacingItem<ReinforcedFurnace> {
		public override void SafeSetDefaults() {
			Item.width = 36;
			Item.height = 24;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(silver: 8, copper: 70);
		}
	}

	// This item places the tile, but the tile drops MachineWorkbenchItem instead since that item has a maxStack of 1 and needs to store the data in unique stacks
	public class CraftableReinforcedFurnaceItem : BaseDatalessMachinePlacingItem<ReinforcedFurnaceItem, ReinforcedFurnace> {
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.Furnace, 1)
				.AddIngredient(ItemID.GrayBrick, 15)
				.AddIngredient(ItemID.RedBrick, 20)
				.AddTile<MachineWorkbench>()
				.Register();
		}
	}
}
