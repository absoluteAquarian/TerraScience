using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;
using TerraScience.Content.Items.Placeable;
using TerraScience.Systems;

namespace TerraScience.Content.Tiles{
	public class TransportJunction : ModTile{
		public override void SetDefaults(){
			//Non-solid, but this is required.  Explanation is in TechMod.PreUpdateEntities()
			Main.tileSolid[Type] = false;
			Main.tileNoSunLight[Type] = false;

			//Having tile object data is REQUIRED for the tile.frameX and tile.frameY to be set BEFORE ModTile.TileFrame is called
			//This is an annoyance, but it's required for junction tiles to merge the surrounding junction-mergeable tiles
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.FlattenAnchors = false;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.None, 0, 0);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleWrapLimit = 16;
			TileObjectData.addTile(Type);

			AddMapEntry(Color.LightGray);
			drop = ModContent.ItemType<TransportJunctionItem>();
		}

		public override bool CanPlace(int i, int j){
			//This hook is called just before the tile is placed, which means we can fool the game into thinking this tile is solid when it really isn't
			TechMod.Instance.SetNetworkTilesSolid();
			return JunctionMergeable.AtLeastOneSurroundingTileIsActive(i, j);
		}

		public override void PlaceInWorld(int i, int j, Item item){
			//(Continuing from CanPlace)... then I can just set it back to false here
			TechMod.Instance.ResetNetworkTilesSolid();

			var tile = Framing.GetTileSafely(i, j);
			//Sanity check; TileObjectData should already handle this
			tile.frameX = (short)(item.placeStyle * 18);

			//Do each axis separately
			var merge = JunctionMergeable.mergeTypes[tile.frameX / 18];
			if((merge & JunctionMerge.Wires_All) != 0){
				if((merge & JunctionMerge.Wires_UpDown) != 0){
					tile.frameX = (short)(JunctionMergeable.FindMergeIndex(JunctionMerge.Wires_UpDown) * 18);

					NetworkCollection.OnWirePlace(new Point16(i, j));
				}

				if((merge & JunctionMerge.Wires_LeftRight) != 0){
					tile.frameX = (short)(JunctionMergeable.FindMergeIndex(JunctionMerge.Wires_LeftRight) * 18);

					NetworkCollection.OnWirePlace(new Point16(i, j));
				}
			}

			if((merge & JunctionMerge.Items_All) != 0){
				if((merge & JunctionMerge.Items_UpDown) != 0){
					tile.frameX = (short)(JunctionMergeable.FindMergeIndex(JunctionMerge.Items_UpDown) * 18);

					NetworkCollection.OnItemPipePlace(new Point16(i, j));
				}

				if((merge & JunctionMerge.Items_LeftRight) != 0){
					tile.frameX = (short)(JunctionMergeable.FindMergeIndex(JunctionMerge.Items_LeftRight) * 18);

					NetworkCollection.OnItemPipePlace(new Point16(i, j));
				}
			}

			if((merge & JunctionMerge.Fluids_All) != 0){
				if((merge & JunctionMerge.Fluids_UpDown) != 0){
					tile.frameX = (short)(JunctionMergeable.FindMergeIndex(JunctionMerge.Fluids_UpDown) * 18);

					NetworkCollection.OnFluidPipePlace(new Point16(i, j));
				}

				if((merge & JunctionMerge.Fluids_LeftRight) != 0){
					tile.frameX = (short)(JunctionMergeable.FindMergeIndex(JunctionMerge.Fluids_LeftRight) * 18);

					NetworkCollection.OnFluidPipePlace(new Point16(i, j));
				}
			}

			//Make sure the tile uses the right thing
			tile.frameX = (short)(item.placeStyle * 18);
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem){
			if(!fail){
				//Tile was mined.  Update the networks
				var tile = Framing.GetTileSafely(i, j);

				var merge = JunctionMergeable.mergeTypes[tile.frameX / 18];
				if((merge & JunctionMerge.Wires_All) != 0)
					NetworkCollection.OnWireKill(new Point16(i, j));
				if((merge & JunctionMerge.Items_All) != 0)
					NetworkCollection.OnItemPipeKill(new Point16(i, j));
				if((merge & JunctionMerge.Fluids_All) != 0)
					NetworkCollection.OnFluidPipeKill(new Point16(i, j));
			}
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) => false;
	}
}
