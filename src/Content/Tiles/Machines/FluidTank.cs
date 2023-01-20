using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SerousEnergyLib;
using SerousEnergyLib.API;
using SerousEnergyLib.API.CrossMod;
using SerousEnergyLib.API.Fluid;
using SerousEnergyLib.API.Machines;
using SerousEnergyLib.Systems;
using SerousEnergyLib.Tiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerraScience.Common;
using TerraScience.Content.Items.Machines;
using TerraScience.Content.MachineEntities;

namespace TerraScience.Content.Tiles.Machines {
	public class FluidTank : BaseMachineTile<FluidTankEntity, FluidTankItem> {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		private static MachineSpriteEffectInformation FillEffectSprite;
		private static Asset<Texture2D> FillAsset;

		protected override void SafeSetStaticDefaults() {
			FillEffectSprite = new MachineSpriteEffectInformation(TechMod.GetEffectPath<FluidTank>("fill"), new Vector2(6), null, true);
		}

		public override void GetMachineDimensions(out uint width, out uint height) {
			width = 2;
			height = 3;
		}

		public override MachineWorkbenchRegistry GetRegistry() {
			return new(Type, static tick => new MachineRegistryDisplayAnimationState("TerraScience/Assets/Machines/FluidTank/Example_tile", 1, 1, 0, 0));
		}

		public override bool PreRightClick(IMachine machine, int x, int y) {
			if (machine is FluidTankEntity entity) {
				var item = Main.LocalPlayer.HeldItem;

				int fluid = TechMod.Sets.FluidTank.FluidImport[item.type];
				int leftover = TechMod.Sets.FluidTank.FluidImportLeftover[item.type];

				if (fluid > -1) {
					// Fill the tank with 1 L of the fluid
					// TODO: vials might not have 1 L
					double liter = 1d;

					entity.FluidStorage[0].Import(fluid, ref liter);

					// Fluid was imported.  If the "leftover" type exists and isn't the same item ID as the item used, drop it and consume the source item
					// Allow "single-use" items to exist, aka items with no leftover
					if (liter < 1d) {
						if (leftover != item.type) {
							item.stack--;

							if (item.stack <= 0)
								item.TurnToAir();

							if (leftover > -1)
								Main.LocalPlayer.QuickSpawnItem(new EntitySource_TileEntity(entity), leftover);
						}

						Netcode.SendReducedData(entity);
					}

					return true;
				}
			}

			return base.PreRightClick(machine, x, y);
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
			if (TileLoader.GetTile(Main.tile[i, j].TileType) is not FluidTank)
				return true;

			Point16 orig = new Point16(i, j);
			Point16 topLeft = TileFunctions.GetTopLeftTileInMultitile(i, j);

			bool firstTile = orig.X == topLeft.X && orig.Y == topLeft.Y;

			if (firstTile && IMachine.TryFindMachineExact(topLeft, out FluidTankEntity machine)) {
				var storage = machine.FluidStorage[0];

				if (!storage.IsEmpty && storage.FluidType != FluidTypeID.None) {
					FillAsset ??= ModContent.Request<Texture2D>(FillEffectSprite.asset, AssetRequestMode.ImmediateLoad);

					var fillSprite = FillEffectSprite;

					double percent = storage.CurrentCapacity / storage.MaxCapacity;

					// Get the fill height
					int assetHeight = FillAsset.Value.Height;
					int height = (int)(assetHeight * percent);
					if (height < 1 && percent > 0)
						height = 1;

					// Initialize the sprite data
					var sprite = fillSprite.GetDrawInformation();

					int diff = assetHeight - height;
					sprite.offset.Y += diff;
					if (diff > 0) {
						// Offset the source Y and height
						Rectangle src = FillAsset.Value.Frame();
						src.Y += diff;
						src.Height -= diff;

						sprite.frame = src;
					}

					// Set the color to the fluid's color
					sprite.color = storage.FluidID.FluidColor;

					if (!storage.FluidID.IsLiquid) {
						// The fluid is a gas, so make it slightly transparent 
						sprite.color *= NetworkDrawing.GasColorTransparency;
					}

					sprite.Draw(spriteBatch, topLeft);
				}
			}

			return true;
		}
	}
}
