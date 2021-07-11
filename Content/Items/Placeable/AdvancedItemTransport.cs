using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Tiles;

namespace TerraScience.Content.Items.Placeable{
	public class AdvancedItemTransport : ItemTransport{
		public override void SetStaticDefaults(){
			base.SetStaticDefaults();
			DisplayName.SetDefault("Advanced Item Pipe");
		}

		public override void SetDefaults(){
			base.SetDefaults();
			item.rare = ItemRarityID.Green;
			item.createTile = ModContent.TileType<AdvancedItemTransportTile>();
			item.value = Item.buyPrice(silver: 5);
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddRecipeGroup(TechMod.ScienceRecipeGroups.EvilBars, 1);
			recipe.AddIngredient(ModContent.ItemType<ItemTransport>(), 25);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this, 25);
			recipe.AddRecipe();
		}
	}
}
