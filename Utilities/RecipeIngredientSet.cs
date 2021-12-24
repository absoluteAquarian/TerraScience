using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;

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

		private IngredientEntry[] entries = new IngredientEntry[14];
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

					item.SetDefaults(group.ValidItems[0]);
					SetRecipeMaterialDisplayName(item);
				}
				
				item.stack = entry.stack;

				return item;
			}
		}

		//Copied from Main.cs
		private void SetRecipeMaterialDisplayName(Item item){
			string hoverItemName = item.Name;
			if(Main.recipe[recipeIndex].ProcessGroupsForText(item.type, out string theText))
				hoverItemName = theText;

			if(Main.recipe[recipeIndex].anyIronBar && item.type == ItemID.IronBar)
				hoverItemName = Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.IronBar);
			else if(Main.recipe[recipeIndex].anyWood && item.type == ItemID.Wood)
				hoverItemName = Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.Wood);
			else if(Main.recipe[recipeIndex].anySand && item.type == ItemID.SandBlock)
				hoverItemName = Language.GetTextValue("LegacyMisc.37") + " " + Lang.GetItemNameValue(ItemID.SandBlock);
			else if(Main.recipe[recipeIndex].anyFragment && item.type == ItemID.FragmentSolar)
				hoverItemName = Language.GetTextValue("LegacyMisc.37") + " " + Language.GetTextValue("LegacyMisc.51");
			else if(Main.recipe[recipeIndex].anyPressurePlate && item.type == ItemID.GrayPressurePlate)
				hoverItemName = Language.GetTextValue("LegacyMisc.37") + " " + Language.GetTextValue("LegacyMisc.38");

			if(item.stack > 1)
				hoverItemName = hoverItemName + " (" + item.stack + ")";

			item.SetNameOverride(hoverItemName);
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
