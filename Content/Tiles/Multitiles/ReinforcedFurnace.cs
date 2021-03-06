﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.Items.Placeable.Machines;
using TerraScience.Content.TileEntities;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles{
	public class ReinforcedFurnace : Machine{
		public override void GetDefaultParams(out string mapName, out uint width, out uint height, out int itemType){
			mapName = "Reinforced Furnace";
			width = 4;
			height = 4;
			itemType = ModContent.ItemType<ReinforcedFurnaceItem>();
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b){
			Tile tile = Framing.GetTileSafely(i, j);
			if(MiscUtils.TryGetTileEntity(new Point16(i, j) - tile.TileCoord(), out ReinforcedFurnaceEntity entity) && entity.ReactionInProgress){
				Vector3 color = new Vector3(0xD5, 0x44, 0x00) * 2.35f;
				r = color.X;
				g = color.Y;
				b = color.Z;
			}
		}

		public override bool HandleMouse(Point16 pos)
			=> TileUtils.HandleMouse<ReinforcedFurnaceEntity>(this, pos, () => true);

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch){
			GetDefaultParams(out _, out uint width, out uint height, out _);

			Tile tile = Framing.GetTileSafely(i, j);
			Point16 frame = tile.TileCoord();
			Point16 pos = new Point16(i, j) - frame;
			bool lastTile = frame.X == width - 1 && frame.Y == height - 1;

			if(MiscUtils.TryGetTileEntity(pos, out ReinforcedFurnaceEntity furnace) && lastTile){
				//If the UI is open, open the door and draw the rest of the things
				//Otherwise, just draw the closed door
				Vector2 offset = MiscUtils.GetLightingDrawOffset();
				Point drawPos = (furnace.Position.ToVector2() * 16 - Main.screenPosition + offset).ToPoint();
				Rectangle draw = new Rectangle(drawPos.X, drawPos.Y, 64, 64);

				if(furnace.ParentState?.Active ?? false){
					//Opened door
					spriteBatch.Draw(this.GetEffectTexture("dooropen"), draw, null, Lighting.GetColor(i, j));

					//If there's any fuel, draw it and check for fire
					if(furnace.ParentState.GetSlot(0).StoredItem.stack > 0){
						spriteBatch.Draw(this.GetEffectTexture("fuel"), draw, null, Color.Lerp(Color.White, Color.Red, furnace.Heat / ReinforcedFurnaceEntity.HeatMax));

						//Draw the fire if we've reached the burning point of wood
						if(furnace.Heat >= 300)
							spriteBatch.Draw(this.GetEffectTexture("fire"), draw, null, Color.White);
					}

					//Make the heater turn more orange as the heat rises (same logic as fuel)
					spriteBatch.Draw(this.GetEffectTexture("heater"), draw, null, Color.Lerp(Color.White, Color.Orange, furnace.Heat / ReinforcedFurnaceEntity.HeatMax));
				}else{
					//Closed door
					spriteBatch.Draw(this.GetEffectTexture("doorclose"), draw, null, Lighting.GetColor(i, j));
				}
			}
		}
	}
}
