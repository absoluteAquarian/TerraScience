using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;
using TerraScience.Content.TileEntities;

namespace TerraScience.Utilities{
	public static class TagUtils{
		public static void SaveFluids(this IFluidMachine machine, TagCompound existing){
			existing.Add("machine_fluids", machine.FluidEntries?.Select(entry => entry.Save()).ToList());
		}

		public static void LoadFluids(this IFluidMachine machine, TagCompound tag){
			if(tag.GetList<TagCompound>("machine_fluids") is List<TagCompound> tags && tags.Count == machine.FluidEntries.Length){
				for(int i = 0; i < machine.FluidEntries.Length; i++)
					machine.FluidEntries[i].Load(tags[i]);
			}
		}
	}
}
