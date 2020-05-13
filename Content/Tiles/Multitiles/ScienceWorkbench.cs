using Terraria;
using Terraria.DataStructures;
using TerraScience.Content.TileEntities;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles{
	public class ScienceWorkbench : Machine{
		public override void GetDefaultParams(out string mapName, out uint width, out uint height){
			mapName = "Science Workbench";
			width = 3;
			height = 3;
		}

		public override Tile[,] GetStructure() => TileUtils.Structures.ScienceWorkbench;

		public override bool HandleMouse(Point16 pos)
			=> TileUtils.HandleMouse<ScienceWorkbenchEntity>(pos, nameof(TileUtils.Structures.ScienceWorkbench), () => true);
	}
}
