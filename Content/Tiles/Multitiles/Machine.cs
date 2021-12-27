using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Items.Placeable.Machines;
using TerraScience.Content.TileEntities;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Systems;
using TerraScience.Systems.Energy;
using TerraScience.Systems.Pipes;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles {
	public abstract class Machine : ModTile{
		public sealed override void SetDefaults(){
			GetDefaultParams(out string mapName, out uint width, out uint height, out _);
			TileUtils.MultitileDefaults(this, mapName, Type, width, height);
		}

		public abstract void GetDefaultParams(out string mapName, out uint width, out uint height, out int itemType);

		/// <summary>
		/// Return <seealso cref="TileUtils.HandleMouse{TEntity}(Machine, Point16, Func{bool})"/> for your MachineEntity here.
		/// </summary>
		public abstract bool HandleMouse(Point16 pos);

		/// <summary>
		/// Allows you to let things happen before the UI is attempted to be opened for this machine.
		/// Return <see langword="true"/> to prevent <seealso cref="HandleMouse(Point16)"/> from executing
		/// </summary>
		public virtual bool PreHandleMouse(Point16 pos) => false;

		public sealed override bool NewRightClick(int i, int j) {
			Tile tile = Framing.GetTileSafely(i, j);
			Point16 pos = new Point16(i, j) - tile.TileCoord();

			return PreHandleMouse(pos) || HandleMouse(pos);
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
				player.showItemIcon2 = this.GetIconType();
			}
		}

		public override void PlaceInWorld(int i, int j, Item item){
			// TODO: TileObject.CanPlace is throwing null-ref exceptions.  why???

			MachineItem mItem = item.modItem as MachineItem;

			GetDefaultParams(out _, out uint width, out uint height, out _);

			Point16 tePos = new Point16(i, j) - new Point16((int)width / 2, (int)height - 1);

			int type = (item.modItem as MachineItem).TileType;

			MachineEntity entity = TileUtils.tileToEntity[type];

			if(entity.Find(tePos.X, tePos.Y) < 0){
				int id = entity.Place(tePos.X, tePos.Y);

				if(Main.netMode == NetmodeID.MultiplayerClient)
					NetMessage.SendData(MessageID.TileEntitySharing, remoteClient: -1, ignoreClient: Main.myPlayer, number: id);
			}

			//Restore the saved data, if it exists
			MachineEntity placed = TileEntity.ByPosition[tePos] as MachineEntity;
			if(mItem.entityData != null)
				placed.Load(mItem.entityData);

			//If this structure has a powered entity on it, try to connect it to nearby networks
			Point16 checkOrig = tePos - new Point16(1, 1);

			bool canUseWires = placed is PoweredMachineEntity;
			bool canUseItemPipes = placed.SlotsCount > 0;
			bool canUseFluidPipes = placed is IFluidMachine;

			for(int cx = checkOrig.X; cx < checkOrig.X + width + 2; cx++){
				for(int cy = checkOrig.Y; cy < checkOrig.Y + height + 2; cy++){
					WorldGen.TileFrame(cx, cy);

					//Ignore the corners
					if((cx == 0 && cy == 0) || (cx == width + 1 && cy == 0) || (cx == 0 && cy == height + 1) || (cx == width + 1 && cy == height + 1))
						continue;

					Point16 test = new Point16(cx, cy);

					if(canUseWires && NetworkCollection.HasWireAt(test, out WireNetwork wireNet))
						wireNet.AddMachine(placed);
					if(canUseItemPipes && NetworkCollection.HasItemPipeAt(test, out ItemNetwork itemNet)){
						itemNet.AddMachine(placed);
						itemNet.pipesConnectedToMachines.Add(test);
					}
					if(canUseFluidPipes && NetworkCollection.HasFluidPipeAt(test, out FluidNetwork fluidNet))
						fluidNet.AddMachine(placed);
				}
			}

			if(Main.netMode == NetmodeID.MultiplayerClient)
				NetMessage.SendTileRange(Main.myPlayer, checkOrig.X, checkOrig.Y, (int)width + 1, (int)height + 1);
		}
	}
}
