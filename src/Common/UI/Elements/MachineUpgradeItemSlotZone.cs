using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.UI;

namespace TerraScience.Common.UI.Elements {
	public class MachineUpgradeItemSlotZone : UIElement {
		private readonly List<MachineUpgradeItemSlot> slots;

		public IReadOnlyList<MachineUpgradeItemSlot> Slots => slots.AsReadOnly();

		public int DefaultContext { get; set; }

		public float DefaultScale { get; set; }

		public MachineUpgradeItemSlotZone(int context = ItemSlot.Context.InventoryItem, float scale = 1) {
			slots = new();

			DefaultContext = context;
			DefaultScale = scale;
		}

		/// <inheritdoc cref="MachineUpgradeItemSlot.OnUpdateItem"/>
		public event MachineUpgradeSlotUpdateItemDelegate OnUpdateItem;

		/// <inheritdoc cref="MachineUpgradeItemSlot.OnRemoveItem"/>
		public event MachineUpgradeSlotRemovedDelegate OnRemoveItem;

		public void InitializeSlots(int slotCount, int maxSlotsPerRow) {
			foreach (var instance in slots)
				instance?.Remove();

			slots.Clear();

			int numSlot = 0;
			float top = 0;

			int slotWidth = TextureAssets.InventoryBack9.Value.Width + 5;
			int slotHeight = TextureAssets.InventoryBack9.Value.Height + 5;

			for (int slot = 0; slot < slotCount; slot++) {
				var instance = new MachineUpgradeItemSlot(slot, DefaultContext, DefaultScale);
				instance.OnUpdateItem += (machine, oldItem, newItem) => OnUpdateItem?.Invoke(machine, oldItem, newItem);
				instance.OnRemoveItem += (machine, slot, oldItem) => OnRemoveItem?.Invoke(machine, slot, oldItem);

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
