using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.Items.Placeable.Machines.Energy.Generators;
using TerraScience.Content.TileEntities.Energy.Generators;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles.EnergyMachines.Basic{
	public class BasicThermoGenerator : Machine{
		public override void GetDefaultParams(out string mapName, out uint width, out uint height, out int itemType){
			mapName = "Basic Thermal Generator";
			width = 3;
			height = 3;
			itemType = ModContent.ItemType<BasicThermoGeneratorItem>();
		}

		public override bool HandleMouse(Point16 pos)
			=> TileUtils.HandleMouse<BasicThermoGeneratorEntity>(this, pos, () => true);

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch){
			GetDefaultParams(out _, out uint width, out uint height, out _);

			Tile tile = Framing.GetTileSafely(i, j);
			Point16 frame = tile.TileCoord();
			Point16 pos = new Point16(i, j) - frame;
			bool lastTile = frame.X == width - 1 && frame.Y == height - 1;

			if(MiscUtils.TryGetTileEntity(pos, out BasicThermoGeneratorEntity entity) && lastTile){
				Vector2 offset = MiscUtils.GetLightingDrawOffset();

				Point drawPos = (entity.Position.ToVector2() * 16 - Main.screenPosition + offset).ToPoint();

				if(entity.ReactionInProgress && entity.StoredFlux < entity.FluxCap){
					spriteBatch.Draw(this.GetEffectTexture("fire"), drawPos.ToVector2(), null, Color.White);
					Lighting.AddLight(drawPos.ToVector2() + new Vector2(width * 16 / 2f, height * 16 / 2f), 0.83f, 0.25f, 0.12f);
				}

				string effect = entity.spinTimer % 36 < 18 ? "1" : "2";
				spriteBatch.Draw(this.GetEffectTexture("turbine spin" + effect), drawPos.ToVector2(), null, Color.White);
			}
		}
	}
}
