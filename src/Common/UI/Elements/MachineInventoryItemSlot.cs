using SerousCommonLib.UI;
using SerousEnergyLib.API.Machines;
using SerousEnergyLib.Systems;
using Terraria;
using Terraria.UI;

namespace TerraScience.Common.UI.Elements {
	public delegate void MachineInventorySlotUpdateItemDelegate(IInventoryMachine machine, Item oldItem, Item newItem);

	public class MachineInventoryItemSlot : EnhancedItemSlot {
		public override Item StoredItem => GetItemAtSlotInActiveMachine();

		public MachineInventoryItemSlot(int slot, int context = ItemSlot.Context.InventoryItem, float scale = 1) : base(slot, context, scale) {
			OnItemChanged += UpdateItemAtSlotInActiveMachine;
		}

		private Item GetItemAtSlotInActiveMachine() {
			if (UIHandler.ActiveMachine is not IInventoryMachine machine)
				return new Item();

			// Ensure that the inventory exists
			IInventoryMachine.Update(machine);

			if (slot < 0 || slot >= machine.Inventory.Length)
				return new Item();

			return machine.Inventory[slot];
		}

		public event MachineInventorySlotUpdateItemDelegate OnUpdateItem;

		private void UpdateItemAtSlotInActiveMachine(Item oldItem) {
			if (UIHandler.ActiveMachine is not IInventoryMachine machine)
				return;

			// Ensure that the inventory exists
			IInventoryMachine.Update(machine);

			machine.Inventory[slot] = storedItem;  // "storedItem" is the item after handling clicks

			OnUpdateItem?.Invoke(machine, oldItem, storedItem);

			Netcode.SyncMachineInventorySlot(machine, slot);
		}
	}
}
