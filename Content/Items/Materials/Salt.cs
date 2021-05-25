using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Materials{
	public class Salt : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Salt");
		}

		public override void SetDefaults(){
			item.width = 28;
			item.height = 22;
			item.scale = 0.6f;
			item.rare = ItemRarityID.Blue;
			item.value = 5;
			item.maxStack = 999;
		}

		public override void AddRecipes(){
			ScienceRecipe recipe = new ScienceRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<Vial_Water>(), 2);
			recipe.AddIngredient(ModContent.ItemType<TerraFluxIndicator>());
			recipe.AddTile(ModContent.TileType<SaltExtractor>());
			recipe.SetResult(this, 1);
			recipe.AddRecipe();

			recipe = new ScienceRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<Vial_Saltwater>(), 2);
			recipe.AddIngredient(ModContent.ItemType<TerraFluxIndicator>());
			recipe.AddTile(ModContent.TileType<SaltExtractor>());
			recipe.SetResult(this, 3);
			recipe.AddRecipe();
		}
	}
}
