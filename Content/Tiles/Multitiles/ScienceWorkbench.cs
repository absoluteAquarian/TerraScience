using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.Items.Placeable.Machines;
using TerraScience.Content.TileEntities;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles{
	public class ScienceWorkbench : Machine{
		public override void GetDefaultParams(out string mapName, out uint width, out uint height, out int itemType){
			mapName = "Science Workbench";
			width = 3;
			height = 3;
			itemType = ModContent.ItemType<ScienceWorkbenchItem>();
		}

		public override bool HandleMouse(Point16 pos)
			=> TileUtils.HandleMouse<ScienceWorkbenchEntity>(this, pos, () => true);
	}
}
