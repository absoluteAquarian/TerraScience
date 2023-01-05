using Microsoft.Xna.Framework.Graphics;
using SerousEnergyLib.TileData;
using SerousEnergyLib.Tiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Common;

namespace TerraScience.Content.Tiles.Networks.Items {
	public class BasicItemPumpTile : BaseNetworkEntryTile, IItemPumpTile {
		public override NetworkType NetworkTypeToPlace => NetworkType.Items;

		public virtual int StackPerExtraction => 1;

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

		public virtual float GetItemSize(int x, int y) => 3.85f * 2;

		public virtual int GetMaxTimer(int x, int y) => 22;

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			// TODO: config for disabling bar drawing
			NetworkDrawing.DrawPumpBar(spriteBatch,
				ModContent.Request<Texture2D>("TerraScience/Assets/Tiles/Networks/Items/Effect_BasicItemPumpTile_bar"),
				new Point16(i, j));
		}
	}
}
