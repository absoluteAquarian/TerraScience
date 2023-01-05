using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Tiles.Networks.Fluids;

namespace TerraScience.Content.Items.Networks.Fluids {
	public class BasicFluidTransportItem : BaseNetworkEntryPlacingItem<BasicFluidTransportTile> {
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
				.AddIngredient(ItemID.WaterBucket, 1)
				.AddTile(TileID.Anvils)
				.Register();
		}

		public override void OnCreate(ItemCreationContext context) {
			if (context is RecipeCreationContext)
				Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_DropAsItem(), ItemID.EmptyBucket, 1);
		}
	}
}
