using Microsoft.Xna.Framework.Graphics;
using SerousCommonLib.UI;
using SerousEnergyLib.API.Machines;
using SerousEnergyLib.Items;
using SerousEnergyLib.Systems;
using Terraria;
using Terraria.ID;
using Terraria.UI;

namespace TerraScience.Common.UI.Elements {
	public delegate void MachineUpgradeSlotUpdateItemDelegate(IMachine machine, Item oldItem, Item newItem);
	public delegate void MachineUpgradeSlotRemovedDelegate(IMachine machine, int slot, Item oldItem);

	public class MachineUpgradeItemSlot : EnhancedItemSlot {
		public override Item StoredItem => GetItemAtSlotInActiveMachine();

		public MachineUpgradeItemSlot(int slot, int context = ItemSlot.Context.InventoryItem, float scale = 1) : base(slot, context, scale) {
			OnItemChanged = UpdateItemAtSlotInActiveMachine;
			ValidItemFunc = CheckUpgradeItemQuantity;
		}

		private bool CheckUpgradeItemQuantity(Item item) {
			if (item.IsAir)
				return true;  // Always permit removing items from the slot

			if (item.ModItem is not BaseUpgradeItem upgrade)
				return false;

			// Check the quantity of upgrades in the machine
			if (UIHandler.ActiveMachine is not IMachine machine)
				return false;

			int maxUpgrades = upgrade.Upgrade.MaxUpgradesPerMachine;

			// Local capturing
			int type = upgrade.Type;

			int existing = machine.Upgrades.FindIndex(u => u.Upgrade.Type == type);
			if (existing > -1) {
				// Only permit "placing" item stacks onto stacks which match them
				if (existing != slot)
					return false;
			} else if (slot == machine.Upgrades.Count) {
				// Item doesn't exist... Add a new item to the upgrade list
				BaseUpgradeItem clone = new Item(upgrade.Item.type, stack: 0).ModItem as BaseUpgradeItem;

				machine.Upgrades.Add(clone);

				// Sync here as well just to be safe
				Netcode.SyncMachineUpgrades(machine);
				
				existing = slot;
			} else {
				// Invalid state
				return false;
			}

			// Dirty hack: temporarily change the max stack of the existing item in the machine
			machine.Upgrades[existing].Item.maxStack = maxUpgrades;

			return true;
		}

		private Item GetItemAtSlotInActiveMachine() {
			if (UIHandler.ActiveMachine is not IMachine machine)
				return new Item();

			// Ensure that the upgrades exists
			IMachine.Update(machine);

			if (slot < 0 || slot >= machine.Upgrades.Count)
				return new Item();

			return machine.Upgrades[slot].Item;
		}

		/// <summary>
		/// This event is called after adding an item or updating an existing upgrade item's stack in <see cref="UIHandler.ActiveMachine"/>
		/// </summary>
		public event MachineUpgradeSlotUpdateItemDelegate OnUpdateItem;

		/// <summary>
		/// This event is called before an upgrade slot is removed from <see cref="UIHandler.ActiveMachine"/>
		/// </summary>
		public event MachineUpgradeSlotRemovedDelegate OnRemoveItem;

		private void UpdateItemAtSlotInActiveMachine(Item oldItem) {
			if (UIHandler.ActiveMachine is not IMachine machine)
				return;

			// Ensure that the upgrades exists
			IMachine.Update(machine);

			// "storedItem" is the item after handling clicks
			if (storedItem.IsAir || storedItem.ModItem is not BaseUpgradeItem upgrade) {
				// Remove the item at this slot
				OnRemoveItem?.Invoke(machine, slot, oldItem);

				machine.Upgrades.RemoveAt(slot);
			} else {
				machine.Upgrades[slot] = upgrade;

				OnUpdateItem?.Invoke(machine, oldItem, storedItem);
			}

			Netcode.SyncMachineUpgrades(machine);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);

			// Dirty hack:  restore the max stack of the item in the machine here
			Item item = StoredItem;
			if (!item.IsAir && item.ModItem is BaseUpgradeItem)
				item.maxStack = ContentSamples.ItemsByType[item.type].maxStack;
		}
	}
}
