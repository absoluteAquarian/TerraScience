using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using SerousEnergyLib.API;
using SerousEnergyLib.API.Energy;
using SerousEnergyLib.API.Machines;
using SerousEnergyLib.API.Machines.Default;
using SerousEnergyLib.API.Machines.UI;
using SerousEnergyLib.API.Sounds;
using SerousEnergyLib.Systems;
using SerousEnergyLib.Systems.Networks;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Common;
using TerraScience.Common.UI.Machines;
using TerraScience.Content.Sounds;
using TerraScience.Content.Tiles.Machines;

namespace TerraScience.Content.MachineEntities {
	public class FurnaceGeneratorEntity : BasePowerGeneratorEntity, IInventoryMachine, ISoundEmittingMachine, IReducedNetcodeMachine, IMachineUIAutoloading<FurnaceGeneratorEntity, FurnaceGeneratorUI> {
		public override int MachineTile => ModContent.TileType<FurnaceGenerator>();

		public override BaseMachineUI MachineUI => MachineUISingletons.GetInstance<FurnaceGeneratorEntity>();

		public override FluxStorage PowerStorage { get; } = new FluxStorage(new TerraFlux(3000d));

		public Item[] Inventory { get; set; }

		public virtual int DefaultInventoryCapacity => 1;

		public CraftingProgress Progress { get; private set; } = new CraftingProgress();

		private int burningItem = -1;

		public virtual bool CanExportItemAtSlot(int slot, Point16 subtile) => false;

		public virtual bool CanImportItemAtSlot(Item import, Point16 subtile, int slot, out int stackImported) {
			stackImported = 0;

			// Item must be burnable
			if (TechMod.Sets.FurnaceGenerator.BurnDuration[import.type] < 1)
				return false;

			return IInventoryMachine.DefaultCanImportItemAtSlot(this, import, subtile, slot, out stackImported);
		}

		public virtual bool CanMergeWithItemPipe(int pipeX, int pipeY, int machineX, int machineY) => true;

		public bool ExportItemAtSlot(ItemNetwork network, int slot, Point16 pathfindingStart, ref int extractCount, bool simulation, out InventoryExtractionResult result) {
			result = default;
			return false;
		}

		public virtual int[] GetExportSlots() => Array.Empty<int>();

		public virtual int[] GetInputSlots() => new int[] { 0 };

		public virtual int[] GetInputSlotsForRecipes() => GetInputSlots();

		public virtual void ImportItemAtSlot(Item import, int slot) => IInventoryMachine.DefaultImportItemAtSlot(this, import, slot);

		public static readonly double ConstantGenerationPerTick = 50 / 60d;  // 50 TF over the course of 1 second

		public override double GetPowerGeneration(double ticks) {
			// Dummy instance?  Return the power generation for when this machine is active
			if (IsDummyInstance)
				return ConstantGenerationPerTick * ticks;

			if (burningItem != -1 && !PowerStorage.IsFull) {
				// An item is being burned.  Generate power
				return ConstantGenerationPerTick * ticks;
			}

			// No conversion is in progress, or the storage is full
			return 0;
		}

