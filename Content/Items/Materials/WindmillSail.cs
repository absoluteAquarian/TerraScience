using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Content.Items.Materials{
	public class WindmillSail : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Sail");
		}

		public override void SetDefaults(){
			Item.width = 44;
			Item.height = 30;
			Item.scale = 0.4f;
			Item.rare = ItemRarityID.White;
			Item.maxStack = 999;
			Item.value = 22;
		}

		public override void AddRecipes(){
			Recipe.Create(this.Type)
				.AddRecipeGroup(RecipeGroupID.Wood, 25)
				.AddIngredient(ItemID.Silk, 10)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
