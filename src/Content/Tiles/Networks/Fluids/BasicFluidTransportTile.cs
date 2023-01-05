using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SerousEnergyLib;
using SerousEnergyLib.API.Fluid;
using SerousEnergyLib.Systems;
using SerousEnergyLib.Systems.Networks;
using SerousEnergyLib.TileData;
using SerousEnergyLib.Tiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace TerraScience.Content.Tiles.Networks.Fluids {
	public class BasicFluidTransportTile : BaseNetworkTile, IFluidTransportTile {
		public override string Texture => base.Texture.Replace("Content", "Assets");

		public override NetworkType NetworkTypeToPlace => NetworkType.Items;

		public virtual double MaxCapacity => 0.25;  // 0.25 Liters
		public virtual double ExportRate => 0.5 / 60d;

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch){
			DrawFluid(spriteBatch, ModContent.Request<Texture2D>("TerraScience/Assets/Tiles/Networks/Fluids/Effect_FluidTransportTile_fluid"), new Point16(i, j));

			return true;
		}

		protected static void DrawFluid(SpriteBatch spriteBatch, Asset<Texture2D> texture, Point16 location) {
			Tile tile = Main.tile[location.X, location.Y];

			if (Network.GetFluidNetworkAt(location.X, location.Y) is FluidNetwork net) {
				var storage = net.Storage;

				if (storage.FluidType > FluidTypeID.None && !storage.IsEmpty) {
					double factor = storage.CurrentCapacity / storage.MaxCapacity;

					factor = Utils.Clamp(factor, 0, 1);

					double alpha = storage.FluidID.IsLiquid
						? 1d
						: 0.65 * factor;

					if (alpha <= 0)
						return;

					Color color = storage.FluidID.FluidColor;

					color = new Color(Lighting.GetColor(location.X, location.Y).ToVector3() * color.ToVector3());

					Vector2 offset = TileFunctions.GetLightingDrawOffset();

					int y = 0;

					if (storage.FluidID.IsLiquid) {
						// Adjust to the proper frame set in the spritesheet
						if (factor < 0.3)
							y = 3;
						else if (factor < 0.6)
							y = 2;
						else if (factor < 0.9)
							y = 1;
					}

					Rectangle frame = texture.Frame(7, 16, tile.TileFrameX / 18, y * 4 + tile.TileFrameY / 18);

					spriteBatch.Draw(texture.Value, location.ToWorldCoordinates(0, 0) + offset - Main.screenPosition, frame, color * (float)alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
				}
			}
		}
	}
}
