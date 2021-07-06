using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;
using TerraScience.Content.TileEntities;

namespace TerraScience.Utilities{
	public static class TagUtils{
		public static void SaveLiquids(this ILiquidMachine machine, TagCompound existing){
			existing.Add("machine_liquids", machine.LiquidEntries?.Select(entry => entry.Save()).ToList());
		}

		public static void SaveGases(this IGasMachine machine, TagCompound existing){
			existing.Add("machine_gases", machine.GasEntries?.Select(entry => entry.Save()).ToList());
		}

		public static void LoadLiquids(this ILiquidMachine machine, TagCompound tag){
			if(tag.GetList<TagCompound>("machine_liquids") is List<TagCompound> tags && tags.Count == machine.LiquidEntries.Length){
				for(int i = 0; i < machine.LiquidEntries.Length; i++)
					machine.LiquidEntries[i].Load(tags[i]);
			}
		}

		public static void LoadGases(this IGasMachine machine, TagCompound tag){
			if(tag.GetList<TagCompound>("machine_gases") is List<TagCompound> tags && tags.Count == machine.GasEntries.Length){
				for(int i = 0; i < machine.GasEntries.Length; i++)
					machine.GasEntries[i].Load(tags[i]);
			}
		}
	}
}
