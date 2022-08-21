using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Items.Materials;
using TerraScience.Utilities;

namespace TerraScience.Content.Items.Placeable{
	public class MachineSupportItem : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Machine Support");
			Tooltip.SetDefault("Used as a replacement for empty space in a multitile machine");
		}

		public override void SetDefaults(){
			Item.maxStack = 999;
			Item.width = 16;
			Item.height = 16;
			Item.useTime = 10;
			Item.useAnimation = 15;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.value = Item.sellPrice(copper: 50);
			Item.useStyle = ItemUseStyleID.Swing;
			Item.createTile = ModContent.TileType<Tiles.Multitiles.MachineSupport>();
			Item.rare = ItemRarityID.White;
			Item.consumable = true;
		}

		public override void AddRecipes(){
			RecipeUtils.SimpleRecipe(ModContent.ItemType<IronPipe>(), 4, TileID.Anvils, this, 8);
		}
	}
}
