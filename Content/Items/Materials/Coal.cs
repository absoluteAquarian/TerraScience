using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Tiles.Multitiles;

namespace TerraScience.Content.Items.Materials {
	public class Coal : ModItem{
		public override string Texture => "Terraria/Item_" + ItemID.Coal;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Charcoal");
			Tooltip.SetDefault("Carbon that's been compressed to a rough ball.");
		}

		public override void SetDefaults(){
			item.width = 18;
			item.height = 18;
			item.rare = ItemRarityID.Blue;
			item.maxStack = 999;
			item.value = 10;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddRecipeGroup(RecipeGroupID.Wood);
			recipe.AddTile(ModContent.TileType<ReinforcedFurnace>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
