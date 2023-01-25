using SerousEnergyLib;
using SerousEnergyLib.API;
using SerousEnergyLib.API.Energy;
using SerousEnergyLib.API.Energy.Default;
using SerousEnergyLib.API.Fluid;
using SerousEnergyLib.API.Machines;
using SerousEnergyLib.API.Machines.Default;
using SerousEnergyLib.API.Machines.UI;
using SerousEnergyLib.API.Upgrades;
using SerousEnergyLib.Systems;
using SerousEnergyLib.Systems.Networks;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.Tiles.Machines;

namespace TerraScience.Content.MachineEntities {
	public class TrashMachineEntity : BaseMachineEntity, IInventoryMachine, IFluidMachine, IPoweredMachine, IReducedNetcodeMachine {
		public override int MachineTile => ModContent.TileType<TrashMachine>();

		public override BaseMachineUI MachineUI => null;  // No UI

		public Item[] Inventory { get; set; }

		public int DefaultInventoryCapacity => 1;

		public FluxStorage PowerStorage { get; } = new FluxStorage(new TerraFlux(100000d));  // Absurd number to account for absurd export rates

		public int EnergyID => SerousMachines.EnergyType<TerraFluxTypeID>();

		public FluidStorage[] FluidStorage { get; set; } = new FluidStorage[] { new FluidStorage(100000d) };  // Absurd number to account for absurd export rates

		public bool CanExportItemAtSlot(int slot, Point16 subtile) => false;

		public bool CanImportItemAtSlot(Item import, Point16 subtile, int slot, out int stackImported) {
			stackImported = import.stack;
			import.stack = 0;

			return true;
		}

		public bool CanMergeWithFluidPipe(int pipeX, int pipeY, int machineX, int machineY) => true;

		public bool CanMergeWithItemPipe(int pipeX, int pipeY, int machineX, int machineY) => true;

		public bool CanMergeWithWire(int wireX, int wireY, int machineX, int machineY) => true;

		public bool CanUpgradeApplyTo(BaseUpgrade upgrade, int slot) => false;

		public bool ExportItemAtSlot(ItemNetwork network, int slot, Point16 pathfindingStart, ref int extractCount, bool simulation, out InventoryExtractionResult result) {
			result = default;
			return false;
		}

		public int[] GetExportSlots() => Array.Empty<int>();

		public int[] GetInputSlots() => new int[] { 0 };

		public int[] GetInputSlotsForRecipes() => Array.Empty<int>();

		public double GetPowerConsumption(double ticks) => 0d;

		public void ImportItemAtSlot(Item import, int slot) {
			IInventoryMachine.Update(this);

			Inventory[slot] = new Item();

			Netcode.SyncMachineInventorySlot(this, slot);

			IInventoryMachine.DefaultImportItemAtSlot(this, import, slot);
		}

		public int SelectFluidExportSource(Point16 pump, Point16 subtile) => -1;

		public int SelectFluidImportDestination(Point16 pipe, Point16 subtile) {
			FluidStorage[0].CurrentCapacity = 0;

			Netcode.SyncMachineFluidStorageSlot(this, 0);

			return 0;
		}

		public int SelectFluidImportDestinationFromType(int fluidType) {
			FluidStorage[0].CurrentCapacity = 0;

			Netcode.SyncMachineFluidStorageSlot(this, 0);

			return 0;
		}

		public override void Update() {
			base.Update();
			IInventoryMachine.Update(this);
			IFluidMachine.Update(this);
			IPoweredMachine.Update(this);
		}

		#region Implement IReducedNetcodeMachine
		public void ReducedNetReceive(BinaryReader reader) {
			IInventoryMachine.Update(this);

			Inventory[0] = new Item();

			FluidStorage[0].CurrentCapacity = 0;

			PowerStorage.CurrentCapacity = TerraFlux.Zero;
		}

		public void ReducedNetSend(BinaryWriter writer) { }
		#endregion
	}
}
