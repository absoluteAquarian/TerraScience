using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Content.Items.Materials{
	public class WindmillSail : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Sail");
		}

		public override void SetDefaults(){
			item.width = 44;
			item.height = 30;
			item.scale = 0.4f;
			item.rare = ItemRarityID.White;
			item.maxStack = 999;
			item.value = 22;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddRecipeGroup(RecipeGroupID.Wood, 25);
			recipe.AddIngredient(ItemID.Silk, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this, 1);
			recipe.AddRecipe();
		}
	}
}
