using Microsoft.Xna.Framework.Graphics;
using SerousEnergyLib.TileData;
using SerousEnergyLib.Tiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ObjectData;
using TerraScience.Common;

namespace TerraScience.Content.Tiles.Networks {
	/// <summary>
	/// A base implementation for am <see cref="IPumpTile"/> tile
	/// </summary>
	public abstract class BasePumpTile : BaseNetworkEntryTile, IPumpTile {
		public virtual PumpDirection GetDirection(int x, int y) {
			Tile tile = Main.tile[x, y];

			int mode = tile.TileFrameX / 18;

			return mode switch {
				0 => PumpDirection.Up,
				1 => PumpDirection.Left,
				2 => PumpDirection.Down,
				3 => PumpDirection.Right,
				_ => PumpDirection.Left
			};
		}

		public abstract int GetMaxTimer(int x, int y);

		protected override void PreRegisterTileObjectData() {
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleWrapLimit = 16;
		}

		/// <summary>
		/// The path to the spritesheet used to draw this pump's moving bar
		/// </summary>
		public virtual string BarTexture => (GetType().Namespace + "/Effect_" + Name + "_bar").Replace('.', '/').Replace("Content", "Assets");

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			NetworkDrawing.DrawPumpBar(spriteBatch,
				ModAssets.PumpBar[Type],
				new Point16(i, j));
		}
	}
}
