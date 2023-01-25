using Microsoft.Xna.Framework.Graphics;
using SerousEnergyLib;
using SerousEnergyLib.API;
using SerousEnergyLib.API.CrossMod;
using SerousEnergyLib.API.Machines;
using SerousEnergyLib.Tiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Content.Items.Machines;
using TerraScience.Content.MachineEntities;

namespace TerraScience.Content.Tiles.Machines {
	public partial class Greenhouse : BaseMachineTile<GreenhouseEntity, GreenhouseItem> {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		protected override void SafeSetStaticDefaults() {
			Main.tileLighted[Type] = true;

			SetPlantInformation();
			SetSpriteData();
		}

		public override void GetMachineDimensions(out uint width, out uint height) {
			width = 2;
			height = 3;
		}

		internal static int FrameDelay = 0;
		internal static int CurrentFrame = -1;

		public override MachineWorkbenchRegistry GetRegistry() {
			return new(Type,
				static tick => new MachineRegistryDisplayAnimationState(TechMod.GetExamplePath<Greenhouse>("tile"), 1, 1, 0, 0),
				GetSecondAnimation);
		}

		private static MachineRegistryDisplayAnimationState GetSecondAnimation(uint tick) {
			if (--FrameDelay < 0 || CurrentFrame < 0 || CurrentFrame > 38) {
				CurrentFrame = Main.rand.Next(0, 39);
				FrameDelay = 36;
			}

			return new MachineRegistryDisplayAnimationState(TechMod.GetExamplePath<Greenhouse>("plants"), 7, 6, CurrentFrame % 7, CurrentFrame / 7, -2, -2);
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			Point16 orig = new Point16(i, j);
			Point16 topLeft = TileFunctions.GetTopLeftTileInMultitile(i, j);

			bool glassTile = orig.Y - topLeft.Y < 2;

			if (glassTile && IMachine.TryFindMachineExact(topLeft, out GreenhouseEntity machine)) {
				float progress = machine.Progress.Progress;

				if (machine.MightBeAbleToGrowAPlant(out var info) && TechMod.Sets.Greenhouse.TryGetPlantSprites(info.soil, info.modifier, info.plant, out var spriteInfo)) {
					// Render the light
					if (progress >= 0.95f && spriteInfo.light is { } light) {
						const float strength = 0.1f;

						r = light.R / 255f * strength;
						g = light.G / 255f * strength;
						b = light.B / 155f * strength;
					}
				}
			}
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			if (TileLoader.GetTile(Main.tile[i, j].TileType) is not Greenhouse)
				return true;

			Point16 orig = new Point16(i, j);
			Point16 topLeft = TileFunctions.GetTopLeftTileInMultitile(i, j);

			bool firstTile = orig.X == topLeft.X && orig.Y == topLeft.Y;

			if (firstTile && IMachine.TryFindMachineExact(topLeft, out GreenhouseEntity machine)) {
				float progress = machine.Progress.Progress;

				MachineSpriteEffectData sprite;

				if (machine.MightBeAbleToGrowAPlant(out var info) && TechMod.Sets.Greenhouse.TryGetPlantSprites(info.soil, info.modifier, info.plant, out var spriteInfo)) {
					// Render the plant
					if (progress < 0.45f) {
						sprite = spriteInfo.immature.GetDrawInformation();

						sprite.Draw(spriteBatch, topLeft);
					} else if (progress < 0.95f) {
						sprite = (spriteInfo.mature ?? spriteInfo.immature).GetDrawInformation();

						sprite.Draw(spriteBatch, topLeft);
					} else {
						sprite = (spriteInfo.blooming ?? spriteInfo.immature).GetDrawInformation();

						sprite.Draw(spriteBatch, topLeft);

						// If the glowmask exists, draw it
						if (spriteInfo.glowmask is { } glowmask) {
							sprite = glowmask.GetDrawInformation();

							sprite.Draw(spriteBatch, topLeft);
						}
					}
				}
				
				if (machine.IsSoilRenderableWithoutAPlant(out MachineSpriteEffectInformation soilSprite)) {
					// Render the soil
					sprite = soilSprite.GetDrawInformation();

					sprite.Draw(spriteBatch, topLeft);
				}
			}

			return true;
		}
	}
}
