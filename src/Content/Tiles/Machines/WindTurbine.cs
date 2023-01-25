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
	public class WindTurbine : BaseMachineTile<WindTurbineEntity, WindTurbineItem> {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		private static MachineSpriteEffectInformation BladeEffectSprite;
		private static Asset<Texture2D> BladeAsset;

		protected override void SafeSetStaticDefaults() {
			if (Main.dedServ)
				return;

			BladeEffectSprite = new MachineSpriteEffectInformation(TechMod.GetEffectPath<WindTurbine>("blades"), new Vector2(8), null, true);
		}

		public override void GetMachineDimensions(out uint width, out uint height) {
			width = 1;
			height = 5;
		}

		public override MachineWorkbenchRegistry GetRegistry() {
			return new(Type, static tick => new MachineRegistryDisplayAnimationState(TechMod.GetExamplePath<WindTurbine>("tile"), 1, 1, 0, 0));
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			if (TileLoader.GetTile(Main.tile[i, j].TileType) is not WindTurbine turbine)
				return;

			turbine.GetMachineDimensions(out uint width, out uint height);

			Point16 orig = new Point16(i, j);
			Point16 topLeft = TileFunctions.GetTopLeftTileInMultitile(i, j);

			bool lastTile = orig.X - topLeft.X == width - 1 && orig.Y - topLeft.Y == height - 1;

			if (lastTile && IMachine.TryFindMachineExact(topLeft, out WindTurbineEntity machine)) {
				BladeAsset ??= ModContent.Request<Texture2D>(BladeEffectSprite.asset);

				var sprite = BladeEffectSprite.GetDrawInformation();

				sprite.origin = new Vector2(16);
				sprite.rotation = machine.bladeRadians;

				sprite.Draw(spriteBatch, topLeft);
			}
		}
	}
}
