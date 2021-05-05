using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Content.Items.Materials{
	public class Shaker_Empty : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Empty Shaker");
		}

		public override void SetDefaults(){
			item.width = 22;
			item.height = 38;
			item.rare = ItemRarityID.White;
			item.value = Item.sellPrice(silver: 1, copper: 45);
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Glass, 7);
			recipe.AddRecipeGroup("IronBar", 2);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
