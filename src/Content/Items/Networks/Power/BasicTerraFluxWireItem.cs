using Terraria.ID;
using TerraScience.Content.Tiles.Networks.Power;

namespace TerraScience.Content.Items.Networks.Power {
	public class BasicTerraFluxWireItem : BaseNetworkEntryPlacingItem<BasicTerraFluxWireTile> {
		public override void SetDefaults() {
			base.SetDefaults();

			Item.width = 32;
			Item.height = 30;
			Item.scale = 0.75f;
			Item.value = 5;
		}

		public override void AddRecipes() {
			CreateRecipe(25)
				.AddRecipeGroup(nameof(ItemID.CopperBar), 1)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
