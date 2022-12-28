using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.GameContent;
using TerraScience.Content.Items.Placeable.Machines.Energy;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles.EnergyMachines{
	public class AutoExtractinator : Machine{
		public override void GetDefaultParams(out string mapName, out uint width, out uint height, out int itemType){
			mapName = "Auto-Extractinator";
			width = 3;
			height = 3;
			itemType = ModContent.ItemType<AutoExtractinatorItem>();
		}

		public override bool HandleMouse(Point16 pos)
			=> TileUtils.HandleMouse<AutoExtractinatorEntity>(this, pos, () => true);

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch){
			Point16 pos = new Point16(i, j);

			//Only draw if the TileCoord is (0, 0), otherwise we'll get weird effects like the things being drawn on top of other tiles
			if(MiscUtils.TryGetTileEntity(pos, out AutoExtractinatorEntity entity) && !entity.RetrieveItem(0).IsAir){
				Vector2 offset = MiscUtils.GetLightingDrawOffset();
				Vector2 drawPos = entity.Position.ToVector2() * 16 - Main.screenPosition + offset;
				
				//Draw the extracting tile slowly sinking into the machine
				var tileType = entity.RetrieveItem(0).createTile;
				//Make sure the texture isn't null
				Main.instance.LoadTiles(tileType);
				Texture2D texture = TextureAssets.Tile[tileType].Value;
				Rectangle frame = texture.Frame(16, 15, 6 + entity.frameRand, 0);
				Rectangle frame2 = texture.Frame(16, 15, 6 + entity.frameRand2, 0);

				//intended position + initial draw height + sink distance * reaction progress / 100
				Vector2 draw = drawPos + new Vector2(16, -12);

				if(entity.RetrieveItem(0).stack > 1)
					spriteBatch.Draw(texture, draw, frame2, Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

				spriteBatch.Draw(texture, draw + new Vector2(0, 16) * entity.ReactionProgress / 100f, frame, Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			}

			return true;
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch){
			GetDefaultParams(out _, out uint width, out uint height, out _);

			Tile tile = Framing.GetTileSafely(i, j);
			Point16 frame = tile.TileCoord();
			Point16 pos = new Point16(i, j) - frame;
			bool lastTile = frame.X == width - 1 && frame.Y == height - 1;

			if(MiscUtils.TryGetTileEntity(pos, out AutoExtractinatorEntity entity) && lastTile){
				//Draw the overlay stuff
				Vector2 offset = MiscUtils.GetLightingDrawOffset();

				Vector2 drawPos = entity.Position.ToVector2() * 16 - Main.screenPosition + offset;
				var color = Lighting.GetColor(i, j);

				spriteBatch.Draw(this.GetEffectTexture("hole"), drawPos, null, color);

				spriteBatch.Draw(this.GetEffectTexture("shifter"), drawPos + new Vector2(3, 0) * (float)Math.Sin(entity.shifterTopSin), null, color);
				spriteBatch.Draw(this.GetEffectTexture("shifter"), drawPos + new Vector2(0, 4) + new Vector2(3, 0) * (float)Math.Sin(entity.shifterBottomSin), null, color);
			}
		}
	}
}
