using Terraria;
using Terraria.DataStructures;
using TerraScience.Content.TileEntities.Energy.Storage;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles.EnergyMachines.Storage{
	public class BasicBattery : Machine{
		public override void GetDefaultParams(out string mapName, out uint width, out uint height){
			mapName = "Battery";
			width = 3;
			height = 4;
		}

		public override Tile[,] Structure => TileUtils.Structures.BasicBattery;

		public override bool HandleMouse(Point16 pos)
			=> TileUtils.HandleMouse<BasicBatteryEntity>(this, pos, () => true);
	}
}
