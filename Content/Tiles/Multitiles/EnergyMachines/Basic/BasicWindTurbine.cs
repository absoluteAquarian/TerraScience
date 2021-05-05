using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using TerraScience.Content.TileEntities.Energy.Generators;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles.EnergyMachines.Basic{
	public class BasicWindTurbine : Machine{
		public override void GetDefaultParams(out string mapName, out uint width, out uint height){
			mapName = "Wind Turbine";
			width = 5;
			height = 8;
		}

		public override Tile[,] Structure => TileUtils.Structures.BasicWindTurbine;

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch){
			Tile tile = Framing.GetTileSafely(i, j);
			Point16 frame = tile.TileCoord();
			Point16 pos = new Point16(i, j) - frame;
			bool lastTile = frame.X == Structure.GetLength(1) - 1 && frame.Y == Structure.GetLength(0) - 1;

			if(MiscUtils.TryGetTileEntity(pos, out BasicWindTurbineEntity wind) && lastTile){
				Vector2 overlayOrigin = new Vector2(40, 40);
				Vector2 draw = pos.ToWorldCoordinates(0, 0) + overlayOrigin + MiscUtils.GetLightingDrawOffset() - Main.screenPosition;

				Texture2D blade = this.GetEffectTexture("blade");

				spriteBatch.Draw(blade, draw, null, Lighting.GetColor(i, j), wind.bladeRotation, overlayOrigin, 1f, SpriteEffects.None, 0f);
			}
		}

		public override bool HandleMouse(Point16 pos)
			=> TileUtils.HandleMouse<BasicWindTurbineEntity>(this, pos, () => true);
	}
}
