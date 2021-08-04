using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Tiles;

namespace TerraScience.Content.Items.Placeable{
	public class MachineMuffler : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Muffler");
			Tooltip.SetDefault("Greatly quietens machines whose center is within a 40-tile radius of this tile");
		}

		public override void SetDefaults(){
			Item.width = 32;
			Item.height = 32;
			Item.scale = 0.8f;
			Item.useTime = 10;
			Item.useAnimation = 15;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.buyPrice(silver: 8);
			Item.maxStack = 999;
			Item.createTile = ModContent.TileType<MachineMufflerTile>();
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.rare = ItemRarityID.Blue;
			Item.consumable = true;
		}

		public override void AddRecipes(){
			CreateRecipe(1)
				.AddRecipeGroup(RecipeGroupID.Wood, 15)
				.AddIngredient(ItemID.Silk, 10)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
