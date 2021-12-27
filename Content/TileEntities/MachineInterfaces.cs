using System.IO;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using TerraScience.Content.ID;

namespace TerraScience.Content.TileEntities{
	public class FluidEntry{
		public MachineFluidID id;
		public float current;
		public float max;

		public readonly MachineFluidID[] validTypes;

		public readonly bool isInput;

		public FluidEntry(float max, bool isInput, params MachineFluidID[] validTypes){
			this.max = max;
			this.isInput = isInput;
			this.validTypes = validTypes;
		}

		public TagCompound Save()
			=> new TagCompound(){
				["cur"] = current,
				["id"] = (int)id
			};

		public void Load(TagCompound tag){
			current = tag.GetFloat("cur");
			id = (MachineFluidID)tag.GetInt("id");
		}

		public void NetSend(BinaryWriter writer){
			writer.Write((ushort)id);
			writer.Write(current);
			writer.Write(max);
		}

		public void NetReceive(BinaryReader reader){
			id = (MachineFluidID)reader.ReadUInt16();
			current = reader.ReadSingle();
			max = reader.ReadSingle();
		}
	}

	public interface IFluidMachine{
		FluidEntry[] FluidEntries{ get; set; }

		int FluidPlaceDelay{ get; set; }

		void TryExportFluid(Point16 pumpPos);
		void TryImportFluid(Point16 pipePos);
	}
}
