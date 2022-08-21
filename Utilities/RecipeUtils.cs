using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Items.Placeable.Machines;
using TerraScience.Content.Tiles.Multitiles;

namespace TerraScience.Utilities{
	public static class RecipeUtils{
		public static readonly Recipe.Condition MadeAtMachine = new Recipe.Condition(NetworkText.FromKey("RecipeConditions.MadeAtMachine"), (Recipe _) => false);

		public static void SimpleRecipe(int itemID, int stack, int tileID, int resultID, int resultStack){
			Recipe.Create(resultID, resultStack)
				.AddIngredient(itemID, stack)
				.AddTile(tileID)
				.Register();
		}

		public static void SimpleRecipe(int itemID, int stack, int tileID, ModItem result, int resultStack){
			SimpleRecipe(itemID, stack, tileID, result.Type, resultStack);
		}

		/// <param name="group">Vanilla recipe groups consist of "Wood", "IronBar", "PresurePlate", "Sand", and "Fragment".</param>
		public static void SimpleRecipe(string group, int stack, int tileID, int resultID, int resultStack){
			Recipe.Create(resultID, resultStack)
				.AddRecipeGroup(group, stack)
				.AddTile(tileID)
				.Register();
		}

		/// <param name="group">Vanilla recipe groups consist of "Wood", "IronBar", "PresurePlate", "Sand", and "Fragment".</param>
		public static void SimpleRecipe(string group, int stack, int tileID, ModItem result, int resultStack){
			SimpleRecipe(group, stack, tileID, result.Type, resultStack);
		}

		public static void ScienceWorkbenchRecipe<T>(Mod mod, RecipeIngredientSet ingredients) where T : MachineItem{
			string name = typeof(T).Name;
			if(!name.EndsWith("Item"))
				throw new ArgumentException("Expected a name that ends with \"Item\", found \"" + name + "\" instead.");

			Recipe recipe = Recipe.Create(mod.Find<ModItem>("Dataless" + name.Substring(0, name.LastIndexOf("Item"))).Type, 1);
			ingredients.Apply(recipe);
			recipe.AddTile(ModContent.TileType<ScienceWorkbench>())
				.Register();
			ingredients.recipeIndex = recipe.RecipeIndex;
		}

		public static void CreateTFRecipe<TMachine>(int input) where TMachine : Machine{
			Recipe.Create(ModContent.ItemType<TerraFluxIndicator>(), 1)
				.AddCondition(MadeAtMachine)
				.AddIngredient(input, 1)
				.AddTile(ModContent.TileType<TMachine>())
				.Register();
		}

		public static void CreateTFRecipeWithRecipeGroupIngredient<TMachine>(int recipeGroup) where TMachine : Machine{
			Recipe.Create(ModContent.ItemType<TerraFluxIndicator>(), 1)
				.AddCondition(MadeAtMachine)
				.AddRecipeGroup(recipeGroup, 1)
				.AddTile(ModContent.TileType<TMachine>())
				.Register();
		}

		public static void CreateTFRecipeWithRecipeGroupIngredient<TMachine>(string recipeGroup) where TMachine : Machine{
			Recipe.Create(ModContent.ItemType<TerraFluxIndicator>(), 1)
				.AddCondition(MadeAtMachine)
				.AddRecipeGroup(recipeGroup, 1)
				.AddTile(ModContent.TileType<TMachine>())
				.Register();
		}
	}
}
