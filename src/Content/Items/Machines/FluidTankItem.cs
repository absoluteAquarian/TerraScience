using SerousEnergyLib.Items;
using Terraria;
using Terraria.ID;
using TerraScience.Content.Items.Networks.Fluids;
using TerraScience.Content.Tiles.Machines;

namespace TerraScience.Content.Items.Machines {
	public class FluidTankItem : BaseMachineItem<FluidTank>, ICraftableMachineItem<CraftableFluidTankItem> {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		public override void SafeSetDefaults() {
			Item.width = 24;
			Item.height = 36;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(silver: 8, copper: 70);
		}
	}

	// This item places the tile, but the tile drops FluidTankItem instead since that item has a maxStack of 1 and needs to store the data in unique stacks
	public class CraftableFluidTankItem : DatalessMachineItem<FluidTankItem, FluidTank> {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		public override void AddRecipes() {
			CreateRecipe()
				.AddRecipeGroup(RecipeGroupID.IronBar, 12)
				.AddIngredient(ItemID.Glass, 20)
				.AddIngredient<BasicFluidPumpItem>(2)
				.AddTile<MachineWorkbench>()
				.Register();
		}
	}
}
