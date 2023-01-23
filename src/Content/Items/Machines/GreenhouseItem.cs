using SerousEnergyLib.Items;
using Terraria;
using Terraria.ID;
using TerraScience.Content.Items.Networks.Fluids;
using TerraScience.Content.Items.Networks.Power;
using TerraScience.Content.Tiles.Machines;

namespace TerraScience.Content.Items.Machines {
	public class GreenhouseItem : BaseMachineItem<Greenhouse>, ICraftableMachineItem<CraftableGreenhouseItem> {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		public override void SafeSetDefaults() {
			Item.width = 24;
			Item.height = 36;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(silver: 4, copper: 65);
		}
	}

	public class CraftableGreenhouseItem : DatalessMachineItem<GreenhouseItem, Greenhouse> {
		public override void AddRecipes() {
			CreateRecipe()
				.AddRecipeGroup(RecipeGroupID.Wood, 10)
				.AddRecipeGroup(RecipeGroupID.IronBar, 6)
				.AddIngredient(ItemID.Glass, 15)
				.AddIngredient<BasicFluidTransportItem>(4)
				.AddIngredient<BasicTerraFluxWireItem>(5)
				.AddTile<MachineWorkbench>()
				.Register();
		}
	}
}
