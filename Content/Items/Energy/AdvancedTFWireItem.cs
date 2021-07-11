using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Tiles.Energy;

namespace TerraScience.Content.Items.Energy{
	public class AdvancedTFWireItem : TFWireItem{
		public override void SetStaticDefaults(){
			base.SetStaticDefaults();
			DisplayName.SetDefault("Advanced Wire");
		}

		public override void SetDefaults(){
			base.SetDefaults();
			item.rare = ItemRarityID.Green;
			item.createTile = ModContent.TileType<AdvancedTFWireTile>();
			item.value = Item.buyPrice(silver: 4);
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.GoldBar, 1);
			recipe.AddIngredient(ModContent.ItemType<TFWireItem>(), 25);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this, 25);
			recipe.AddRecipe();
		}
	}
}
