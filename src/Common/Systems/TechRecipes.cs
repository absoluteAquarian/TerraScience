using SerousEnergyLib.API;
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
