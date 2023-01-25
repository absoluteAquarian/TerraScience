using SerousEnergyLib.Items;
using Terraria;
using Terraria.ID;
using TerraScience.Content.Items.Networks.Power;
using TerraScience.Content.Tiles.Machines;

namespace TerraScience.Content.Items.Machines {
	public class BatteryItem : BaseMachineItem<Battery>, ICraftableMachineItem<CraftableBatteryItem> {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		public override void SafeSetDefaults() {
			Item.width = 24;
			Item.height = 36;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(silver: 5, copper: 45);
		}
	}

	public class CraftableBatteryItem : DatalessMachineItem<BatteryItem, Battery> {
		public override void AddRecipes() {
			CreateRecipe()
				.AddRecipeGroup(RecipeGroupID.Wood, 10)
				.AddRecipeGroup(RecipeGroupID.IronBar, 12)
				.AddIngredient<BasicTerraFluxWireItem>(20)
				.AddTile<MachineWorkbench>()
				.Register();
		}
	}
}
