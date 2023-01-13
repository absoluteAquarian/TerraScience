using SerousEnergyLib.API.Machines.UI;
using Terraria.UI;
using TerraScience.Common.UI.Elements;

namespace TerraScience.Common.UI.Machines {
	public class BasicUpgradesPage : BaseMachineUIPage {
		public MachineUpgradeItemSlotZone itemZone;

		public BasicUpgradesPage(BaseMachineUI parent) : base(parent, "Upgrades") {
			OnPageSelected += RecalculateParent;
		}

		private void RecalculateParent() => parentUI.NeedsToRecalculate = true;

		public override void OnInitialize() {
			itemZone = new MachineUpgradeItemSlotZone(context: ItemSlot.Context.InventoryItem);
			itemZone.OnUpdateItem += (machine, oldItem, newItem) => parentUI.NeedsToRecalculate = true;
			itemZone.OnRemoveItem += (machine, slot, oldItem) => parentUI.NeedsToRecalculate = true;

			itemZone.Left.Set(20, 0f);
			itemZone.Top.Set(20, 0f);

			foreach (var upgradeSlot in itemZone.Slots)
				upgradeSlot.OnRemoveItem += (machine, slot, oldItem) => RecalculateParent();

			Append(itemZone);
		}

		public void Refresh(int slotCount, int maxSlotsPerRow) {
			itemZone.InitializeSlots(slotCount, maxSlotsPerRow);
			itemZone.Recalculate();
		}
	}
}
