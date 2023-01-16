using SerousEnergyLib.API;
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
using Terraria.ModLoader.IO;
using TerraScience.Common.UI.Machines;
using TerraScience.Content.Tiles.Machines;

namespace TerraScience.Content.MachineEntities {
	public class FluidTankEntity : BaseMachineEntity, IFluidMachine, IInventoryMachine, IReducedNetcodeMachine, IMachineUIAutoloading<FluidTankEntity, FluidTankUI> {
		public override int MachineTile => ModContent.TileType<FluidTank>();

		// BaseMachineEntity usage requires overriding this, even though IMachineUIAutoloading<,> does this already
		public override BaseMachineUI MachineUI => MachineUISingletons.GetInstance<FluidTankEntity>();

		public virtual FluidStorage[] FluidStorage { get; set; } = new FluidStorage[] {
			new FluidStorage(10d)  // Stores 10 Liters
		};

		public Item[] Inventory { get; set; }

		public virtual int DefaultInventoryCapacity => 4;

		// Machine can't use upgrades
		// TODO: capacity upgrades?
		public override bool CanUpgradeApply(BaseUpgrade upgrade) => false;

		public override void Update() {
			base.Update();

			IInventoryMachine.Update(this);
			IFluidMachine.Update(this);

			// If there's an item in the import input slot, try to import its fluid
			var inv = Inventory;
			ref Item importInput = ref inv[0], importOutput = ref inv[1];

			if (!importInput.IsAir && TechMod.Sets.FluidTank.CanBePlacedInFluidImportSlot[importInput.type]) {
				int fluid = TechMod.Sets.FluidTank.FluidImport[importInput.type];
				int leftover = TechMod.Sets.FluidTank.FluidImportLeftover[importInput.type];

				if (fluid > -1 && (importOutput.IsAir || leftover < 0 || (importOutput.type == leftover && importOutput.stack < importOutput.maxStack))) {
					// Import the fluid
					// TODO: vials might not have 1 L
					double liters = 1d;

					FluidStorage[0].Import(fluid, ref liters);

					// Fluid was imported.  Consume the input if applicable and generate the output item
					if (liters < 1d) {
						if (importInput.type == leftover) {
							// Move the item to the output slot
							if (importOutput.IsAir)
								importOutput = importInput.Clone();
							else
								importOutput.stack++;

							Netcode.SyncMachineInventorySlot(this, 1);
						} else if (leftover > -1) {
							// Generate the item
							if (importOutput.IsAir)
								importOutput = new Item(leftover, 1);
							else
								importOutput.stack++;

							Netcode.SyncMachineInventorySlot(this, 1);
						}

						importInput.stack--;

						if (importInput.stack <= 0)
							importInput.TurnToAir();

						Netcode.SyncMachineInventorySlot(this, 0);
					}
				}
			}

			// If there's an item in the export input slot, try to export some fluid
			ref Item exportInput = ref inv[2], exportOutput = ref inv[3];

			if (!exportInput.IsAir && TechMod.Sets.FluidTank.CanBePlacedInFluidExportSlot[exportInput.type] && (exportOutput.IsAir || exportOutput.stack < exportOutput.maxStack)) {
				double liters = TechMod.Sets.FluidTank.FluidExportQuantity[exportInput.type];
				double required = liters;

				var storage = FluidStorage[0];

				if (liters > 0 && !storage.IsEmpty && storage.FluidType > FluidTypeID.None) {
					storage.Export(ref liters, out int exportedType);

					if (liters != required) {
						// Export failed
						storage.Import(exportedType, ref liters);
					} else {
						int export = GetFluidExportResult(exportInput.type, exportedType);

						if (export > -1) {
							if (!exportOutput.IsAir && export != exportOutput.type) {
								// Export failed
								storage.Import(exportedType, ref liters);
							} else {
								// Export the fluids
								if (exportOutput.IsAir)
									exportOutput = new Item(export, 1);
								else
									exportOutput.stack++;

								exportInput.stack--;

								if (exportInput.stack <= 0)
									exportInput.TurnToAir();

								Netcode.SyncMachineInventorySlot(this, 0);
								Netcode.SyncMachineInventorySlot(this, 1);
							}
						}
					}
				}
			}

			// Always send the reduced data to clients
			Netcode.SendReducedData(this);
		}

		// Always permit pipe merging
		public virtual bool CanMergeWithFluidPipe(int pipeX, int pipeY, int machineX, int machineY) => true;

		public virtual bool CanUpgradeApplyTo(BaseUpgrade upgrade, int slot) => true;

		// Machine does not use recipes
		public virtual int[] GetInputSlotsForRecipes() => Array.Empty<int>();

		public virtual int SelectFluidExportSource(Point16 pump, Point16 subtile) => 0;

