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
			
			foreach (var upgradeSlot in itemZone.Slots)
				upgradeSlot.OnRemoveItem += (machine, slot, oldItem) => RecalculateParent();
		}

		public void Refresh(int slotCount, int maxSlotsPerRow) {
			itemZone.InitializeSlots(slotCount, maxSlotsPerRow);
			itemZone.Recalculate();
		}
	}
}
