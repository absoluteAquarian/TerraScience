using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
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
	public class Battery : BaseMachineTile<BatteryEntity, BatteryItem> {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		private static MachineSpriteEffectInformation EnergyEffectSheet;
		private static Asset<Texture2D> EnergyAsset;

		protected override void SafeSetStaticDefaults() {
			if (Main.dedServ)
				return;

			EnergyEffectSheet = new MachineSpriteEffectInformation(TechMod.GetEffectPath<Battery>("energy"), new Vector2(10, 4), null, affectedByLight: false);
		}

		public override void GetMachineDimensions(out uint width, out uint height) {
			width = 2;
			height = 2;
		}

		public override MachineWorkbenchRegistry GetRegistry() {
			return new(Type, static tick => new MachineRegistryDisplayAnimationState(TechMod.GetExamplePath<Battery>("tile"), 1, 1, 0, 0),
				static tick => new MachineRegistryDisplayAnimationState(TechMod.GetExamplePath<Battery>("anim"), 13, 1, tick % (13 * 20) / 20, 0, -2, -2));
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			if (TileLoader.GetTile(Main.tile[i, j].TileType) is not Battery battery)
				return;

			battery.GetMachineDimensions(out uint width, out uint height);

			Point16 orig = new Point16(i, j);
			Point16 topLeft = TileFunctions.GetTopLeftTileInMultitile(i, j);

			bool lastTile = orig.X - topLeft.X == width - 1 && orig.Y - topLeft.Y == height - 1;

			if (lastTile && IMachine.TryFindMachineExact(topLeft, out BatteryEntity machine) && !machine.PowerStorage.IsEmpty) {
				EnergyAsset ??= ModContent.Request<Texture2D>(EnergyEffectSheet.asset, AssetRequestMode.ImmediateLoad);

				Rectangle src = EnergyAsset.Value.Bounds;

				double factor = (double)machine.PowerStorage.CurrentCapacity / (double)machine.PowerStorage.MaxCapacity;
				int pixels = (int)(src.Height * factor);

				// "factor" is guaranteed to be > 0 here
				if (pixels < 1)
					pixels = 1;

				int diff = src.Height - pixels;
				src.Y += diff;
				src.Height -= diff;

				var sprite = EnergyEffectSheet.GetDrawInformation();
				sprite.frame = src;
				sprite.offset.Y += diff;

				sprite.Draw(spriteBatch, topLeft);
			}
		}
	}
}