		public virtual int SelectFluidImportDestination(Point16 pipe, Point16 subtile) => 0;

		public virtual int SelectFluidImportDestinationFromType(int fluidType) => 0;

		public virtual bool CanMergeWithItemPipe(int pipeX, int pipeY, int machineX, int machineY) => true;

		public virtual int[] GetInputSlots() => new int[] { 0, 2 };

		/// <summary>
		/// Returns whether <paramref name="import"/> can be imported into this machine
		/// </summary>
		/// <param name="machine">The machine to examine for inventory slot requirements</param>
		/// <param name="import">The item to input</param>
		/// <param name="insertionSlot">Whether the fluid insertion slots (<see langword="true"/>) or the fluid extraction slots (<see langword="false"/>) are being imported to</param>
		public static bool ItemIsValidForImport(FluidTankEntity machine, Item import, bool insertionSlot) {
			if (machine is null || import.IsAir)
				return false;

			Item inv;
			if (insertionSlot) {
				inv = machine.Inventory[1];

				return TechMod.Sets.FluidTank.CanBePlacedInFluidImportSlot[import.type] && (inv.IsAir || TechMod.Sets.FluidTank.FluidImportLeftover[import.type] == inv.type);
			}

			inv = machine.Inventory[3];
			var storage = machine.FluidStorage[0];

			return TechMod.Sets.FluidTank.CanBePlacedInFluidExportSlot[import.type]
				&& (inv.IsAir || storage.IsEmpty || storage.FluidType == FluidTypeID.None || GetFluidExportResult(import.type, storage.FluidType) == inv.type);
		}

		public static int GetFluidExportResult(int itemType, int fluidType) => TechMod.Sets.FluidTank.FluidExportResult[fluidType]?[itemType] ?? -1;

		public virtual bool CanImportItemAtSlot(Item import, Point16 subtile, int slot, out int stackImported) {
			stackImported = 0;
			
			if (subtile != Point16.NegativeOne && !DoesSlotSideMatch(slot, subtile, Position))
				return false;

			return ItemIsValidForImport(this, import, slot == 0) && IInventoryMachine.DefaultCanImportItemAtSlot(this, import, subtile, slot, out stackImported);
		}

		public virtual void ImportItemAtSlot(Item import, int slot) => IInventoryMachine.DefaultImportItemAtSlot(this, import, slot);

		public virtual int[] GetExportSlots() => new int[] { 1, 3 };

		public virtual bool ExportItemAtSlot(ItemNetwork network, int slot, Point16 pathfindingStart, ref int extractCount, bool simulation, out InventoryExtractionResult result)
			=> IInventoryMachine.DefaultExportItemAtSlot(this, network, slot, pathfindingStart, ref extractCount, simulation, out result);

		public virtual bool CanExportItemAtSlot(int slot, Point16 subtile) => DoesSlotSideMatch(slot, subtile, Position);

		public static bool DoesSlotSideMatch(int slot, Point16 subtile, Point16 entityLocation) {
			return slot switch {
				// Slots 0 and 1 can only be imported to and exported from if the subtile is on the left side of the machine
				0 or 1 => subtile.X == entityLocation.X,
				// Slots 2 and 3 can only be imported to and exported from if the subtile is on the right side of the machine
				2 or 3 => subtile.X != entityLocation.X,
				_ => false
			};
		}

		public override void SaveData(TagCompound tag) {
			base.SaveData(tag);
			IFluidMachine.SaveData(this, tag);
			IInventoryMachine.SaveData(this, tag);
		}

		public override void LoadData(TagCompound tag) {
			base.LoadData(tag);
			IFluidMachine.LoadData(this, tag);
			IInventoryMachine.LoadData(this, tag);
		}

		public override void NetSend(BinaryWriter writer) {
			base.NetSend(writer);
			IFluidMachine.NetSend(this, writer);
			IInventoryMachine.NetSend(this, writer);
		}

		public override void NetReceive(BinaryReader reader) {
			base.NetReceive(reader);
			IFluidMachine.NetReceive(this, reader);
			IInventoryMachine.NetReceive(this, reader);
		}

		#region Implement IReducedNetcodeMachine
		public void ReducedNetSend(BinaryWriter writer) {
			var storage = FluidStorage[0];

			writer.Write(storage.FluidID is FluidTypeID id ? id.Type : -1);
			writer.Write(storage.CurrentCapacity);
		}

		public void ReducedNetReceive(BinaryReader reader) {
			var storage = FluidStorage[0];

			int id = reader.ReadInt32();
			storage.FluidID = id == -1 ? null : FluidLoader.Get(id);

			storage.CurrentCapacity = reader.ReadDouble();
		}
		#endregion
	}
}
