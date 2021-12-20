using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Content.Items.Materials {
	public class Silicon : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Silicon");
			Tooltip.SetDefault("The backbone of anything mechanical");
		}

		public override void SetDefaults(){
			item.width = 26;
			item.height = 20;
			item.scale = 0.9f;
			item.rare = ItemRarityID.White;
			item.value = 2;
			item.maxStack = 999;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddRecipeGroup(TechMod.ScienceRecipeGroups.Sand, 1);
			recipe.AddTile(TileID.Extractinator);
			recipe.SetResult(this, 1);
			recipe.AddRecipe();
		}
	}
}
