using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using TerraScience.Content.Items;
using TerraScience.Content.Items.Placeable.Machines;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.UI;

namespace TerraScience.Utilities{
	public static class RecipeUtils{
		public static void SimpleRecipe(int itemID, int stack, int tileID, int resultID, int resultStack){
			ModRecipe recipe = new ModRecipe(TerraScience.Instance);

			recipe.AddIngredient(itemID, stack);
			recipe.AddTile(tileID);
			recipe.SetResult(resultID, resultStack);
			recipe.AddRecipe();
		}

		public static void SimpleRecipe(int itemID, int stack, int tileID, ModItem result, int resultStack){
			ModRecipe recipe = new ModRecipe(TerraScience.Instance);

			recipe.AddIngredient(itemID, stack);
			recipe.AddTile(tileID);
			recipe.SetResult(result, resultStack);
			recipe.AddRecipe();
		}

		/// <param name="group">Vanilla recipe groups consist of "Wood", "IronBar", "PresurePlate", "Sand", and "Fragment".</param>
		public static void SimpleRecipe(string group, int stack, int tileID, int resultID, int resultStack){
			ModRecipe recipe = new ModRecipe(TerraScience.Instance);

			recipe.AddRecipeGroup(group, stack);
			recipe.AddTile(tileID);
			recipe.SetResult(resultID, resultStack);
			recipe.AddRecipe();
		}

		/// <param name="group">Vanilla recipe groups consist of "Wood", "IronBar", "PresurePlate", "Sand", and "Fragment".</param>
		public static void SimpleRecipe(string group, int stack, int tileID, ModItem result, int resultStack){
			ModRecipe recipe = new ModRecipe(TerraScience.Instance);

			recipe.AddRecipeGroup(group, stack);
			recipe.AddTile(tileID);
			recipe.SetResult(result, resultStack);
			recipe.AddRecipe();
		}

		public static void ScienceWorkbenchRecipe<T>(Mod mod, (short type, int stack)[] ingredients) where T : MachineItem{
			if(ingredients.Length != 9)
				throw new ArgumentException($"Expected 9 ingredients for a Science Workbench recipe, only found {ingredients.Length}");
			
			string name = typeof(T).Name;
			if(!name.EndsWith("Item"))
				throw new ArgumentException("Expected a name that ends with \"Item\", found \"" + name + "\" instead.");

			ScienceRecipe recipe = new ScienceRecipe(mod);
			
			recipe.AddIngredient(ingredients[0].type, ingredients[0].stack);
			recipe.AddIngredient(ingredients[1].type, ingredients[1].stack);
			recipe.AddIngredient(ingredients[2].type, ingredients[2].stack);

			recipe.AddIngredient(ingredients[3].type, ingredients[3].stack);
			recipe.AddIngredient(ingredients[4].type, ingredients[4].stack);
			recipe.AddIngredient(ingredients[5].type, ingredients[5].stack);

			recipe.AddIngredient(ingredients[6].type, ingredients[6].stack);
			recipe.AddIngredient(ingredients[7].type, ingredients[7].stack);
			recipe.AddIngredient(ingredients[8].type, ingredients[8].stack);

			recipe.AddTile(ModContent.TileType<ScienceWorkbench>());
			recipe.SetResult(mod.ItemType("Dataless" + name.Substring(0, name.LastIndexOf("Item"))), 1);
			recipe.AddRecipe();
		}

		public static void ScienceWorkbenchRecipe<T>(Mod mod, (int type, int stack)[] ingredients) where T : MachineItem{
			if(ingredients.Length != 9)
				throw new ArgumentException($"Expected 9 ingredients for a Science Workbench recipe, only found {ingredients.Length}");
			
			string name = typeof(T).Name;
			if(!name.EndsWith("Item"))
				throw new ArgumentException("Expected a name that ends with \"Item\", found \"" + name + "\" instead.");

			ScienceRecipe recipe = new ScienceRecipe(mod);
			
			recipe.AddIngredient(ingredients[0].type, ingredients[0].stack);
			recipe.AddIngredient(ingredients[1].type, ingredients[1].stack);
			recipe.AddIngredient(ingredients[2].type, ingredients[2].stack);

			recipe.AddIngredient(ingredients[3].type, ingredients[3].stack);
			recipe.AddIngredient(ingredients[4].type, ingredients[4].stack);
			recipe.AddIngredient(ingredients[5].type, ingredients[5].stack);

			recipe.AddIngredient(ingredients[6].type, ingredients[6].stack);
			recipe.AddIngredient(ingredients[7].type, ingredients[7].stack);
			recipe.AddIngredient(ingredients[8].type, ingredients[8].stack);

			recipe.AddTile(ModContent.TileType<ScienceWorkbench>());
			recipe.SetResult(mod.ItemType("Dataless" + name.Substring(0, name.LastIndexOf("Item"))), 1);
			recipe.AddRecipe();
		}

		public static bool HasRecipe(ScienceWorkbenchUI ui, out int resultType, out int resultStack, List<Recipe> newRecipes){
			newRecipes.Clear();

			FieldInfo field = typeof(TerraScience).GetField("recipes", BindingFlags.Instance | BindingFlags.NonPublic);
			IList<ModRecipe> allRecipes = (IList<ModRecipe>)field.GetValue(ModContent.GetInstance<TerraScience>());
			List<ModRecipe> workbenchRecipes = allRecipes.Where(r => r.requiredTile.Contains(ModContent.TileType<ScienceWorkbench>())).ToList();

			List<Item> items = new List<Item>();
			bool recipeFound = false;
			foreach(ModRecipe recipe in workbenchRecipes){
				recipeFound = false;

				items.Clear();
				for(int slot = 0; slot < ui.SlotsLength - 1; slot++)
					items.Add(ui.GetSlot(slot).StoredItem);

				//Search through the recipes
				for(int i = ui.SlotsLength - 2; i >= 0; i--){
					bool prevFound = recipeFound;

					Item slotItem = items[i];
					Item recipeItem = recipe.requiredItem[i];

					//If this slot is empty, but the recipe's slot isn't, then we don't have a recipe
					if(slotItem.IsAir && !recipeItem.IsAir){
						recipeFound = false;
						break;
					}

					//If both slots are empty, then there isn't an item here (wow, shocking)
					if(slotItem.IsAir && recipeItem.IsAir)
						continue;

					//If both slots are not empty, then we might have a valid recipe.  Check the types and stacks
					if(!slotItem.IsAir && !recipeItem.IsAir && slotItem.type == recipeItem.type && slotItem.stack >= recipeItem.stack)
						recipeFound = true;
					else
						recipeFound = false;

					//No recipe will have only one ingredient
					if(recipeFound && !prevFound && i == 0)
						recipeFound = false;
				}

				//If 'recipeFound' is still true here, then it's a good recipe.  Use it!
				if(recipeFound)
					newRecipes.Add(recipe);
			}

			if(newRecipes.Count > 0){
				if(ui.CurrentRecipe >= newRecipes.Count)
					ui.CurrentRecipe = newRecipes.Count - 1;

				Recipe recipe = newRecipes[ui.CurrentRecipe];
				resultType = recipe.createItem.type;
				resultStack = recipe.createItem.stack;

				return true;
			}

			resultType = 0;
			resultStack = 0;
			return false;
		}
	}
}
