using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.Items.Placeable.Machines.Energy.Storage;
using TerraScience.Content.TileEntities.Energy.Storage;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles.EnergyMachines.Storage{
	public class BasicBattery : Machine{
		public override void GetDefaultParams(out string mapName, out uint width, out uint height, out int itemType){
			mapName = "Battery";
			width = 3;
			height = 4;
			itemType = ModContent.ItemType<BasicBatteryItem>();
		}

		public override bool HandleMouse(Point16 pos)
			=> TileUtils.HandleMouse<BasicBatteryEntity>(this, pos, () => true);
	}
}
