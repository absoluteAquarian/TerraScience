using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.UI;
using TerraScience.Content.Items.Placeable;
using TerraScience.Content.TileEntities;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Content.Tiles.Multitiles;
using TerraScience.Content.UI;
using TerraScience.Systems.Energy;
using static Terraria.ID.TileID;

namespace TerraScience.Utilities{
	public static class TileUtils{
		/// <summary>
		/// An array of all viable tile types that a multitile structure can be made from.
		/// </summary>
		public static ushort[] StructureTileIDs;

		public static ushort SupportType => (ushort)ModContent.TileType<MachineSupport>();
		public static ushort BlastFurnaceType => (ushort)ModContent.TileType<BlastBrickTile>();

		public static Dictionary<int, MachineEntity> tileToEntity;

		public static Dictionary<int, Tile[,]> tileToStructure;

		public static Dictionary<int, string> tileToStructureName;

		public static class Structures{
			public static Tile[,] SaltExtractor;
			public static Tile[,] ScienceWorkbench;
			public static Tile[,] ReinforcedFurncace;
			public static Tile[,] BlastFurnace;

			public static Tile[,] AirIonizer;
			public static Tile[,] Electrolyzer;

			public static Tile[,] BasicWindTurbine;

			public static Tile[,] BasicBattery;

			public static void Unload(){
				StructureTileIDs = null;
				SaltExtractor = null;
				ScienceWorkbench = null;
				ReinforcedFurncace = null;
				BlastFurnace = null;

				AirIonizer = null;
				Electrolyzer = null;

				BasicWindTurbine = null;
				
				BasicBattery = null;
			}
		}

		public static Vector2 TileEntityCenter(TileEntity entity, Tile[,] structure) {
			Point16 topLeft = entity.Position;
			Point16 size = new Point16(structure.GetLength(1), structure.GetLength(0));
			Vector2 worldTopLeft = topLeft.ToVector2() * 16;
			return worldTopLeft + size.ToVector2() * 8; // * 16 / 2
		}

		public static Tile NewTile(ushort type, TileSlopeVariant slope = TileSlopeVariant.Solid){
			//Slopes are at   x000 xxxx xxxx xxxx in the sTileHeader
			//Halfbrick is at xxxx x0xx xxxx xxxx in the sTileHeader

			Tile tile = new Tile();
			if(slope == TileSlopeVariant.HalfBrick)
				tile.halfBrick(true);
			else if(slope != TileSlopeVariant.Solid)
				tile.slope((byte)(slope));
			tile.type = type;
			return tile;
		}

		public static bool TileIsViable(int x, int y){
			Tile tile = Framing.GetTileSafely(x, y);

			//Return if the tile actually existed and its type is in the StructureIDs array
			return tile.nactive() && StructureTileIDs.Contains(tile.type);
		}

