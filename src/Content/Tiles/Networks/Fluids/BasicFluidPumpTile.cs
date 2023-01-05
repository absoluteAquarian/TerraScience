using Microsoft.Xna.Framework.Graphics;
using SerousEnergyLib.TileData;
using SerousEnergyLib.Tiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Common;

namespace TerraScience.Content.Tiles.Networks.Fluids {
	public class BasicFluidPumpTile : BaseNetworkEntryTile, IFluidPumpTile {
		public override NetworkType NetworkTypeToPlace => NetworkType.Fluids;

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

		public virtual int GetMaxTimer(int x, int y) => 34;

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			NetworkDrawing.DrawFluid(spriteBatch,
				ModContent.Request<Texture2D>("TerraScience/Assets/Tiles/Networks/Fluids/Effect_BasicFluidPumpTile_fluid"),
				new Point16(i, j),
				columnsPerSet: 4,
				rowsPerSet: 1);

			return true;
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			// TODO: config for disabling bar drawing
			NetworkDrawing.DrawPumpBar(spriteBatch,
				ModContent.Request<Texture2D>("TerraScience/Assets/Tiles/Networks/Fluids/Effect_BasicFluidPumpTile_bar"),
				new Point16(i, j));
		}
	}
}
