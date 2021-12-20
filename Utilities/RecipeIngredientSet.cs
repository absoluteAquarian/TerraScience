using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace TerraScience.Utilities{
	public class RecipeIngredientSet{
		private struct IngredientEntry{
			public bool isRecipeGroup;
			public int ID;
			public int stack;

			public static IngredientEntry AddIngredient(int itemType, int stack)
				=> new IngredientEntry(){
					isRecipeGroup = false,
					ID = itemType,
					stack = stack
				};

			public static IngredientEntry AddRecipeGroup(int group, int stack)
				=> new IngredientEntry(){
					isRecipeGroup = true,
					ID = group,
					stack = stack
				};

			public static IngredientEntry AddRecipeGroup(string group, int stack){
				if(!RecipeGroup.recipeGroupIDs.TryGetValue(group, out int id))
					throw new ArgumentException($"A recipe group with the name \"{group}\" does not exist");

				return new IngredientEntry(){
					isRecipeGroup = true,
					ID = id,
					stack = stack
				};
			}
		}

		private List<IngredientEntry> entries = new List<IngredientEntry>();

		public RecipeIngredientSet AddIngredient(int itemType, int stack = 1){
			entries.Add(IngredientEntry.AddIngredient(itemType, stack));
			return this;
		}

		public RecipeIngredientSet AddIngredient<T>(int stack = 1) where T : ModItem{
			entries.Add(IngredientEntry.AddIngredient(ModContent.ItemType<T>(), stack));
			return this;
		}

		public RecipeIngredientSet AddRecipeGroup(int group, int stack = 1){
			entries.Add(IngredientEntry.AddRecipeGroup(group, stack));
			return this;
		}

		public RecipeIngredientSet AddRecipeGroup(string group, int stack = 1){
			entries.Add(IngredientEntry.AddRecipeGroup(group, stack));
			return this;
		}

		public void Apply(ModRecipe recipe){
			if(recipe.RecipeIndex > 0)
				throw new ArgumentException("Recipe has already been added to the game.  Cannot add ingredients to the recipe instance");

			foreach(var entry in entries){
				if(entry.isRecipeGroup)
					recipe.AddRecipeGroup(entry.ID, entry.stack);
				else
					recipe.AddIngredient(entry.ID, entry.stack);
			}
		}
	}
}