		public static bool TryReplaceStructure(int x, int y, out Point16 topLeftTileLocation, out Tile[,] structure, out int tileType){
			//Check against each structure until we find a valid one
			//If none are found, return false
			foreach(var pair in tileToStructure){
				if(TryFindStructure(x, y, pair.Value, out topLeftTileLocation)){
					structure = pair.Value;
					tileType = pair.Key;

					//If this structure has a powered entity on it, try to connect it to nearby networks
					MachineEntity entity = tileToEntity[tileType];

					if(entity is PoweredMachineEntity pme){
						Point16 checkOrig = topLeftTileLocation - new Point16(1, 1);

						int width = structure.GetLength(1), height = structure.GetLength(0);
						for(int cx = checkOrig.X; cx < width + 2; cx++){
							for(int cy = checkOrig.Y; cy < height + 2; cy++){
								//Ignore the corners
								if((cx == 0 && cy == 0) || (cx == width + 1 && cy == 0) || (cx == 0 && cy == height + 1) || (cx == width + 1 && cy == height + 1))
									continue;

								if(NetworkCollection.HasWireAt(new Point16(cx, cy), out WireNetwork net)){
									net.connectedMachines.Add(pme);
									return true;
								}
							}
						}
					}

					return true;
				}
			}

			//No structures found
			topLeftTileLocation = new Point16(-1, -1);
			structure = null;
			tileType = -1;
			return false;
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
					Tile tile = Framing.GetTileSafely(tileX, tileY);
					//Tile must be active and not actuated, not have liquid in it and be the same type as the top-left tile in the structure
					if(tile.nactive() && tile.liquid == 0 && (structure[0, 0].slope() == tile.slope() || structure[0, 0].halfBrick() == tile.halfBrick()) && tile.type == structure[0, 0].type){
						//The tile matches, check the rest of them
						for(int tx = 0; tx < width; tx++){
							for(int ty = 0; ty < height; ty++){
								Tile structureTile = Main.tile[tileX + tx, tileY + ty];
								//If the tile doesn't match, end this local 2-nest loop and go back into the outer 2-nest loop
								if(structureTile.nactive() && structureTile.slope() != structure[ty, tx].slope() || structureTile.halfBrick() != structure[ty, tx].halfBrick() || structure[ty, tx].type != structureTile.type)
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

		public static Texture2D GetEffectTexture(this ModTile multitile, string effect)
			=> ModContent.GetTexture($"TerraScience/Content/Tiles/Multitiles/Overlays/Effect_{multitile.Name}_{effect}");

		public static void KillMachine(int i, int j, ref bool fail, ref bool noItem, Tile[,] structure){
			//Do things only when the tile is destroyed and its actually this tile
			Point16 mouse = Main.MouseWorld.ToTileCoordinates16();
			Tile tile = Main.tile[i, j];
			int tileX = tile.frameX / 18;
			int tileY = tile.frameY / 18;

			int columns = structure.GetLength(1);
			int rows = structure.GetLength(0);

			//Only run this code if the tile at the mouse is the same one as (i, j) and the tile is actually being destroyed
			if(!fail && i == mouse.X && j == mouse.Y){
				noItem = true;

				//Determine which tile in the structure was removed and place the others
				Tile structureTile = structure[tileY, tileX];
				int itemType = 0;

				//Determine the dropped item type
				switch(structureTile.type){
					case CopperPlating:
						itemType = ItemID.CopperPlating;
						break;
					case TinPlating:
						itemType = ItemID.TinPlating;
						break;
					case Glass:
						itemType = ItemID.Glass;
						break;
					case GrayBrick:
						itemType = ItemID.GrayBrick;
						break;
					case WoodBlock:
						itemType = ItemID.Wood;
						break;
					case RedBrick:
						itemType = ItemID.RedBrick;
						break;
				}

				//Modded tiles
				if(itemType == 0){
					if(structureTile.type == SupportType)
						itemType = ModContent.ItemType<MachineSupportItem>();
					else if(structureTile.type == BlastFurnaceType)
						itemType = ModContent.ItemType<BlastBrick>();
				}

				//Spawn the item
				if(itemType > 0)
					Item.NewItem(i * 16, j * 16, 16, 16, itemType);

				//Replace the other tiles
				for(int c = 0; c < columns; c++){
					for(int r = 0; r < rows; r++){
						//Only replace the tile if it's not this one
						if(r != tileY || c != tileX){
							Tile newTile = Main.tile[i - tileX + c, j - tileY + r];
							newTile.CopyFrom(structure[r, c]);
							newTile.active(true);
						}

						//If there's a machine entity present, kill it
						//Copy of TileEntity.Kill(int, int), but modified
						Point16 pos = new Point16(i - tileX + c, j - tileY + r);
						if(TileEntity.ByPosition.ContainsKey(pos)){
							TileEntity tileEntity = TileEntity.ByPosition[pos];
							if(tileEntity is MachineEntity me){
								me.OnKill();
								TileEntity.ByID.Remove(tileEntity.ID);
								TileEntity.ByPosition.Remove(pos);

								//Remove this machine from the wire networks if it's a powered machine
								if(tileEntity is PoweredMachineEntity pme)
									NetworkCollection.RemoveMachine(pme);
							}
						}

						//Only run this code on the last tile in the structure
						if(c == columns - 1 && r == rows - 1){
							//Update the frames for the tiles
							int minX = i - tileX - columns;
							int minY = j - tileY - rows;
							int sizeX = 2 * tileX;
							int sizeY = 2 * tileY;
							WorldGen.RangeFrame(minX, minY, sizeX, sizeY);
							//...and send a net message
							if(Main.netMode == NetmodeID.MultiplayerClient)
								NetMessage.SendTileRange(-1, minX, minY, sizeX, sizeY);
						}
					}
				}
			}
		}

		public static void MultitileDefaults(ModTile tile, string mapName, int type, uint width, uint height){
			Main.tileNoAttach[type] = true;
			Main.tileFrameImportant[type] = true;

			TileObjectData.newTile.CoordinateHeights = MiscUtils.Create1DArray(16, height);
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.Height = (int)height;
			TileObjectData.newTile.Width = (int)width;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.WaterDeath = false;
			TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
			TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
			TileObjectData.addTile(type);

			ModTranslation name = tile.CreateMapEntryName();
			name.SetDefault(mapName);
			tile.AddMapEntry(new Color(0xd1, 0x89, 0x32), name);

			tile.mineResist = 3f;
			//Metal sound
			tile.soundType = SoundID.Tink;
			tile.soundStyle = 1;
		}

		public static bool HandleMouse<TEntity>(Machine machine, Point16 tilePos, Func<bool> additionalCondition) where TEntity : MachineEntity{
			if(MiscUtils.TryGetTileEntity(tilePos, out TEntity entity) && additionalCondition()){
				TerraScience instance = TerraScience.Instance;
				string name = tileToStructureName[instance.TileType(machine.Name)];

				UserInterface ui = instance.machineLoader.GetInterface(name);

				//Force the current one to close if another one of the same type is going to be opened
				if(ui.CurrentState is MachineUI mui && mui.UIEntity.Position != tilePos)
					instance.machineLoader.HideUI(mui.MachineName);

				if(ui.CurrentState == null)
					instance.machineLoader.ShowUI(name, entity);
				else
					instance.machineLoader.HideUI(name);

				return true;
			}

			return false;
		}
	}

	public enum TileSlopeVariant{
		Solid,
		DownLeft,
		DownRight,
		UpLeft,
		UpRight,
		HalfBrick
	}
}
