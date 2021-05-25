using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Tiles;

namespace TerraScience.Content.Items.Placeable{
	public class ItemTransport : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Item Pipe");
			Tooltip.SetDefault("Transfers items between machines and chests");
		}

		public override void SetDefaults(){
			item.width = 34;
			item.height = 8;
			item.scale = 0.75f;
			item.rare = ItemRarityID.White;
			item.maxStack = 999;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useTime = 10;
			item.useAnimation = 15;
			item.createTile = ModContent.TileType<ItemTransportTile>();
			item.value = 5;
			item.consumable = true;
			item.autoReuse = true;
			item.useTurn = true;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 1);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this, 25);
			recipe.AddRecipe();
		}
	}
}
