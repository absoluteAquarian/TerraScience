using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerraScience.Common.UI.Elements {
	public sealed class Thermostat : UIElement {
		private int bodySegments;

		private double minTemp, maxTemp, currentTemp;

		public double CurrentTemperature {
			get => currentTemp;
			set => currentTemp = Utils.Clamp(value, minTemp, maxTemp);
		}

		private double FillPercentage => (currentTemp - minTemp) / (maxTemp - minTemp);

		private int FillingHeight => (int)((Height.Pixels - bottomHeight + bottomShaftHeight) * FillPercentage);

		private static Asset<Texture2D> Texture;

		public Thermostat(int bodySegments, double minTemp, double maxTemp, double defaultTemp) {
			this.bodySegments = bodySegments;
			currentTemp = defaultTemp;

			SetTemperatureBounds(minTemp, maxTemp);
			ArrangeSelf();
		}

		public void SetSegmentCount(int bodySegments) {
			if (this.bodySegments == bodySegments)
				return;

			this.bodySegments = bodySegments;
			ArrangeSelf();
			Recalculate();
		}

		public void SetTemperatureBounds(double minTemp, double maxTemp) {
			if (minTemp >= maxTemp)
				throw new ArgumentOutOfRangeException(nameof(minTemp), "Minimum temperature must be less than maximum temperature");

			this.minTemp = minTemp;
			this.maxTemp = maxTemp;

			currentTemp = Utils.Clamp(currentTemp, minTemp, maxTemp);
		}

		const int width = 52;
		const int topHeight = 20;
		const int middleY = topHeight + 2, middleHeight = 20;
		const int bottomY = 44, bottomHeight = 56;
		const int bottomShaftHeight = 12;

		const int fillX = width + 2;

		private void ArrangeSelf() {
			Width.Set(width, 0f);
			Height.Set(topHeight + middleHeight * bodySegments + bottomHeight, 0f);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			Texture ??= ModContent.Request<Texture2D>("TerraScience/Assets/UI/thermostat");

			// Draw the filling
			DrawFilling(spriteBatch);

			// Draw the outline
			DrawOutline(spriteBatch);
		}

		private void DrawFilling(SpriteBatch spriteBatch) {
			int height = FillingHeight;
			Color color = GetColor();

			DrawFilling_Bottom(spriteBatch, height, color);
			DrawFilling_Middle(spriteBatch, height, color);
			DrawFilling_Top(spriteBatch, height, color);
		}

		private Color GetColor() {
			double percentage = FillPercentage;

			if (percentage < 0.1)
				return new Color() { PackedValue = 0xffff2600 };  // Dark blue
			else if (percentage < 0.1 + 0.8 / 5.0)
				return new Color() { PackedValue = 0xffff9400 };  // Blue
			else if (percentage < 0.1 + 2 * 0.8 / 5)
				return new Color() { PackedValue = 0xffffd800 };  // Sky blue
			else if (percentage < 0.1 + 3 * 0.8 / 5)
				return new Color() { PackedValue = 0xff00ffff };  // Yellow
			else if (percentage < 0.1 + 4 * 0.8 / 5)
				return new Color() { PackedValue = 0xff00d8ff };  // Faded yellow
			else if (percentage < 0.9)
				return new Color() { PackedValue = 0xff006aff };  // Orange

			return new Color() { PackedValue = 0xff0000ff };  // Red
		}

		private void DrawFilling_Bottom(SpriteBatch spriteBatch, int fillingHeight, Color color) {
			int fillTop, fillHeight;
			if (fillingHeight < bottomShaftHeight) {
				int heightOffset = bottomShaftHeight - fillingHeight;

				fillTop = bottomY + heightOffset;
				fillHeight = bottomHeight - heightOffset;
			} else {
				fillTop = bottomY;
				fillHeight = bottomHeight;
			}

			Rectangle rect = new Rectangle(fillX, fillTop, width, fillHeight);

			spriteBatch.Draw(Texture.Value, GetDimensions().Position() + new Vector2(0, Height.Pixels - fillHeight), rect, color);
		}

		private void DrawFilling_Middle(SpriteBatch spriteBatch, int fillingHeight, Color color) {
			// Draw the segments from bottom to top
			var texture = Texture.Value;
			var position = GetDimensions().Position();

			for (int i = 0; i < bodySegments; i++) {
				int thresholdBottom = bottomShaftHeight + middleHeight * i;
				int thresholdTop = thresholdBottom + middleHeight;
				if (fillingHeight <= thresholdBottom)
					break;

				int fillTop, fillHeight;
				if (fillingHeight < thresholdTop) {
					int heightOffset = thresholdTop - fillingHeight;

					fillTop = middleY + heightOffset;
					fillHeight = middleHeight - heightOffset;
				} else {
					fillTop = middleY;
					fillHeight = middleHeight;
				}

				Rectangle rect = new Rectangle(fillX, fillTop, width, fillHeight);

				spriteBatch.Draw(texture, position + new Vector2(0, Height.Pixels - bottomHeight - (i + 1) * middleHeight), rect, color);
			}
		}

		private void DrawFilling_Top(SpriteBatch spriteBatch, int fillingHeight, Color color) {
			int thresholdBottom = bottomShaftHeight + middleHeight * bodySegments;
			int thresholdTop = thresholdBottom + topHeight;
			
			if (fillingHeight <= thresholdBottom)
				return;

			int fillTop, fillHeight;
			if (fillingHeight < thresholdTop) {
				int heightOffset = thresholdTop - fillingHeight;

				fillTop = heightOffset;
				fillHeight = topHeight - heightOffset;
			} else {
				fillTop = 0;
				fillHeight = topHeight;
			}

			Rectangle rect = new Rectangle(fillX, fillTop, width, fillHeight);
			spriteBatch.Draw(Texture.Value, GetDimensions().Position() + new Vector2(0, fillTop), rect, color);
		}

		private void DrawOutline(SpriteBatch spriteBatch) {
			DrawOutline_Bottom(spriteBatch);
			DrawOutline_Middle(spriteBatch);
			DrawOutline_Top(spriteBatch);
		}

		private void DrawOutline_Bottom(SpriteBatch spriteBatch) {
			spriteBatch.Draw(Texture.Value, GetDimensions().Position() + new Vector2(0, Height.Pixels - bottomHeight), new Rectangle(0, bottomY, width, bottomHeight), Color.White);
		}

		private void DrawOutline_Middle(SpriteBatch spriteBatch) {
			// Draw the segments from bottom to top
			var texture = Texture.Value;
			var position = GetDimensions().Position();

			for (int i = 0; i < bodySegments; i++)
				spriteBatch.Draw(texture, position + new Vector2(0, Height.Pixels - bottomHeight - (i + 1) * middleHeight), new Rectangle(0, middleY, width, middleHeight), Color.White);
		}

		private void DrawOutline_Top(SpriteBatch spriteBatch) {
			spriteBatch.Draw(Texture.Value, GetDimensions().Position(), new Rectangle(0, 0, width, topHeight), Color.White);
		}

		public override void Update(GameTime gameTime) {
			if (ContainsPoint(Main.MouseScreen))
				Main.instance.MouseText(Language.GetTextValue("Mods.TerraScience.UI.ThermostatHover", CurrentTemperature, maxTemp));
		}
	}
}
