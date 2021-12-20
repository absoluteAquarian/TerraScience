using Terraria.ModLoader;

namespace TerraScience.Utilities{
	public class NoStationCraftingRecipe : ModRecipe{
		public NoStationCraftingRecipe(Mod mod) : base(mod){ }

		public override bool RecipeAvailable() => false;
	}
}
