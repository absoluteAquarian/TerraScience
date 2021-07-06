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
			item.width = 16;
			item.height = 16;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useTurn = true;
			item.autoReuse = true;
			item.consumable = true;
			item.maxStack = 999;
			item.createTile = ModContent.TileType<BlastBrickTile>();
			item.value = 2;
			item.rare = ItemRarityID.Blue;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.GrayBrick, 1);
			recipe.AddIngredient(ModContent.ItemType<Coal>(), 5);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this, 1);
			recipe.AddRecipe();
		}
	}
}
