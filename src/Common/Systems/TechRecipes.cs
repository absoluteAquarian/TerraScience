using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TerraScience.Common.Systems {
	public class TechRecipes : ModSystem {
		public override void AddRecipeGroups() {
			RegisterRecipeGroup(nameof(ItemID.CopperBar), ItemID.CopperBar, ItemID.TinBar);
		}

		public static void RegisterRecipeGroup(string groupName, params int[] validTypes) {
			int displayedItemType = validTypes[0];

			RecipeGroup.RegisterGroup(groupName, new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(displayedItemType)}", validTypes));
		}
	}
}
