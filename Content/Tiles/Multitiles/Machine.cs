using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles{
	public abstract class Machine : ModTile{
		public sealed override void SetDefaults(){
			GetDefaultParams(out string mapName, out uint width, out uint height);
			TileUtils.MultitileDefaults(this, mapName, Type, width, height);
		}

		public abstract void GetDefaultParams(out string mapName, out uint width, out uint height);

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

		public abstract Tile[,] GetStructure();

		public sealed override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
			=> TileUtils.KillMachine(i, j, ref fail, ref noItem, GetStructure());
	}
}
