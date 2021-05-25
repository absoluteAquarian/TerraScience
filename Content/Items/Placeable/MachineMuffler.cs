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
			item.width = 32;
			item.height = 32;
			item.scale = 0.8f;
			item.useTime = 10;
			item.useAnimation = 15;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.value = Item.buyPrice(silver: 8);
			item.maxStack = 999;
			item.createTile = ModContent.TileType<MachineMufflerTile>();
			item.useTurn = true;
			item.autoReuse = true;
			item.rare = ItemRarityID.Blue;
			item.consumable = true;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddRecipeGroup(RecipeGroupID.Wood, 15);
			recipe.AddIngredient(ItemID.Silk, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this, 1);
			recipe.AddRecipe();
		}
	}
}
