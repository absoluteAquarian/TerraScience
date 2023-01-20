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
	public class FluidGauge : UIElement, IColorable {
		private int segmentWidth;
		private int segmentHeight;

		private double maxCapacity, currentCapacity;

		public double CurrentCapacity {
			get => currentCapacity;
			set => currentCapacity = Utils.Clamp(value, 0, maxCapacity);
		}

		private double FillPercentage => maxCapacity == 0 ? 0 : currentCapacity / maxCapacity;

		public Color Color { get; set; } = Color.White;

		private static Asset<Texture2D> Texture;

		public FluidGauge(double max, int segmentColumns, int segmentRows) {
			SetMaxCapacity(max);
			SetSegmentDimensions_WithoutRecalculate(segmentColumns, segmentRows);
		}

		public FluidGauge(double max, float pixelWidth, float pixelHeight) {
			SetMaxCapacity(max);
			SetDimensionsToAtLeast_WithoutRecalculate(pixelWidth, pixelHeight);
		}

		public void SetSegmentDimensions(int segmentColumns, int segmentRows) {
			SetSegmentDimensions_WithoutRecalculate(segmentColumns, segmentRows);
			Recalculate();
		}

		private void SetSegmentDimensions_WithoutRecalculate(int segmentColumns, int segmentRows) {
			if (segmentColumns < 6)
				throw new ArgumentOutOfRangeException(nameof(segmentColumns), "Segment column count must be greater than or equal to 6");
			if (segmentRows < 3)
				throw new ArgumentOutOfRangeException(nameof(segmentRows), "Segment row count must be greater than or equal to 3");

			segmentWidth = segmentColumns;
			segmentHeight = segmentRows;

			ArrangeSelf();
		}

		public void SetDimensionsToAtLeast(float pixelWidth, float pixelHeight) {
			SetDimensionsToAtLeast_WithoutRecalculate(pixelWidth, pixelHeight);
			Recalculate();
		}

		private void SetDimensionsToAtLeast_WithoutRecalculate(float pixelWidth, float pixelHeight) {
			const int minPixelWidth = 6 * singleSegmentWidth;
			const int minPixelHeight = rowOneTwoSegmentHeight + rowOneTwoSegmentHeight + rowThreeSegmentHeight;

			if (pixelWidth < minPixelWidth)
				throw new ArgumentOutOfRangeException(nameof(pixelWidth), "Pixel width must be greater than or equal to " + minPixelWidth + " pixels");
			if (pixelHeight < minPixelHeight)
				throw new ArgumentOutOfRangeException(nameof(pixelHeight), "Pixel height must be greater than or equal to " + minPixelHeight + " pixels");

			int widthSegmentCount = (int)Math.Ceiling(pixelWidth / singleSegmentWidth);
			int heightSegmentCount = 2 + (int)Math.Ceiling((pixelHeight - rowOneTwoSegmentHeight - rowThreeSegmentHeight) / rowOneTwoSegmentHeight);

			SetSegmentDimensions_WithoutRecalculate(widthSegmentCount, heightSegmentCount);
		}

		public void SetMaxCapacity(double max) {
			if (max <= 0)
				throw new ArgumentOutOfRangeException(nameof(max), "Maximum capacity must be greater than 0");

			maxCapacity = max;

			currentCapacity = Utils.Clamp(currentCapacity, 0, max);
		}

		private void ArrangeSelf() {
			Width.Set(segmentWidth * singleSegmentWidth, 0f);
			Height.Set((segmentHeight - 1) * rowOneTwoSegmentHeight + rowThreeSegmentHeight, 0f);
		}

		const int singleSegmentWidth = 20;
		const int rowOneTwoSegmentHeight = 24;
		const int rowThreeSegmentHeight = 26;

		const int srcX = singleSegmentWidth + 2;
		const int rowTwoY = rowOneTwoSegmentHeight + 2;
		const int rowThreeY = rowTwoY + rowOneTwoSegmentHeight + 2;

		private static readonly Rectangle[,] gaugeSource = new Rectangle[3, 6] {
			{
				new Rectangle(0,        0, singleSegmentWidth, rowOneTwoSegmentHeight),
				new Rectangle(srcX,     0, singleSegmentWidth, rowOneTwoSegmentHeight),
				new Rectangle(srcX * 2, 0, singleSegmentWidth, rowOneTwoSegmentHeight),
				new Rectangle(srcX * 3, 0, singleSegmentWidth, rowOneTwoSegmentHeight),
				new Rectangle(srcX * 4, 0, singleSegmentWidth, rowOneTwoSegmentHeight),
				new Rectangle(srcX * 5, 0, singleSegmentWidth, rowOneTwoSegmentHeight),
			},
			{
				new Rectangle(0,        rowTwoY, singleSegmentWidth, rowOneTwoSegmentHeight),
				new Rectangle(srcX,     rowTwoY, singleSegmentWidth, rowOneTwoSegmentHeight),
				new Rectangle(srcX * 2, rowTwoY, singleSegmentWidth, rowOneTwoSegmentHeight),
				new Rectangle(srcX * 3, rowTwoY, singleSegmentWidth, rowOneTwoSegmentHeight),
				new Rectangle(srcX * 4, rowTwoY, singleSegmentWidth, rowOneTwoSegmentHeight),
				new Rectangle(srcX * 5, rowTwoY, singleSegmentWidth, rowOneTwoSegmentHeight),
			},
			{
				new Rectangle(0,        rowThreeY, singleSegmentWidth, rowThreeSegmentHeight),
				new Rectangle(srcX,     rowThreeY, singleSegmentWidth, rowThreeSegmentHeight),
				new Rectangle(srcX * 2, rowThreeY, singleSegmentWidth, rowThreeSegmentHeight),
				new Rectangle(srcX * 3, rowThreeY, singleSegmentWidth, rowThreeSegmentHeight),
				new Rectangle(srcX * 4, rowThreeY, singleSegmentWidth, rowThreeSegmentHeight),
				new Rectangle(srcX * 5, rowThreeY, singleSegmentWidth, rowThreeSegmentHeight),
			}
		};

		const int backSrcRowOneTwoY = rowThreeY + rowThreeSegmentHeight + 2;
		const int backSrcRowThreeY = backSrcRowOneTwoY + (rowOneTwoSegmentHeight + 2) * 2;

		private static readonly Rectangle backSrcRowOneTwo = new Rectangle(0, backSrcRowOneTwoY, singleSegmentWidth, rowOneTwoSegmentHeight);
		private static readonly Rectangle backSrcRowThree = new Rectangle(0, backSrcRowThreeY, singleSegmentWidth, rowThreeSegmentHeight);
		
		const int fillingColumnOneThreeWidth = singleSegmentWidth - 2;
		const int fillingRowOneHeight = rowOneTwoSegmentHeight - 2;
		const int fillingRowThreeHeight = rowThreeSegmentHeight - 2;

		const int fillingColumnOneX = singleSegmentWidth + 4;
		const int fillingColumnTwoX = fillingColumnOneX + fillingColumnOneThreeWidth + 2;
		const int fillingColumnThreeX = fillingColumnTwoX + singleSegmentWidth + 2;
		const int fillingRowOneY = backSrcRowOneTwoY + 2;
		const int fillingRowTwoY = fillingRowOneY + fillingRowOneHeight + 2;
		const int fillingRowThreeY = fillingRowTwoY + fillingRowOneHeight + 2;

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			Texture ??= ModContent.Request<Texture2D>("TerraScience/Assets/UI/tank");

			DrawBack(spriteBatch);

			DrawFilling(spriteBatch);

			DrawOutline(spriteBatch);
		}

		private void DrawBack(SpriteBatch spriteBatch) {
			var position = GetDimensions().Position();
			var texture = Texture.Value;

			for (int y = 0; y < 3; y++) {
				Rectangle src = y < 2 ? backSrcRowOneTwo : backSrcRowThree;

				float top = position.Y;
				if (y == 1)
					top += rowOneTwoSegmentHeight;
				else if (y == 2)
					top += Height.Pixels - rowThreeSegmentHeight;

				float height = y switch {
					0 => rowOneTwoSegmentHeight + 4,
					1 => Height.Pixels - rowOneTwoSegmentHeight - rowThreeSegmentHeight + 4,
					2 => rowThreeSegmentHeight,
					_ => -1
				};

				for (int x = 0; x < 3; x++) {
					// Draw the segment, but stretched
					float left = position.X;
					if (x == 1)
						left += singleSegmentWidth;
					else if (x == 2)
						left += Width.Pixels - singleSegmentWidth;

					float width = x switch {
						0 => singleSegmentWidth + 4,
						1 => Width.Pixels - singleSegmentWidth * 2 + 4,
						2 => singleSegmentWidth,
						_ => -1
					};

					Rectangle dest = new Rectangle((int)left, (int)top, (int)width, (int)height);

					spriteBatch.Draw(texture, dest, src, Color.White);
				}
			}
		}

		private void DrawFilling(SpriteBatch spriteBatch) {
			// Ensure future proofing for animated fillings by NOT stretching the slices
			var position = GetDimensions().Position();
			var texture = Texture.Value;

			int total = (int)((Height.Pixels - 4) * FillPercentage);
			if (total < 1 && FillPercentage > 0)
				total = 1;

			int bottom = 0, top;

			// Draw from bottom to top
			int srcX, srcY, srcWidth, srcHeight;
			float drawOffsetX, drawOffsetY = Height.Pixels - 2;
			for (int y = 0; y < segmentHeight; y++) {
				if (y == 0) {
					srcY = fillingRowThreeY;
					srcHeight = fillingRowThreeHeight;
				} else if (y == segmentHeight - 1) {
					srcY = fillingRowOneY;
					srcHeight = fillingRowOneHeight;
				} else {
					srcY = fillingRowTwoY;
					srcHeight = rowOneTwoSegmentHeight;
				}

				if (total <= bottom) {
					// Row cannot be drawn
					break;
				}

				top = bottom + srcHeight;

				if (total < top) {
					// Row will need to be shortened
					int diff = top - total;
					srcY += diff;
					srcHeight -= diff;
				}

				drawOffsetX = 2;
				drawOffsetY -= srcHeight;

				for (int x = 0; x < segmentWidth; x++) {
					if (x == 0) {
						srcX = fillingColumnOneX;
						srcWidth = fillingColumnOneThreeWidth;
					} else if (x == segmentWidth - 1) {
						srcX = fillingColumnThreeX;
						srcWidth = fillingColumnOneThreeWidth;
					} else {
						srcX = fillingColumnTwoX;
						srcWidth = singleSegmentWidth;
					}

					// For some reason, the segments aren't perfectly next to each other...
					Vector2 stretchFillingScale = Vector2.One;
					if (x < segmentWidth - 1)
						stretchFillingScale.X = (float)(srcWidth + 2) / srcWidth;
					if (y > 0)
						stretchFillingScale.Y = (float)(srcHeight + (y == 1 ? 4 : 2)) / srcHeight;

					spriteBatch.Draw(texture, position + new Vector2(drawOffsetX, drawOffsetY), new Rectangle(srcX, srcY, srcWidth, srcHeight), Color, 0f, Vector2.Zero, stretchFillingScale, SpriteEffects.None, 0);

					drawOffsetX += srcWidth;
				}

				// Adjust the new "bottom pixel"
				bottom = top;
			}
		}

		private void DrawOutline(SpriteBatch spriteBatch) {
			var position = GetDimensions().Position();
			var texture = Texture.Value;

			float drawX, drawY = 0;
			for (int y = 0; y < segmentHeight; y++) {
				// The the row offset in the 2D array of sources
				int srcY = y;
				if (y > 0 && y < segmentHeight - 1)
					srcY = 1;
				else if (y == segmentHeight - 1)
					srcY = 2;

				drawX = 0;

				for (int x = 0; x < segmentWidth; x++) {
					// The the column offset in the 2D array of sources
					int srcX = x;
					if (x > 3 && x < segmentWidth - 1)
						srcX = 4;
					else if (x == segmentWidth - 1)
						srcX = 5;

					Rectangle src = gaugeSource[srcY, srcX];

					spriteBatch.Draw(texture, position + new Vector2(drawX, drawY), src, Color.White);

					drawX += singleSegmentWidth;
				}

				drawY += srcY switch {
					0 or 1 => rowOneTwoSegmentHeight,
					2 => rowThreeSegmentHeight,
					_ => 0
				};
			}
		}

		public override void Update(GameTime gameTime) {
			if (ContainsPoint(Main.MouseScreen))
				Main.instance.MouseText(Language.GetTextValue("Mods.TerraScience.UI.FluidGaugeHover", CurrentCapacity, maxCapacity));
		}
	}
}
