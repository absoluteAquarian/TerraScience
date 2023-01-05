using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Tiles.Networks.Items;

namespace TerraScience.Content.Items.Networks.Items {
	public class BasicItemPumpItem : BasePumpItem<BasicItemPumpTile> {
		public override void SetDefaults() {
			base.SetDefaults();

			Item.width = 34;
			Item.height = 8;
			Item.scale = 0.75f;
			Item.value = 70;
			Item.rare = ItemRarityID.Blue;
		}

		public override void AddRecipes() {
			CreateRecipe(5)
				.AddIngredient(ModContent.ItemType<BasicItemTransportItem>(), 5)
				.AddRecipeGroup(RecipeGroupID.IronBar, 3)
				.AddIngredient(ItemID.Gel, 10)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
