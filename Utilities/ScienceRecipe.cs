using Terraria.ModLoader;

namespace TerraScience.Utilities{
	public class ScienceRecipe : ModRecipe{
		public ScienceRecipe(Mod mod) : base(mod){ }

		//Cannot be crafted via normal means
		public override bool RecipeAvailable() => false;
	}
}
