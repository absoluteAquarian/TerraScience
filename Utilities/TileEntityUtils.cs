using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.ID;
using TerraScience.Content.Items.Tools;
using TerraScience.Content.TileEntities;
using TerraScience.Content.Tiles;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Systems;
using TerraScience.Systems.Pipes;

namespace TerraScience.Utilities{
	public static class TileEntityUtils{
		public static void UpdateOutputSlot(MachineFluidID intendedFluid, Item input, Item output, ref float storedFluid){
			if(storedFluid <= 0 || input.IsAir)
				return;

			//Check that the output is either 1) air or 2) is storing the same type of gas as "intendedFluid"
			if(output.IsAir || (output.modItem is Capsule capsule && capsule.FluidType == intendedFluid)){
				do{
					if(output.IsAir){
						output.SetDefaults(TechMod.GetCapsuleType(intendedFluid));
						output.stack = 0;
					}

					input.stack--;
					if(input.stack == 0)
						input.TurnToAir();

					output.stack++;

					storedFluid--;
				}while(!input.IsAir && storedFluid > 0);
			}
		}

		public static void TryInsertOutput(this MachineEntity entity, int outputSlotsStart, int outputSlotsEnd, int inputType, int inputStack){
			//Find the first slot that the items can stack to.  If that stack isn't enough, overflow to the next slot
			for(int i = outputSlotsStart; i < outputSlotsEnd + 1; i++){
				Item item = entity.RetrieveItem(i);
				if(item.IsAir){
					item.SetDefaults(inputType);
					item.type = inputType;
					item.stack = inputStack;
					break;
				}

				if(item.type == inputType && item.stack < item.maxStack){
					if(item.stack + inputStack <= item.maxStack){
						item.stack += inputStack;
						break;
					}else{
						inputStack -= item.maxStack - item.stack;
						item.stack = item.maxStack;
					}
				}
			}
		}

		public static void StopReactionIfOutputSlotsAreFull(this MachineEntity entity, int outputSlotsStart, int outputSlotsEnd){
			//Check that all slots aren't full.  If they are, abort early
			bool allFull = true;
			for(int i = outputSlotsStart; i < outputSlotsEnd + 1; i++)
				if(entity.RetrieveItem(i).IsAir)
					allFull = false;

			if(allFull)
				entity.ReactionInProgress = false;
		}

		public static bool TryFindMachineEntity(Point16 pos, out MachineEntity entity){
			entity = null;
			
			Tile tile = Framing.GetTileSafely(pos);
			ModTile mTile = ModContent.GetModTile(tile.type);

			if(mTile is Machine){
				Point16 origin = pos - tile.TileCoord();

				if(TileEntity.ByPosition.TryGetValue(origin, out TileEntity te) && (entity = te as MachineEntity) != null)
					return true;
			}

			return false;
		}

		public static void TryImportFluids<T>(this T entity, Point16 pipePos, int indexToExtract) where T : MachineEntity, IFluidMachine{
			if(indexToExtract < 0 || indexToExtract >= entity.FluidEntries.Length)
				return;

			var entry = entity.FluidEntries[indexToExtract];

			if(!entry.isInput
				|| !NetworkCollection.HasFluidPipeAt(pipePos, out FluidNetwork net)
				|| net.fluidType != MachineFluidID.None
				|| (entry.id != MachineFluidID.None && entry.id != net.fluidType)
				|| (entry.validTypes?.Length > 0 && Array.FindIndex(entry.validTypes, id => id == net.fluidType) == -1))
				return;

			Tile tile = Framing.GetTileSafely(pipePos);
			ModTile modTile = ModContent.GetModTile(tile.type);

			float rate = modTile is FluidTransportTile transport
				? transport.ExportRate
				: (modTile is FluidPumpTile
					? ModContent.GetInstance<FluidTransportTile>().ExportRate
					: -1);

			if(rate <= 0)
				return;

			float exported = Math.Min(rate, net.Capacity);

			if(exported + entry.current > entry.max)
				exported = entry.max - entry.current;

			if(exported <= 0)
				return;

			if(entry.id == MachineFluidID.None)
				entry.id = net.fluidType;

			entry.current += exported;
			net.StoredFluid -= exported;

			if(net.StoredFluid <= 0)
				net.fluidType = MachineFluidID.None;
		}

		public static void TryExportFluids<T>(this T entity, Point16 pumpPos, int indexToExtract) where T : MachineEntity, IFluidMachine{
			if(indexToExtract < 0 || indexToExtract >= entity.FluidEntries.Length)
				return;

			var entry = entity.FluidEntries[indexToExtract];

			if(entry.isInput
				|| !NetworkCollection.HasFluidPipeAt(pumpPos, out FluidNetwork net)
				|| net.fluidType != MachineFluidID.None
				|| (entry.id != MachineFluidID.None && net.fluidType != MachineFluidID.None && entry.id != net.fluidType))
				return;

			Tile tile = Framing.GetTileSafely(pumpPos);
			ModTile modTile = ModContent.GetModTile(tile.type);

			if(!(modTile is FluidPumpTile pump) || pump.GetConnectedMachine(pumpPos) != entity)
				return;

			float rate = pump.CapacityExtractedPerPump;

			float extracted = Math.Min(rate, entry.current);

			if(net.fluidType == MachineFluidID.None)
				net.fluidType = entry.id;

			if(net.StoredFluid + extracted > net.Capacity)
				extracted = net.Capacity - net.StoredFluid;

			net.StoredFluid += extracted;
			entry.current -= extracted;

			if(entry.current <= 0)
				entry.id = MachineFluidID.None;
		}

		public static void SendFluids<T>(this T entity, BinaryWriter writer) where T : MachineEntity, IFluidMachine{
			writer.Write((short)entity.FluidPlaceDelay);

			writer.Write((byte)entity.FluidEntries.Length);

			for(int i = 0; i < entity.FluidEntries.Length; i++)
				entity.FluidEntries[i].NetSend(writer);
		}

		public static void ReceiveFluids<T>(this T entity, BinaryReader reader) where T : MachineEntity, IFluidMachine{
			entity.FluidPlaceDelay = reader.ReadInt16();

			byte fluids = reader.ReadByte();

			for(int i = 0; i < fluids; i++)
				entity.FluidEntries[i].NetReceive(reader);
		}
	}
}
