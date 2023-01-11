using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerraScience.Common.UI.Elements {
	public enum ArrowElementOrientation : byte {
		Left = 0,
		Up,
		Right,
		Down
	}

	public class BasicThinArrow : UIElement, IColorable {
		public readonly ArrowElementOrientation orientation;
		private int targetLength;

		private float fillPercentage;
		public float FillPercentage {
			get => fillPercentage;
			set => fillPercentage = Utils.Clamp(value, 0, 1);
		}

		private readonly Asset<Texture2D> texture;

		private float BodyScale => (targetLength - headLength - tailLength) / (float)bodyLength;

		private int MaxFill => targetLength - headLength - tailLength + headFillLength + tailFillLength;

		private int PixelsDrawn => (int)(MaxFill * FillPercentage);

		public Color Color { get; set; }

		public BasicThinArrow(ArrowElementOrientation orientation, int targetLength) {
			this.orientation = orientation;
			this.targetLength = targetLength;

			ArrangeSelf();

			texture = ModContent.Request<Texture2D>("TerraScience/Assets/UI/basic arrow thin " + orientation switch {
				ArrowElementOrientation.Left => "left",
				ArrowElementOrientation.Up => "up",
				ArrowElementOrientation.Right => "right",
				ArrowElementOrientation.Down => "down",
				_ => throw new Exception()
			});
		}

		public void SetLength(int targetLength) {
			this.targetLength = targetLength;
			ArrangeSelf();
			Recalculate();
		}

		const int tailLength = 4;
		const int bodyLength = 2;
		const int headLength = 24;
		
		const int thickness = 44;

		const int tailFillLength = tailLength - 2;
		const int headFillLength = headLength - 2;

		private void ArrangeSelf() {
			if (targetLength % 2 != 0)
				throw new InvalidOperationException("Target length must be divisible by 2");

			if (targetLength < headLength + bodyLength + tailLength)
				throw new InvalidOperationException("Target length must be >= " + (headLength + bodyLength + tailLength));

			if (orientation is ArrowElementOrientation.Left or ArrowElementOrientation.Right) {
				Width.Set(targetLength, 0f);
				Height.Set(thickness, 0f);
			} else if (orientation is ArrowElementOrientation.Up or ArrowElementOrientation.Down) {
				Width.Set(thickness, 0f);
				Height.Set(targetLength, 0f);
			} else
				throw new InvalidOperationException("Invalid orientation: " + orientation);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			Rectangle headSrc, bodySrc, tailSrc;

			switch (orientation) {
				case ArrowElementOrientation.Left:
					headSrc = new Rectangle(0, 0, headLength, thickness);
					bodySrc = new Rectangle(headLength + 2, 0, bodyLength, thickness);
					tailSrc = new Rectangle(headLength + 2 + bodyLength + 2, 0, tailLength, thickness);

					DrawHorizontalArrowOutline(spriteBatch, tailSrc, bodySrc, headSrc);
					
					headSrc.Y += thickness + 2;
					bodySrc.Y += thickness + 2;
					tailSrc.Y += thickness + 2;

					DrawHorizontalArrowFill(spriteBatch, headSrc, headFillLength, bodySrc, tailSrc, tailFillLength, leftToRight: false);
					break;
				case ArrowElementOrientation.Up:
					headSrc = new Rectangle(0, 0, thickness, headLength);
					bodySrc = new Rectangle(0, headLength + 2, thickness, bodyLength);
					tailSrc = new Rectangle(0, headLength + 2 + bodyLength + 2, thickness, tailLength);

					DrawVerticalArrowOutline(spriteBatch, headSrc, bodySrc, tailSrc);

					headSrc.X += thickness + 2;
					bodySrc.X += thickness + 2;
					tailSrc.X += thickness + 2;

					DrawVerticalArrowFill(spriteBatch, headSrc, headFillLength, bodySrc, tailSrc, tailFillLength, topToBottom: false);
					break;
				case ArrowElementOrientation.Right:
					tailSrc = new Rectangle(0, 0, tailLength, thickness);
					bodySrc = new Rectangle(tailLength + 2, 0, bodyLength, thickness);
					headSrc = new Rectangle(tailLength + 2 + bodyLength + 2, 0, headLength, thickness);

					DrawHorizontalArrowOutline(spriteBatch, tailSrc, bodySrc, headSrc);
					
					tailSrc.Y += thickness + 2;
					bodySrc.Y += thickness + 2;
					headSrc.Y += thickness + 2;

					DrawHorizontalArrowFill(spriteBatch, tailSrc, tailFillLength, bodySrc, headSrc, headFillLength, leftToRight: true);
					break;
				case ArrowElementOrientation.Down:
					tailSrc = new Rectangle(0, 0, thickness, tailLength);
					bodySrc =  new Rectangle(0, tailLength + 2, thickness, bodyLength);
					headSrc = new Rectangle(0, tailLength + 2 + bodyLength + 2, thickness, headLength);

					DrawVerticalArrowOutline(spriteBatch, tailSrc, bodySrc, headSrc);
					
					tailSrc.X += thickness + 2;
					bodySrc.X += thickness + 2;
					headSrc.X += thickness + 2;

					DrawVerticalArrowFill(spriteBatch, tailSrc, tailFillLength, bodySrc, headSrc, headFillLength, topToBottom: true);
					break;
			}
		}

		private void DrawHorizontalArrowOutline(SpriteBatch spriteBatch, Rectangle leftSrc, Rectangle bodySrc, Rectangle rightSrc) {
			var texture = this.texture.Value;
			var position = GetDimensions().Position();

			// Draw the left slice
			spriteBatch.Draw(texture, position, leftSrc, Color.White);

			// Draw the body
			float bodyX = leftSrc.Width;
			Rectangle destRect = new Rectangle((int)(position.X + bodyX), (int)position.Y, (int)(bodySrc.Width * BodyScale), bodySrc.Height);
			spriteBatch.Draw(texture, destRect, bodySrc, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);

			// Draw the right slice
			float rightX = Width.Pixels - rightSrc.Width;
			spriteBatch.Draw(texture, position + new Vector2(rightX, 0), rightSrc, Color.White);
		}

		private void DrawVerticalArrowOutline(SpriteBatch spriteBatch, Rectangle topSrc, Rectangle bodySrc, Rectangle bottomSrc) {
			var texture = this.texture.Value;
			var position = GetDimensions().Position();

			// Draw the top slice
			spriteBatch.Draw(texture, position, topSrc, Color.White);

			// Draw the body
			float bodyY = topSrc.Height;
			Rectangle destRect = new Rectangle((int)position.X, (int)(position.Y + bodyY), bodySrc.Width, (int)(bodySrc.Height * BodyScale));
			spriteBatch.Draw(texture, destRect, bodySrc, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);

			// Draw the bottom slice
			float bottomY = Height.Pixels - bottomSrc.Height;
			spriteBatch.Draw(texture, position + new Vector2(0, bottomY), bottomSrc, Color.White);
		}

		private void DrawHorizontalArrowFill(SpriteBatch spriteBatch, Rectangle leftSrc, int leftFillRange, Rectangle bodySrc, Rectangle rightSrc, int rightFillRange, bool leftToRight) {
			Vector2 position = GetDimensions().Position();

			var headSrc = leftToRight ? rightSrc : leftSrc;
			var tailSrc = leftToRight ? leftSrc : rightSrc;

			var headFillRange = leftToRight ? rightFillRange : leftFillRange;
			var tailFillRange = leftToRight ? leftFillRange : rightFillRange;

			DrawArrowFillTail(spriteBatch, position, tailFillRange, tailSrc, leftToRight, true);
			DrawArrowFillBody(spriteBatch, position, tailSrc.Width, bodySrc, tailFillRange, headFillRange, leftToRight, true);
			DrawArrowFillHead(spriteBatch, position, headFillRange, headSrc, leftToRight, true);
		}

		private void DrawVerticalArrowFill(SpriteBatch spriteBatch, Rectangle topSrc, int topFillRange, Rectangle bodySrc, Rectangle bottomSrc, int bottomFillRange, bool topToBottom) {
			Vector2 position = GetDimensions().Position();

			var headSrc = topToBottom ? bottomSrc : topSrc;
			var tailSrc = topToBottom ? topSrc : bottomSrc;

			var headFillRange = topToBottom ? bottomFillRange : topFillRange;
			var tailFillRange = topToBottom ? topFillRange : bottomFillRange;

			DrawArrowFillTail(spriteBatch, position, tailFillRange, tailSrc, topToBottom, false);
			DrawArrowFillBody(spriteBatch, position, tailSrc.Width, bodySrc, tailFillRange, headFillRange, topToBottom, false);
			DrawArrowFillHead(spriteBatch, position, headFillRange, headSrc, topToBottom, false);
		}

		private void DrawArrowFillTail(SpriteBatch spriteBatch, Vector2 position, int tailFillRange, Rectangle tailSrc, bool fromLeftOrTop, bool xAxis) {
			int pixels = PixelsDrawn;

			if (pixels <= 0)
				return;

			float tailCoord;
			float tailLength = tailSrc.Width;
			if (fromLeftOrTop) {
				if (pixels < tailFillRange) {
					if (xAxis)
						tailSrc.Width -= tailFillRange - pixels;
					else
						tailSrc.Height -= tailFillRange - pixels;
				}

				tailCoord = 0;
			} else {
				int fillOffset = 0;
				if (pixels < tailFillRange) {
					fillOffset = tailFillRange - pixels;

					if (xAxis) {
						tailSrc.Width -= fillOffset;
						tailSrc.X += fillOffset;
					} else {
						tailSrc.Height -= fillOffset;
						tailSrc.Y += fillOffset;
					}
				}

				tailCoord = (xAxis ? Width.Pixels : Height.Pixels) - tailLength + fillOffset;
			}

			Vector2 offset = xAxis ? new Vector2(tailCoord, 0) : new Vector2(0, tailCoord);
			spriteBatch.Draw(texture.Value, position + offset, tailSrc, Color);
		}

		private void DrawArrowFillBody(SpriteBatch spriteBatch, Vector2 position, int tailLength, Rectangle bodySrc, int tailFillRange, int headFillRange, bool fromLeftOrTop, bool xAxis) {
			int pixels = PixelsDrawn;

			if (pixels <= tailFillRange)
				return;

			int max = MaxFill;

			int destSize = pixels <= max - headFillRange ? (int)((pixels - tailFillRange) * BodyScale) : (int)((max - tailFillRange - headFillRange) * BodyScale);

			int bodyCoord;
			if (fromLeftOrTop)
				bodyCoord = (int)((xAxis ? position.X : position.Y) + tailLength);
			else
				bodyCoord = (int)((xAxis ? position.X + Width.Pixels : position.Y + Height.Pixels) - tailLength - destSize);

			Rectangle dest = xAxis ? new Rectangle(bodyCoord, (int)position.Y, destSize, bodySrc.Height) : new Rectangle((int)position.X, bodyCoord, bodySrc.Width, destSize);

			spriteBatch.Draw(texture.Value, dest, bodySrc, Color);
		}

		private void DrawArrowFillHead(SpriteBatch spriteBatch, Vector2 position, int headFillRange, Rectangle headSrc, bool fromLeftOrTop, bool xAxis) {
			int pixels = PixelsDrawn;
			int max = MaxFill;

			if (pixels <= max - headFillRange)
				return;
			
			float headLength = xAxis ? headSrc.Width : headSrc.Height;
			float headCoord;
			if (fromLeftOrTop) {
				if (pixels < max) {
					if (xAxis)
						headSrc.Width -= max - pixels;
					else
						headSrc.Height -= max - pixels;
				}

				headCoord = (xAxis ? Width.Pixels : Height.Pixels) - headLength;
			} else {
				int fillOffset = 0;
				if (pixels < max) {
					fillOffset = max - pixels;

					if (xAxis) {
						headSrc.Width -= fillOffset;
						headSrc.X += fillOffset;
					} else {
						headSrc.Height -= fillOffset;
						headSrc.Y += fillOffset;
					}
				}

				headCoord = fillOffset;
			}

			Vector2 offset = xAxis ? new Vector2(headCoord, 0) : new Vector2(0, headCoord);
			spriteBatch.Draw(texture.Value, position + offset, headSrc, Color);
		}
	}
}
