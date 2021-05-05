using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.World.Generation;
using TerraScience.Systems.Energy;

namespace TerraScience.World{
	public class TerraScienceWorld : ModWorld{
		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight){
			CustomWorldGen.MoreShinies(tasks);
		}

		public override TagCompound Save()
			=> new TagCompound(){
				["networks"] = NetworkCollection.Save()
			};

		public override void Load(TagCompound tag){
			NetworkCollection.EnsureNetworkIsInitialized();

			if(tag.ContainsKey("networks"))
				NetworkCollection.Load(tag.GetCompound("networks"));
		}

		public override void PreUpdate(){
			NetworkCollection.EnsureNetworkIsInitialized();
		}

		public override void PostUpdate(){
			NetworkCollection.CleanupNetworks();
		}
	}
}
