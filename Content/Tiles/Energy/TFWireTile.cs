using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.Items.Energy;
using TerraScience.Systems.Energy;

namespace TerraScience.Content.Tiles.Energy{
	public class TFWireTile : ModTile{
		public override void SetDefaults(){
			//Non-solid, but this is required.  Explanation is in TerraScience.PreUpdateEntities()
			Main.tileSolid[Type] = false;
			Main.tileNoSunLight[Type] = false;
			AddMapEntry(Color.Orange);
			drop = ModContent.ItemType<TFWireItem>();
		}

		public override bool CanPlace(int i, int j){
			//This hook is called just before the tile is placed, which means we can fool the game into thinking this tile is solid when it really isn't
			Main.tileSolid[Type] = true;
			return true;
		}

		public override void PlaceInWorld(int i, int j, Item item){
			NetworkCollection.OnWirePlace(new Point16(i, j));

			//(Continuing from CanPlace)... then I can just set it back to false here
			Main.tileSolid[Type] = false;
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem){
			if(!fail){
				//Tile was mined.  Update the networks
				NetworkCollection.OnWireKill(new Point16(i, j));
			}
		}
	}
}
