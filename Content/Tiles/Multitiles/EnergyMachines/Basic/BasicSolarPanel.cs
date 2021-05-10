using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.Items.Placeable.Machines.Energy.Generators;
using TerraScience.Content.TileEntities.Energy.Generators;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles.EnergyMachines.Basic{
	public class BasicSolarPanel : Machine{
		public override void GetDefaultParams(out string mapName, out uint width, out uint height, out int itemType){
			mapName = "Basic Solar Panel";
			width = 3;
			height = 3;
			itemType = ModContent.ItemType<BasicSolarPanelItem>();
		}

		public override bool HandleMouse(Point16 pos)
			=> TileUtils.HandleMouse<BasicSolarPanelEntity>(this, pos, () => true);

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch){
			GetDefaultParams(out _, out uint width, out uint height, out _);

			Tile tile = Framing.GetTileSafely(i, j);
			Point16 frame = tile.TileCoord();
			Point16 pos = new Point16(i, j) - frame;
			bool lastTile = frame.X == width - 1 && frame.Y == height - 1;

			if(MiscUtils.TryGetTileEntity(pos, out BasicSolarPanelEntity entity) && lastTile){
				Vector2 overlayOrigin = new Vector2(24, 10);
				Vector2 draw = pos.ToWorldCoordinates(0, 0) + overlayOrigin + MiscUtils.GetLightingDrawOffset() - Main.screenPosition;

				Texture2D panel = this.GetEffectTexture("panel");

				spriteBatch.Draw(panel, draw, null, Lighting.GetColor(i, j), entity.panelRotation, panel.Size() / 2f, 1f, SpriteEffects.None, 0f);
			}
		}
	}
}
