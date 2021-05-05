using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using static Terraria.ID.TileID;
using Terraria.ModLoader;
using TerraScience.Utilities;
using System.Linq;
using System;
using System.Reflection;

namespace TerraScience.API.StructureData{
	public static class StructureExtractor{
		//Sizes have to be hardcoded.  This can't be avoided without dealing with a bunch of headaches.  Oh well.
		public static Dictionary<string, (byte, byte)> sizes;

		public static Dictionary<Color, ushort> structureTypes;

		public static void Load(){
			TileUtils.StructureTileIDs = new ushort[]{ CopperPlating, TinPlating, GrayBrick, Glass, WoodBlock, TileUtils.SupportType, RedBrick, WoodenBeam, TileUtils.BlastFurnaceType };

			//Load the sizes
			sizes = new Dictionary<string, (byte, byte)>();

			//Height: 3
			AddMachine(nameof(TileUtils.Structures.SaltExtractor), 3, 4);
			AddMachine(nameof(TileUtils.Structures.ScienceWorkbench), 3, 3);
			AddMachine(nameof(TileUtils.Structures.AirIonizer), 3, 3);

			//Height: 4
			AddMachine(nameof(TileUtils.Structures.ReinforcedFurncace), 4, 4);
			AddMachine(nameof(TileUtils.Structures.Electrolyzer), 4, 5);
			AddMachine(nameof(TileUtils.Structures.BasicBattery), 4, 3);

			//Height: 5
			AddMachine(nameof(TileUtils.Structures.BlastFurnace), 5, 5);

			//Height: 8
			AddMachine(nameof(TileUtils.Structures.BasicWindTurbine), 8, 5);

			//Parse the image
			structureTypes = new Dictionary<Color, ushort>();
			if(!Main.dedServ){
				Texture2D data = ModContent.GetTexture("TerraScience/API/StructureData/data");

				Color[] p = new Color[data.Width * data.Height];
				data.GetData(p);

				Color[,] pixels = p.To2DArray(data.Width, data.Height);

				//Parse the colour→tile ID section
				int typeIndex = 0;
				for(int r = 0; r < 3; r++){
					for(int c = 0; c < data.Width; c++){
						Color pixel = pixels[c, r];
						if(pixel.A == 0)
							break;

						structureTypes.Add(pixel, TileUtils.StructureTileIDs[typeIndex]);
						typeIndex++;
					}
				}

				//Parse the machines
				int row = 4;
				int column = 0;
				int oldRow = row;
				(byte, byte) oldSize = (4, 3);
				var machineNames = TileUtils.tileToStructureName.Values.ToList();
				foreach(string name in sizes.Keys){
					(byte, byte) size = sizes[name];

					if(oldSize.Item2 != size.Item2){
						row = oldRow;
						column = 0;

						row += oldSize.Item2 * 2 + 1;
						oldRow = row;

						oldSize = size;
					}

					//Parse this specific machine
					if(!machineNames.Contains(name))
						throw new Exception($"Invalid structure name: {name}");

					var field = typeof(TileUtils.Structures).GetField(name, BindingFlags.Public | BindingFlags.Static);
					Tile[,] structure = new Tile[size.Item2, size.Item1];

					for(int r = row, realR = 0; r < row + size.Item2 * 2; r += 2, realR++){
						for(int c = column, realC = 0; c < column + size.Item1 * 2; c += 2, realC++){
							//Determine which type and TileSlopeVariant to use
							ushort structureType = 0;
							TileSlopeVariant variant;
							bool[] empty = new bool[4]{ true, true, true, true };

							for(int tr = 0; tr < 2; tr++){
								for(int tc = 0; tc < 2; tc++){
									Color pixel = pixels[c + tc, r + tr];
									if(pixel.A != 0){
										if(structureType == 0)
											structureType = structureTypes[pixel];

										empty[tr * 2 + tc] = false;
									}
								}
							}

							if(structureType == 0)
								throw new Exception($"({c}, {r}) ({realC}, {realR}) Structure contained an empty tile: {name}");

							if(!empty[0] && !empty[1] && !empty[2] && !empty[3])
								variant = TileSlopeVariant.Solid;
							else if(!empty[0] && empty[1] && !empty[2] && !empty[3])
								variant = TileSlopeVariant.DownLeft;
							else if(empty[0] && !empty[1] && !empty[2] && !empty[3])
								variant = TileSlopeVariant.DownRight;
							else if(!empty[0] && !empty[1] && !empty[2] && empty[3])
								variant = TileSlopeVariant.UpLeft;
							else if(!empty[0] && !empty[1] && empty[2] && !empty[3])
								variant = TileSlopeVariant.UpRight;
							else if(empty[0] && empty[1] && !empty[2] && !empty[3])
								variant = TileSlopeVariant.HalfBrick;
							else
								throw new Exception($"({c}, {r}) ({realC}, {realR}) One of the structure squares for this multitile was invalid: {name}");

							structure[realR, realC] = TileUtils.NewTile(structureType, variant);
						}
					}

					field.SetValue(null, structure);

					TerraScience.Instance.Logger.Debug($"Structure \"{name}\" was loaded with a width of {size.Item1} and height of {size.Item2}");

					column += size.Item1 * 2 + 1;
				}
			}
		}

		private static void AddMachine(string name, byte height, byte width)
			=> sizes.Add(name, (width, height));

		public static void Unload(){
			sizes = null;
		}
	}
}
