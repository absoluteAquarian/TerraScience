using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.UI;

namespace TerraScience.Common.UI.Elements {
	public class MachineInventoryItemSlotZone : UIElement {
		private readonly List<MachineInventoryItemSlot> slots;

		public IReadOnlyList<MachineInventoryItemSlot> Slots => slots.AsReadOnly();

		public int DefaultContext { get; set; }

		public float DefaultScale { get; set; }

		public MachineInventoryItemSlotZone(int[] slots, int maxSlotsPerRow, int context = ItemSlot.Context.InventoryItem, float scale = 1) {
			ArgumentNullException.ThrowIfNull(slots);

			if (slots.Length == 0)
				throw new ArgumentNullException(nameof(slots), "Slots array was empty");

			this.slots = new();

			DefaultContext = context;
			DefaultScale = scale;

			InitializeSlots(slots, maxSlotsPerRow);
		}

		/// <summary>
		/// This event is called after adding an item or updating an existing item's stack in the inventory for <see cref="UIHandler.ActiveMachine"/>
		/// </summary>
		public event MachineInventorySlotUpdateItemDelegate OnUpdateItem;

		public void InitializeSlots(int[] slots, int maxSlotsPerRow) {
			foreach (var instance in this.slots)
				instance?.Remove();

			this.slots.Clear();

			int numSlot = 0;
			float top = 0;

			int slotWidth = TextureAssets.InventoryBack9.Value.Width + 5;
			int slotHeight = TextureAssets.InventoryBack9.Value.Height + 5;

			foreach (int slot in slots) {
				var instance = new MachineInventoryItemSlot(slot, DefaultContext, DefaultScale);
				instance.OnUpdateItem += (machine, oldItem, newItem) => OnUpdateItem?.Invoke(machine, oldItem, newItem);

				instance.Left.Set(numSlot * slotWidth, 0f);
				instance.Top.Set(top, 0f);

				if (++numSlot >= maxSlotsPerRow) {
					top += slotHeight;
					numSlot = 0;
				}

				this.slots.Add(instance);

				Append(instance);
			}

			Width.Set(maxSlotsPerRow * slotWidth, 0f);
			Height.Set(top + slotHeight, 0f);

			Recalculate();
		}
	}
}
