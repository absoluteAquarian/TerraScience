using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.Items.Placeable.Machines;
using TerraScience.Content.TileEntities;
using TerraScience.Systems;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles{
	public class Tesseract : Machine{
		public override void GetDefaultParams(out string mapName, out uint width, out uint height, out int itemType){
			mapName = "Tesseract";
			width = 3;
			height = 3;
			itemType = ModContent.ItemType<TesseractItem>();
		}

		public override bool HandleMouse(Point16 pos)
			=> TileUtils.HandleMouse<TesseractEntity>(this, pos, null);

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch){
			GetDefaultParams(out _, out uint width, out uint height, out _);

			Tile tile = Framing.GetTileSafely(i, j);
			Point16 frame = tile.TileCoord();
			Point16 pos = new Point16(i, j) - frame;
			bool lastTile = frame.X == width - 1 && frame.Y == height - 1;

			if(MiscUtils.TryGetTileEntity(pos, out TesseractEntity entity) && lastTile && entity.BoundNetwork != null && TesseractNetwork.TryGetEntry(entity.BoundNetwork, out _)){
				//Draw the overlay stuff
				Vector2 offset = MiscUtils.GetLightingDrawOffset();

				Vector2 drawPos = entity.Position.ToVector2() * 16 - Main.screenPosition + offset;
				var color = Lighting.GetColor(i, j);

				int frameY = (int)Main.GameUpdateCount % (11 * 7) / 7;

				var texture = this.GetEffectTexture("active");
				spriteBatch.Draw(texture, drawPos, texture.Frame(1, 11, 0, frameY), color);
			}
		}
	}
}
