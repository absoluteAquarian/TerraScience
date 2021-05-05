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
		public static ushort SupportType => (ushort)ModContent.TileType<MachineSupport>();
		public static ushort BlastFurnaceType => (ushort)ModContent.TileType<BlastBrickTile>();

		public static Dictionary<int, MachineEntity> tileToEntity;

		public static Dictionary<int, string> tileToStructureName;

		public static Vector2 TileEntityCenter(TileEntity entity, int tileType) {
			Machine tile = ModContent.GetModTile(tileType) as Machine;
			tile.GetDefaultParams(out _, out uint width, out uint height, out _);

			Point16 topLeft = entity.Position;
			Point16 size = new Point16((int)width, (int)height);
			Vector2 worldTopLeft = topLeft.ToVector2() * 16;
			return worldTopLeft + size.ToVector2() * 8; // * 16 / 2
		}

		public static Point16 Frame(this Tile tile)
			=> new Point16(tile.frameX, tile.frameY);

		public static Point16 TileCoord(this Tile tile)
			=> new Point16(tile.frameX / 18, tile.frameY / 18);

		public static Texture2D GetEffectTexture(this ModTile multitile, string effect)
			=> ModContent.GetTexture($"TerraScience/Content/Tiles/Multitiles/Overlays/Effect_{multitile.Name}_{effect}");

		public static void KillMachine(int i, int j, int type){
			Tile tile = Main.tile[i, j];
			Machine mTile = ModContent.GetModTile(type) as Machine;
			mTile.GetDefaultParams(out _, out uint width, out uint height, out _);

			Point16 tePos = new Point16(i, j) - tile.TileCoord();
			if(TileEntity.ByPosition.ContainsKey(tePos)){
				TileEntity tileEntity = TileEntity.ByPosition[tePos];
				if(tileEntity is MachineEntity me){
					me.OnKill();
					TileEntity.ByID.Remove(tileEntity.ID);
					TileEntity.ByPosition.Remove(pos);

					//Remove this machine from the wire networks if it's a powered machine
					if(tileEntity is PoweredMachineEntity pme)
						NetworkCollection.RemoveMachine(pme);
				}
			}

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

		public static void MultitileDefaults(ModTile tile, string mapName, int type, uint width, uint height, int item){
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
			TileObjectData.newTile.Origin = new Point16((int)width / 2, (int)height - 1);
			TileObjectData.addTile(type);

			ModTranslation name = tile.CreateMapEntryName();
			name.SetDefault(mapName);
			tile.AddMapEntry(new Color(0xd1, 0x89, 0x32), name);

			tile.mineResist = 3f;
			//Metal sound
			tile.soundType = SoundID.Tink;
			tile.soundStyle = 1;

			tile.drop = item;
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
