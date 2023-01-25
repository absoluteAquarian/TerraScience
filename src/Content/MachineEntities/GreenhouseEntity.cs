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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.API;
using TerraScience.Common.UI.Machines;
using TerraScience.Content.Tiles.Machines;

namespace TerraScience.Content.MachineEntities {
	public class GreenhouseEntity : BaseMachineEntity, IInventoryMachine, IFluidMachine, IPoweredMachine, IItemOutputGeneratorMachine, IReducedNetcodeMachine, IMachineUIAutoloading<GreenhouseEntity, GreenhouseUI> {
		public override int MachineTile => ModContent.TileType<Greenhouse>();

		public override BaseMachineUI MachineUI => MachineUISingletons.GetInstance<GreenhouseEntity>();

		public Item[] Inventory { get; set; }

		public virtual FluidStorage[] FluidStorage { get; set; } = new FluidStorage[] { GetDefaultStorage(6d) };

		protected static FluidStorage GetDefaultStorage(double max) {
			List<int> ids = new();

			for (int i = 0; i < FluidLoader.Count; i++) {
				if (TechMod.Sets.Greenhouse.IsFluidPermitted[i])
					ids.Add(i);
			}

			return new FluidStorage(max, ids.ToArray());
		}

		public virtual int DefaultInventoryCapacity => 8;  // soil + modifier + plant + 5 output slots

		public virtual FluxStorage PowerStorage { get; } = new FluxStorage(new TerraFlux(5000f));

		public virtual int EnergyID => SerousMachines.EnergyType<TerraFluxTypeID>();

		public CraftingProgress Progress { get; set; } = new();

		public virtual bool CanExportItemAtSlot(int slot, Point16 subtile) => true;

		public virtual bool CanImportItemAtSlot(Item import, Point16 subtile, int slot, out int stackImported)
			=> IInventoryMachine.DefaultCanImportItemAtSlot(this, import, subtile, slot, out stackImported);

		public virtual bool CanMergeWithFluidPipe(int pipeX, int pipeY, int machineX, int machineY) => true;

		public virtual bool CanMergeWithItemPipe(int pipeX, int pipeY, int machineX, int machineY) => true;

		public virtual bool CanMergeWithWire(int wireX, int wireY, int machineX, int machineY) => true;

		public virtual bool CanUpgradeApplyTo(BaseUpgrade upgrade, int slot) => true;

		public virtual bool ExportItemAtSlot(ItemNetwork network, int slot, Point16 pathfindingStart, ref int extractCount, bool simulation, out InventoryExtractionResult result)
			=> IInventoryMachine.DefaultExportItemAtSlot(this, network, slot, pathfindingStart, ref extractCount, simulation, out result);

		public virtual int[] GetExportSlots() => new int[] { 3, 4, 5, 6, 7 };

		public virtual int[] GetInputSlots() => Array.Empty<int>();

		public virtual int[] GetInputSlotsForRecipes() => new int[] { 0, 1, 2 };

		public const double EnergyConsumptionWhileActive = 35d / 60;  // 35 TF/s

		public virtual double GetPowerConsumption(double ticks) {
			// Dummy instance?  Show the power consumption while active
			if (IsDummyInstance)
				return EnergyConsumptionWhileActive;

			ref Item soil = ref Inventory[0];
			ref Item modifier = ref Inventory[1];
			ref Item plant = ref Inventory[2];

			return MightBeAbleToGrowAPlant(out _) ? EnergyConsumptionWhileActive : 0;
		}

		public virtual void ImportItemAtSlot(Item import, int slot) => IInventoryMachine.DefaultImportItemAtSlot(this, import, slot);

		public virtual int SelectFluidExportSource(Point16 pump, Point16 subtile) => -1;

		public virtual int SelectFluidImportDestination(Point16 pipe, Point16 subtile) => 0;

		public virtual int SelectFluidImportDestinationFromType(int fluidType) {
			var storage = FluidStorage[0];

			if (storage.FluidType <= FluidTypeID.None || storage.IsEmpty)
				return 0;

			if (storage.FluidType == fluidType)
				return 0;

			return -1;
		}

		private int oldSoil = -1, oldModifier = -1, oldPlant = -1;

