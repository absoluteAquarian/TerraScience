using Microsoft.Xna.Framework.Graphics;
using SerousEnergyLib.TileData;
using SerousEnergyLib.Tiles;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Common;

namespace TerraScience.Content.Tiles.Networks.Fluids {
	public class BasicFluidPumpTile : BasePumpTile, IFluidPumpTile {
		public override NetworkType NetworkTypeToPlace => NetworkType.Fluids;

		public override int GetMaxTimer(int x, int y) => 34;

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			NetworkDrawing.DrawFluid(spriteBatch,
				ModContent.Request<Texture2D>("TerraScience/Assets/Tiles/Networks/Fluids/Effect_BasicFluidPumpTile_fluid"),
				new Point16(i, j),
				columnsPerSet: 4,
				rowsPerSet: 1);

			return true;
		}
	}
}
