using System.IO;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using TerraScience.Content.ID;

namespace TerraScience.Content.TileEntities{
	public class LiquidEntry{
		public MachineLiquidID id;
		public float current;
		public float max;

		public readonly MachineLiquidID[] validTypes;

		public readonly bool isInput;

		public LiquidEntry(float max, bool isInput, params MachineLiquidID[] validTypes){
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
			id = (MachineLiquidID)tag.GetInt("id");
		}

		public void NetSend(BinaryWriter writer){
			writer.Write((ushort)id);
			writer.Write(current);
			writer.Write(max);
		}

		public void NetReceive(BinaryReader reader){
			id = (MachineLiquidID)reader.ReadUInt16();
			current = reader.ReadSingle();
			max = reader.ReadSingle();
		}
	}

	public class GasEntry{
		public MachineGasID id;
		public float current;
		public float max;

		public readonly MachineGasID[] validTypes;

		public readonly bool isInput;

		public GasEntry(float max, bool isInput, params MachineGasID[] validTypes){
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
			id = (MachineGasID)tag.GetInt("id");
		}

		public void NetSend(BinaryWriter writer){
			writer.Write((ushort)id);
			writer.Write(current);
			writer.Write(max);
		}

		public void NetReceive(BinaryReader reader){
			id = (MachineGasID)reader.ReadUInt16();
			current = reader.ReadSingle();
			max = reader.ReadSingle();
		}
	}

	public interface IGasMachine{
		GasEntry[] GasEntries{ get; set; }

		int GasPlaceDelay{ get; set; }

		void TryExportGas(Point16 pumpPos);
		void TryImportGas(Point16 pipePos);
	}

	public interface ILiquidMachine{
		LiquidEntry[] LiquidEntries{ get; set; }

		int LiquidPlaceDelay{ get; set; }

		void TryExportLiquid(Point16 pumpPos);
		void TryImportLiquid(Point16 pipePos);
	}
}
