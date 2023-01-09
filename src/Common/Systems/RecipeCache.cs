using SerousEnergyLib.Items;
using SerousEnergyLib.Tiles;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace TerraScience.Common.Systems {
	public class RecipeCache : ModSystem {
		public static Recipe[] EnabledRecipes { get; private set; }

		public static Dictionary<int, Recipe[]> MachineItemToRecipes { get; private set; }

		public static Dictionary<int, Recipe[]> RecipesUsingMachineTile { get; private set; }

		public override void PostSetupRecipes() {
			EnabledRecipes = Main.recipe.Take(Recipe.numRecipes).Where(static r => !r.Disabled).ToArray();

			MachineItemToRecipes = EnabledRecipes
				.Where(static r => r.createItem.ModItem is BaseMachineItem)
				.GroupBy(static r => r.createItem.type)
				.ToDictionary(static x => x.Key, static x => x.ToArray());

			RecipesUsingMachineTile = EnabledRecipes
				.Where(static r => r.requiredTile.Count == 1 && TileLoader.GetTile(r.requiredTile[0]) is BaseMachineTile)
				.GroupBy(static r => r.requiredTile[0])
				.ToDictionary(static x => x.Key, static x => x.ToArray());
		}
	}
}
