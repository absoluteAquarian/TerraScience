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
			item.maxStack = 999;
			item.width = 16;
			item.height = 16;
			item.useTime = 10;
			item.useAnimation = 15;
			item.useTurn = true;
			item.autoReuse = true;
			item.value = Item.sellPrice(copper: 50);
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.createTile = ModContent.TileType<Tiles.Multitiles.MachineSupport>();
			item.rare = ItemRarityID.White;
			item.consumable = true;
		}

		public override void AddRecipes(){
			RecipeUtils.SimpleRecipe(ModContent.ItemType<IronPipe>(), 4, TileID.Anvils, this, 8);
		}
	}
}
