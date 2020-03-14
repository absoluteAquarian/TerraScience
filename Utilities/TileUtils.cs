using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using static Terraria.ID.TileID;

namespace TerraScience.Utilities{
	public static class TileUtils{
		/// <summary>
		/// An array of all viable tile types that a multitile structure can be made from.
		/// </summary>
		public static readonly int[] StructureTileIDs = new int[]{ CopperPlating, TinPlating, GrayBrick, Glass };

		public static class Structures{
			public static Tile[,] SaltExtractor;

			public static void SetupStructures(){
				SaltExtractor = new Tile[,]{
					{MakeTileInstance(Glass, TileSlopeVariant.HalfBrick), MakeTileInstance(CopperPlating), MakeTileInstance(CopperPlating), MakeTileInstance(Glass, TileSlopeVariant.HalfBrick)},
					{MakeTileInstance(Glass), MakeTileInstance(GrayBrick), MakeTileInstance(GrayBrick), MakeTileInstance(Glass)},
					{MakeTileInstance(Glass), MakeTileInstance(GrayBrick), MakeTileInstance(GrayBrick), MakeTileInstance(Glass)}
				};
			}

			public static void Unload(){
				SaltExtractor = null;
			}
		}

		public static Vector2 TileEntityCenter(TileEntity entity, Tile[,] structure) {
			Point16 topLeft = entity.Position;
			Point16 size = new Point16(structure.GetLength(1), structure.GetLength(0));
			Vector2 worldTopLeft = topLeft.ToVector2() * 16;
			return worldTopLeft + size.ToVector2() * 8; // * 16 / 2
		}

		public static Tile MakeTileInstance(ushort type, TileSlopeVariant slope = TileSlopeVariant.Solid){
			//Slopes are at   x000 xxxx xxxx xxxx in the sTileHeader
			//Halfbrick is at xxxx x0xx xxxx xxxx in the sTileHeader

			Tile tile = new Tile();
			if(slope == TileSlopeVariant.HalfBrick)
				tile.halfBrick(true);
			else if(slope != TileSlopeVariant.Solid)
				tile.slope((byte)slope);
			tile.type = type;
			return tile;
		}

		public static bool TileIsViable(int x, int y){
			Tile tile = Framing.GetTileSafely(x, y);

			//Return if the tile actually existed and its type is in the StructureIDs array
			return tile.nactive() && StructureTileIDs.Contains(tile.type);
		}

		public static bool TryReplaceStructure(int x, int y, out Point16 topLeftTileLocation, out Tile[,] structure){
			//Check against each structure until we find a valid one
			//If none are found, return false
			if(TryFindStructure(x, y, Structures.SaltExtractor, out topLeftTileLocation)){
				structure = Structures.SaltExtractor;
				return true;
			}else{
				topLeftTileLocation = new Point16(-1, -1);
				structure = null;
				return false;
			}
		}

		private static bool TryFindStructure(int x, int y, Tile[,] structure, out Point16 topLeftTileLocation){
			// TODO: optimization?

			int width = structure.GetLength(1);
			int height = structure.GetLength(0);
			
			//Bounds checking
			if(x - width < 0 || x + width > Main.maxTilesX || y - height < 0 || y + height > Main.maxTilesY){
				topLeftTileLocation = new Point16(-1, -1);
				return false;
			}

			//Loop through all possibile tiles the structure can be in around the given (x, y) tile coordinate
			for(int tileX = x - width; tileX <= x; tileX++){
				for(int tileY = y - height; tileY <= y; tileY++){
					Tile tile = Main.tile[tileX, tileY];
					//Tile must be active, not actuated, not have liquid in it and be the same type as the top-left tile in the structure
					if(tile.nactive() && tile.liquid == 0 && (structure[0, 0].slope() == tile.slope() || structure[0, 0].halfBrick() == tile.halfBrick()) && tile.type == structure[0, 0].type){
						//The tile matches, check the rest of them
						for(int tx = 0; tx < width; tx++){
							for(int ty = 0; ty < height; ty++){
								Tile structureTile = Main.tile[tileX + tx, tileY + ty];
								//If the tile doesn't match, end this local 2-nest loop and go back into the outer 2-nest loop
								if(structureTile.slope() != structure[ty, tx].slope() || structureTile.halfBrick() != structure[ty, tx].halfBrick() || structure[ty, tx].type != structureTile.type)
									goto invalidStructure;
							}
						}

						//If we've gotten here, then the structure is valid.
						topLeftTileLocation = new Point16(tileX, tileY);
						return true;
					}
				}
			}

			//If we've gotten here, then there wasn't a structure or it was invalid
			invalidStructure:
			topLeftTileLocation = new Point16(-1, -1);
			return false;
		}

		public static Point16 Frame(this Tile tile)
			=> new Point16(tile.frameX, tile.frameY);

		public static Point16 TileCoord(this Tile tile)
			=> new Point16(tile.frameX / 18, tile.frameY / 18);
	}

	public enum TileSlopeVariant{
		Solid,
		HalfBrick,
		DownRight,
		DownLeft,
		UpRight,
		UpLeft
	}
}
