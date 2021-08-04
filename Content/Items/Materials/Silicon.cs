using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Materials{
	public class Silicon : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Silicon");
			Tooltip.SetDefault("The backbone of anything mechanical");
		}

		public override void SetDefaults(){
			Item.width = 26;
			Item.height = 20;
			Item.scale = 0.9f;
			Item.rare = ItemRarityID.White;
			Item.value = 2;
			Item.maxStack = 999;
		}

		public override void AddRecipes(){
			CreateRecipe(1)
				.AddRecipeGroup(TechMod.ScienceRecipeGroups.Sand, 1)
				.AddTile(TileID.Extractinator)
				.AddCondition(NetworkText.FromLiteral(TechMod.RecipeDescription_MadeAtMachine), recipe => false)
				.Register();
		}
	}
}
