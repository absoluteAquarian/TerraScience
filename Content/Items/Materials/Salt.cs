using Terraria;
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
			Item.width = 28;
			Item.height = 22;
			Item.scale = 0.6f;
			Item.rare = ItemRarityID.Blue;
			Item.value = 5;
			Item.maxStack = 999;
		}

		public override void AddRecipes(){
			Recipe.Create(this.Type, 1)
				.AddIngredient(ModContent.ItemType<Vial_Water>(), 2)
				.AddIngredient(ModContent.ItemType<TerraFluxIndicator>())
				.AddTile(ModContent.TileType<SaltExtractor>())
				.AddCondition(RecipeUtils.MadeAtMachine)
				.Register();

			Recipe.Create(this.Type, 3)
				.AddIngredient(ModContent.ItemType<Vial_Saltwater>(), 2)
				.AddIngredient(ModContent.ItemType<TerraFluxIndicator>())
				.AddTile(ModContent.TileType<SaltExtractor>())
				.AddCondition(RecipeUtils.MadeAtMachine)
				.Register();
		}
	}
}
