using SerousEnergyLib.API;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.Tiles.Machines;

namespace TerraScience.Common.Systems {
	public class TechRecipes : ModSystem {
		public override void AddRecipeGroups() {
			RegisterRecipeGroup(nameof(ItemID.CopperBar), ItemID.CopperBar, ItemID.TinBar);
		}

		public static void RegisterRecipeGroup(string groupName, params int[] validTypes) {
			int displayedItemType = validTypes[0];

			RecipeGroup.RegisterGroup(groupName, new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(displayedItemType)}", validTypes));
		}

		public override void AddRecipes() {
			LoaderUtils.ResetStaticMembers(typeof(Sets), true);

			Ticks woodToCharcoalMin = TechMod.Sets.ReinforcedFurnace.ConversionDuration.Min();
			Ticks woodToCharcoalMax = TechMod.Sets.ReinforcedFurnace.ConversionDuration.Max();

			Sets.ReinforcedFurnace.all.Add(new MachineRecipe<ReinforcedFurnace>()
				.AddRecipeGroup(RecipeGroupID.Wood, 1)
				.AddPossibleOutput<Charcoal>(1)
				.AddTimeVarianceRequirement(woodToCharcoalMin, woodToCharcoalMax)
				.CreateAndRegisterAllPossibleRecipes());
		}

		public override void Unload() {
			Sets.ReinforcedFurnace.all = null;
		}

		public static Item GetIngredientItem(Recipe recipe, int index) {
			if (recipe is null || index < 0 || index >= recipe.requiredItem.Count)
				return new Item();

			Item item = recipe.requiredItem[index].Clone();
			if (recipe.HasRecipeGroup(RecipeGroupID.Wood) && item.type == ItemID.Wood)
				item.SetNameOverride(Language.GetText("LegacyMisc.37").Value + " " + Lang.GetItemNameValue(ItemID.Wood));
			if (recipe.HasRecipeGroup(RecipeGroupID.Sand) && item.type == ItemID.SandBlock)
				item.SetNameOverride(Language.GetText("LegacyMisc.37").Value + " " + Lang.GetItemNameValue(ItemID.SandBlock));
			if (recipe.HasRecipeGroup(RecipeGroupID.IronBar) && item.type == ItemID.IronBar)
				item.SetNameOverride(Language.GetText("LegacyMisc.37").Value + " " + Lang.GetItemNameValue(ItemID.IronBar));
			if (recipe.HasRecipeGroup(RecipeGroupID.Fragment) && item.type == ItemID.FragmentSolar)
				item.SetNameOverride(Language.GetText("LegacyMisc.37").Value + " " + Language.GetText("LegacyMisc.51").Value);
			if (recipe.HasRecipeGroup(RecipeGroupID.PressurePlate) && item.type == ItemID.GrayPressurePlate)
				item.SetNameOverride(Language.GetText("LegacyMisc.37").Value + " " + Language.GetText("LegacyMisc.38").Value);
			if (ProcessGroupsForText(recipe, item.type, out string nameOverride))
				item.SetNameOverride(nameOverride);

			return item;
		}

		internal static bool ProcessGroupsForText(Recipe recipe, int type, out string theText) {
			foreach (int num in recipe.acceptedGroups) {
				if (RecipeGroup.recipeGroups[num].ContainsItem(type)) {
					theText = RecipeGroup.recipeGroups[num].GetText();
					return true;
				}
			}

			theText = "";
			return false;
		}

		public static class Sets {
			public static void RegisterRecipe(List<MachineRecipe> all, ref MachineRecipe recipeField, MachineRecipe recipeObject) {
				recipeField = recipeObject;
				all.Add(recipeField);
			}

			public static class ReinforcedFurnace {
				internal static List<MachineRecipe> all = new();
				public static IReadOnlyList<MachineRecipe> All => all;
			}
		}
	}
}
