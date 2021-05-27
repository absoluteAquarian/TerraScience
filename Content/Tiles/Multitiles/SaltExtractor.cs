using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.Items.Placeable.Machines;
using TerraScience.Content.TileEntities;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles{
	public class SaltExtractor : Machine{
		public override void GetDefaultParams(out string mapName, out uint width, out uint height, out int itemType){
			mapName = "Salt Extractor";
			width = 4;
			height = 3;
			itemType = ModContent.ItemType<SaltExtractorItem>();
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b){
			Tile tile = Framing.GetTileSafely(i, j);
			if(MiscUtils.TryGetTileEntity(new Point16(i, j) - tile.TileCoord(), out SaltExtractorEntity se) && se.ReactionInProgress){
				Vector3 color = new Vector3(0xD5, 0x44, 0x00) * 2.35f;
				r = color.X;
				g = color.Y;
				b = color.Z;
			}
		}

		public override bool PreHandleMouse(Point16 pos)
			=> TileUtils.TryPlaceLiquidInMachine<SaltExtractorEntity>(this, pos);

		public override bool HandleMouse(Point16 pos){
			var id = MiscUtils.GetIDFromItem(Main.LocalPlayer.HeldItem.type);

			return TileUtils.HandleMouse<SaltExtractorEntity>(this, pos, () => MiscUtils.TryGetTileEntity(pos, out SaltExtractorEntity entity) && !Array.Exists(entity.LiquidEntries[0].validTypes, t => t == id));
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			//Draw the water in the side vials
			GetDefaultParams(out _, out uint width, out uint height, out _);

			Tile tile = Framing.GetTileSafely(i, j);
			Point16 frame = tile.TileCoord();
			Point16 pos = new Point16(i, j) - frame;
			bool lastTile = frame.X == width - 1 && frame.Y == height - 1;

			int maxWaterDrawDiff = 22;

			if(MiscUtils.TryGetTileEntity(pos, out SaltExtractorEntity se) && lastTile){
				//Do the rest of the things
				float curWaterRatio = se.LiquidEntries[0].current / se.LiquidEntries[0].max;
				float invRatio = 1f - curWaterRatio;
				Vector2 offset = MiscUtils.GetLightingDrawOffset();

				Point drawPos = (se.Position.ToVector2() * 16 - Main.screenPosition + offset).ToPoint();

				Rectangle draw = new Rectangle(drawPos.X, drawPos.Y + 16 + (int)(maxWaterDrawDiff * invRatio), 64, (int)(maxWaterDrawDiff * curWaterRatio));
				Rectangle source = new Rectangle(0, (int)(maxWaterDrawDiff * invRatio) + 16, 64, (int)(maxWaterDrawDiff * curWaterRatio));
				Rectangle fireDraw = new Rectangle(drawPos.X, drawPos.Y, 64, 48);

				//Draw the bars if the machine has enough liquid
				if(curWaterRatio > 1f / maxWaterDrawDiff)
					spriteBatch.Draw(this.GetEffectTexture("water"), draw, source, Lighting.GetColor(i, j));

				//Draw the fire if the machine active
				if(se.ReactionInProgress)
					spriteBatch.Draw(this.GetEffectTexture("fire"), fireDraw, Color.White);
			}
		}
	}
}