		public override void Update() {
			base.Update();

			IInventoryMachine.Update(this);
			IFluidMachine.Update(this);
			IPoweredMachine.Update(this);

			ref Item soil = ref Inventory[0];
			ref Item modifier = ref Inventory[1];
			ref Item plant = ref Inventory[2];

			if (soil.type != oldSoil || modifier.type != oldModifier || plant.type != oldPlant) {
				// Reset the progress
				Progress.Progress = 0;
			}

			if (MightBeAbleToGrowAPlant(out var recipe) && !IInventoryMachine.ExportInventoryIsFull(this) && HasEnoughInputFluid(recipe)) {
				// Machine must have enough power for the growth process to continue
				if (IPoweredMachine.AttemptToConsumePower(this)) {
					Ticks duration = recipe.growthTime;

					if (IMachine.ProgressStepWithUpgrades(this, Progress, duration.ticks)) {
						ConsumeInputFluid(recipe);
						GenerateOutputs(recipe);
					}
				}
			} else {
				// Machine is inactive
				Progress.Progress = 0;
			}

			oldSoil = soil.type;
			oldModifier = modifier.type;
			oldPlant = plant.type;

			Netcode.SendReducedData(this);
		}

		public bool MightBeAbleToGrowAPlant(out GreenhouseInputInformation info) {
			ref Item soil = ref Inventory[0];
			ref Item modifier = ref Inventory[1];
			ref Item plant = ref Inventory[2];

			return TechMod.Sets.Greenhouse.TryGetPlantInformation(soil.type, modifier.type, plant.type, out info);
		}

		public bool IsSoilRenderableWithoutAPlant(out MachineSpriteEffectInformation info) {
			ref Item soil = ref Inventory[0];
			ref Item modifier = ref Inventory[1];

			return TechMod.Sets.Greenhouse.TryGetSoilSprite(soil.type, modifier.type, out info);
		}

		private bool HasEnoughInputFluid(GreenhouseInputInformation info) {
			if (info.requiredFluid <= FluidTypeID.None)
				return true;

			var storage = FluidStorage[0];

			return storage.FluidType == info.requiredFluid && storage.CurrentCapacity >= info.requiredFluidQuantity;
		}

		private void ConsumeInputFluid(GreenhouseInputInformation info) {
			if (info.requiredFluid <= FluidTypeID.None)
				return;

			double liters = info.requiredFluidQuantity;
			var storage = FluidStorage[0];

			storage.Export(ref liters, out _);

			Netcode.SyncMachineFluidStorageSlot(this, 0);
		}

		private void GenerateOutputs(GreenhouseInputInformation info) {
			foreach (var output in info.possibleOutputs) {
				if (IItemOutputGeneratorMachine.ShouldBlockItemOutput(this, output.type))
					continue;

				double chance = IMachine.GetLuckThreshold(this, output.chance);

				// Roll the dice
				if (Main.rand.NextDouble() < chance) {
					// Generate the item and add it to this machine's export slots
					int stack = IItemOutputGeneratorMachine.CalculateItemStack(this, Main.rand.Next(output.stackMin, output.stackMax + 1));

					Item item = new Item(output.type, stack);

					IInventoryMachine.AddItemToExportInventory(this, item);
				}

				// No more room to put items inside of
				if (IInventoryMachine.ExportInventoryIsFull(this))
					break;
			}
		}

		public override void SaveData(TagCompound tag) {
			base.SaveData(tag);
			IInventoryMachine.SaveData(this, tag);
			IFluidMachine.SaveData(this, tag);
			IPoweredMachine.SaveData(this, tag);

			TagCompound progress = new();
			Progress.SaveData(progress);
			tag["progress"] = progress;
		}

		public override void LoadData(TagCompound tag) {
			base.LoadData(tag);
			IInventoryMachine.LoadData(this, tag);
			IFluidMachine.LoadData(this, tag);
			IPoweredMachine.LoadData(this, tag);

			if (tag.TryGet("progress", out TagCompound progress))
				Progress.LoadData(progress);
			else
				Progress = new();
		}

		public override void NetSend(BinaryWriter writer) {
			base.NetSend(writer);
			IInventoryMachine.NetSend(this, writer);
			IFluidMachine.NetSend(this, writer);
			IPoweredMachine.NetSend(this, writer);
			ReducedNetSend(writer);
		}

		public override void NetReceive(BinaryReader reader) {
			base.NetReceive(reader);
			IInventoryMachine.NetReceive(this, reader);
			IFluidMachine.NetReceive(this, reader);
			IPoweredMachine.NetReceive(this, reader);
			ReducedNetReceive(reader);
		}

		#region Implement IReducedNetcodeMachine
		public void ReducedNetSend(BinaryWriter writer) {
			Progress.Send(writer);

			writer.Write(oldSoil);
			writer.Write(oldModifier);
			writer.Write(oldPlant);
		}

		public void ReducedNetReceive(BinaryReader reader) {
			Progress ??= new();
			Progress.Receive(reader);

			oldSoil = reader.ReadInt32();
			oldModifier = reader.ReadInt32();
			oldPlant = reader.ReadInt32();
		}
		#endregion
	}
}
