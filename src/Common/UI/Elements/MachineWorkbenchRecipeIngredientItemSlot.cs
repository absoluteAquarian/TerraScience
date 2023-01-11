using SerousCommonLib.UI;
using Terraria;
using Terraria.UI;
using TerraScience.Common.Systems;

namespace TerraScience.Common.UI.Elements {
	public class MachineWorkbenchRecipeIngredientItemSlot : EnhancedItemSlot {
		public MachineWorkbenchRecipeIngredientItemSlot(Recipe recipe, int slot, int context = ItemSlot.Context.BankItem, float scale = 1) : base(slot, context, scale) {
			storedItem = TechRecipes.GetIngredientItem(recipe, slot);
			IgnoreClicks = true;
		}
	}
}
