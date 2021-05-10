using System;
using System.Text;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Items.Placeable.Machines;
using TerraScience.Content.TileEntities;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Content.UI;
using TerraScience.Systems.Energy;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles{
	public abstract class Machine : ModTile{
		public sealed override void SetDefaults(){
			GetDefaultParams(out string mapName, out uint width, out uint height, out _);
			TileUtils.MultitileDefaults(this, mapName, Type, width, height);
		}

		public abstract void GetDefaultParams(out string mapName, out uint width, out uint height, out int itemType);

		/// <summary>
		/// Return <seealso cref="TileUtils.HandleMouse{TEntity}(Point16, string, Func{bool})"/> for your MachineEntity here.
		/// </summary>
		public abstract bool HandleMouse(Point16 pos);

		public virtual bool PreHandleMouse(Point16 pos) => false;

		public sealed override bool NewRightClick(int i, int j) {
			Tile tile = Framing.GetTileSafely(i, j);
			Point16 pos = new Point16(i, j) - tile.TileCoord();

			if(PreHandleMouse(pos))
				return true;
			return HandleMouse(pos);
		}

		public sealed override void KillMultiTile(int i, int j, int frameX, int frameY)
			=> TileUtils.KillMachine(i, j, Type);

		public sealed override void MouseOver(int i, int j){
			Tile tile = Framing.GetTileSafely(i, j);
			Point16 pos = new Point16(i, j) - tile.TileCoord();

			if(MiscUtils.TryGetTileEntity(pos, out MachineEntity entity) && entity.MachineName != null){
				Player player = Main.LocalPlayer;
				player.mouseInterface = true;
				player.noThrow = 2;
				player.showItemIcon = true;
				player.showItemIcon2 = TerraScience.Instance.machineLoader.GetState<MachineUI>(entity.MachineName).GetIconType();
			}
		}

		public override void PlaceInWorld(int i, int j, Item item){
			MachineItem mItem = item.modItem as MachineItem;

			GetDefaultParams(out _, out uint width, out uint height, out _);

			Point16 tePos = new Point16(i, j) - new Point16((int)width / 2, (int)height - 1);

			int type = (item.modItem as MachineItem).TileType;

			/*
			Main.NewText($"Attempting to access entity dictionary with type \"{ModContent.GetModTile(type).Name}\" (ID: {type})");

			StringBuilder sb = new StringBuilder(200);
			sb.Append("Keys: ");
			foreach(var key in TileUtils.tileToEntity.Keys)
				sb.Append($"\"{ModContent.GetModTile(key).Name}\" ({key}), ");
			sb.Remove(sb.Length - 2, 2);

			Main.NewText(sb.ToString());
			*/

			MachineEntity entity = TileUtils.tileToEntity[type];

			if(entity.Find(tePos.X, tePos.Y) < 0){
				int id = entity.Place(tePos.X, tePos.Y);

				if(Main.netMode == NetmodeID.MultiplayerClient)
					NetMessage.SendData(MessageID.TileEntitySharing, remoteClient: -1, ignoreClient: Main.myPlayer, number: id);
			}

			//Restore the saved data, if it exists
			MachineEntity placed = TileEntity.ByPosition[tePos] as MachineEntity;
			if(mItem.entityData != null){
				placed.Load(mItem.entityData);

			//	Main.NewText("Machine loaded data from item");
			}

			//If this structure has a powered entity on it, try to connect it to nearby networks
			if(placed is PoweredMachineEntity pme){
				Point16 checkOrig = tePos - new Point16(1, 1);

				for(int cx = checkOrig.X; cx < checkOrig.X + width + 2; cx++){
					for(int cy = checkOrig.Y; cy < checkOrig.Y + height + 2; cy++){
						WorldGen.TileFrame(cx, cy);

						//Ignore the corners
						if((cx == 0 && cy == 0) || (cx == width + 1 && cy == 0) || (cx == 0 && cy == height + 1) || (cx == width + 1 && cy == height + 1))
							continue;

						if(NetworkCollection.HasWireAt(new Point16(cx, cy), out WireNetwork net) && !net.connectedMachines.Contains(pme)){
							net.connectedMachines.Add(pme);

						//	Main.NewText($"Connected machine \"{pme.MachineName}\" to network {net.id}");
						}
					}
				}

				if(Main.netMode == NetmodeID.MultiplayerClient)
					NetMessage.SendTileRange(Main.myPlayer, checkOrig.X, checkOrig.Y, (int)width + 1, (int)height + 1);
			}
		}
	}
}
