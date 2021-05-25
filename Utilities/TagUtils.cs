using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;
using TerraScience.Content.ID;
using TerraScience.Content.TileEntities;

namespace TerraScience.Utilities{
	public static class TagUtils{
		public static void SaveLiquids(this ILiquidMachine machine, TagCompound existing){
			existing.Add("machine_liquids", machine.LiquidTypes?.Select(id => (int)id).ToList());
			existing.Add("machine_liquid_counts", machine.StoredLiquidAmounts?.ToList());
		}

		public static void SaveGases(this IGasMachine machine, TagCompound existing){
			existing.Add("machine_gases", machine.GasTypes?.Select(id => (int)id).ToList());
			existing.Add("machine_gas_counts", machine.StoredGasAmounts?.ToList());
		}

		public static void LoadLiquids(this ILiquidMachine machine, TagCompound tag, int failsafeArrayLength){
			if(tag.GetList<int>("machine_liquids") is List<int> ids && tag.GetList<float>("machine_liquid_counts") is List<float> counts && ids.Count > 0 && ids.Count == counts.Count){
				machine.LiquidTypes = ids.Select(id => (MachineLiquidID)id).ToArray();
				machine.StoredLiquidAmounts = counts.ToArray();
			}else{
				machine.LiquidTypes = new MachineLiquidID[failsafeArrayLength];
				machine.StoredLiquidAmounts = new float[failsafeArrayLength];
			}
		}

		public static void LoadGases(this IGasMachine machine, TagCompound tag, int failsafeArrayLength){
			if(tag.GetList<int>("machine_gases") is List<int> ids && tag.GetList<float>("machine_gas_counts") is List<float> counts && ids.Count > 0 && ids.Count == counts.Count){
				machine.GasTypes = ids.Select(id => (MachineGasID)id).ToArray();
				machine.StoredGasAmounts = counts.ToArray();
			}else{
				machine.GasTypes = new MachineGasID[failsafeArrayLength];
				machine.StoredGasAmounts = new float[failsafeArrayLength];
			}
		}
	}
}
