using SerousEnergyLib.Items;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace TerraScience.Common.Systems {
	public class RecipeCache : ModSystem {
		public static Recipe[] EnabledRecipes { get; private set; }

		public static Dictionary<int, Recipe[]> MachineItemToRecipes { get; private set; }

		public override void PostSetupRecipes() {
			EnabledRecipes = Main.recipe.Take(Recipe.numRecipes).Where(static r => !r.Disabled).ToArray();

			MachineItemToRecipes = EnabledRecipes
				.Where(static r => r.createItem.ModItem is BaseMachineItem)
				.GroupBy(static r => r.createItem.type)
				.ToDictionary(static x => x.Key, static x => x.ToArray());
		}
	}
}
