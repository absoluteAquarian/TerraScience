using SerousEnergyLib.Items;
using Terraria;
using Terraria.ID;
using TerraScience.Content.Items.Networks.Power;
using TerraScience.Content.Tiles.Machines;

namespace TerraScience.Content.Items.Machines {
	public class WindTurbineItem : BaseMachineItem<WindTurbine>, ICraftableMachineItem<CraftableWindTurbineItem> {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		public override void SafeSetDefaults() {
			Item.width = 24;
			Item.height = 48;
			Item.scale = 0.82f;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(silver: 12, copper: 25);
		}
	}

	public class CraftableWindTurbineItem : DatalessMachineItem<WindTurbineItem, WindTurbine> {
		public override void AddRecipes() {
			CreateRecipe()
				.AddRecipeGroup(RecipeGroupID.Wood, 6)
				.AddRecipeGroup(RecipeGroupID.IronBar, 18)
				.AddRecipeGroup(nameof(ItemID.GoldBar), 8)
				.AddIngredient<BasicTerraFluxWireItem>(8)
				.AddTile<MachineWorkbench>()
				.Register();
		}
	}
}
