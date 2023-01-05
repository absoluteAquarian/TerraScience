using Terraria.ID;
using TerraScience.Content.Tiles.Networks.Items;

namespace TerraScience.Content.Items.Networks.Items {
	public class BasicItemTransportItem : BaseNetworkEntryPlacingItem<BasicItemTransportTile> {
		public override void SetDefaults() {
			base.SetDefaults();

			Item.width = 34;
			Item.height = 8;
			Item.scale = 0.75f;
			Item.value = 5;
		}

		public override void AddRecipes() {
			CreateRecipe(25)
				.AddRecipeGroup(RecipeGroupID.IronBar, 1)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
