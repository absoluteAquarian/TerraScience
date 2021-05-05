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
			item.width = 32;
			item.height = 30;
			item.scale = 0.75f;
			item.rare = ItemRarityID.White;
			item.maxStack = 999;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useTime = 10;
			item.useAnimation = 15;
			item.createTile = ModContent.TileType<TFWireTile>();
			item.value = 5;
			item.consumable = true;
			item.autoReuse = true;
			item.useTurn = true;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.CopperBar, 1);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this, 25);
			recipe.AddRecipe();
		}
	}
}
