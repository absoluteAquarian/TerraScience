using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Items.Materials;
using TerraScience.Content.TileEntities;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles{
	public class Electrolyzer : Machine{
		//Wow a lot of this is a copy-pasta of the Salt Extractor.  I should fix that

		public override void GetDefaultParams(out string mapName, out uint width, out uint height){
			mapName = "Electrolyzer";
			width = 5;
			height = 4;
		}

		public override Tile[,] Structure => TileUtils.Structures.Electrolyzer;

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch){
			int maxWaterDrawDiff = 42;

			//Only draw behind if this is the top-left tile.  Otherwise, the things start drawing on top of other tiles
			if(MiscUtils.TryGetTileEntity(new Point16(i, j), out ElectrolyzerEntity entity)){
				//Draw order: water back, bars, water front
				float curWaterRatio = entity.StoredLiquid / ElectrolyzerEntity.MaxLiquid;
				float invRatio = 1f - curWaterRatio;
				Vector2 offset = MiscUtils.GetLightingDrawOffset();

				Point drawPos = (entity.Position.ToVector2() * 16 - Main.screenPosition + offset).ToPoint();

				Rectangle draw = new Rectangle(drawPos.X, drawPos.Y + 18 + (int)(maxWaterDrawDiff * invRatio), 80, (int)(maxWaterDrawDiff * curWaterRatio));
				Rectangle source = new Rectangle(0, (int)(maxWaterDrawDiff * invRatio) + 18, 80, (int)(maxWaterDrawDiff * curWaterRatio));

				if(entity.StoredLiquid > 0)
					spriteBatch.Draw(this.GetEffectTexture("water"), draw, source, Lighting.GetColor(i, j));

				spriteBatch.Draw(this.GetEffectTexture("bars"), drawPos.ToVector2(), null, Lighting.GetColor(i, j));

				if(entity.StoredLiquid > 0)
					spriteBatch.Draw(this.GetEffectTexture("water"), draw, source, Lighting.GetColor(i, j) * (50f / 255f));
			}

			return true;
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch){
			Tile tile = Framing.GetTileSafely(i, j);
			Point16 pos = new Point16(i, j) - tile.TileCoord();
			if(MiscUtils.TryGetTileEntity(pos, out ElectrolyzerEntity entity)){
				//Draw order: battery, lights, gas overlay, tanks
				Vector2 offset = MiscUtils.GetLightingDrawOffset();

				Point drawPos = (entity.Position.ToVector2() * 16 - Main.screenPosition + offset).ToPoint();

				if(!entity.RetrieveItem(0).IsAir){
					spriteBatch.Draw(this.GetEffectTexture("battery"), drawPos.ToVector2(), null, Lighting.GetColor(i, j));

					if(entity.ReactionInProgress){
						spriteBatch.Draw(this.GetEffectTexture("lightgreen"), drawPos.ToVector2(), null, Color.White);
						Lighting.AddLight(drawPos.ToVector2() + new Vector2(44, 20), 0f, 0.87f, 0f);
					}else{
						spriteBatch.Draw(this.GetEffectTexture("lightred"), drawPos.ToVector2(), null, Color.White);
						Lighting.AddLight(drawPos.ToVector2() + new Vector2(36, 20), 0.87f, 0f, 0f);
					}
				}

				float hydroFactor = entity.RetrieveItem(1).stack / 100f;
				float oxyFactor = entity.RetrieveItem(2).stack / 100f;

				if(hydroFactor > 0)
					spriteBatch.Draw(this.GetEffectTexture("gashydrogen"), drawPos.ToVector2(), null, Lighting.GetColor(i, j) * (hydroFactor * 75f / 255f));
				if(oxyFactor > 0)
					spriteBatch.Draw(this.GetEffectTexture("gasoxygen"), drawPos.ToVector2(), null, Lighting.GetColor(i, j) * (oxyFactor * 75f / 255f));

				spriteBatch.Draw(this.GetEffectTexture("tanks"), drawPos.ToVector2(), null, Lighting.GetColor(i, j));
			}
		}

		public override bool PreHandleMouse(Point16 pos){
			if(MiscUtils.TryGetTileEntity(pos, out ElectrolyzerEntity ee) && Main.LocalPlayer.HeldItemIsViableForElectrolyzer(pos) && ee.WaterPlaceDelay == 0 && ee.StoredLiquid < ElectrolyzerEntity.MaxLiquid - 1){
				ee.WaterPlaceDelay = ElectrolyzerEntity.MaxPlaceDelay;
				ee.StoredLiquid++;
				Main.PlaySound(SoundID.Splash);

				//Only mess with the player items if the Salt Extractor isn't full
				if (ee.StoredLiquid > ElectrolyzerEntity.MaxLiquid)
					ee.StoredLiquid = ElectrolyzerEntity.MaxLiquid;
				else{
					Item heldItem = Main.LocalPlayer.HeldItem;

					//And give the player back the container they used (unless it's the bottomless bucket)
					if(heldItem.type == ItemID.WaterBucket){
						Main.LocalPlayer.HeldItem.stack--;
						Main.LocalPlayer.QuickSpawnItem(ItemID.EmptyBucket);
					}else if(heldItem.type == ModContent.ItemType<Vial_Water>()){
						Main.LocalPlayer.HeldItem.stack--;
						Main.LocalPlayer.QuickSpawnItem(ModContent.ItemType<EmptyVial>());
					}
				}

				//Something happened
				return true;
			}

			return false;
		}

		public override bool HandleMouse(Point16 pos)
			=> TileUtils.HandleMouse<ElectrolyzerEntity>(this, pos, () => !Main.LocalPlayer.HeldItemIsViableForElectrolyzer(pos));
	}
}
