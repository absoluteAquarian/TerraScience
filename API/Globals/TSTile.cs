using Terraria;
using Terraria.ModLoader;
using TerraScience.Content.Tiles.Energy;

namespace TerraScience.API.Globals{
	public class TSTile : GlobalTile{
		public override bool CanPlace(int i, int j, int type){
			//Fool the game into thinking that wires are solid tiles
			Main.tileSolid[ModContent.TileType<TFWireTile>()] = true;
			return true;
		}

		public override void PlaceInWorld(int i, int j, Item item){
			Main.tileSolid[ModContent.TileType<TFWireTile>()] = false;
		}
	}
}
