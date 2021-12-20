using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Items.Energy;
using TerraScience.Content.Tiles.Multitiles;

namespace TerraScience.Content.Items.Materials {
	public class BasicMachineCore : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Basic Machine Core");
			Tooltip.SetDefault("The core component for basic machinery");
		}

		public override void SetDefaults(){
			item.width = 22;
			item.height = 28;
			item.scale = 0.8f;
			item.rare = ItemRarityID.Blue;
			item.value = Item.buyPrice(silver: 3, copper: 20);
			item.maxStack = 999;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.GoldBar, 2);
			recipe.AddIngredient(ItemID.IronBar, 5);
			recipe.AddIngredient(ModContent.ItemType<TFWireItem>(), 8);
			recipe.AddIngredient(ModContent.ItemType<Silicon>(), 15);
			recipe.AddTile(ModContent.TileType<ScienceWorkbench>());
			recipe.SetResult(this, 1);
			recipe.AddRecipe();
		}
	}
}
