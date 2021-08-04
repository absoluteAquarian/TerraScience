using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerraScience.Content.Items.Tools{
	public class Battery9V : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("9-Volt Battery");
			Tooltip.SetDefault("Packs a lot of power in a small package" +
				"\nA necessity for some machines to operate");
		}

		public override void SetDefaults(){
			Item.width = 18;
			Item.height = 22;
			Item.rare = ItemRarityID.White;
			Item.value = Item.sellPrice(silver: 3);
			Item.maxStack = 10;
		}

		public override void AddRecipes(){
			CreateRecipe(5)
				.AddIngredient(ItemID.CopperBar, 1)
				.AddIngredient(ItemID.IronBar, 1)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
