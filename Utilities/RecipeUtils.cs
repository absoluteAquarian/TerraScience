using System;
using Terraria.ModLoader;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Items.Placeable.Machines;
using TerraScience.Content.Tiles.Multitiles;

namespace TerraScience.Utilities{
	public static class RecipeUtils{
		public static void SimpleRecipe(int itemID, int stack, int tileID, int resultID, int resultStack){
			ModRecipe recipe = new ModRecipe(TechMod.Instance);

			recipe.AddIngredient(itemID, stack);
			recipe.AddTile(tileID);
			recipe.SetResult(resultID, resultStack);
			recipe.AddRecipe();
		}

		public static void SimpleRecipe(int itemID, int stack, int tileID, ModItem result, int resultStack){
			ModRecipe recipe = new ModRecipe(TechMod.Instance);

			recipe.AddIngredient(itemID, stack);
			recipe.AddTile(tileID);
			recipe.SetResult(result, resultStack);
			recipe.AddRecipe();
		}

		/// <param name="group">Vanilla recipe groups consist of "Wood", "IronBar", "PresurePlate", "Sand", and "Fragment".</param>
		public static void SimpleRecipe(string group, int stack, int tileID, int resultID, int resultStack){
			ModRecipe recipe = new ModRecipe(TechMod.Instance);

			recipe.AddRecipeGroup(group, stack);
			recipe.AddTile(tileID);
			recipe.SetResult(resultID, resultStack);
			recipe.AddRecipe();
		}

		/// <param name="group">Vanilla recipe groups consist of "Wood", "IronBar", "PresurePlate", "Sand", and "Fragment".</param>
		public static void SimpleRecipe(string group, int stack, int tileID, ModItem result, int resultStack){
			ModRecipe recipe = new ModRecipe(TechMod.Instance);

			recipe.AddRecipeGroup(group, stack);
			recipe.AddTile(tileID);
			recipe.SetResult(result, resultStack);
			recipe.AddRecipe();
		}

		public static void ScienceWorkbenchRecipe<T>(Mod mod, RecipeIngredientSet ingredients) where T : MachineItem{
			string name = typeof(T).Name;
			if(!name.EndsWith("Item"))
				throw new ArgumentException("Expected a name that ends with \"Item\", found \"" + name + "\" instead.");

			ModRecipe recipe = new ModRecipe(mod);
			
			ingredients.Apply(recipe);

			recipe.AddTile(ModContent.TileType<ScienceWorkbench>());
			recipe.SetResult(mod.ItemType("Dataless" + name.Substring(0, name.LastIndexOf("Item"))), 1);
			recipe.AddRecipe();

			ingredients.recipeIndex = recipe.RecipeIndex;
		}

		public static void CreateTFRecipe<TMachine>(int input) where TMachine : Machine{
			NoStationCraftingRecipe recipe = new NoStationCraftingRecipe(TechMod.Instance);
			recipe.AddIngredient(input, 1);
			recipe.AddTile(ModContent.TileType<TMachine>());
			recipe.SetResult(ModContent.ItemType<TerraFluxIndicator>(), 1);
			recipe.AddRecipe();
		}

		public static void CreateTFRecipeWithRecipeGroupIngredient<TMachine>(int recipeGroup) where TMachine : Machine{
			NoStationCraftingRecipe recipe = new NoStationCraftingRecipe(TechMod.Instance);
			recipe.AddRecipeGroup(recipeGroup, 1);
			recipe.AddTile(ModContent.TileType<TMachine>());
			recipe.SetResult(ModContent.ItemType<TerraFluxIndicator>(), 1);
			recipe.AddRecipe();
		}

		public static void CreateTFRecipeWithRecipeGroupIngredient<TMachine>(string recipeGroup) where TMachine : Machine{
			NoStationCraftingRecipe recipe = new NoStationCraftingRecipe(TechMod.Instance);
			recipe.AddRecipeGroup(recipeGroup, 1);
			recipe.AddTile(ModContent.TileType<TMachine>());
			recipe.SetResult(ModContent.ItemType<TerraFluxIndicator>(), 1);
			recipe.AddRecipe();
		}
	}
}
