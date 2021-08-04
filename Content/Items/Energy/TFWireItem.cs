using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Tiles.Energy;

namespace TerraScience.Content.Items.Energy{
	public class TFWireItem : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Wire");
			Tooltip.SetDefault("Used to connect machines to energy sources");
		}

		public override void SetDefaults(){
			Item.width = 32;
			Item.height = 30;
			Item.scale = 0.75f;
			Item.rare = ItemRarityID.White;
			Item.maxStack = 999;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 10;
			Item.useAnimation = 15;
			Item.createTile = ModContent.TileType<TFWireTile>();
			Item.value = 5;
			Item.consumable = true;
			Item.autoReuse = true;
			Item.useTurn = true;
		}

		public override void AddRecipes(){
			CreateRecipe(25)
				.AddIngredient(ItemID.CopperBar, 1)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
