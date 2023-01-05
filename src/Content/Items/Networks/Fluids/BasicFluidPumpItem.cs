using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Tiles.Networks.Fluids;

namespace TerraScience.Content.Items.Networks.Fluids {
	public class BasicFluidPumpItem : BasePumpItem<BasicFluidPumpTile> {
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
				.AddIngredient(ModContent.ItemType<BasicFluidTransportItem>(), 5)
				.AddRecipeGroup(RecipeGroupID.IronBar, 3)
				.AddIngredient(ItemID.Gel, 10)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
