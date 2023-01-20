using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerraScience.Common.UI.Elements {
	public class PowerGauge : UIElement, IColorable {
		private int bodySegments;

		private double maxPower, currentPower;

		public double CurrentPower {
			get => currentPower;
			set => currentPower = Utils.Clamp(value, 0, maxPower);
		}

		public string TypeIDShortName { get; set; } = "TF";

		private double FillPercentage => maxPower == 0 ? 0 : currentPower / maxPower;

		private int FillingHeight => (int)((Height.Pixels - 4) * FillPercentage);

		public Color Color { get; set; } = Color.White;

		private static Asset<Texture2D> Texture;

		public PowerGauge(double max, float height) {
			SetMaxCapacity(max);
			SetHeight_WithoutRecalculate(height);
		}

		public void SetMaxCapacity(double max) {
			if (max <= 0)
				throw new ArgumentOutOfRangeException(nameof(max), "Maximum power capacity must be greater than 0");

			maxPower = max;

			currentPower = Utils.Clamp(currentPower, 0, max);
		}

		public void SetHeight(float pixelHeight) {
			SetHeight_WithoutRecalculate(pixelHeight);
			Recalculate();
		}

		private void SetHeight_WithoutRecalculate(float pixelHeight) {
			const int minPixelHeight = topHeight + middleHeight + bottomHeight;

			if (pixelHeight < minPixelHeight)
				throw new ArgumentOutOfRangeException("Pixel height must be greater than or equal to " + minPixelHeight + " pixels");

			bodySegments = (int)Math.Ceiling((pixelHeight - topHeight - bottomHeight) / middleHeight);

			ArrangeSelf();
		}

		const int width = 12;
		const int topHeight = 10, middleHeight = 8, bottomHeight = 10;
		
		const int fillingWidth = width - 4;
		const int fillingHeight = topHeight - 2;

		private static readonly Rectangle outlineSrcTop = new Rectangle(0, 0, width, topHeight);
		private static readonly Rectangle outlineSrcMiddle = new Rectangle(0, topHeight + 2, width, middleHeight);
		private static readonly Rectangle outlineSrcBottom = new Rectangle(0, topHeight + middleHeight + 4, width, bottomHeight);

		private static readonly Rectangle fillingBackground = new Rectangle(width + 2, 0, fillingWidth, fillingHeight);

		private static readonly Rectangle filling = new Rectangle(width + 2, fillingHeight + 2, fillingWidth, fillingHeight);

		private void ArrangeSelf() {
			Width.Set(width, 0f);
			Height.Set(topHeight + middleHeight * bodySegments + bottomHeight, 0f);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			Texture ??= ModContent.Request<Texture2D>("TerraScience/Assets/UI/power");

			var position = GetDimensions().Position();
			var texture = Texture.Value;
			var color = Color;

			// Draw the outline
			float drawOffsetY = 0;
			for (int y = 0; y < bodySegments + 2; y++) {
				Rectangle src;
				if (y == 0)
					src = outlineSrcTop;
				else if (y == bodySegments + 1)
					src = outlineSrcBottom;
				else
					src = outlineSrcMiddle;

				spriteBatch.Draw(texture, position + new Vector2(0, drawOffsetY), src, Color.White);

				drawOffsetY += src.Height;
			}

			// Draw the background
			drawOffsetY = 2;
			for (int y = 0; y < bodySegments + 2; y++) {
				spriteBatch.Draw(texture, position + new Vector2(2, drawOffsetY), fillingBackground, color);

				drawOffsetY += fillingBackground.Height;
			}
			
			// Draw the filling, bottom to top
			int pixels = FillingHeight;
			int bottom = 0, top;

			drawOffsetY = Height.Pixels - 2;
			for (int y = 0; y < bodySegments + 2; y++) {
				int srcY = filling.Y;
				int srcHeight = fillingHeight;

				if (pixels <= bottom) {
					// No more filling to draw
					break;
				}

				top = bottom + fillingHeight;

				if (pixels < top) {
					// Filling needs to be shortened
					int diff = top - pixels;
					srcY += diff;
					srcHeight -= diff;
				}

				drawOffsetY -= srcHeight;

				Rectangle src = filling with { Y = srcY, Height = srcHeight };

				spriteBatch.Draw(texture, position + new Vector2(2, drawOffsetY), src, color);

				// Adjust the new "bottom pixel"
				bottom = top;
			}
		}

		public override void Update(GameTime gameTime) {
			if (ContainsPoint(Main.MouseScreen))
				Main.instance.MouseText(Language.GetTextValue("Mods.TerraScience.UI.PowerGaugeHover", CurrentPower, maxPower, TypeIDShortName));
		}
	}
}
