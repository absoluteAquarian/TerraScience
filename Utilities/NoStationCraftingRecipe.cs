using Terraria.ModLoader;
using Terraria;

namespace TerraScience.Utilities{
	public class NoStationCraftingRecipe : Recipe {
		public NoStationCraftingRecipe(Mod mod) : base(mod){ }

		public override bool RecipeAvailable() => false;
	}
}
