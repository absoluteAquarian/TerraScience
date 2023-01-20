using SerousEnergyLib.Items;
using Terraria;
using Terraria.ID;
using TerraScience.Content.Items.Networks.Power;
using TerraScience.Content.Tiles.Machines;

namespace TerraScience.Content.Items.Machines {
	public class FurnaceGeneratorItem : BaseMachineItem<FurnaceGenerator>, ICraftableMachineItem<CraftableFurnaceGeneratorItem> {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		public override void SafeSetDefaults() {
			Item.width = 24;
			Item.height = 36;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(silver: 8, copper: 70);
		}
	}

	public class CraftableFurnaceGeneratorItem : DatalessMachineItem<FurnaceGeneratorItem, FurnaceGenerator> {
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.Furnace, 1)
				.AddIngredient(ItemID.GrayBrick, 20)
				.AddRecipeGroup(RecipeGroupID.IronBar, 8)
				.AddRecipeGroup(nameof(ItemID.GoldBar), 2)
				.AddIngredient<BasicTerraFluxWireItem>(5)
				.AddTile<MachineWorkbench>()
				.Register();
		}
	}
}
