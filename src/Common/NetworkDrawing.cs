using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SerousEnergyLib.API.Fluid;
using SerousEnergyLib.Systems.Networks;
using SerousEnergyLib.Systems;
using SerousEnergyLib;
using Terraria.DataStructures;
using Terraria;
using Microsoft.Xna.Framework;
using SerousEnergyLib.TileData;
using System;
using SerousEnergyLib.Tiles;
using Terraria.ModLoader;
using SerousEnergyLib.Common.Configs;
using System.Linq;

namespace TerraScience.Common {
	public static class NetworkDrawing {
		public const float GasColorTransparency = 0.65f;

		public static void DrawFluid(SpriteBatch spriteBatch, Asset<Texture2D> texture, Point16 location, int columnsPerSet, int rowsPerSet) {
			Tile tile = Main.tile[location.X, location.Y];

			foreach (FluidNetwork net in Network.GetFluidNetworksAt(location.X, location.Y)) {
				var storage = net.Storage;

				if (storage.FluidType > FluidTypeID.None && !storage.IsEmpty) {
					double factor = storage.CurrentCapacity / storage.MaxCapacity;

					factor = Utils.Clamp(factor, 0, 1);

					double alpha = storage.FluidID.IsLiquid
						? 1d
						: GasColorTransparency * factor;

					if (alpha <= 0)
						return;

					Color color = storage.FluidID.FluidColor;

					// TODO: TechMod.Sets for "glowing" fluids
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

					Rectangle frame = texture.Frame(columnsPerSet, rowsPerSet * 4, tile.TileFrameX / 18, y * rowsPerSet + tile.TileFrameY / 18);

					spriteBatch.Draw(texture.Value, location.ToWorldCoordinates(0, 0) + offset - Main.screenPosition, frame, color * (float)alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
				}
			}
		}

		public static void DrawPumpBar(SpriteBatch spriteBatch, Asset<Texture2D> texture, Point16 location) {
			Tile tile = Main.tile[location.X, location.Y];

			if (TileLoader.GetTile(tile.TileType) is not IPumpTile pump)
				return;

			if (Network.GetNetworkAt(location.X, location.Y, NetworkType.Items | NetworkType.Fluids) is not NetworkInstance net)
				return;

			// Get the timer
			int timer;
			if (net is ItemNetwork itemNet)
				itemNet.TryGetPumpTimer(location, out timer);
			else if (net is FluidNetwork fluidNet)
				fluidNet.TryGetPumpTimer(location, out timer);
			else
				return;  // Invalid network

			if (timer < 0)
				return;

			/*  Graph:   Cosine                       Sine
			              _|_                          |   ___
			            .' | `.            '.          | .'   `.
			    _______/___|___\_______    __\_________|/_______\___
			          /    |    \             \       /|         \
			    '-__-'     |     '-__-'        '-___-' |          '-
			 */
			float sin;

			if (RenderingConfig.PlayPumpAnimations) {
				float max = pump.GetMaxTimer(location.X, location.Y);
				//Get the value on the graph for this sinusoidal movement
				float time = (timer - max / 4) / (max / 2);
				float radians = MathHelper.Pi * time;
				sin = (float)Math.Sin(radians);
			} else
				sin = -1;

			// Move it up to above the X-axis
			sin += 1;

			// Then stretch it to fit the entire movement of the pump
			sin *= 8f / 2f;

			// Find the direction the offset needs to sway in
			PumpDirection direction = tile.Get<NetworkTaggedInfo>().PumpDirection;

			Vector2 offsetDirection = direction switch {
				PumpDirection.Left => -Vector2.UnitX,
				PumpDirection.Up => -Vector2.UnitY,
				PumpDirection.Right => Vector2.UnitX,
				PumpDirection.Down => Vector2.UnitY,
				_ => Vector2.Zero
			};

			if (offsetDirection == Vector2.Zero)
				return;

			Rectangle frame = texture.Frame(4, 1, tile.TileFrameX / 18, 0, sizeOffsetX: -2, sizeOffsetY: -2);

			Vector2 offset = TileFunctions.GetLightingDrawOffset();

			Vector2 drawPosition = location.ToWorldCoordinates(0, 0) - Main.screenPosition + offset + offsetDirection * sin;

			Color color = Lighting.GetColor(location.X, location.Y);

			spriteBatch.Draw(texture.Value, drawPosition, frame, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
		}
	}
}
