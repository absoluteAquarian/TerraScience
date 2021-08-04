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
			Item.width = 34;
			Item.height = 8;
			Item.scale = 0.75f;
			Item.rare = ItemRarityID.White;
			Item.maxStack = 999;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 10;
			Item.useAnimation = 15;
			Item.createTile = ModContent.TileType<ItemTransportTile>();
			Item.value = 5;
			Item.consumable = true;
			Item.autoReuse = true;
			Item.useTurn = true;
		}

		public override void AddRecipes(){
			CreateRecipe(25)
				.AddRecipeGroup(RecipeGroupID.IronBar, 1)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
