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

		public static void ScienceWorkbenchRecipe<T>(Mod mod, (short type, int stack)[] ingredients) where T : MachineItem{
			if(ingredients.Length != 9)
				throw new ArgumentException($"Expected 9 ingredients for a Science Workbench recipe, only found {ingredients.Length}");
			
			string name = typeof(T).Name;
			if(!name.EndsWith("Item"))
				throw new ArgumentException("Expected a name that ends with \"Item\", found \"" + name + "\" instead.");

			Recipe.Create(mod.Find<ModItem>("Dataless" + name.Substring(0, name.LastIndexOf("Item"))).Type, 1)
				.AddIngredient(ingredients[0].type, ingredients[0].stack)
				.AddIngredient(ingredients[1].type, ingredients[1].stack)
				.AddIngredient(ingredients[2].type, ingredients[2].stack)
				.AddIngredient(ingredients[3].type, ingredients[3].stack)
				.AddIngredient(ingredients[4].type, ingredients[4].stack)
				.AddIngredient(ingredients[5].type, ingredients[5].stack)
				.AddIngredient(ingredients[6].type, ingredients[6].stack)
				.AddIngredient(ingredients[7].type, ingredients[7].stack)
				.AddIngredient(ingredients[8].type, ingredients[8].stack)
				.AddTile(ModContent.TileType<ScienceWorkbench>())
				.AddCondition(MadeAtMachine)
				.Register();
		}

		public static void ScienceWorkbenchRecipe<T>(Mod mod, (int type, int stack)[] ingredients) where T : MachineItem{
			if(ingredients.Length != 9)
				throw new ArgumentException($"Expected 9 ingredients for a Science Workbench recipe, only found {ingredients.Length}");
			
			string name = typeof(T).Name;
			if(!name.EndsWith("Item"))
				throw new ArgumentException("Expected a name that ends with \"Item\", found \"" + name + "\" instead.");

			Recipe.Create(mod.Find<ModItem>("Dataless" + name.Substring(0, name.LastIndexOf("Item"))).Type, 1)
				.AddIngredient(ingredients[0].type, ingredients[0].stack)
				.AddIngredient(ingredients[1].type, ingredients[1].stack)
				.AddIngredient(ingredients[2].type, ingredients[2].stack)
				.AddIngredient(ingredients[3].type, ingredients[3].stack)
				.AddIngredient(ingredients[4].type, ingredients[4].stack)
				.AddIngredient(ingredients[5].type, ingredients[5].stack)
				.AddIngredient(ingredients[6].type, ingredients[6].stack)
				.AddIngredient(ingredients[7].type, ingredients[7].stack)
				.AddIngredient(ingredients[8].type, ingredients[8].stack)
				.AddTile(ModContent.TileType<ScienceWorkbench>())
				.AddCondition(MadeAtMachine)
				.Register();
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
