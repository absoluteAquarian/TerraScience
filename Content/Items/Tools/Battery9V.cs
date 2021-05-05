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
			item.width = 18;
			item.height = 22;
			item.rare = ItemRarityID.White;
			item.value = Item.sellPrice(silver: 3);
			item.maxStack = 10;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.CopperBar, 1);
			recipe.AddIngredient(ItemID.IronBar, 1);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this, 5);
			recipe.AddRecipe();
		}
	}
}
