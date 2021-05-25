using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.ID;
using TerraScience.Content.Items.Tools;
using TerraScience.Content.TileEntities;
using TerraScience.Content.Tiles.Multitiles;

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
	}
}
