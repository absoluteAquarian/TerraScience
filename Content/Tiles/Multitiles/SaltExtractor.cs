using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.ID;
using TerraScience.Content.Items.Materials;
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

		public override bool PreHandleMouse(Point16 pos){
			if(MiscUtils.TryGetTileEntity(pos, out SaltExtractorEntity se) && Main.LocalPlayer.HeldItemIsViableForSaltExtractor(pos) && se.WaterPlaceDelay == 0 && se.StoredLiquid < SaltExtractorEntity.MaxLiquid - 1){
				se.WaterPlaceDelay = SaltExtractorEntity.MaxPlaceDelay;
				se.StoredLiquid++;
				se.PlaySound(SoundID.Splash);

				//Only mess with the player items if the Salt Extractor isn't full
				if (se.StoredLiquid > SaltExtractorEntity.MaxLiquid)
					se.StoredLiquid = SaltExtractorEntity.MaxLiquid;
				else{
					Item heldItem = Main.LocalPlayer.HeldItem;

					//Set the liquid type
					if(heldItem.type == ItemID.WaterBucket || heldItem.type == ItemID.BottomlessBucket || heldItem.type == ModContent.ItemType<Vial_Water>())
						se.LiquidTypes[0] = MachineLiquidID.Water;
					else if(heldItem.type == ModContent.ItemType<Vial_Saltwater>())
						se.LiquidTypes[0] = MachineLiquidID.Saltwater;

					//And give the player back the container they used (unless it's the bottomless bucket)
					if(heldItem.type == ItemID.WaterBucket){
						Main.LocalPlayer.HeldItem.stack--;
						Main.LocalPlayer.QuickSpawnItem(ItemID.EmptyBucket);
					}else if(heldItem.type == ModContent.ItemType<Vial_Saltwater>() || heldItem.type == ModContent.ItemType<Vial_Water>()){
						Main.LocalPlayer.HeldItem.stack--;
						Main.LocalPlayer.QuickSpawnItem(ModContent.ItemType<EmptyVial>());
					}

					se.ReactionInProgress = true;
				}

				//Something happened
				return true;
			}

			return false;
		}

		public override bool HandleMouse(Point16 pos)
			=> TileUtils.HandleMouse<SaltExtractorEntity>(this, pos, () => !Main.LocalPlayer.HeldItemIsViableForSaltExtractor(pos));

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
				float curWaterRatio = se.StoredLiquid / SaltExtractorEntity.MaxLiquid;
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
