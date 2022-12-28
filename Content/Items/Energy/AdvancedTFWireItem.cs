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
			Item.rare = ItemRarityID.Green;
			Item.createTile = ModContent.TileType<AdvancedTFWireTile>();
			Item.value = Item.buyPrice(silver: 4);
		}

		public override void AddRecipes(){
			Recipe.Create(this.Type, 25)
				.AddIngredient(ItemID.GoldBar, 1)
				.AddIngredient(ModContent.ItemType<TFWireItem>(), 25)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
