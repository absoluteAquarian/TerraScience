using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.Items.Placeable.Machines.Energy.Storage;
using TerraScience.Content.TileEntities.Energy.Storage;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles.EnergyMachines.Storage{
	public class BasicBattery : Machine{
		public override void GetDefaultParams(out string mapName, out uint width, out uint height, out int itemType){
			mapName = "Battery";
			width = 3;
			height = 4;
			itemType = ModContent.ItemType<BasicBatteryItem>();
		}

		public override bool HandleMouse(Point16 pos)
			=> TileUtils.HandleMouse<BasicBatteryEntity>(this, pos, () => true);

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch){
			GetDefaultParams(out _, out uint width, out uint height, out _);

			Tile tile = Framing.GetTileSafely(i, j);
			Point16 frame = tile.TileCoord();
			Point16 pos = new Point16(i, j) - frame;
			bool lastTile = frame.X == width - 1 && frame.Y == height - 1;

			if(MiscUtils.TryGetTileEntity(pos, out BasicBatteryEntity entity) && lastTile){
				//Draw the overlay stuff
				Vector2 offset = MiscUtils.GetLightingDrawOffset();

				Vector2 drawPos = entity.Position.ToVector2() * 16 - Main.screenPosition + offset;
				var color = Lighting.GetColor(i, j);

				spriteBatch.Draw(this.GetEffectTexture("charge"), drawPos, null, color);

				if((float)entity.StoredFlux > 0){
					float rectHeight = 42 * (float)entity.StoredFlux / (float)entity.FluxCap;
					Rectangle lightRect = new Rectangle(14, 10 + 42 - (int)rectHeight, 20, (int)rectHeight);

					spriteBatch.Draw(this.GetEffectTexture("light"), drawPos + lightRect.Location.ToVector2(), lightRect, Color.White);
				}
			}
		}
	}
}
