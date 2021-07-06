using System;
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
		public static void UpdateOutputSlot(MachineGasID intendedGas, Item input, Item output, ref float storedGas){
			if(storedGas <= 0 || input.IsAir)
				return;

			//Check that the output is either 1) air or 2) is storing the same type of gas as "intendedGas"
			if(output.IsAir || (output.modItem is Capsule capsule && capsule.GasType == intendedGas)){
				do{
					if(output.IsAir){
						output.SetDefaults(TechMod.GetCapsuleType(intendedGas));
						output.stack = 0;
					}

					input.stack--;
					if(input.stack == 0)
						input.TurnToAir();

					output.stack++;

					storedGas--;
				}while(!input.IsAir && storedGas > 0);
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

		public static void TryImportLiquids<T>(this T entity, Point16 pipePos, int indexToExtract) where T : MachineEntity, ILiquidMachine{
			if(indexToExtract < 0 || indexToExtract >= entity.LiquidEntries.Length)
				return;

			var entry = entity.LiquidEntries[indexToExtract];

			if(!NetworkCollection.HasFluidPipeAt(pipePos, out FluidNetwork net)
				|| net.gasType != MachineGasID.None
				|| net.liquidType == MachineLiquidID.None
				|| !entry.isInput
				|| (entry.id != MachineLiquidID.None && entry.id != net.liquidType)
				|| (entry.validTypes?.Length > 0 && Array.FindIndex(entry.validTypes, id => id == net.liquidType) == -1))
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

			if(entry.id == MachineLiquidID.None)
				entry.id = net.liquidType;

			entry.current += exported;
			net.StoredFluid -= exported;

			if(net.StoredFluid <= 0)
				net.liquidType = MachineLiquidID.None;
		}

		public static void TryExportLiquids<T>(this T entity, Point16 pumpPos, int indexToExtract) where T : MachineEntity, ILiquidMachine{
			if(indexToExtract < 0 || indexToExtract >= entity.LiquidEntries.Length)
				return;

			var entry = entity.LiquidEntries[indexToExtract];

			if(!NetworkCollection.HasFluidPipeAt(pumpPos, out FluidNetwork net)
				|| net.gasType != MachineGasID.None
				|| entry.isInput
				|| (entry.id != MachineLiquidID.None && net.liquidType != MachineLiquidID.None && entry.id != net.liquidType))
				return;

			Tile tile = Framing.GetTileSafely(pumpPos);
			ModTile modTile = ModContent.GetModTile(tile.type);

			if(!(modTile is FluidPumpTile pump) || pump.GetConnectedMachine(pumpPos) != entity)
				return;

			float rate = pump.CapacityExtractedPerPump;

			float extracted = Math.Min(rate, entry.current);

			if(net.liquidType == MachineLiquidID.None)
				net.liquidType = entry.id;

			if(net.StoredFluid + extracted > net.Capacity)
				extracted = net.Capacity - net.StoredFluid;

			net.StoredFluid += extracted;
			entry.current -= extracted;

			if(entry.current <= 0)
				entry.id = MachineLiquidID.None;
		}

		public static void TryImportGases<T>(this T entity, Point16 pipePos, int indexToExtract) where T : MachineEntity, IGasMachine{
			if(indexToExtract < 0 || indexToExtract >= entity.GasEntries.Length)
				return;

			var entry = entity.GasEntries[indexToExtract];

			if(!NetworkCollection.HasFluidPipeAt(pipePos, out FluidNetwork net)
				|| net.liquidType != MachineLiquidID.None
				|| net.gasType == MachineGasID.None
				|| !entry.isInput
				|| (entry.id != MachineGasID.None && entry.id != net.gasType)
				|| (entry.validTypes?.Length > 0 && Array.FindIndex(entry.validTypes, id => id == net.gasType) == -1))
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

			if(entry.id == MachineGasID.None)
				entry.id = net.gasType;

			entry.current += exported;
			net.StoredFluid -= exported;

			if(net.StoredFluid <= 0)
				net.gasType = MachineGasID.None;
		}

		public static void TryExportGases<T>(this T entity, Point16 pumpPos, int indexToExtract) where T : MachineEntity, IGasMachine{
			if(indexToExtract < 0 || indexToExtract >= entity.GasEntries.Length)
				return;

			var entry = entity.GasEntries[indexToExtract];

			if(!NetworkCollection.HasFluidPipeAt(pumpPos, out FluidNetwork net) || net.liquidType != MachineLiquidID.None || entry.isInput || (entry.id != MachineGasID.None && net.gasType != MachineGasID.None && entry.id != net.gasType))
				return;

			Tile tile = Framing.GetTileSafely(pumpPos);
			ModTile modTile = ModContent.GetModTile(tile.type);

			if(!(modTile is FluidPumpTile pump) || pump.GetConnectedMachine(pumpPos) != entity)
				return;

			float rate = pump.CapacityExtractedPerPump;

			float extracted = Math.Min(rate, entry.current);

			if(net.gasType == MachineGasID.None)
				net.gasType = entry.id;

			if(net.StoredFluid + extracted > net.Capacity)
				extracted = net.Capacity - net.StoredFluid;

			net.StoredFluid += extracted;
			entry.current -= extracted;

			if(entry.current <= 0)
				entry.id = MachineGasID.None;
		}
	}
}
