using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerraScience.Content.TileEntities;

namespace TerraScience{
	public class TerraScienceWorld : ModWorld{
		public override TagCompound Save(){
			//Save the tile entities
			var tes = TileEntity.ByPosition.Where(kvp => kvp.Value is SaltExtractorEntity).Select(kvp => new KeyValuePair<Point16, SaltExtractorEntity>(kvp.Key, kvp.Value as SaltExtractorEntity));
			var positions = tes.Select(te => te.Key).ToList();
			var entities = tes.Select(te => te.Value).ToList();
			return new TagCompound{
				["multitilePositions"] = positions,
				["multitileEntities"] = entities
			};
		}

		public override void Load(TagCompound tag){
			//Load the saved tile entities
			var positions = tag.GetList<Point16>("multitilePositions");
			var entities = tag.GetList<SaltExtractorEntity>("multitileEntities");

			for(int i = 0; i < positions.Count; i++){
				TileEntity.ByPosition.Add(positions[i], entities[i]);
			}
		}
	}
}
