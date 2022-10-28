using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Tiles.Multitiles;

namespace TerraScience.Content.Items.Materials {
	public class Coal : ModItem{
		public override string Texture => "Terraria/Images/Item_" + ItemID.Coal;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Charcoal");
			Tooltip.SetDefault("Carbon that's been compressed to a rough ball.");
		}

		public override void SetDefaults(){
			Item.width = 18;
			Item.height = 18;
			Item.rare = ItemRarityID.Blue;
			Item.maxStack = 999;
			Item.value = 10;
		}

		public override void AddRecipes(){
			Recipe.Create(this.Type)
				.AddRecipeGroup(RecipeGroupID.Wood)
				.AddTile(ModContent.TileType<ReinforcedFurnace>())
				.Register();
		}
	}
}
