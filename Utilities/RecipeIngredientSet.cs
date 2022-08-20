using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;
using System.Linq;
using System.Reflection;

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

		private readonly IngredientEntry[] entries = new IngredientEntry[14];
		private int curIndex = 0;

		internal int recipeIndex;

		public Item this[int index]{
			get{
				//Construct the item based on the entry
				IngredientEntry entry = entries[index];

				Item item = new Item();

				if(!entry.isRecipeGroup)
					item.SetDefaults(entry.ID);
				else{
					RecipeGroup group = RecipeGroup.recipeGroups[entry.ID];

					item.SetDefaults(group.ValidItems.First());
					Main.instance.GetType()
						.GetMethod("SetRecipeMaterialDisplayName", BindingFlags.NonPublic | BindingFlags.Instance)
						.Invoke(Main.instance, new object[]{entry.ID});
				}
				
				item.stack = entry.stack;

				return item;
			}
		}

		public RecipeIngredientSet AddIngredient(int itemType, int stack = 1){
			if(curIndex == entries.Length)
				throw new InvalidOperationException("Too many ingredients added.  Recipes can only have 14 ingredient slots");

			entries[curIndex++] = IngredientEntry.AddIngredient(itemType, stack);
			return this;
		}

		public RecipeIngredientSet AddIngredient<T>(int stack = 1) where T : ModItem{
			if(curIndex == entries.Length)
				throw new InvalidOperationException("Too many ingredients added.  Recipes can only have 14 ingredient slots");

			entries[curIndex++] = IngredientEntry.AddIngredient(ModContent.ItemType<T>(), stack);
			return this;
		}

		public RecipeIngredientSet AddRecipeGroup(int group, int stack = 1){
			if(curIndex == entries.Length)
				throw new InvalidOperationException("Too many ingredients added.  Recipes can only have 14 ingredient slots");

			entries[curIndex++] = IngredientEntry.AddRecipeGroup(group, stack);
			return this;
		}

		public RecipeIngredientSet AddRecipeGroup(string group, int stack = 1){
			if(curIndex == entries.Length)
				throw new InvalidOperationException("Too many ingredients added.  Recipes can only have 14 ingredient slots");

			entries[curIndex++] = IngredientEntry.AddRecipeGroup(group, stack);
			return this;
		}

		public void Apply(Recipe recipe){
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
