using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Content.Items.Materials{
	public class Shaker_Empty : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Empty Shaker");
		}

		public override void SetDefaults(){
			Item.width = 22;
			Item.height = 38;
			Item.rare = ItemRarityID.White;
			Item.value = Item.sellPrice(silver: 1, copper: 45);
		}

		public override void AddRecipes(){
			CreateRecipe(1)
				.AddIngredient(ItemID.Glass, 7)
				.AddRecipeGroup(RecipeGroupID.IronBar, 2)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
