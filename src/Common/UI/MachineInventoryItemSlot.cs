using SerousCommonLib.UI;
using SerousEnergyLib.API.Machines;
using SerousEnergyLib.Systems;
using Terraria;

namespace TerraScience.Common.UI {
	public delegate void MachineInventorySlotUpdateItemDelegate(IInventoryMachine machine, Item oldItem, Item newItem);

	public class MachineInventoryItemSlot : EnhancedItemSlot {
		public override Item StoredItem => GetItemAtSlotInActiveMachine();

		public MachineInventoryItemSlot(int slot, int context = 0, float scale = 1) : base(slot, context, scale) {
			OnItemChanged = UpdateItemAtSlotInActiveMachine;
		}

		private Item GetItemAtSlotInActiveMachine() {
			if (UIHandler.ActiveMachine is not IInventoryMachine machine)
				return new Item();

			// Ensure that the inventory exists
			IInventoryMachine.Update(machine);

			return machine.Inventory[this.slot];
		}

		public event MachineInventorySlotUpdateItemDelegate OnUpdateItem;

		private void UpdateItemAtSlotInActiveMachine(Item oldItem) {
			if (UIHandler.ActiveMachine is not IInventoryMachine machine)
				return;

			// Ensure that the inventory exists
			IInventoryMachine.Update(machine);

			machine.Inventory[this.slot] = storedItem;  // "storedItem" is the item after handling clicks

			OnUpdateItem?.Invoke(machine, oldItem, storedItem);
		}
	}
}
