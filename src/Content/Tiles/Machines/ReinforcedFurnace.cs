﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SerousEnergyLib;
using SerousEnergyLib.API;
using SerousEnergyLib.API.CrossMod;
using SerousEnergyLib.API.Machines;
using SerousEnergyLib.Tiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerraScience.Content.Items.Machines;
using TerraScience.Content.MachineEntities;
using TerraScience.Content.Sounds;

namespace TerraScience.Content.Tiles.Machines {
	public class ReinforcedFurnace : BaseMachineTile<ReinforcedFurnaceEntity, ReinforcedFurnaceItem> {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		private static MachineSpriteEffectInformation FireEffectSheet;
		private static Asset<Texture2D> FireAsset;

		protected override void SafeSetStaticDefaults() {
			Main.tileLighted[Type] = true;

			if (Main.dedServ)
				return;

			string path = TechMod.GetEffectPath<ReinforcedFurnace>("fuel_sheet");
			Texture2D fuelSheet = ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad).Value;

			TechMod.Sets.ReinforcedFurnace.ItemInFurnace[ItemID.Wood] = new MachineSpriteEffectInformation(path, new Vector2(14), fuelSheet.Frame(1, 7, 0, 0), true);
			TechMod.Sets.ReinforcedFurnace.ItemInFurnace[ItemID.BorealWood] = new MachineSpriteEffectInformation(path, new Vector2(14), fuelSheet.Frame(1, 7, 0, 1), true);
			TechMod.Sets.ReinforcedFurnace.ItemInFurnace[ItemID.RichMahogany] = new MachineSpriteEffectInformation(path, new Vector2(14), fuelSheet.Frame(1, 7, 0, 2), true);
			TechMod.Sets.ReinforcedFurnace.ItemInFurnace[ItemID.Ebonwood] = new MachineSpriteEffectInformation(path, new Vector2(14), fuelSheet.Frame(1, 7, 0, 3), true);
			TechMod.Sets.ReinforcedFurnace.ItemInFurnace[ItemID.Shadewood] = new MachineSpriteEffectInformation(path, new Vector2(14), fuelSheet.Frame(1, 7, 0, 4), true);
			TechMod.Sets.ReinforcedFurnace.ItemInFurnace[ItemID.PalmWood] = new MachineSpriteEffectInformation(path, new Vector2(14), fuelSheet.Frame(1, 7, 0, 5), true);
			TechMod.Sets.ReinforcedFurnace.ItemInFurnace[ItemID.Pearlwood] = new MachineSpriteEffectInformation(path, new Vector2(14), fuelSheet.Frame(1, 7, 0, 6), true);

			FireEffectSheet = new MachineSpriteEffectInformation(TechMod.GetEffectPath<ReinforcedFurnace>("fire"), new Vector2(10, 8), null, false);
		}

		public override void GetMachineDimensions(out uint width, out uint height) {
			width = 3;
			height = 2;
		}

		public override MachineWorkbenchRegistry GetRegistry() {
			return new(Type,
				static tick => new MachineRegistryDisplayAnimationState(TechMod.GetExamplePath<ReinforcedFurnace>("tile"), 1, 1, 0, 0),
				static tick => new MachineRegistryDisplayAnimationState(TechMod.GetExamplePath<ReinforcedFurnace>("anim"), 1, 4, 0, tick % (4 * 15) / 15));
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			// Kill the looping sound
			if (IMachine.TryFindMachineExact(new Point16(i, j), out ReinforcedFurnaceEntity entity)) {
				ISoundEmittingMachine.StopSound(
					emitter: entity,
					RegisteredSounds.IDs.ReinforcedFurnace.Burning,
					ref entity.burning,
					ref entity.servPlayingBurningSound);
			}

			base.KillMultiTile(i, j, frameX, frameY);
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			Point16 orig = new Point16(i, j);
			Point16 topLeft = TileFunctions.GetTopLeftTileInMultitile(i, j);

			bool flameTile = orig.X - topLeft.X == 1 && orig.Y == topLeft.Y;

			if (flameTile && IMachine.TryFindMachine(new Point16(i, j), out ReinforcedFurnaceEntity machine) && machine.IsActive(out double requiredHeat) && machine.CurrentTemperature >= requiredHeat) {
				// Entity is active and is actively "burning" items.  Emit light
				const float strength = 0.3f;

				r = 0xd5 / 255f * strength;
				g = 0x44 / 255f * strength;
				b = 0x00 / 155f * strength;
			}
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			if (TileLoader.GetTile(Main.tile[i, j].TileType) is not ReinforcedFurnace furnace)
				return;

			furnace.GetMachineDimensions(out uint width, out uint height);

			Point16 orig = new Point16(i, j);
			Point16 topLeft = TileFunctions.GetTopLeftTileInMultitile(i, j);

			bool lastTile = orig.X - topLeft.X == width - 1 && orig.Y - topLeft.Y == height - 1;

			if (lastTile && IMachine.TryFindMachineExact(topLeft, out ReinforcedFurnaceEntity machine) && machine.IsActive(out double requiredHeat)) {
				// Draw the input and flame when applicable
				var ingredientSprite = TechMod.Sets.ReinforcedFurnace.ItemInFurnace[machine.Inventory[0].type];

				// Assume that the info is valid.  If the asset doesn't exist, MachineSpriteEffectData will throw an exception
				var sprite = ingredientSprite.GetDrawInformation();
				machine.GetHeatTargets(out double minHeat, out double maxHeat, out _);
				sprite.color = Color.Lerp(Color.White, Color.Orange, (float)((machine.CurrentTemperature - minHeat) / (maxHeat - minHeat)));

				sprite.Draw(spriteBatch, topLeft);

				if (machine.CurrentTemperature >= requiredHeat) {
					// Animate the fire
					FireAsset ??= ModContent.Request<Texture2D>(FireEffectSheet.asset, AssetRequestMode.ImmediateLoad);

					sprite = FireEffectSheet.GetDrawInformation();
					sprite.frame = FireAsset.Frame(1, 4, 0, (int)(Main.GameUpdateCount % 60 / 15));

					sprite.Draw(spriteBatch, topLeft);
				}
			}
		}
	}
}
