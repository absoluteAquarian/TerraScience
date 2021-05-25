using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;
using TerraScience.Content.Items.Placeable;
using TerraScience.Systems;

namespace TerraScience.Content.Tiles{
	public class FluidPumpTile : JunctionMergeable{
		public override JunctionType MergeType => JunctionType.Fluids;

		public override void SafeSetDefaults(){
			//Having tile object data is REQUIRED for the tile.frameX and tile.frameY to be set BEFORE ModTile.TileFrame is called
			//This is an annoyance, but it's required for junction tiles to merge the surrounding junction-mergeable tiles
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.FlattenAnchors = false;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.None, 0, 0);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleWrapLimit = 16;
			TileObjectData.addTile(Type);

			AddMapEntry(Color.LightBlue);
			drop = ModContent.ItemType<FluidPump>();
		}

		public override void PlaceInWorld(int i, int j, Item item){
			base.PlaceInWorld(i, j, item);

			item.placeStyle %= 4;

			var tile = Framing.GetTileSafely(i, j);
			//Sanity check; TileObjectData should already handle this
			tile.frameX = (short)(item.placeStyle * 18);

			NetworkCollection.OnFluidPipePlace(new Point16(i, j));
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem){
			if(!fail){
				//Tile was mined.  Update the networks
				NetworkCollection.OnFluidPipeKill(new Point16(i, j));
			}
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) => false;
	}
}
