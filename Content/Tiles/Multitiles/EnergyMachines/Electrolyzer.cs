using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.Items.Placeable.Machines.Energy;
using TerraScience.Content.Items.Tools;
using TerraScience.Content.TileEntities.Energy;
using TerraScience.Utilities;

namespace TerraScience.Content.Tiles.Multitiles.EnergyMachines{
	public class Electrolyzer : Machine{
		//Wow a lot of this is a copy-pasta of the Salt Extractor.  I should fix that

		public override void GetDefaultParams(out string mapName, out uint width, out uint height, out int itemType){
			mapName = "Electrolyzer";
			width = 5;
			height = 4;
			itemType = ModContent.ItemType<ElectrolyzerItem>();
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch){
			int maxWaterDrawDiff = 42;

			//Only draw behind if this is the top-left tile.  Otherwise, the things start drawing on top of other tiles
			if(MiscUtils.TryGetTileEntity(new Point16(i, j), out ElectrolyzerEntity entity)){
				//Draw order: water back, bars, water front
				float curWaterRatio = entity.FluidEntries[0].current / entity.FluidEntries[0].max;
				float invRatio = 1f - curWaterRatio;
				Vector2 offset = MiscUtils.GetLightingDrawOffset();

				Point drawPos = (entity.Position.ToVector2() * 16 - Main.screenPosition + offset).ToPoint();

				Rectangle draw = new Rectangle(drawPos.X, drawPos.Y + 18 + (int)(maxWaterDrawDiff * invRatio), 80, (int)(maxWaterDrawDiff * curWaterRatio));
				Rectangle source = new Rectangle(0, (int)(maxWaterDrawDiff * invRatio) + 18, 80, (int)(maxWaterDrawDiff * curWaterRatio));

				if(entity.FluidEntries[0].current > 0)
					spriteBatch.Draw(this.GetEffectTexture("water"), draw, source, Lighting.GetColor(i, j));

				spriteBatch.Draw(this.GetEffectTexture("bars"), drawPos.ToVector2(), null, Lighting.GetColor(i, j));

				if(entity.FluidEntries[0].current > 0)
					spriteBatch.Draw(this.GetEffectTexture("water"), draw, source, Lighting.GetColor(i, j) * (50f / 255f));
			}

			return true;
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch){
			GetDefaultParams(out _, out uint width, out uint height, out _);

			Tile tile = Framing.GetTileSafely(i, j);
			Point16 frame = tile.TileCoord();
			Point16 pos = new Point16(i, j) - frame;
			bool lastTile = frame.X == width - 1 && frame.Y == height - 1;

			if(MiscUtils.TryGetTileEntity(pos, out ElectrolyzerEntity entity) && lastTile){
				//Draw order: battery, lights, gas overlay, tanks
				Vector2 offset = MiscUtils.GetLightingDrawOffset();

				Point drawPos = (entity.Position.ToVector2() * 16 - Main.screenPosition + offset).ToPoint();

				if(!entity.RetrieveItem(0).IsAir)
					spriteBatch.Draw(this.GetEffectTexture("battery"), drawPos.ToVector2(), null, Lighting.GetColor(i, j));

				if(entity.ReactionInProgress){
					spriteBatch.Draw(this.GetEffectTexture("lightgreen"), drawPos.ToVector2(), null, Color.White);
					Lighting.AddLight(drawPos.ToVector2() + new Vector2(44, 20), 0f, 0.87f, 0f);
				}else{
					spriteBatch.Draw(this.GetEffectTexture("lightred"), drawPos.ToVector2(), null, Color.White);
					Lighting.AddLight(drawPos.ToVector2() + new Vector2(36, 20), 0.87f, 0f, 0f);
				}

				float gas1Factor = entity.FluidEntries[1].current / entity.FluidEntries[1].max;
				float gas2Factor = entity.FluidEntries[2].current / entity.FluidEntries[2].max;

				if(gas1Factor > 0){
					Color color = Capsule.GetBackColor(entity.FluidEntries[1].id);
					color = MiscUtils.MixLightColors(Lighting.GetColor(i, j), color);
					spriteBatch.Draw(this.GetEffectTexture("gashydrogen"), drawPos.ToVector2(), null, color * (gas1Factor * 135f / 255f));
				}
				if(gas2Factor > 0){
					Color color = Capsule.GetBackColor(entity.FluidEntries[2].id);
					color = MiscUtils.MixLightColors(Lighting.GetColor(i, j), color);
					spriteBatch.Draw(this.GetEffectTexture("gasoxygen"), drawPos.ToVector2(), null, color * (gas2Factor * 135f / 255f));
				}

				spriteBatch.Draw(this.GetEffectTexture("tanks"), drawPos.ToVector2(), null, Lighting.GetColor(i, j));
			}
		}

		public override bool PreHandleMouse(Point16 pos)
			=> TileUtils.TryPlaceLiquidInMachine<ElectrolyzerEntity>(this, pos);

		public override bool HandleMouse(Point16 pos){
			var id = MiscUtils.GetFluidIDFromItem(Main.LocalPlayer.HeldItem.type);

			return TileUtils.HandleMouse<ElectrolyzerEntity>(this, pos, () => MiscUtils.TryGetTileEntity(pos, out ElectrolyzerEntity entity) && !Array.Exists(entity.FluidEntries[0].validTypes, t => t == id));
		}
	}
}
