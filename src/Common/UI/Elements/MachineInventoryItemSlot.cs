﻿using Microsoft.Xna.Framework;
using SerousCommonLib.UI;
using SerousEnergyLib.API.Machines;
using SerousEnergyLib.Systems;
using Terraria;
using Terraria.UI;

namespace TerraScience.Common.UI.Elements {
	public delegate void MachineInventorySlotUpdateItemDelegate(IInventoryMachine machine, Item oldItem, Item newItem);

	public class MachineInventoryItemSlot : EnhancedItemSlot {
		public string hoverText;

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

		/// <summary>
		/// This event is called after adding an item or updating an existing item's stack in the inventory for <see cref="UIHandler.ActiveMachine"/>
		/// </summary>
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

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);

			if (hoverText is not null && StoredItem.IsAir && ContainsPoint(Main.MouseScreen))
				Main.instance.MouseText(hoverText);
		}
	}
}
