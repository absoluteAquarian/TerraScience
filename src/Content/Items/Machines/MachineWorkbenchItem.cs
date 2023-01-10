using SerousEnergyLib.Items;
using Terraria;
using Terraria.ID;
using TerraScience.Content.Tiles.Machines;

namespace TerraScience.Content.Items.Machines {
	public class MachineWorkbenchItem : BaseMachineItem<MachineWorkbench>, ICraftableMachineItem<CraftableMachineWorkbenchItem> {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		public override void SafeSetDefaults() {
			Item.width = 24;
			Item.height = 24;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(silver: 4, copper: 30);
		}
	}

	// This item places the tile, but the tile drops MachineWorkbenchItem instead since that item has a maxStack of 1 and needs to store the data in unique stacks
	public class CraftableMachineWorkbenchItem : DatalessMachineItem<MachineWorkbenchItem, MachineWorkbench> {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		public override void AddRecipes() {
			CreateRecipe()
				.AddRecipeGroup(RecipeGroupID.Wood, 20)
				.AddRecipeGroup(nameof(ItemID.CopperBar), 5)
				.AddIngredient(ItemID.Glass, 8)
				.AddIngredient(ItemID.GrayBrick, 30)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
