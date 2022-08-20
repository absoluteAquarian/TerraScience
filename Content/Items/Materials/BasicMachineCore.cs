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
			Item.width = 22;
			Item.height = 28;
			Item.scale = 0.8f;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(silver: 3, copper: 20);
			Item.maxStack = 999;
		}

		public override void AddRecipes(){
			Recipe.Create(this.Type, 1)
				.AddIngredient(ItemID.GoldBar, 2)
				.AddIngredient(ItemID.IronBar, 5)
				.AddIngredient(ModContent.ItemType<TFWireItem>(), 8)
				.AddIngredient(ModContent.ItemType<Silicon>(), 15)
				.AddTile(ModContent.TileType<ScienceWorkbench>())
				.Register();
		}
	}
}
