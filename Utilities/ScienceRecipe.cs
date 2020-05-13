using Terraria.ModLoader;

namespace TerraScience.Utilities{
	public class ScienceRecipe : ModRecipe{
		public ScienceRecipe(Mod mod) : base(mod){ }

		//The item should only be crafted through the Science Workbench's UI
		public override bool RecipeAvailable() => false;
	}
}
