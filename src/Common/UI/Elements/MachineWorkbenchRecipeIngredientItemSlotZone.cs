using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace TerraScience.Common.UI.Elements {
	public class MachineWorkbenchRecipeIngredientItemSlotZone : UIElement {
		private readonly List<MachineWorkbenchRecipeIngredientItemSlot> slots;

		public IReadOnlyList<MachineWorkbenchRecipeIngredientItemSlot> Slots => slots.AsReadOnly();

		public int DefaultContext { get; set; }

		public float DefaultScale { get; set; }

		public MachineWorkbenchRecipeIngredientItemSlotZone(int context = ItemSlot.Context.BankItem, float scale = 1) {
			slots = new();

			DefaultContext = context;
			DefaultScale = scale;
		}

		public void InitializeSlots(Recipe recipe, int maxSlotsPerRow) {
			foreach (var instance in slots)
				instance?.Remove();

			slots.Clear();

			if (recipe is null)
				return;

			int numSlot = 0;
			float top = 0;

			int slotWidth = TextureAssets.InventoryBack9.Value.Width + 5;
			int slotHeight = TextureAssets.InventoryBack9.Value.Height + 5;

			for (int slot = 0; slot < recipe.requiredItem.Count; slot++) {
				var instance = new MachineWorkbenchRecipeIngredientItemSlot(recipe, slot, DefaultContext, DefaultScale);

				instance.Left.Set(numSlot * slotWidth, 0f);
				instance.Top.Set(top, 0f);

				if (++numSlot >= maxSlotsPerRow) {
					top += slotHeight;
					numSlot = 0;
				}

				slots.Add(instance);

				Append(instance);
			}

			Width.Set(maxSlotsPerRow * slotWidth, 0f);
			Height.Set(top + slotHeight, 0f);

			Recalculate();
		}
	}
}