		// NOTE: a ModTileEntity which inherits from IPowerGeneratorEntity will run its Update before all non-generator entities!
		// In order to make the entity not update twice, BasePowerGeneratorEntity provides a GeneratorUpdate() method for ease of use
		public override void GeneratorUpdate() {
			IInventoryMachine.Update(this);
			
			// If the progress just finished, reset the machine's state
			if (burningItem > -1 && Progress.Progress == 0)
				burningItem = -1;

			// In order to make GetPowerConsumption() not cause side effects, it will only read data
			// Thus, item consumption must be checked here
			ref Item input = ref Inventory[0];

			// Attempt to consume an item
			if (burningItem == -1 && !PowerStorage.IsFull) {
				if (!input.IsAir && TechMod.Sets.FurnaceGenerator.BurnDuration[input.type] > 0) {
					burningItem = input.type;

					if (input.consumable) {
						input.stack--;

						if (input.stack <= 0)
							input.TurnToAir();

						Netcode.SyncMachineInventorySlot(this, 0);
					}
				} else {
					// Input item doesn't exist or is invalid
					burningItem = -1;
				}
			}

			if (burningItem > -1) {
				Ticks duration = TechMod.Sets.FurnaceGenerator.BurnDuration[burningItem];

				if (duration > 0) {
					// Freeze the progress if the power storage is full
					if (!PowerStorage.IsFull) {
						Progress.Step(1f / duration.ticks);

						if (TileLoader.GetTile(Main.tile[Position.X, Position.Y].TileType) is FurnaceGenerator furnace) {
							furnace.GetMachineDimensions(out uint width, out uint height);
							Vector2 soundPos = Position.ToWorldCoordinates(width * 8, height * 8);
						
							ISoundEmittingMachine.EmitSound(
								emitter: this,
								RegisteredSounds.Styles.FurnaceGenerator.Running,
								NetcodeSoundMode.SendVolume | NetcodeSoundMode.SendPosition,
								ref running,
								ref servPlaying,
								soundPos);
						}
					} else {
						ISoundEmittingMachine.StopSound(
							emitter: this,
							RegisteredSounds.IDs.FurnaceGenerator.Running,
							ref running,
							ref servPlaying);
					}
				} else {
					burningItem = -1;
					Progress.Progress = 0;
				}
			}

			if (burningItem < ItemID.None) {
				ISoundEmittingMachine.StopSound(
					emitter: this,
					RegisteredSounds.IDs.FurnaceGenerator.Running,
					ref running,
					ref servPlaying);
			}

			Netcode.SendReducedData(this);
		}

		public override void SaveData(TagCompound tag) {
			base.SaveData(tag);

			IInventoryMachine.SaveData(this, tag);

			TagCompound progress = new();
			Progress.SaveData(progress);
			tag["progress"] = progress;

			tag["item"] = IdentifierIO.SaveItemID(burningItem);
		}

		public override void LoadData(TagCompound tag) {
			base.LoadData(tag);

			IInventoryMachine.LoadData(this, tag);

			if (tag.TryGet("progress", out TagCompound progress))
				Progress.LoadData(progress);
			else
				Progress = new();

			if (tag.TryGet("item", out TagCompound item))
				burningItem = IdentifierIO.LoadItemID(item);
			else
				burningItem = -1;
		}

		public override void NetSend(BinaryWriter writer) {
			base.NetSend(writer);
			IInventoryMachine.NetSend(this, writer);
			ReducedNetSend(writer);
		}

		public override void NetReceive(BinaryReader reader) {
			base.NetReceive(reader);
			IInventoryMachine.NetReceive(this, reader);
			ReducedNetReceive(reader);
		}

		#region Implement ISoundEmittingMachine
		private SlotId running = SlotId.Invalid;
		private bool servPlaying;

		public void OnSoundPlayingPacketReceived(in SlotId soundSlot, int id, int extraInformation) {
			if (id == RegisteredSounds.IDs.FurnaceGenerator.Running)
				running = soundSlot;
		}

		public void OnSoundUpdatePacketReceived(int id, SoundStyle data, NetcodeSoundMode mode, Vector2? location, int extraInformation) {
			if (id == RegisteredSounds.IDs.FurnaceGenerator.Running) {
				if (SoundEngine.TryGetActiveSound(running, out var activeSound)) {
					if ((mode & NetcodeSoundMode.SendPosition) == NetcodeSoundMode.SendPosition)
						activeSound.Position = location;
					if ((mode & NetcodeSoundMode.SendVolume) == NetcodeSoundMode.SendVolume)
						activeSound.Volume = data.Volume;
				}
			}
		}

		public void OnSoundStopPacketReceived(int id, int extraInformation) {
			if (id == RegisteredSounds.IDs.FurnaceGenerator.Running) {
				if (SoundEngine.TryGetActiveSound(running, out var activeSound))
					activeSound.Stop();

				running = SlotId.Invalid;
			}
		}
		#endregion

		#region Implement IReducedNetcodeMachine
		public void ReducedNetSend(BinaryWriter writer) {
			Progress.Send(writer);

			writer.Write(burningItem);

			PowerStorage.Send(writer);
		}

		public void ReducedNetReceive(BinaryReader reader) {
			Progress ??= new();
			Progress.Receive(reader);

			burningItem = reader.ReadInt32();

			PowerStorage.Receive(reader);
		}
		#endregion
	}
}
