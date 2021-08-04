using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Tiles.Multitiles;

namespace TerraScience.Content.Items.Placeable{
	public class BlastBrick : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Blast Furnace Brick");
			Tooltip.SetDefault("Bricks that are able to withstand the high temperatures" +
				"\nneeded in a Blast Furnace.");
		}

		public override void SetDefaults(){
			Item.width = 16;
			Item.height = 16;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.consumable = true;
			Item.maxStack = 999;
			Item.createTile = ModContent.TileType<BlastBrickTile>();
			Item.value = 2;
			Item.rare = ItemRarityID.Blue;
		}

		public override void AddRecipes(){
			CreateRecipe(1)
				.AddIngredient(ItemID.GrayBrick, 1)
				.AddIngredient(ModContent.ItemType<Coal>(), 5)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
