using SerousEnergyLib.API;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
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
			Sets.ReinforcedFurnace = new List<MachineRecipe>() {
				new MachineRecipe<ReinforcedFurnace>()
					.AddRecipeGroup(RecipeGroupID.Wood, 1)
					.AddPossibleOutput<Charcoal>(1)
					.CreateAndRegisterAllPossibleRecipes()
			};
		}

		public override void Unload() {
			Sets.ReinforcedFurnace = null;
		}

		public static class Sets {
			public static List<MachineRecipe> ReinforcedFurnace { get; internal set; }
		}
	}
}
